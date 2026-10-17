namespace StangaNetLib.Gdpr.Dsar.Models;

/// <summary>Lifecycle status of a Data Subject Access Request.</summary>
public enum DsarStatus
{
    /// <summary>Request received and logged; identity verification may be pending.</summary>
    Pending = 1,

    /// <summary>Identity verified; the request is being actively processed.</summary>
    InProgress = 2,

    /// <summary>Request fulfilled within the statutory deadline.</summary>
    Completed = 3,

    /// <summary>
    /// Request formally rejected, e.g. manifestly unfounded or excessive (Art. 12(5)).
    /// A written reason must be provided to the data subject.
    /// </summary>
    Rejected = 4,

    /// <summary>
    /// Deadline extended by up to two additional months due to complexity or volume (Art. 12(3)).
    /// The data subject must be informed within one month of receipt.
    /// </summary>
    Extended = 5,
}
