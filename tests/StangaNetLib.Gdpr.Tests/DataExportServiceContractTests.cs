using FluentAssertions;
using StangaNetLib.Core.Common;
using StangaNetLib.Gdpr.Dsar.Abstractions;
using StangaNetLib.Gdpr.Dsar.Models;

namespace StangaNetLib.Gdpr.Tests;

// ── InMemoryDataExportService ──────────────────────────────────────────────────
// Reference in-memory implementation of IDataExportService.

file sealed class InMemoryDataExportService : IDataExportService
{
    private readonly Dictionary<string, string> _store;
    private readonly bool _simulateFailure;

    public InMemoryDataExportService(
        Dictionary<string, string>? store          = null,
        bool                        simulateFailure = false)
    {
        _store           = store ?? new();
        _simulateFailure = simulateFailure;
    }

    public Task<Result<DataExportResult>> ExportAsync(string userId, CancellationToken ct = default)
    {
        if (_simulateFailure)
            return Task.FromResult(
                Result<DataExportResult>.Failure(Error.Internal("Simulated export failure")));

        var json   = _store.TryGetValue(userId, out var data) ? data : "{}";
        var result = new DataExportResult
        {
            UserId      = userId,
            JsonContent = json,
            ExportedAt  = DateTimeOffset.UtcNow,
        };
        return Task.FromResult(Result<DataExportResult>.Success(result));
    }
}

// ── Tests ─────────────────────────────────────────────────────────────────────

public sealed class DataExportServiceContractTests
{
    [Fact]
    public async Task ExportAsync_ReturnsSuccess_WithMatchingUserId()
    {
        var sut    = new InMemoryDataExportService();
        var result = await sut.ExportAsync("user-42", TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value!.UserId.Should().Be("user-42");
    }

    [Fact]
    public async Task ExportAsync_ReturnsNonEmptyJsonContent()
    {
        var store  = new Dictionary<string, string> { ["user-1"] = """{"email":"u@example.com"}""" };
        var sut    = new InMemoryDataExportService(store);
        var result = await sut.ExportAsync("user-1", TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value!.JsonContent.Should().NotBeNullOrWhiteSpace();
        result.Value.JsonContent.Should().Contain("email");
    }

    [Fact]
    public async Task ExportAsync_ExportedAt_IsRecent()
    {
        var sut    = new InMemoryDataExportService();
        var before = DateTimeOffset.UtcNow;
        var result = await sut.ExportAsync("user-1", TestContext.Current.CancellationToken);
        var after  = DateTimeOffset.UtcNow;

        result.Value!.ExportedAt.Should().BeOnOrAfter(before)
              .And.BeOnOrBefore(after);
    }

    [Fact]
    public async Task ExportAsync_DefaultContentType_IsApplicationJson()
    {
        var sut    = new InMemoryDataExportService();
        var result = await sut.ExportAsync("user-1", TestContext.Current.CancellationToken);

        result.Value!.ContentType.Should().Be("application/json");
    }

    [Fact]
    public async Task ExportAsync_UnknownUser_ReturnsEmptyJsonObject()
    {
        var sut    = new InMemoryDataExportService();
        var result = await sut.ExportAsync("unknown-user", TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value!.JsonContent.Should().Be("{}");
    }

    [Fact]
    public async Task ExportAsync_OnFailure_ReturnsFailureResult()
    {
        var sut    = new InMemoryDataExportService(simulateFailure: true);
        var result = await sut.ExportAsync("user-1", TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
    }
}
