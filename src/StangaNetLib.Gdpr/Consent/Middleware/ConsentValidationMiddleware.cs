using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using StangaNetLib.Gdpr.Consent.Abstractions;
using StangaNetLib.Gdpr.Consent.Attributes;
using System.Security.Claims;

namespace StangaNetLib.Gdpr.Consent.Middleware;

/// <summary>
/// Checks that the authenticated user has granted all consent types declared
/// via <see cref="RequireConsentAttribute"/> on the matched endpoint.
/// Anonymous requests and endpoints without the attribute are passed through unchanged.
/// Returns HTTP 403 with a JSON error body when a required consent is missing or revoked.
/// </summary>
/// <remarks>
/// Requires <see cref="IConsentRepository"/> to be registered in the DI container
/// by the consuming project before calling <c>UseStangaNetLibGdpr()</c>.
/// Must be placed after <c>UseRouting()</c> (so <c>GetEndpoint()</c> resolves)
/// and after <c>UseAuthentication()</c> (so <c>HttpContext.User</c> is populated).
/// </remarks>
public sealed class ConsentValidationMiddleware
{
    private readonly RequestDelegate                       _next;
    private readonly IConsentRepository                    _consentRepository;
    private readonly ILogger<ConsentValidationMiddleware>  _logger;

    /// <summary>Initialises the middleware with its dependencies.</summary>
    /// <param name="next">The next middleware in the pipeline.</param>
    /// <param name="consentRepository">Repository used to look up the user's consent decisions.</param>
    /// <param name="logger">Logger for access-denied events.</param>
    public ConsentValidationMiddleware(
        RequestDelegate                          next,
        IConsentRepository                       consentRepository,
        ILogger<ConsentValidationMiddleware>     logger)
    {
        _next              = next;
        _consentRepository = consentRepository;
        _logger            = logger;
    }

    /// <summary>Processes the request, checking consent for each <see cref="RequireConsentAttribute"/> on the endpoint.</summary>
    /// <param name="context">The current HTTP context.</param>
    public async Task InvokeAsync(HttpContext context)
    {
        var endpoint   = context.GetEndpoint();
        var attributes = endpoint?.Metadata.GetOrderedMetadata<RequireConsentAttribute>();

        if (attributes is null || attributes.Count == 0)
        {
            await _next(context);
            return;
        }

        // Consent is per-authenticated-user; anonymous requests bypass the check.
        if (context.User.Identity?.IsAuthenticated != true)
        {
            await _next(context);
            return;
        }

        var userId = context.User.FindFirst("sub")?.Value
                  ?? context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (userId is null)
        {
            _logger.LogWarning(
                "ConsentValidationMiddleware: authenticated user has no sub/NameIdentifier claim.");
            await _next(context);
            return;
        }

        foreach (var attr in attributes)
        {
            var consentResult = await _consentRepository.GetAsync(
                userId, attr.ConsentType, context.RequestAborted);

            if (consentResult.IsFailure || !consentResult.Value!.IsGranted)
            {
                _logger.LogInformation(
                    "Access denied: user {UserId} has not granted consent '{ConsentType}'.",
                    userId, attr.ConsentType);

                context.Response.StatusCode  = StatusCodes.Status403Forbidden;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(new
                {
                    error       = $"Consent '{attr.ConsentType}' is required to access this resource.",
                    consentType = attr.ConsentType,
                });
                return;
            }
        }

        await _next(context);
    }
}
