namespace StangaNetLib.Gdpr.Dsar.Models;

/// <summary>
/// Result of a user data export (GDPR Art. 15 — Right of Access / Art. 20 — Right to Data Portability).
/// Art. 20 requires that data provided for portability is in a structured, commonly used,
/// machine-readable format that allows the data subject to transmit it to another controller.
/// </summary>
public sealed record DataExportResult
{
    /// <summary>Identifier of the data subject whose data was exported.</summary>
    public string          UserId      { get; init; } = string.Empty;

    /// <summary>UTC timestamp when the export was produced.</summary>
    public DateTimeOffset  ExportedAt  { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// UTF-8 JSON string containing all personal data organized by category.
    /// The consuming project's <see cref="Abstractions.IDataExportService"/> implementation
    /// is responsible for populating this field with data from every table that holds
    /// user-attributable records.
    /// </summary>
    public string          JsonContent { get; init; } = "{}";

    /// <summary>
    /// MIME type of <see cref="JsonContent"/>. Default: <c>application/json</c>.
    /// Set to <c>text/csv</c> or another structured format when the consumer produces
    /// a different machine-readable output for Art. 20 portability requests.
    /// </summary>
    public string          ContentType { get; init; } = "application/json";

    /// <summary>
    /// Indicates whether this export fulfils the Art. 20 data-portability right
    /// (i.e. data provided by the subject, processed by automated means under consent or contract).
    /// When <c>true</c>, the response should be delivered in a portable, interoperable format.
    /// </summary>
    public bool            IsPortabilityRequest { get; init; }
}
