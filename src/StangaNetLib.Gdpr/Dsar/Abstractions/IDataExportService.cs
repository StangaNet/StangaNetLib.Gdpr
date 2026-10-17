using StangaNetLib.Core.Common;
using StangaNetLib.Gdpr.Dsar.Models;

namespace StangaNetLib.Gdpr.Dsar.Abstractions;

/// <summary>
/// Exports all personal data held for a user (GDPR Art. 15 — Right of Access / Art. 20 — Right to Data Portability).
/// The consuming project implements this interface by querying its own data model.
/// </summary>
public interface IDataExportService
{
    /// <summary>
    /// Collects and serializes all personal data for the specified user.
    /// The returned <see cref="DataExportResult.JsonContent"/> must include data from every
    /// repository or table that holds user-attributable records.
    /// </summary>
    /// <param name="userId">The subject whose data is exported.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// A successful <see cref="Result{T}"/> with the <see cref="DataExportResult"/> containing the serialized personal data and export metadata;
    /// a failed <see cref="Result{T}"/> if the export could not be completed.
    /// </returns>
    Task<Result<DataExportResult>> ExportAsync(string userId, CancellationToken ct = default);
}
