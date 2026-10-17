namespace StangaNetLib.Gdpr.Objection.Models;

/// <summary>
/// Records a data subject's objection to processing of their personal data (GDPR Art. 21).
/// <para>
/// Objections to <b>direct marketing</b> (Art. 21(2)) are absolute and must always be upheld.
/// Objections to other processing may be overridden only if the controller demonstrates
/// compelling legitimate grounds that override the data subject's interests (Art. 21(1)).
/// </para>
/// </summary>
public sealed record DataProcessingObjection
{
    /// <summary>Unique identifier for this objection record.</summary>
    public Guid            Id                { get; init; } = Guid.NewGuid();

    /// <summary>Identifier of the data subject raising the objection.</summary>
    public string          UserId            { get; init; } = string.Empty;

    /// <summary>Processing purpose being objected to (e.g. "DirectMarketing", "Profiling", "LegitimateInterest").</summary>
    public string          ProcessingPurpose { get; init; } = string.Empty;

    /// <summary>Grounds for the objection as stated by the data subject.</summary>
    public string?         Grounds           { get; init; }

    /// <summary>
    /// Whether the objection concerns direct marketing.
    /// When true, the right is absolute (Art. 21(2)) — <see cref="OverriddenAt"/> must never be set.
    /// </summary>
    public bool            IsDirectMarketing { get; init; }

    /// <summary>UTC timestamp when the objection was submitted.</summary>
    public DateTimeOffset  ObjectedAt        { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>Set if the objection was upheld and processing ceased for this purpose.</summary>
    public DateTimeOffset? UpheldAt          { get; init; }

    /// <summary>
    /// Set if a non-marketing objection was overridden by compelling legitimate grounds (Art. 21(1)).
    /// Must never be set when <see cref="IsDirectMarketing"/> is true.
    /// </summary>
    public DateTimeOffset? OverriddenAt      { get; init; }

    /// <summary>Reason for overriding the objection (required when <see cref="OverriddenAt"/> is set).</summary>
    public string?         OverrideReason    { get; init; }

    /// <summary><c>true</c> while the objection is pending a controller decision.</summary>
    public bool            IsActive          => UpheldAt is null && OverriddenAt is null;
}
