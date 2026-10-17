using StangaNetLib.Core.Common;
using StangaNetLib.Gdpr.Objection.Models;

namespace StangaNetLib.Gdpr.Objection.Abstractions;

/// <summary>
/// Tracks and fulfils requests to correct inaccurate or complete incomplete personal data (GDPR Art. 16).
/// The controller must act without undue delay; the one-month deadline under Art. 12(3) applies.
/// The consuming project provides the persistence implementation.
/// </summary>
public interface IDataRectificationService
{
    /// <summary>Creates a new rectification request for the specified data field.</summary>
    /// <param name="userId">The subject requesting the correction.</param>
    /// <param name="field">The field or data category to be corrected (e.g. "Name", "DateOfBirth").</param>
    /// <param name="requestedChange">The correction requested by the data subject.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// A successful <see cref="Result{T}"/> with the newly created <see cref="RectificationRequest"/>;
    /// a failed <see cref="Result{T}"/> if the record could not be persisted.
    /// </returns>
    Task<Result<RectificationRequest>> RequestAsync(
        string            userId,
        string            field,
        string            requestedChange,
        CancellationToken ct = default);

    /// <summary>Returns all open (not yet completed) rectification requests for the user.</summary>
    /// <param name="userId">The subject whose pending requests are queried.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Open requests ordered by <see cref="RectificationRequest.RequestedAt"/> ascending.</returns>
    Task<IReadOnlyList<RectificationRequest>> GetPendingAsync(
        string userId, CancellationToken ct = default);

    /// <summary>Marks a rectification request as completed once the data has been corrected.</summary>
    /// <param name="requestId">ID of the <see cref="RectificationRequest"/> to complete.</param>
    /// <param name="internalNotes">Internal notes on how and what was corrected (not disclosed to the data subject).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// A successful <see cref="Result{T}"/> with the updated <see cref="RectificationRequest"/> and <see cref="RectificationRequest.CompletedAt"/> set;
    /// a failed <see cref="Result{T}"/> with <see cref="Error.NotFound(string,object)"/> if the request does not exist.
    /// </returns>
    Task<Result<RectificationRequest>> CompleteAsync(
        Guid              requestId,
        string?           internalNotes  = null,
        CancellationToken ct             = default);
}
