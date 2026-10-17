using StangaNetLib.Core.Common;
using StangaNetLib.Gdpr.Objection.Models;

namespace StangaNetLib.Gdpr.Objection.Abstractions;

/// <summary>
/// Manages restrictions on processing a user's personal data (GDPR Art. 18).
/// While a restriction is active, the controller may only store the data; all other processing
/// requires the data subject's explicit consent or specific legal grounds (Art. 18(2)).
/// The data subject must be informed before any restriction is lifted (Art. 18(3)).
/// The consuming project provides the persistence implementation.
/// </summary>
public interface IProcessingRestrictionService
{
    /// <summary>
    /// Returns all active (not yet lifted) processing restrictions for the user.
    /// Returns an empty collection when processing is unrestricted.
    /// </summary>
    /// <param name="userId">The subject whose active restrictions are queried.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Active restrictions ordered by <see cref="ProcessingRestriction.CreatedAt"/> descending.</returns>
    Task<IReadOnlyList<ProcessingRestriction>> GetActiveAsync(
        string userId, CancellationToken ct = default);

    /// <summary>
    /// Returns <c>true</c> if the user has at least one active restriction covering the given scope.
    /// Use <c>"*"</c> to check for any restriction regardless of scope.
    /// </summary>
    /// <param name="userId">The subject to check.</param>
    /// <param name="scope">The processing scope to check. Use <c>"*"</c> to match any scope.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns><c>true</c> if an active restriction exists for this scope; otherwise <c>false</c>.</returns>
    Task<bool> IsRestrictedAsync(
        string userId, string scope = "*", CancellationToken ct = default);

    /// <summary>Imposes a processing restriction on the user's data for the specified scope and reason.</summary>
    /// <param name="userId">The subject for whom processing is restricted.</param>
    /// <param name="reason">The Art. 18(1) ground for the restriction.</param>
    /// <param name="scope">The processing scope covered by the restriction. Use <c>"*"</c> to restrict all processing.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// A successful <see cref="Result{T}"/> with the newly created <see cref="ProcessingRestriction"/>;
    /// a failed <see cref="Result{T}"/> if the restriction could not be persisted.
    /// </returns>
    Task<Result<ProcessingRestriction>> RestrictAsync(
        string            userId,
        string            reason,
        string            scope = "*",
        CancellationToken ct    = default);

    /// <summary>
    /// Lifts an existing restriction, allowing processing to resume.
    /// The consuming project is responsible for notifying the data subject before calling this method (Art. 18(3)).
    /// </summary>
    /// <param name="restrictionId">ID of the <see cref="ProcessingRestriction"/> to lift.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// A successful <see cref="Result{T}"/> with the updated <see cref="ProcessingRestriction"/> and <see cref="ProcessingRestriction.LiftedAt"/> set;
    /// a failed <see cref="Result{T}"/> with <see cref="Error.NotFound(string,object)"/> if the restriction does not exist.
    /// </returns>
    Task<Result<ProcessingRestriction>> LiftAsync(
        Guid              restrictionId,
        CancellationToken ct = default);
}
