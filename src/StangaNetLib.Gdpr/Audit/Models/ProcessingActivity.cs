using StangaNetLib.Gdpr.Consent.Models;

namespace StangaNetLib.Gdpr.Audit.Models;

/// <summary>
/// An entry in the Record of Processing Activities (RoPA) maintained under GDPR Art. 30.
/// Controllers with ≥ 250 employees, or whose processing is likely to result in a risk to
/// data subjects' rights, are legally required to maintain this register and make it available
/// to the supervisory authority on request.
/// </summary>
public sealed record ProcessingActivity
{
    /// <summary>Unique identifier for this RoPA entry.</summary>
    public Guid                    Id                    { get; init; } = Guid.NewGuid();

    /// <summary>Descriptive name of the processing activity (e.g. "Customer CRM", "Newsletter Delivery").</summary>
    public string                  Name                  { get; init; } = string.Empty;

    /// <summary>Specific purpose of the processing (Art. 30(1)(b)).</summary>
    public string                  Purpose               { get; init; } = string.Empty;

    /// <summary>Lawful basis under Art. 6 (and Art. 9 for special-category data).</summary>
    public LawfulBasis             LawfulBasis           { get; init; }

    /// <summary>Categories of data subjects involved (e.g. "Customers", "Employees", "Minors").</summary>
    public IReadOnlyList<string>   DataSubjectCategories { get; init; } = [];

    /// <summary>Categories of personal data processed (e.g. "contact details", "health data", "financial data").</summary>
    public IReadOnlyList<string>   DataCategories        { get; init; } = [];

    /// <summary>Categories of recipients to whom data is or may be disclosed (Art. 30(1)(d)).</summary>
    public IReadOnlyList<string>   Recipients            { get; init; } = [];

    /// <summary>
    /// Third-country transfers and the safeguards applied (Art. 30(1)(e), Art. 46/49).
    /// Null if no transfers outside the EEA take place.
    /// </summary>
    public string?                 ThirdCountryTransfers { get; init; }

    /// <summary>Planned erasure deadlines and retention justification (Art. 30(1)(f)).</summary>
    public string                  RetentionPolicy       { get; init; } = string.Empty;

    /// <summary>General description of technical and organisational security measures (Art. 30(1)(g)).</summary>
    public string?                 SecurityMeasures      { get; init; }

    /// <summary>UTC timestamp when this RoPA entry was first created.</summary>
    public DateTimeOffset          CreatedAt             { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>UTC timestamp of the last update to this entry (e.g. after an annual review).</summary>
    public DateTimeOffset          UpdatedAt             { get; init; } = DateTimeOffset.UtcNow;
}
