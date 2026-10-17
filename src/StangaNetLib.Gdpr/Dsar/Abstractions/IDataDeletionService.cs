using StangaNetLib.Core.Common;

namespace StangaNetLib.Gdpr.Dsar.Abstractions;

/// <summary>
/// Handles the GDPR right to erasure (Art. 17).
/// The consuming project implements this interface based on its own data model.
/// </summary>
public interface IDataDeletionService
{
    /// <summary>
    /// Permanently and irrecoverably deletes all personal data for the user.
    /// Use when the user explicitly invokes the right to erasure and there is no legal basis
    /// to retain the data (no ongoing contract, no legal obligation).
    /// </summary>
    /// <param name="userId">The subject whose data is to be permanently deleted.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// A successful <see cref="Result"/> when all data was deleted;
    /// a failed <see cref="Result"/> if the operation could not be completed.
    /// </returns>
    Task<Result> DeleteUserDataAsync(string userId, CancellationToken ct = default);

    /// <summary>
    /// Replaces all identifiable fields with pseudonymized (encrypted) or anonymized (hashed)
    /// equivalents while retaining aggregate statistics and audit trails.
    /// Use when data must be kept for statistical or legal reasons but identity must be removed.
    /// </summary>
    /// <param name="userId">The subject whose identifiable fields are to be pseudonymized.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// A successful <see cref="Result"/> when pseudonymization completed;
    /// a failed <see cref="Result"/> if the operation could not be completed.
    /// </returns>
    Task<Result> PseudonymizeUserDataAsync(string userId, CancellationToken ct = default);
}
