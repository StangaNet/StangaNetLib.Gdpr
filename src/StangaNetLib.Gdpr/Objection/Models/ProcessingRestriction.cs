namespace StangaNetLib.Gdpr.Objection.Models;

/// <summary>
/// Records an active restriction on processing a user's personal data (GDPR Art. 18).
/// While the restriction is active the controller may only store the data;
/// all other processing requires the data subject's explicit consent or specific legal grounds (Art. 18(2)).
/// The data subject must be informed before the restriction is lifted (Art. 18(3)).
/// </summary>
public sealed record ProcessingRestriction
{
    /// <summary>Unique identifier for this restriction record.</summary>
    public Guid            Id        { get; init; } = Guid.NewGuid();

    /// <summary>Identifier of the data subject to whom this restriction applies.</summary>
    public string          UserId    { get; init; } = string.Empty;

    /// <summary>
    /// Art. 18(1) ground that triggered the restriction (e.g. "Accuracy contested",
    /// "Unlawful processing — erasure opposed", "Legal claims pending").
    /// </summary>
    public string          Reason    { get; init; } = string.Empty;

    /// <summary>
    /// Processing scope covered by this restriction (e.g. "MarketingEmails", "Profiling").
    /// Use <c>"*"</c> to restrict all processing.
    /// </summary>
    public string          Scope     { get; init; } = "*";

    /// <summary>UTC timestamp when the restriction was imposed.</summary>
    public DateTimeOffset  CreatedAt { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>Set when the restriction is lifted following Art. 18(2) grounds.</summary>
    public DateTimeOffset? LiftedAt  { get; init; }

    /// <summary><c>true</c> while the restriction has not yet been lifted.</summary>
    public bool            IsActive  => LiftedAt is null;
}
