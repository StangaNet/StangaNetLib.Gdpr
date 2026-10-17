using StangaNetLib.Core.Common;
using StangaNetLib.Gdpr.Dsar.Models;

namespace StangaNetLib.Gdpr.Dsar.Abstractions;

/// <summary>
/// Manages the full lifecycle of Data Subject Access Requests (DSARs).
/// Enforces the one-month response deadline mandated by GDPR Art. 12(3), extendable
/// by two additional months for complex or numerous requests.
/// The consuming project provides the persistence implementation.
/// </summary>
public interface IDsarService
{
    /// <summary>
    /// Creates and persists a new DSAR, setting <see cref="DsarRecord.DeadlineAt"/>
    /// to one calendar month from the current UTC time (Art. 12(3)).
    /// </summary>
    /// <param name="userId">The subject submitting the request.</param>
    /// <param name="type">The category of right being exercised.</param>
    /// <param name="requestDetails">Free-text description of the data subject's request.</param>
    /// <param name="ipAddress">IP address at time of submission (identity verification and audit trail).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// A successful <see cref="Result{T}"/> with the newly created <see cref="DsarRecord"/> in status <see cref="DsarStatus.Pending"/>;
    /// a failed <see cref="Result{T}"/> if the record could not be persisted.
    /// </returns>
    Task<Result<DsarRecord>> CreateAsync(
        string            userId,
        DsarType          type,
        string?           requestDetails = null,
        string?           ipAddress      = null,
        CancellationToken ct             = default);

    /// <summary>Returns all DSARs for the specified user, ordered by <see cref="DsarRecord.ReceivedAt"/> descending.</summary>
    /// <param name="userId">The subject whose requests are queried.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>All DSARs for the user, newest first.</returns>
    Task<IReadOnlyList<DsarRecord>> GetByUserAsync(
        string userId, CancellationToken ct = default);

    /// <summary>Returns all DSARs currently in <see cref="DsarStatus.Pending"/> or <see cref="DsarStatus.InProgress"/> state.</summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Open DSARs ordered by <see cref="DsarRecord.DeadlineAt"/> ascending (most urgent first).</returns>
    Task<IReadOnlyList<DsarRecord>> GetOpenAsync(CancellationToken ct = default);

    /// <summary>Returns all open DSARs whose <see cref="DsarRecord.DeadlineAt"/> has passed without resolution.</summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Overdue DSARs ordered by deadline ascending.</returns>
    Task<IReadOnlyList<DsarRecord>> GetOverdueAsync(CancellationToken ct = default);

    /// <summary>
    /// Marks a DSAR as resolved by setting <see cref="DsarRecord.Status"/> to
    /// <see cref="DsarStatus.Completed"/> or <see cref="DsarStatus.Rejected"/>.
    /// </summary>
    /// <param name="dsarId">ID of the DSAR to resolve.</param>
    /// <param name="status">Target status — <see cref="DsarStatus.Completed"/> or <see cref="DsarStatus.Rejected"/>.</param>
    /// <param name="internalNotes">Internal processing notes (not disclosed to the data subject).</param>
    /// <param name="reason">Required when <paramref name="status"/> is <see cref="DsarStatus.Rejected"/>; communicated to the data subject.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// A successful <see cref="Result{T}"/> with the updated <see cref="DsarRecord"/> and <see cref="DsarRecord.ResolvedAt"/> set;
    /// a failed <see cref="Result{T}"/> with <see cref="Error.NotFound(string,object)"/> if the DSAR does not exist.
    /// </returns>
    Task<Result<DsarRecord>> ResolveAsync(
        Guid              dsarId,
        DsarStatus        status,
        string?           internalNotes  = null,
        string?           reason         = null,
        CancellationToken ct             = default);

    /// <summary>
    /// Extends the deadline by up to two additional months (Art. 12(3)), setting the status to
    /// <see cref="DsarStatus.Extended"/>. The data subject must be notified within one month of receipt.
    /// </summary>
    /// <param name="dsarId">ID of the DSAR whose deadline is extended.</param>
    /// <param name="reason">The reason for the extension; communicated to the data subject.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// A successful <see cref="Result{T}"/> with the updated <see cref="DsarRecord"/> and the extended <see cref="DsarRecord.DeadlineAt"/>;
    /// a failed <see cref="Result{T}"/> with <see cref="Error.NotFound(string,object)"/> if the DSAR does not exist.
    /// </returns>
    Task<Result<DsarRecord>> ExtendDeadlineAsync(
        Guid              dsarId,
        string            reason,
        CancellationToken ct = default);
}
