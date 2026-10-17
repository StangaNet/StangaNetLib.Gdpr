using StangaNetLib.Core.Common;
using StangaNetLib.Gdpr.Consent.Models;

namespace StangaNetLib.Gdpr.Consent.Abstractions;

/// <summary>
/// CRUD contract for user consent records with full history.
/// The consuming project provides an EF Core or other persistence implementation.
/// </summary>
public interface IConsentRepository
{
    /// <summary>
    /// Returns the current (most recent) consent record for the given type.
    /// </summary>
    /// <param name="userId">The subject whose consent is queried.</param>
    /// <param name="consentType">The consent category to look up (e.g. "Marketing").</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// A successful <see cref="Result{T}"/> with the latest <see cref="ConsentRecord"/> when found;
    /// a failed <see cref="Result{T}"/> with <see cref="Error.NotFound(string,object)"/> if the user has never
    /// made a decision on this consent type.
    /// </returns>
    Task<Result<ConsentRecord>> GetAsync(
        string userId, string consentType, CancellationToken ct = default);

    /// <summary>Returns the latest consent record for each consent type the user has interacted with.</summary>
    /// <param name="userId">The subject whose consents are queried.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>One record per consent type, representing the most recent decision. Empty if the user has no consent history.</returns>
    Task<IReadOnlyList<ConsentRecord>> GetAllAsync(
        string userId, CancellationToken ct = default);

    /// <summary>
    /// Returns the full history of consent decisions for a given type,
    /// ordered by <see cref="ConsentRecord.GrantedAt"/> descending.
    /// </summary>
    /// <param name="userId">The subject whose consent history is queried.</param>
    /// <param name="consentType">The consent category whose history is retrieved.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>All historical records for the type, newest first. Empty if no decision has ever been made.</returns>
    Task<IReadOnlyList<ConsentRecord>> GetHistoryAsync(
        string userId, string consentType, CancellationToken ct = default);

    /// <summary>
    /// Records a consent grant. Creates a new <see cref="ConsentRecord"/> with <c>IsGranted = true</c>.
    /// If a prior record exists it is kept in history; this creates a new entry.
    /// </summary>
    /// <param name="userId">The subject granting consent.</param>
    /// <param name="consentType">The consent category being granted (e.g. "Marketing").</param>
    /// <param name="ipAddress">IP address at the time of the decision (evidentiary).</param>
    /// <param name="userAgent">User-Agent string at the time of the decision (evidentiary).</param>
    /// <param name="policyVersion">Version of the privacy notice shown to the user. Default: <c>"1.0"</c>.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// A successful <see cref="Result{T}"/> with the newly created <see cref="ConsentRecord"/>;
    /// a failed <see cref="Result{T}"/> if the operation could not be persisted.
    /// </returns>
    Task<Result<ConsentRecord>> GrantAsync(
        string            userId,
        string            consentType,
        string?           ipAddress     = null,
        string?           userAgent     = null,
        string            policyVersion = "1.0",
        CancellationToken ct            = default);

    /// <summary>
    /// Records a consent revocation. Creates a new <see cref="ConsentRecord"/> with <c>IsGranted = false</c>.
    /// </summary>
    /// <param name="userId">The subject revoking consent.</param>
    /// <param name="consentType">The consent category being revoked.</param>
    /// <param name="ipAddress">IP address at the time of the decision (evidentiary).</param>
    /// <param name="userAgent">User-Agent string at the time of the decision (evidentiary).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// A successful <see cref="Result{T}"/> with the newly created revocation record;
    /// a failed <see cref="Result{T}"/> with <see cref="Error.NotFound(string,object)"/> if the user had never granted consent for this type.
    /// </returns>
    Task<Result<ConsentRecord>> RevokeAsync(
        string            userId,
        string            consentType,
        string?           ipAddress = null,
        string?           userAgent = null,
        CancellationToken ct        = default);

    /// <summary>Revokes all active consents for the user.</summary>
    /// <param name="userId">The subject whose consents are revoked.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// A successful <see cref="Result{T}"/> with the number of consent types that were revoked (zero when none were active);
    /// a failed <see cref="Result{T}"/> if the operation could not be persisted.
    /// </returns>
    Task<Result<int>> RevokeAllAsync(string userId, CancellationToken ct = default);
}
