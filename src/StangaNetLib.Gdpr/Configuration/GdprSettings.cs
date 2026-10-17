namespace StangaNetLib.Gdpr.Configuration;

/// <summary>
/// Configuration options for StangaNetLib.Gdpr.
/// Bind from <c>appsettings.json</c> under the <c>"GdprSettings"</c> section.
/// </summary>
public sealed class GdprSettings
{
    /// <summary>The <c>appsettings.json</c> section name: <c>"GdprSettings"</c>.</summary>
    public const string SectionName = "GdprSettings";

    /// <summary>Legal name of the data controller (titolare del trattamento). Required for privacy notices.</summary>
    public string DataControllerName { get; set; } = string.Empty;

    /// <summary>Email address of the Data Protection Officer (DPO). Required under Art. 37 when a DPO is designated.</summary>
    public string DpoEmail { get; set; } = string.Empty;

    /// <summary>
    /// Name of the supervisory authority relevant to the controller's jurisdiction.
    /// Default: <c>"Garante per la protezione dei dati personali"</c> (Italian authority).
    /// </summary>
    public string SupervisoryAuthorityName { get; set; } = "Garante per la protezione dei dati personali";

    /// <summary>
    /// Website of the supervisory authority. Default: <c>"https://www.garanteprivacy.it"</c>.
    /// </summary>
    public string SupervisoryAuthorityUrl { get; set; } = "https://www.garanteprivacy.it";

    /// <summary>
    /// Base64-encoded 256-bit (32-byte) AES key used by <see cref="StangaNetLib.Gdpr.Pseudonymization.PseudonymizationService"/>.
    /// Required. Generate with: <c>Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))</c>
    /// </summary>
    public string PseudonymizationKey { get; set; } = string.Empty;
}
