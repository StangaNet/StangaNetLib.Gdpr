namespace StangaNetLib.Gdpr.Objection.Attributes;

/// <summary>
/// Marks an endpoint as requiring that the authenticated user does not have an active
/// processing restriction (GDPR Art. 18) for the given scope.
/// The <see cref="Middleware.ProcessingRestrictionMiddleware"/> enforces this at runtime,
/// returning HTTP 451 when a restriction is active.
/// </summary>
/// <remarks>
/// For restrictions covering all scopes, use the default <c>"*"</c> scope.
/// For granular per-feature restrictions, specify a descriptive scope name
/// (e.g. <c>"MarketingEmails"</c>, <c>"Profiling"</c>).
/// </remarks>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public sealed class RequireNoRestrictionAttribute : Attribute
{
    /// <summary>
    /// The processing scope to check. Default: <c>"*"</c> (any active restriction blocks access).
    /// Must match the <see cref="Models.ProcessingRestriction.Scope"/> used when the restriction was imposed.
    /// </summary>
    public string Scope { get; }

    /// <summary>
    /// Initialises the attribute with the processing scope to guard against restrictions.
    /// </summary>
    /// <param name="scope">
    /// The processing scope to check against active restrictions.
    /// Use <c>"*"</c> (default) to block access when any restriction is active, regardless of scope.
    /// Use a specific scope name (e.g. <c>"MarketingEmails"</c>) for more granular enforcement.
    /// </param>
    public RequireNoRestrictionAttribute(string scope = "*")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(scope);
        Scope = scope;
    }
}
