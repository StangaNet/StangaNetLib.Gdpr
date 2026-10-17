namespace StangaNetLib.Gdpr.Audit.Models;

/// <summary>
/// Tracks a personal data breach from discovery through notification (GDPR Art. 33/34).
/// <para>
/// Art. 33: medium/high-severity breaches must be reported to the supervisory authority within 72 hours of discovery.
/// Art. 34: high-severity breaches must also be communicated directly to affected data subjects without undue delay.
/// All breaches must be documented internally regardless of severity (Art. 33(5)).
/// </para>
/// </summary>
public sealed record DataBreachRecord
{
    /// <summary>Unique identifier for this breach record.</summary>
    public Guid                   Id                     { get; init; } = Guid.NewGuid();

    /// <summary>UTC timestamp when the breach was first detected.</summary>
    public DateTimeOffset         DetectedAt             { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>72-hour Art. 33 notification deadline, calculated from <see cref="DetectedAt"/>.</summary>
    public DateTimeOffset         NotificationDeadlineAt { get; init; }

    /// <summary>Risk classification that determines which notification obligations apply.</summary>
    public DataBreachSeverity     Severity               { get; init; }

    /// <summary>
    /// Nature of the breach, categories and approximate number of data subjects and records
    /// affected (Art. 33(3)(a)).
    /// </summary>
    public string                 Description            { get; init; } = string.Empty;

    /// <summary>Categories of personal data involved (e.g. "email", "health data", "financial data").</summary>
    public IReadOnlyList<string>  DataCategories         { get; init; } = [];

    /// <summary>Approximate number of data subjects affected. Use <c>-1</c> when unknown.</summary>
    public int                    AffectedSubjectsCount  { get; init; } = -1;

    /// <summary>Measures taken or proposed to remediate the breach and mitigate its effects (Art. 33(3)(d)).</summary>
    public string?                RemediationMeasures    { get; init; }

    /// <summary>UTC timestamp when the supervisory authority was notified (Art. 33).</summary>
    public DateTimeOffset?        AuthorityNotifiedAt    { get; init; }

    /// <summary>UTC timestamp when affected data subjects were notified (Art. 34). Applies to high-severity breaches.</summary>
    public DateTimeOffset?        SubjectsNotifiedAt     { get; init; }

    /// <summary><c>true</c> when the supervisory authority notification has been recorded.</summary>
    public bool                   IsAuthorityNotified    => AuthorityNotifiedAt is not null;

    /// <summary><c>true</c> when the data subject notification has been recorded.</summary>
    public bool                   IsSubjectsNotified     => SubjectsNotifiedAt is not null;

    /// <summary>
    /// <c>true</c> when a supervisory authority notification is required (severity ≥ medium),
    /// has not yet been sent, and the 72-hour deadline has passed.
    /// </summary>
    public bool                   IsAuthorityOverdue     =>
        Severity >= DataBreachSeverity.Medium
        && !IsAuthorityNotified
        && DateTimeOffset.UtcNow > NotificationDeadlineAt;
}
