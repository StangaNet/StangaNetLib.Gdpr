using Microsoft.AspNetCore.Builder;
using StangaNetLib.Gdpr.Consent.Middleware;
using StangaNetLib.Gdpr.Objection.Middleware;

namespace StangaNetLib.Gdpr.Infrastructure.Extensions;

/// <summary>
/// Extension methods for <see cref="IApplicationBuilder"/> that add StangaNetLib.Gdpr middleware to the pipeline.
/// </summary>
public static class ApplicationBuilderExtensions
{
    /// <summary>
    /// Adds <see cref="ConsentValidationMiddleware"/> to the pipeline.
    /// Enforces <see cref="StangaNetLib.Gdpr.Consent.Attributes.RequireConsentAttribute"/> on matched endpoints,
    /// returning HTTP 403 when the authenticated user has not granted the required consent.
    /// </summary>
    /// <remarks>
    /// Must be placed <b>after</b> <c>UseRouting()</c> so that <c>GetEndpoint()</c> resolves
    /// the matched endpoint and its metadata.<br/>
    /// Must be placed <b>after</b> <c>UseAuthentication()</c> and <c>UseAuthorization()</c>
    /// (or <c>UseStangaNetLibAuth()</c>) so that <c>HttpContext.User</c> is populated.
    /// </remarks>
    /// <param name="app">The application builder.</param>
    /// <returns>The same <paramref name="app"/> instance for chaining.</returns>
    public static IApplicationBuilder UseStangaNetLibGdpr(this IApplicationBuilder app)
    {
        app.UseMiddleware<ConsentValidationMiddleware>();
        return app;
    }

    /// <summary>
    /// Adds <see cref="ProcessingRestrictionMiddleware"/> to the pipeline.
    /// Enforces <see cref="StangaNetLib.Gdpr.Objection.Attributes.RequireNoRestrictionAttribute"/> on matched endpoints,
    /// returning HTTP 451 when the authenticated user has an active processing restriction (Art. 18).
    /// </summary>
    /// <remarks>
    /// Requires <see cref="StangaNetLib.Gdpr.Objection.Abstractions.IProcessingRestrictionService"/> to be registered in the DI container.<br/>
    /// Must be placed <b>after</b> <c>UseRouting()</c> and <b>after</b> <c>UseAuthentication()</c>.
    /// </remarks>
    /// <param name="app">The application builder.</param>
    /// <returns>The same <paramref name="app"/> instance for chaining.</returns>
    public static IApplicationBuilder UseStangaNetLibGdprRestrictions(this IApplicationBuilder app)
    {
        app.UseMiddleware<ProcessingRestrictionMiddleware>();
        return app;
    }
}
