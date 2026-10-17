using FluentAssertions;
using StangaNetLib.Core.Common;
using StangaNetLib.Gdpr.Dsar.Abstractions;

namespace StangaNetLib.Gdpr.Tests;

// ── InMemoryDataDeletionService ───────────────────────────────────────────────
// Reference in-memory implementation of IDataDeletionService.

file sealed class InMemoryDataDeletionService : IDataDeletionService
{
    private readonly HashSet<string> _users;
    private readonly HashSet<string> _pseudonymized = new();
    private readonly bool            _simulateFailure;

    public InMemoryDataDeletionService(
        IEnumerable<string>? existingUsers  = null,
        bool                 simulateFailure = false)
    {
        _users           = [..existingUsers ?? []];
        _simulateFailure = simulateFailure;
    }

    public bool UserExists(string userId)      => _users.Contains(userId);
    public bool IsPseudonymized(string userId) => _pseudonymized.Contains(userId);

    public Task<Result> DeleteUserDataAsync(string userId, CancellationToken ct = default)
    {
        if (_simulateFailure)
            return Task.FromResult(Result.Failure(Error.Internal("Simulated failure")));

        _users.Remove(userId);
        _pseudonymized.Remove(userId);
        return Task.FromResult(Result.Success());
    }

    public Task<Result> PseudonymizeUserDataAsync(string userId, CancellationToken ct = default)
    {
        if (_simulateFailure)
            return Task.FromResult(Result.Failure(Error.Internal("Simulated failure")));

        if (_users.Contains(userId))
            _pseudonymized.Add(userId);

        return Task.FromResult(Result.Success());
    }
}

// ── Tests ─────────────────────────────────────────────────────────────────────

public sealed class DataDeletionServiceContractTests
{
    // ── DeleteUserDataAsync ───────────────────────────────────────────────

    [Fact]
    public async Task DeleteUserDataAsync_ExistingUser_ReturnsSuccess()
    {
        var sut    = new InMemoryDataDeletionService(["user-1"]);
        var result = await sut.DeleteUserDataAsync("user-1", TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteUserDataAsync_RemovesUserData()
    {
        var sut = new InMemoryDataDeletionService(["user-1"]);

        await sut.DeleteUserDataAsync("user-1", TestContext.Current.CancellationToken);

        sut.UserExists("user-1").Should().BeFalse();
    }

    [Fact]
    public async Task DeleteUserDataAsync_OnFailure_ReturnsFailureResult()
    {
        var sut    = new InMemoryDataDeletionService(simulateFailure: true);
        var result = await sut.DeleteUserDataAsync("user-1", TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteUserDataAsync_IsPermanent_DataCannotBeRetrieved()
    {
        var sut = new InMemoryDataDeletionService(["user-1"]);
        await sut.DeleteUserDataAsync("user-1", TestContext.Current.CancellationToken);

        sut.UserExists("user-1").Should().BeFalse("permanent deletion must erase all traces");
    }

    // ── PseudonymizeUserDataAsync ─────────────────────────────────────────

    [Fact]
    public async Task PseudonymizeUserDataAsync_ReturnsSuccess()
    {
        var sut    = new InMemoryDataDeletionService(["user-1"]);
        var result = await sut.PseudonymizeUserDataAsync("user-1", TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task PseudonymizeUserDataAsync_MarksPseudonymized()
    {
        var sut = new InMemoryDataDeletionService(["user-1"]);
        await sut.PseudonymizeUserDataAsync("user-1", TestContext.Current.CancellationToken);

        sut.IsPseudonymized("user-1").Should().BeTrue();
    }

    [Fact]
    public async Task PseudonymizeUserDataAsync_DoesNotDeleteUser()
    {
        var sut = new InMemoryDataDeletionService(["user-1"]);
        await sut.PseudonymizeUserDataAsync("user-1", TestContext.Current.CancellationToken);

        sut.UserExists("user-1").Should().BeTrue("pseudonymization retains statistical data");
    }

    [Fact]
    public async Task PseudonymizeUserDataAsync_OnFailure_ReturnsFailureResult()
    {
        var sut    = new InMemoryDataDeletionService(simulateFailure: true);
        var result = await sut.PseudonymizeUserDataAsync("user-1", TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
    }
}
