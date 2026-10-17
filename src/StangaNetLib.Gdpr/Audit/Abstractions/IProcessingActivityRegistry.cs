using StangaNetLib.Core.Common;
using StangaNetLib.Gdpr.Audit.Models;
using StangaNetLib.Gdpr.Consent.Models;

namespace StangaNetLib.Gdpr.Audit.Abstractions;

/// <summary>
/// Maintains the Record of Processing Activities (RoPA) required by GDPR Art. 30.
/// Controllers with ≥ 250 employees, or whose processing is likely to result in a risk to
/// data subjects' rights, must maintain this register and make it available to the supervisory
/// authority on request.
/// The consuming project provides the persistence implementation.
/// </summary>
public interface IProcessingActivityRegistry
{
    /// <summary>Adds a new processing activity to the RoPA.</summary>
    /// <param name="activity">The activity to register. <see cref="ProcessingActivity.Id"/> is assigned by the implementation.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// A successful <see cref="Result{T}"/> with the persisted <see cref="ProcessingActivity"/> and its assigned identifier;
    /// a failed <see cref="Result{T}"/> if the activity could not be persisted.
    /// </returns>
    Task<Result<ProcessingActivity>> AddAsync(
        ProcessingActivity activity, CancellationToken ct = default);

    /// <summary>Returns the full RoPA, ordered alphabetically by <see cref="ProcessingActivity.Name"/>.</summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>All registered processing activities, sorted by name.</returns>
    Task<IReadOnlyList<ProcessingActivity>> GetAllAsync(
        CancellationToken ct = default);

    /// <summary>Returns processing activities filtered by their lawful basis under Art. 6.</summary>
    /// <param name="basis">The lawful basis to filter by.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Activities whose <see cref="ProcessingActivity.LawfulBasis"/> matches <paramref name="basis"/>, sorted by name.</returns>
    Task<IReadOnlyList<ProcessingActivity>> GetByLawfulBasisAsync(
        LawfulBasis basis, CancellationToken ct = default);

    /// <summary>Updates an existing activity entry (e.g. after an annual review).</summary>
    /// <param name="activity">The updated activity data. Must have a valid <see cref="ProcessingActivity.Id"/>.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// A successful <see cref="Result{T}"/> with the updated <see cref="ProcessingActivity"/>;
    /// a failed <see cref="Result{T}"/> with <see cref="Error.NotFound(string,object)"/> if the activity does not exist.
    /// </returns>
    Task<Result<ProcessingActivity>> UpdateAsync(
        ProcessingActivity activity, CancellationToken ct = default);

    /// <summary>Removes a processing activity from the RoPA when the activity has been discontinued.</summary>
    /// <param name="activityId">ID of the <see cref="ProcessingActivity"/> to remove.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// A successful <see cref="Result"/> when the activity was found and removed;
    /// a failed <see cref="Result"/> with <see cref="Error.NotFound(string,object)"/> if not found.
    /// </returns>
    Task<Result> RemoveAsync(Guid activityId, CancellationToken ct = default);
}
