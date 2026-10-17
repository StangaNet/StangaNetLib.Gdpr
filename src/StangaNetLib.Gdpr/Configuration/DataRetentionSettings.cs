namespace StangaNetLib.Gdpr.Configuration;

/// <summary>
/// Configures the data retention periods used by scheduled cleanup jobs.
/// Bind from <c>appsettings.json</c> under the <c>"DataRetentionSettings"</c> section.
/// </summary>
public sealed class DataRetentionSettings
{
    /// <summary>The <c>appsettings.json</c> section name: <c>"DataRetentionSettings"</c>.</summary>
    public const string SectionName = "DataRetentionSettings";

    /// <summary>Days after which data of inactive users can be purged. Default: <c>730</c> (2 years).</summary>
    public int InactiveUserDays { get; set; } = 730;

    /// <summary>Days after which data of soft-deleted users must be erased. Default: <c>30</c>.</summary>
    public int DeletedUserDays { get; set; } = 30;

    /// <summary>Days after which messages and user-generated content can be purged. Default: <c>365</c> (1 year).</summary>
    public int MessageDays { get; set; } = 365;

    /// <summary>Days after which audit log entries can be purged. Default: <c>1825</c> (5 years).</summary>
    public int AuditLogDays { get; set; } = 1825;
}
