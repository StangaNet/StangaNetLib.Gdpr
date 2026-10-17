using FluentAssertions;
using StangaNetLib.Core.Common;
using StangaNetLib.Gdpr.Consent.Abstractions;
using StangaNetLib.Gdpr.Consent.Models;
using static StangaNetLib.Core.Common.ErrorType;

namespace StangaNetLib.Gdpr.Tests;

// ── InMemoryConsentRepository ─────────────────────────────────────────────────
// Reference in-memory implementation of IConsentRepository.
// Consuming projects provide their own EF Core implementations.

file sealed class InMemoryConsentRepository : IConsentRepository
{
    private readonly List<ConsentRecord> _records = new();

    public Task<Result<ConsentRecord>> GetAsync(
        string userId, string consentType, CancellationToken ct = default)
    {
        var record = _records
            .Where(r => r.UserId == userId && r.ConsentType == consentType)
            .OrderByDescending(r => r.GrantedAt)
            .FirstOrDefault();

        return Task.FromResult(record is not null
            ? Result<ConsentRecord>.Success(record)
            : Result<ConsentRecord>.Failure(Error.NotFound("Consent", $"{userId}:{consentType}")));
    }

    public Task<IReadOnlyList<ConsentRecord>> GetAllAsync(
        string userId, CancellationToken ct = default)
    {
        var latest = _records
            .Where(r => r.UserId == userId)
            .GroupBy(r => r.ConsentType)
            .Select(g => g.OrderByDescending(r => r.GrantedAt).First())
            .ToList();

        return Task.FromResult<IReadOnlyList<ConsentRecord>>(latest);
    }

    public Task<IReadOnlyList<ConsentRecord>> GetHistoryAsync(
        string userId, string consentType, CancellationToken ct = default)
    {
        var history = _records
            .Where(r => r.UserId == userId && r.ConsentType == consentType)
            .OrderByDescending(r => r.GrantedAt)
            .ToList();

        return Task.FromResult<IReadOnlyList<ConsentRecord>>(history);
    }

    public Task<Result<ConsentRecord>> GrantAsync(
        string userId, string consentType, string? ipAddress = null,
        string? userAgent = null, string policyVersion = "1.0", CancellationToken ct = default)
    {
        var record = new ConsentRecord
        {
            UserId        = userId,
            ConsentType   = consentType,
            IsGranted     = true,
            IpAddress     = ipAddress,
            UserAgent     = userAgent,
            PolicyVersion = policyVersion,
        };
        _records.Add(record);
        return Task.FromResult(Result<ConsentRecord>.Success(record));
    }

    public async Task<Result<ConsentRecord>> RevokeAsync(
        string userId, string consentType, string? ipAddress = null,
        string? userAgent = null, CancellationToken ct = default)
    {
        var current = await GetAsync(userId, consentType, ct);
        if (current.IsFailure)
            return Result<ConsentRecord>.Failure(Error.NotFound("Consent", $"{userId}:{consentType}"));

        var record = new ConsentRecord
        {
            UserId      = userId,
            ConsentType = consentType,
            IsGranted   = false,
            IpAddress   = ipAddress,
            UserAgent   = userAgent,
        };
        _records.Add(record);
        return Result<ConsentRecord>.Success(record);
    }

    public async Task<Result<int>> RevokeAllAsync(string userId, CancellationToken ct = default)
    {
        var all         = await GetAllAsync(userId, ct);
        var activeTypes = all.Where(r => r.IsGranted).Select(r => r.ConsentType).ToList();

        foreach (var t in activeTypes)
            _records.Add(new ConsentRecord { UserId = userId, ConsentType = t, IsGranted = false });

        return Result<int>.Success(activeTypes.Count);
    }

    public void SeedRecord(ConsentRecord record) => _records.Add(record);
}

// ── Tests ─────────────────────────────────────────────────────────────────────

public sealed class ConsentRepositoryContractTests
{
    // ── GetAsync ─────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAsync_NeverGranted_ReturnsNotFound()
    {
        var sut    = new InMemoryConsentRepository();
        var result = await sut.GetAsync("user-1", "Marketing", TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(NotFound);
    }

    [Fact]
    public async Task GetAsync_AfterGrant_ReturnsGrantedRecord()
    {
        var sut = new InMemoryConsentRepository();
        await sut.GrantAsync("user-1", "Marketing", ct: TestContext.Current.CancellationToken);

        var result = await sut.GetAsync("user-1", "Marketing", TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value!.IsGranted.Should().BeTrue();
        result.Value.UserId.Should().Be("user-1");
        result.Value.ConsentType.Should().Be("Marketing");
    }

    [Fact]
    public async Task GetAsync_AfterRevoke_ReturnsRevokedRecord()
    {
        var sut = new InMemoryConsentRepository();
        await sut.GrantAsync("user-1", "Marketing", ct: TestContext.Current.CancellationToken);
        await sut.RevokeAsync("user-1", "Marketing", ct: TestContext.Current.CancellationToken);

        var result = await sut.GetAsync("user-1", "Marketing", TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value!.IsGranted.Should().BeFalse();
    }

    // ── GrantAsync ────────────────────────────────────────────────────────

    [Fact]
    public async Task GrantAsync_ReturnsSuccessWithGrantedRecord()
    {
        var sut    = new InMemoryConsentRepository();
        var result = await sut.GrantAsync("user-2", "Analytics",
            ipAddress: "1.2.3.4", userAgent: "UA/1.0",
            ct: TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value!.IsGranted.Should().BeTrue();
        result.Value.IpAddress.Should().Be("1.2.3.4");
        result.Value.UserAgent.Should().Be("UA/1.0");
        result.Value.PolicyVersion.Should().Be("1.0");
    }

    [Fact]
    public async Task GrantAsync_CustomPolicyVersion_IsPreserved()
    {
        var sut    = new InMemoryConsentRepository();
        var result = await sut.GrantAsync("user-2", "Analytics",
            policyVersion: "2.3", ct: TestContext.Current.CancellationToken);

        result.Value!.PolicyVersion.Should().Be("2.3");
    }

    // ── RevokeAsync ────────────────────────────────────────────────────────

    [Fact]
    public async Task RevokeAsync_WhenNeverGranted_ReturnsNotFound()
    {
        var sut    = new InMemoryConsentRepository();
        var result = await sut.RevokeAsync("user-1", "Marketing",
            ct: TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(NotFound);
    }

    [Fact]
    public async Task RevokeAsync_AfterGrant_ReturnsSuccessWithRevokedRecord()
    {
        var sut = new InMemoryConsentRepository();
        await sut.GrantAsync("user-1", "Analytics", ct: TestContext.Current.CancellationToken);

        var result = await sut.RevokeAsync("user-1", "Analytics",
            ct: TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value!.IsGranted.Should().BeFalse();
    }

    // ── GetHistoryAsync ────────────────────────────────────────────────────

    [Fact]
    public async Task GetHistoryAsync_MultipleDecisions_ReturnsNewestFirst()
    {
        var sut = new InMemoryConsentRepository();
        var ct  = TestContext.Current.CancellationToken;

        sut.SeedRecord(new ConsentRecord { UserId = "user-1", ConsentType = "Marketing", IsGranted = true,  GrantedAt = DateTimeOffset.UtcNow.AddSeconds(-2) });
        sut.SeedRecord(new ConsentRecord { UserId = "user-1", ConsentType = "Marketing", IsGranted = false, GrantedAt = DateTimeOffset.UtcNow.AddSeconds(-1) });
        sut.SeedRecord(new ConsentRecord { UserId = "user-1", ConsentType = "Marketing", IsGranted = true,  GrantedAt = DateTimeOffset.UtcNow });

        var history = await sut.GetHistoryAsync("user-1", "Marketing", ct);

        history.Should().HaveCount(3);
        history[0].IsGranted.Should().BeTrue("most recent decision was a grant");
    }

    [Fact]
    public async Task GetHistoryAsync_NeverGranted_ReturnsEmpty()
    {
        var sut     = new InMemoryConsentRepository();
        var history = await sut.GetHistoryAsync("user-1", "Marketing",
            TestContext.Current.CancellationToken);

        history.Should().BeEmpty();
    }

    // ── GetAllAsync ────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAllAsync_ReturnsLatestPerConsentType()
    {
        var sut = new InMemoryConsentRepository();
        var ct  = TestContext.Current.CancellationToken;

        sut.SeedRecord(new ConsentRecord { UserId = "user-1", ConsentType = "Marketing", IsGranted = true,  GrantedAt = DateTimeOffset.UtcNow.AddSeconds(-3) });
        sut.SeedRecord(new ConsentRecord { UserId = "user-1", ConsentType = "Analytics", IsGranted = true,  GrantedAt = DateTimeOffset.UtcNow.AddSeconds(-2) });
        sut.SeedRecord(new ConsentRecord { UserId = "user-1", ConsentType = "Marketing", IsGranted = false, GrantedAt = DateTimeOffset.UtcNow.AddSeconds(-1) });

        var all = await sut.GetAllAsync("user-1", ct);

        all.Should().HaveCount(2);
        all.Single(r => r.ConsentType == "Marketing").IsGranted.Should().BeFalse();
        all.Single(r => r.ConsentType == "Analytics").IsGranted.Should().BeTrue();
    }

    [Fact]
    public async Task GetAllAsync_NoHistory_ReturnsEmpty()
    {
        var sut = new InMemoryConsentRepository();
        var all = await sut.GetAllAsync("user-1", TestContext.Current.CancellationToken);

        all.Should().BeEmpty();
    }

    // ── RevokeAllAsync ────────────────────────────────────────────────────

    [Fact]
    public async Task RevokeAllAsync_RevokesAllActiveConsents_ReturnsCount()
    {
        var sut = new InMemoryConsentRepository();
        var ct  = TestContext.Current.CancellationToken;
        await sut.GrantAsync("user-1", "Marketing",  ct: ct);
        await sut.GrantAsync("user-1", "Analytics",  ct: ct);
        await sut.GrantAsync("user-1", "Profiling",  ct: ct);

        var result = await sut.RevokeAllAsync("user-1", ct);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(3);
    }

    [Fact]
    public async Task RevokeAllAsync_AfterRevoke_AllConsentsAreRevoked()
    {
        var sut = new InMemoryConsentRepository();
        var ct  = TestContext.Current.CancellationToken;
        await sut.GrantAsync("user-1", "Marketing", ct: ct);
        await sut.GrantAsync("user-1", "Analytics", ct: ct);

        await sut.RevokeAllAsync("user-1", ct);

        var all = await sut.GetAllAsync("user-1", ct);
        all.Should().AllSatisfy(r => r.IsGranted.Should().BeFalse());
    }

    [Fact]
    public async Task RevokeAllAsync_NoActiveConsents_ReturnsZero()
    {
        var sut    = new InMemoryConsentRepository();
        var result = await sut.RevokeAllAsync("user-1", TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(0);
    }

}
