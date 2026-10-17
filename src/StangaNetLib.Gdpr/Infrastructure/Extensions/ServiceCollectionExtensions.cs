using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StangaNetLib.Gdpr.Consent.Abstractions;
using StangaNetLib.Gdpr.Dsar.Abstractions;
using StangaNetLib.Gdpr.Objection.Abstractions;
using StangaNetLib.Gdpr.Audit.Abstractions;
using StangaNetLib.Gdpr.Pseudonymization.Abstractions;
using StangaNetLib.Gdpr.Pseudonymization;
using StangaNetLib.Gdpr.Configuration;

namespace StangaNetLib.Gdpr.Infrastructure.Extensions;

/// <summary>
/// Extension methods for <see cref="IServiceCollection"/> that register StangaNetLib.Gdpr services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers all StangaNetLib.Gdpr services: <see cref="IPseudonymizationService"/> (singleton),
    /// <c>GdprSettings</c>, and <c>DataRetentionSettings</c>.
    /// </summary>
    /// <remarks>
    /// <para>After calling this method, the consuming project must also register:</para>
    /// <list type="bullet">
    ///   <item><see cref="StangaNetLib.Gdpr.Consent.Abstractions.IConsentRepository"/> — consent persistence (required by <c>UseStangaNetLibGdpr()</c>)</item>
    ///   <item><see cref="StangaNetLib.Core.Auditing.IAuditService"/> — audit trail persistence</item>
    ///   <item><see cref="StangaNetLib.Gdpr.Dsar.Abstractions.IDataExportService"/> — application-specific data export / Art. 15 logic</item>
    ///   <item><see cref="StangaNetLib.Gdpr.Dsar.Abstractions.IDataDeletionService"/> — application-specific erasure / Art. 17 logic</item>
    ///   <item><see cref="StangaNetLib.Gdpr.Dsar.Abstractions.IDsarService"/> — DSAR lifecycle tracking / Art. 12 (optional)</item>
    ///   <item><see cref="StangaNetLib.Gdpr.Objection.Abstractions.IProcessingRestrictionService"/> — restriction management / Art. 18 (required by <c>UseStangaNetLibGdprRestrictions()</c>)</item>
    ///   <item><see cref="StangaNetLib.Gdpr.Objection.Abstractions.IDataRectificationService"/> — rectification tracking / Art. 16 (optional)</item>
    ///   <item><see cref="StangaNetLib.Gdpr.Objection.Abstractions.IDataProcessingObjectionService"/> — objection management / Art. 21 (optional)</item>
    ///   <item><see cref="StangaNetLib.Gdpr.Audit.Abstractions.IDataBreachService"/> — breach notification tracking / Art. 33-34 (optional)</item>
    ///   <item><see cref="StangaNetLib.Gdpr.Audit.Abstractions.IProcessingActivityRegistry"/> — RoPA management / Art. 30 (optional)</item>
    /// </list>
    /// </remarks>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="configuration">Application configuration used to bind <c>GdprSettings</c> and <c>DataRetentionSettings</c>.</param>
    /// <returns>The same <paramref name="services"/> instance for chaining.</returns>
    public static IServiceCollection AddStangaNetLibGdpr(
        this IServiceCollection services,
        IConfiguration          configuration)
    {
        services.Configure<GdprSettings>(
            configuration.GetSection(GdprSettings.SectionName));

        services.Configure<DataRetentionSettings>(
            configuration.GetSection(DataRetentionSettings.SectionName));

        services.AddSingleton<IPseudonymizationService, PseudonymizationService>();

        return services;
    }
}
