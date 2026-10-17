namespace StangaNetLib.Gdpr.Consent.Attributes;

/// <summary>
/// Marks an endpoint as requiring a specific user consent before access is granted.
/// The <see cref="Middleware.ConsentValidationMiddleware"/> reads this attribute at runtime.
/// </summary>
/// <remarks>
/// Multiple attributes can be stacked on the same controller or action: all listed consent
/// types must be granted by the authenticated user. Anonymous requests bypass the check.
/// </remarks>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public sealed class RequireConsentAttribute : Attribute
{
    /// <summary>The consent category required (e.g. "Marketing", "Analytics").</summary>
    public string ConsentType { get; }

    /// <param name="consentType">
    /// The consent category required.
    /// Must match the value stored via <see cref="Abstractions.IConsentRepository"/>.
    /// </param>
    public RequireConsentAttribute(string consentType)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(consentType);
        ConsentType = consentType;
    }
}
