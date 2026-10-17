namespace StangaNetLib.Gdpr.Audit.Models;

/// <summary>
/// Risk-based severity classification for personal data breaches under GDPR Art. 33/34.
/// Determines which notification obligations apply.
/// </summary>
public enum DataBreachSeverity
{
    /// <summary>
    /// Low risk — unlikely to result in a risk to the rights and freedoms of natural persons.
    /// No notification to the supervisory authority is required (Art. 33(1) exception).
    /// Must still be documented internally (Art. 33(5)).
    /// </summary>
    Low = 1,

    /// <summary>
    /// Medium risk — likely to result in a risk to the rights and freedoms of natural persons.
    /// Notification to the supervisory authority required within 72 hours of discovery (Art. 33).
    /// </summary>
    Medium = 2,

    /// <summary>
    /// High risk — likely to result in a <em>high</em> risk to the rights and freedoms of natural persons.
    /// Notification to the supervisory authority (Art. 33) AND directly to affected data subjects (Art. 34)
    /// are both required without undue delay.
    /// </summary>
    High = 3,
}
