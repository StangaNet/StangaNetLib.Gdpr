namespace StangaNetLib.Gdpr.Objection.Models;

/// <summary>
/// Tracks a request to correct inaccurate or complete incomplete personal data (GDPR Art. 16).
/// The controller must act without undue delay. The one-month response deadline under Art. 12(3) applies.
/// </summary>
public sealed record RectificationRequest
{
    /// <summary>Unique identifier for this rectification request.</summary>
    public Guid            Id              { get; init; } = Guid.NewGuid();

    /// <summary>Identifier of the data subject requesting the correction.</summary>
    public string          UserId          { get; init; } = string.Empty;

    /// <summary>The field or data category to be corrected (e.g. "Name", "DateOfBirth", "Address").</summary>
    public string          Field           { get; init; } = string.Empty;

    /// <summary>Correction requested by the data subject.</summary>
    public string          RequestedChange { get; init; } = string.Empty;

    /// <summary>UTC timestamp when the correction request was submitted.</summary>
    public DateTimeOffset  RequestedAt     { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>UTC timestamp when the data was corrected and the request fulfilled.</summary>
    public DateTimeOffset? CompletedAt     { get; init; }

    /// <summary><c>true</c> once the data has been corrected and <see cref="CompletedAt"/> is set.</summary>
    public bool            IsCompleted     => CompletedAt is not null;

    /// <summary>Internal notes on how and what was corrected (not disclosed to the data subject).</summary>
    public string?         InternalNotes   { get; init; }
}
