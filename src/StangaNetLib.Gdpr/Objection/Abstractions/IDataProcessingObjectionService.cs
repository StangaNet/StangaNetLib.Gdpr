using StangaNetLib.Core.Common;
using StangaNetLib.Gdpr.Objection.Models;

namespace StangaNetLib.Gdpr.Objection.Abstractions;

/// <summary>
/// Manages objections to the processing of personal data (GDPR Art. 21).
/// <para>
/// Objections to <b>direct marketing</b> (Art. 21(2)) are absolute: the controller must cease
/// processing immediately and may not override them.
/// Objections to other processing (Art. 21(1)) may be overridden only if the controller can
/// demonstrate compelling legitimate grounds that override the data subject's interests.
/// </para>
/// The consuming project provides the persistence implementation.
/// </summary>
public interface IDataProcessingObjectionService
{
    /// <summary>Records the data subject's objection to the specified processing purpose.</summary>
    /// <param name="userId">The subject raising the objection.</param>
    /// <param name="processingPurpose">The processing purpose being objected to (e.g. "DirectMarketing", "Profiling").</param>
    /// <param name="grounds">Grounds for the objection as stated by the data subject.</param>
    /// <param name="isDirectMarketing">
    /// Set <c>true</c> when the objection concerns direct marketing (Art. 21(2) — absolute right).
    /// </param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// A successful <see cref="Result{T}"/> with the newly created <see cref="DataProcessingObjection"/>;
    /// a failed <see cref="Result{T}"/> if the record could not be persisted.
    /// </returns>
    Task<Result<DataProcessingObjection>> ObjectAsync(
        string            userId,
        string            processingPurpose,
        string?           grounds           = null,
        bool              isDirectMarketing = false,
        CancellationToken ct                = default);

    /// <summary>Returns all active (not yet resolved) objections for the user.</summary>
    /// <param name="userId">The subject whose objections are queried.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Active objections ordered by <see cref="DataProcessingObjection.ObjectedAt"/> descending.</returns>
    Task<IReadOnlyList<DataProcessingObjection>> GetActiveAsync(
        string userId, CancellationToken ct = default);

    /// <summary>
    /// Returns <c>true</c> if the user has an active unresolved objection covering the given processing purpose.
    /// </summary>
    /// <param name="userId">The subject to check.</param>
    /// <param name="processingPurpose">The processing purpose to check.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns><c>true</c> if an active objection exists for this purpose; otherwise <c>false</c>.</returns>
    Task<bool> HasActiveObjectionAsync(
        string userId, string processingPurpose, CancellationToken ct = default);

    /// <summary>Upholds the objection — processing for the stated purpose must cease immediately.</summary>
    /// <param name="objectionId">ID of the <see cref="DataProcessingObjection"/> to uphold.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// A successful <see cref="Result{T}"/> with the updated record and <see cref="DataProcessingObjection.UpheldAt"/> set;
    /// a failed <see cref="Result{T}"/> with <see cref="Error.NotFound(string,object)"/> if the objection does not exist.
    /// </returns>
    Task<Result<DataProcessingObjection>> UpholdAsync(
        Guid objectionId, CancellationToken ct = default);

    /// <summary>
    /// Overrides a non-marketing objection based on compelling legitimate grounds (Art. 21(1)).
    /// Overriding a direct-marketing objection (Art. 21(2)) is an absolute right; the implementation
    /// must return <see cref="Error.Forbidden(string)"/> in that case.
    /// </summary>
    /// <param name="objectionId">ID of the <see cref="DataProcessingObjection"/> to override.</param>
    /// <param name="overrideReason">The compelling legitimate grounds that justify continuing processing.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// A successful <see cref="Result{T}"/> with the updated record and <see cref="DataProcessingObjection.OverriddenAt"/> set;
    /// a failed <see cref="Result{T}"/> with <see cref="Error.NotFound(string,object)"/> if the objection does not exist;
    /// a failed <see cref="Result{T}"/> with <see cref="Error.Forbidden(string)"/> if <see cref="DataProcessingObjection.IsDirectMarketing"/> is <c>true</c>.
    /// </returns>
    Task<Result<DataProcessingObjection>> OverrideAsync(
        Guid              objectionId,
        string            overrideReason,
        CancellationToken ct = default);
}
