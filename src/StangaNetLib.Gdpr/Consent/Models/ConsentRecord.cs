namespace StangaNetLib.Gdpr.Consent.Models;

/// <summary>
/// Immutable record of a user's consent decision.
/// Each grant or revocation creates a new record; the full history is preserved.
/// </summary>
public sealed record ConsentRecord
{
    /// <summary>Unique identifier for this consent record.</summary>
    public Guid            Id            { get; init; } = Guid.NewGuid();

    /// <summary>Identifier of the data subject to whom this record belongs.</summary>
    public string          UserId        { get; init; } = string.Empty;

    /// <summary>Consent category (e.g. "Marketing", "Analytics"). Defined by the consuming application.</summary>
    public string          ConsentType   { get; init; } = string.Empty;

    /// <summary><c>true</c> if consent was granted; <c>false</c> if it was revoked.</summary>
    public bool            IsGranted     { get; init; }

    /// <summary>UTC timestamp when this consent decision was recorded.</summary>
    public DateTimeOffset  GrantedAt     { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>Set when this record is superseded by a later grant or revocation.</summary>
    public DateTimeOffset? RevokedAt     { get; init; }

    /// <summary>IP address at time of consent decision (for evidentiary purposes).</summary>
    public string?         IpAddress     { get; init; }

    /// <summary>User-Agent string at time of consent decision.</summary>
    public string?         UserAgent     { get; init; }

    /// <summary>Version of the consent form / privacy notice shown to the user. Default: <c>"1.0"</c>.</summary>
    public string          PolicyVersion { get; init; } = "1.0";

    /// <summary>
    /// Lawful basis under Art. 6 on which this processing relies.
    /// For most consent records this will be <see cref="LawfulBasis.Consent"/>;
    /// other bases apply when consent is collected alongside a different legal ground
    /// (e.g. <see cref="LawfulBasis.Contract"/> for essential service communications).
    /// </summary>
    public LawfulBasis     LawfulBasis   { get; init; } = LawfulBasis.Consent;
}
