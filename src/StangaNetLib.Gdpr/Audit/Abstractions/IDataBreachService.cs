using StangaNetLib.Core.Common;
using StangaNetLib.Gdpr.Audit.Models;

namespace StangaNetLib.Gdpr.Audit.Abstractions;

/// <summary>
/// Manages the personal data breach notification lifecycle (GDPR Art. 33/34).
/// <para>
/// Art. 33: medium/high-severity breaches must be reported to the supervisory authority within 72 hours of discovery.
/// Art. 34: high-severity breaches must also be communicated directly to affected data subjects without undue delay.
/// Art. 33(5): all breaches must be documented internally regardless of severity.
/// </para>
/// The consuming project provides the persistence implementation.
/// </summary>
public interface IDataBreachService
{
    /// <summary>
    /// Records a newly detected breach and sets <see cref="DataBreachRecord.NotificationDeadlineAt"/>
    /// to 72 hours from <paramref name="detectedAt"/> (defaults to UTC now if not specified).
    /// </summary>
    /// <param name="severity">Risk classification that determines which notification obligations apply.</param>
    /// <param name="description">Nature of the breach, data categories affected, and approximate scope (Art. 33(3)(a)).</param>
    /// <param name="dataCategories">Categories of personal data involved (e.g. "email", "health data").</param>
    /// <param name="affectedSubjectsCount">Approximate number of data subjects affected. Use <c>-1</c> when unknown.</param>
    /// <param name="remediationMeasures">Measures taken or proposed to address the breach (Art. 33(3)(d)).</param>
    /// <param name="detectedAt">UTC time of discovery. Defaults to <see cref="DateTimeOffset.UtcNow"/> when <c>null</c>.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// A successful <see cref="Result{T}"/> with the newly created <see cref="DataBreachRecord"/> and the 72-hour deadline set;
    /// a failed <see cref="Result{T}"/> if the record could not be persisted.
    /// </returns>
    Task<Result<DataBreachRecord>> ReportAsync(
        DataBreachSeverity      severity,
        string                  description,
        IReadOnlyList<string>   dataCategories,
        int                     affectedSubjectsCount = -1,
        string?                 remediationMeasures   = null,
        DateTimeOffset?         detectedAt            = null,
        CancellationToken       ct                    = default);

    /// <summary>
    /// Returns all medium/high-severity breaches that have not yet been reported to the supervisory authority (Art. 33).
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Breaches pending authority notification, ordered by detection time ascending.</returns>
    Task<IReadOnlyList<DataBreachRecord>> GetPendingAuthorityNotificationAsync(
        CancellationToken ct = default);

    /// <summary>
    /// Returns all high-severity breaches where affected data subjects have not yet been notified (Art. 34).
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>High-severity breaches pending subject notification, ordered by detection time ascending.</returns>
    Task<IReadOnlyList<DataBreachRecord>> GetPendingSubjectNotificationAsync(
        CancellationToken ct = default);

    /// <summary>
    /// Returns all breaches whose 72-hour supervisory authority notification deadline has passed
    /// without a notification being recorded.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Overdue breaches ordered by deadline ascending.</returns>
    Task<IReadOnlyList<DataBreachRecord>> GetOverdueNotificationsAsync(
        CancellationToken ct = default);

    /// <summary>Records that the supervisory authority has been notified (Art. 33).</summary>
    /// <param name="breachId">ID of the <see cref="DataBreachRecord"/> to update.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// A successful <see cref="Result{T}"/> with the updated record and <see cref="DataBreachRecord.AuthorityNotifiedAt"/> set;
    /// a failed <see cref="Result{T}"/> with <see cref="Error.NotFound(string,object)"/> if the breach record does not exist.
    /// </returns>
    Task<Result<DataBreachRecord>> MarkAuthorityNotifiedAsync(
        Guid breachId, CancellationToken ct = default);

    /// <summary>
    /// Records that affected data subjects have been notified (Art. 34).
    /// Applies to high-severity breaches only.
    /// </summary>
    /// <param name="breachId">ID of the <see cref="DataBreachRecord"/> to update.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// A successful <see cref="Result{T}"/> with the updated record and <see cref="DataBreachRecord.SubjectsNotifiedAt"/> set;
    /// a failed <see cref="Result{T}"/> with <see cref="Error.NotFound(string,object)"/> if the breach record does not exist.
    /// </returns>
    Task<Result<DataBreachRecord>> MarkSubjectsNotifiedAsync(
        Guid breachId, CancellationToken ct = default);
}
