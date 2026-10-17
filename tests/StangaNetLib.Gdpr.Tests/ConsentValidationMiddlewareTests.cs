using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Logging.Abstractions;
using StangaNetLib.Core.Common;
using StangaNetLib.Gdpr.Consent.Abstractions;
using StangaNetLib.Gdpr.Consent.Attributes;
using StangaNetLib.Gdpr.Consent.Middleware;
using StangaNetLib.Gdpr.Consent.Models;
using System.Security.Claims;

namespace StangaNetLib.Gdpr.Tests;

// ── FakeConsentRepository ────────────────────────────────────────────────────

file sealed class FakeConsentRepository : IConsentRepository
{
    private readonly Dictionary<(string UserId, string ConsentType), ConsentRecord> _store = new();

    public void Seed(string userId, string consentType, bool isGranted)
        => _store[(userId, consentType)] = new ConsentRecord
        {
            UserId      = userId,
            ConsentType = consentType,
            IsGranted   = isGranted,
        };

    public Task<Result<ConsentRecord>> GetAsync(string userId, string consentType, CancellationToken ct = default)
        => Task.FromResult(_store.TryGetValue((userId, consentType), out var r)
            ? Result<ConsentRecord>.Success(r)
            : Result<ConsentRecord>.Failure(Error.NotFound("Consent", $"{userId}:{consentType}")));

    public Task<IReadOnlyList<ConsentRecord>> GetAllAsync(string userId, CancellationToken ct = default)
        => Task.FromResult<IReadOnlyList<ConsentRecord>>([]);

    public Task<IReadOnlyList<ConsentRecord>> GetHistoryAsync(string userId, string consentType, CancellationToken ct = default)
        => Task.FromResult<IReadOnlyList<ConsentRecord>>([]);

    public Task<Result<ConsentRecord>> GrantAsync(string userId, string consentType, string? ipAddress = null,
        string? userAgent = null, string policyVersion = "1.0", CancellationToken ct = default)
        => throw new NotImplementedException();

    public Task<Result<ConsentRecord>> RevokeAsync(string userId, string consentType, string? ipAddress = null,
        string? userAgent = null, CancellationToken ct = default)
        => throw new NotImplementedException();

    public Task<Result<int>> RevokeAllAsync(string userId, CancellationToken ct = default)
        => throw new NotImplementedException();
}

// ── Minimal IEndpointFeature for tests ───────────────────────────────────────

file sealed class EndpointFeature : IEndpointFeature
{
    public Endpoint? Endpoint { get; set; }
}

// ── Tests ─────────────────────────────────────────────────────────────────────

public sealed class ConsentValidationMiddlewareTests
{
    private static ConsentValidationMiddleware Create(
        IConsentRepository repo,
        RequestDelegate?   next = null)
    {
        next ??= _ => Task.CompletedTask;
        return new ConsentValidationMiddleware(
            next, repo, NullLogger<ConsentValidationMiddleware>.Instance);
    }

    private static DefaultHttpContext MakeContext(
        bool      authenticated = false,
        string    userId        = "user-1",
        string[]? consentTypes  = null)
    {
        var ctx = new DefaultHttpContext();
        ctx.Response.Body = new MemoryStream();

        if (authenticated)
        {
            ctx.User = new ClaimsPrincipal(
                new ClaimsIdentity([new Claim("sub", userId)], "Test"));
        }

        if (consentTypes is { Length: > 0 })
        {
            var attributes = consentTypes
                .Select(t => (object)new RequireConsentAttribute(t))
                .ToList();

            ctx.Features.Set<IEndpointFeature>(new EndpointFeature
            {
                Endpoint = new Endpoint(
                    _ => Task.CompletedTask,
                    new EndpointMetadataCollection(attributes),
                    "Test Endpoint"),
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
        var sut = Create(new FakeConsentRepository(),
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
        var sut = Create(new FakeConsentRepository(),
            _ => { nextCalled = true; return Task.CompletedTask; });
        var ctx = MakeContext(authenticated: false, consentTypes: ["Marketing"]);

        await sut.InvokeAsync(ctx);

        nextCalled.Should().BeTrue();
    }

    // ── Consent granted → pass through ────────────────────────────────────

    [Fact]
    public async Task InvokeAsync_ConsentGranted_CallsNext()
    {
        var repo = new FakeConsentRepository();
        repo.Seed("user-1", "Marketing", isGranted: true);

        var nextCalled = false;
        var sut = Create(repo, _ => { nextCalled = true; return Task.CompletedTask; });
        var ctx = MakeContext(authenticated: true, userId: "user-1", consentTypes: ["Marketing"]);

        await sut.InvokeAsync(ctx);

        nextCalled.Should().BeTrue();
        ctx.Response.StatusCode.Should().Be(200);
    }

    // ── Consent never recorded → 403 ─────────────────────────────────────

    [Fact]
    public async Task InvokeAsync_ConsentMissing_Returns403()
    {
        var repo       = new FakeConsentRepository(); // nothing seeded
        var nextCalled = false;
        var sut = Create(repo, _ => { nextCalled = true; return Task.CompletedTask; });
        var ctx = MakeContext(authenticated: true, userId: "user-1", consentTypes: ["Marketing"]);

        await sut.InvokeAsync(ctx);

        nextCalled.Should().BeFalse();
        ctx.Response.StatusCode.Should().Be(403);
    }

    // ── Consent explicitly revoked → 403 ──────────────────────────────────

    [Fact]
    public async Task InvokeAsync_ConsentRevoked_Returns403()
    {
        var repo = new FakeConsentRepository();
        repo.Seed("user-1", "Analytics", isGranted: false);

        var nextCalled = false;
        var sut = Create(repo, _ => { nextCalled = true; return Task.CompletedTask; });
        var ctx = MakeContext(authenticated: true, userId: "user-1", consentTypes: ["Analytics"]);

        await sut.InvokeAsync(ctx);

        nextCalled.Should().BeFalse();
        ctx.Response.StatusCode.Should().Be(403);
    }

    // ── Multiple consents: all granted → pass ────────────────────────────

    [Fact]
    public async Task InvokeAsync_AllConsentsGranted_CallsNext()
    {
        var repo = new FakeConsentRepository();
        repo.Seed("user-1", "Marketing", isGranted: true);
        repo.Seed("user-1", "Analytics", isGranted: true);

        var nextCalled = false;
        var sut = Create(repo, _ => { nextCalled = true; return Task.CompletedTask; });
        var ctx = MakeContext(
            authenticated: true, userId: "user-1",
            consentTypes: ["Marketing", "Analytics"]);

        await sut.InvokeAsync(ctx);

        nextCalled.Should().BeTrue();
    }

    // ── Multiple consents: one missing → 403 ─────────────────────────────

    [Fact]
    public async Task InvokeAsync_OneConsentMissing_Returns403()
    {
        var repo = new FakeConsentRepository();
        repo.Seed("user-1", "Marketing", isGranted: true);
        // "Analytics" not seeded

        var nextCalled = false;
        var sut = Create(repo, _ => { nextCalled = true; return Task.CompletedTask; });
        var ctx = MakeContext(
            authenticated: true, userId: "user-1",
            consentTypes: ["Marketing", "Analytics"]);

        await sut.InvokeAsync(ctx);

        nextCalled.Should().BeFalse();
        ctx.Response.StatusCode.Should().Be(403);
    }

    // ── 403 body contains the missing consent type ────────────────────────

    [Fact]
    public async Task InvokeAsync_ConsentMissing_ResponseBodyContainsConsentType()
    {
        var repo = new FakeConsentRepository();
        var sut  = Create(repo);
        var ctx  = MakeContext(authenticated: true, userId: "user-1", consentTypes: ["Marketing"]);

        await sut.InvokeAsync(ctx);

        var body = await ReadBodyAsync(ctx);
        body.Should().Contain("Marketing");
    }
}
