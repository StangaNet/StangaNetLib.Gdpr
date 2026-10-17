namespace StangaNetLib.Gdpr.Consent.Models;

/// <summary>
/// The six lawful bases for processing personal data under GDPR Art. 6.
/// At least one must apply for every processing activity; the basis must be documented
/// in the Record of Processing Activities (Art. 30).
/// </summary>
public enum LawfulBasis
{
    /// <summary>Art. 6(1)(a) — The data subject has given consent to the processing.</summary>
    Consent = 1,

    /// <summary>Art. 6(1)(b) — Processing is necessary for the performance of a contract with the data subject.</summary>
    Contract = 2,

    /// <summary>Art. 6(1)(c) — Processing is necessary to comply with a legal obligation.</summary>
    LegalObligation = 3,

    /// <summary>Art. 6(1)(d) — Processing is necessary to protect the vital interests of the data subject or another person.</summary>
    VitalInterests = 4,

    /// <summary>Art. 6(1)(e) — Processing is necessary for the performance of a task carried out in the public interest.</summary>
    PublicTask = 5,

    /// <summary>
    /// Art. 6(1)(f) — Processing is necessary for the legitimate interests of the controller or a third party,
    /// except where overridden by the fundamental rights of the data subject.
    /// </summary>
    LegitimateInterests = 6,
}
