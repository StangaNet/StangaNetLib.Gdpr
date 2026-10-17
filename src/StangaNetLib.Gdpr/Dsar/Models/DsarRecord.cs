namespace StangaNetLib.Gdpr.Dsar.Models;

/// <summary>
/// Tracks a Data Subject Access Request (DSAR) throughout its full lifecycle.
/// GDPR Art. 12(3) requires response within one calendar month of receipt,
/// extendable by two months for complex or numerous requests (Art. 12(3)).
/// </summary>
public sealed record DsarRecord
{
    /// <summary>Unique identifier for this DSAR record.</summary>
    public Guid           Id             { get; init; } = Guid.NewGuid();

    /// <summary>Identifier of the data subject who submitted the request.</summary>
    public string         UserId         { get; init; } = string.Empty;

    /// <summary>The category of right being exercised.</summary>
    public DsarType       Type           { get; init; }

    /// <summary>Current lifecycle status of the request.</summary>
    public DsarStatus     Status         { get; init; } = DsarStatus.Pending;

    /// <summary>UTC timestamp when the request was received. Starts the one-month response clock.</summary>
    public DateTimeOffset ReceivedAt     { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Statutory deadline: one calendar month from <see cref="ReceivedAt"/> (Art. 12(3)).
    /// Extended to three months when <see cref="Status"/> is <see cref="DsarStatus.Extended"/>.
    /// </summary>
    public DateTimeOffset DeadlineAt     { get; init; }

    /// <summary>UTC timestamp when the request was resolved (completed or rejected).</summary>
    public DateTimeOffset? ResolvedAt    { get; init; }

    /// <summary>Free-text description of the data subject's request.</summary>
    public string?        RequestDetails { get; init; }

    /// <summary>Internal processing notes (not disclosed to the data subject).</summary>
    public string?        InternalNotes  { get; init; }

    /// <summary>
    /// Reason communicated to the data subject when <see cref="Status"/> is
    /// <see cref="DsarStatus.Rejected"/> or <see cref="DsarStatus.Extended"/>.
    /// </summary>
    public string?        Reason         { get; init; }

    /// <summary>IP address at time of submission (identity verification and audit trail).</summary>
    public string?        IpAddress      { get; init; }

    /// <summary><c>true</c> when the request is still open and the statutory deadline has passed.</summary>
    public bool           IsOverdue      => Status is DsarStatus.Pending or DsarStatus.InProgress or DsarStatus.Extended
                                            && DateTimeOffset.UtcNow > DeadlineAt;
}
