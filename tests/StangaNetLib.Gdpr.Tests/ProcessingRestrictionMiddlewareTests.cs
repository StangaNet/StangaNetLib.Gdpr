using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Logging.Abstractions;
using StangaNetLib.Core.Common;
using StangaNetLib.Gdpr.Objection.Abstractions;
using StangaNetLib.Gdpr.Objection.Attributes;
using StangaNetLib.Gdpr.Objection.Middleware;
using StangaNetLib.Gdpr.Objection.Models;
using System.Security.Claims;

namespace StangaNetLib.Gdpr.Tests;

// ── FakeProcessingRestrictionService ─────────────────────────────────────────

file sealed class FakeProcessingRestrictionService : IProcessingRestrictionService
{
    private readonly HashSet<(string UserId, string Scope)> _restrictions = new();

    public void Seed(string userId, string scope = "*") => _restrictions.Add((userId, scope));

    public Task<IReadOnlyList<ProcessingRestriction>> GetActiveAsync(string userId, CancellationToken ct = default)
        => Task.FromResult<IReadOnlyList<ProcessingRestriction>>([]);

    public Task<bool> IsRestrictedAsync(string userId, string scope = "*", CancellationToken ct = default)
    {
        var restricted = _restrictions.Contains((userId, scope))
                      || _restrictions.Contains((userId, "*"));
        return Task.FromResult(restricted);
    }

    public Task<Result<ProcessingRestriction>> RestrictAsync(string userId, string reason,
        string scope = "*", CancellationToken ct = default)
        => throw new NotImplementedException();

    public Task<Result<ProcessingRestriction>> LiftAsync(Guid restrictionId, CancellationToken ct = default)
        => throw new NotImplementedException();
}

// ── Tests ─────────────────────────────────────────────────────────────────────

public sealed class ProcessingRestrictionMiddlewareTests
{
    private static ProcessingRestrictionMiddleware Create(
        IProcessingRestrictionService service,
        RequestDelegate?              next = null)
    {
        next ??= _ => Task.CompletedTask;
        return new ProcessingRestrictionMiddleware(
            next, service, NullLogger<ProcessingRestrictionMiddleware>.Instance);
    }

    private static DefaultHttpContext MakeContext(
        bool    authenticated = false,
        string  userId        = "user-1",
        string? scope         = null)
    {
        var ctx = new DefaultHttpContext();
        ctx.Response.Body = new MemoryStream();

        if (authenticated)
        {
            ctx.User = new ClaimsPrincipal(
                new ClaimsIdentity([new Claim("sub", userId)], "Test"));
        }

        if (scope is not null)
        {
            ctx.Features.Set<IEndpointFeature>(new EndpointFeatureStub
            {
                Endpoint = new Endpoint(
                    _ => Task.CompletedTask,
                    new EndpointMetadataCollection(new RequireNoRestrictionAttribute(scope)),
                    "Test"),
            });
        }

        return ctx;
    }

    private static async Task<string> ReadBodyAsync(DefaultHttpContext ctx)
    {
        ctx.Response.Body.Seek(0, SeekOrigin.Begin);
        return await new StreamReader(ctx.Response.Body).ReadToEndAsync();
    }

    // ── No attribute → pass through ───────────────────────────────────────

    [Fact]
    public async Task InvokeAsync_NoAttribute_CallsNext()
    {
        var nextCalled = false;
        var sut = Create(new FakeProcessingRestrictionService(),
            _ => { nextCalled = true; return Task.CompletedTask; });
        var ctx = MakeContext(authenticated: true);

        await sut.InvokeAsync(ctx);

        nextCalled.Should().BeTrue();
    }

    // ── Anonymous user → pass through ────────────────────────────────────

    [Fact]
    public async Task InvokeAsync_AnonymousUser_CallsNext()
    {
        var nextCalled = false;
        var sut = Create(new FakeProcessingRestrictionService(),
            _ => { nextCalled = true; return Task.CompletedTask; });
        var ctx = MakeContext(authenticated: false, scope: "*");

        await sut.InvokeAsync(ctx);

        nextCalled.Should().BeTrue();
    }

    // ── No restriction active → pass through ─────────────────────────────

    [Fact]
    public async Task InvokeAsync_NoRestriction_CallsNext()
    {
        var nextCalled = false;
        var sut = Create(new FakeProcessingRestrictionService(),
            _ => { nextCalled = true; return Task.CompletedTask; });
        var ctx = MakeContext(authenticated: true, userId: "user-1", scope: "*");

        await sut.InvokeAsync(ctx);

        nextCalled.Should().BeTrue();
        ctx.Response.StatusCode.Should().Be(200);
    }

    // ── Active restriction → HTTP 451 ────────────────────────────────────

    [Fact]
    public async Task InvokeAsync_ActiveRestriction_Returns451()
    {
        var service = new FakeProcessingRestrictionService();
        service.Seed("user-1", "*");

        var nextCalled = false;
        var sut = Create(service, _ => { nextCalled = true; return Task.CompletedTask; });
        var ctx = MakeContext(authenticated: true, userId: "user-1", scope: "*");

        await sut.InvokeAsync(ctx);

        nextCalled.Should().BeFalse();
        ctx.Response.StatusCode.Should().Be(451);
    }

    // ── Restriction on different scope → pass through ─────────────────────

    [Fact]
    public async Task InvokeAsync_RestrictionOnDifferentScope_CallsNext()
    {
        var service = new FakeProcessingRestrictionService();
        service.Seed("user-1", "MarketingEmails");

        var nextCalled = false;
        var sut = Create(service, _ => { nextCalled = true; return Task.CompletedTask; });
        var ctx = MakeContext(authenticated: true, userId: "user-1", scope: "Profiling");

        await sut.InvokeAsync(ctx);

        nextCalled.Should().BeTrue();
    }

    // ── Wildcard restriction blocks all scopes ────────────────────────────

    [Fact]
    public async Task InvokeAsync_WildcardRestriction_BlocksSpecificScopeEndpoint()
    {
        var service = new FakeProcessingRestrictionService();
        service.Seed("user-1", "*");

        var nextCalled = false;
        var sut = Create(service, _ => { nextCalled = true; return Task.CompletedTask; });
        var ctx = MakeContext(authenticated: true, userId: "user-1", scope: "Profiling");

        await sut.InvokeAsync(ctx);

        nextCalled.Should().BeFalse();
        ctx.Response.StatusCode.Should().Be(451);
    }

    // ── 451 body contains the scope ───────────────────────────────────────

    [Fact]
    public async Task InvokeAsync_ActiveRestriction_ResponseBodyContainsScope()
    {
        var service = new FakeProcessingRestrictionService();
        service.Seed("user-1", "Profiling");

        var sut = Create(service);
        var ctx = MakeContext(authenticated: true, userId: "user-1", scope: "Profiling");

        await sut.InvokeAsync(ctx);

        var body = await ReadBodyAsync(ctx);
        body.Should().Contain("Profiling");
    }

    // ── User without sub/NameIdentifier claim → pass through ──────────────

    [Fact]
    public async Task InvokeAsync_AuthenticatedWithoutSubClaim_CallsNext()
    {
        var nextCalled = false;
        var sut = Create(new FakeProcessingRestrictionService(),
            _ => { nextCalled = true; return Task.CompletedTask; });

        var ctx = new DefaultHttpContext();
        ctx.Response.Body = new MemoryStream();
        ctx.User = new ClaimsPrincipal(
            new ClaimsIdentity([new Claim("email", "user@example.com")], "Test"));
        ctx.Features.Set<IEndpointFeature>(new EndpointFeatureStub
        {
            Endpoint = new Endpoint(
                _ => Task.CompletedTask,
                new EndpointMetadataCollection(new RequireNoRestrictionAttribute()),
                "Test"),
        });

        await sut.InvokeAsync(ctx);

        nextCalled.Should().BeTrue();
    }
}

file sealed class EndpointFeatureStub : IEndpointFeature
{
    public Endpoint? Endpoint { get; set; }
}
