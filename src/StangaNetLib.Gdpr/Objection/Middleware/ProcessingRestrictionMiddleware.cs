using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using StangaNetLib.Gdpr.Objection.Abstractions;
using StangaNetLib.Gdpr.Objection.Attributes;
using System.Security.Claims;

namespace StangaNetLib.Gdpr.Objection.Middleware;

/// <summary>
/// Blocks access for authenticated users who have an active processing restriction (GDPR Art. 18)
/// on endpoints decorated with <see cref="RequireNoRestrictionAttribute"/>.
/// Returns HTTP 451 (Unavailable For Legal Reasons) when a restriction is active.
/// Anonymous requests and endpoints without the attribute are passed through unchanged.
/// </summary>
/// <remarks>
/// Requires <see cref="IProcessingRestrictionService"/> to be registered in the DI container.
/// Must be placed after <c>UseRouting()</c> and after <c>UseAuthentication()</c>.
/// Add via <c>UseStangaNetLibGdprRestrictions()</c> in your pipeline configuration.
/// </remarks>
public sealed class ProcessingRestrictionMiddleware
{
    private readonly RequestDelegate                             _next;
    private readonly IProcessingRestrictionService              _restrictionService;
    private readonly ILogger<ProcessingRestrictionMiddleware>   _logger;

    /// <summary>Initialises the middleware with its dependencies.</summary>
    /// <param name="next">The next middleware in the pipeline.</param>
    /// <param name="restrictionService">Service used to check whether the user has an active processing restriction.</param>
    /// <param name="logger">Logger for access-denied events.</param>
    public ProcessingRestrictionMiddleware(
        RequestDelegate                           next,
        IProcessingRestrictionService             restrictionService,
        ILogger<ProcessingRestrictionMiddleware>  logger)
    {
        _next               = next;
        _restrictionService = restrictionService;
        _logger             = logger;
    }

    /// <summary>Processes the request, blocking access when the user has an active restriction matching the endpoint's scope.</summary>
    /// <param name="context">The current HTTP context.</param>
    public async Task InvokeAsync(HttpContext context)
    {
        var endpoint  = context.GetEndpoint();
        var attribute = endpoint?.Metadata.GetMetadata<RequireNoRestrictionAttribute>();

        if (attribute is null)
        {
            await _next(context);
            return;
        }

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
                "ProcessingRestrictionMiddleware: authenticated user has no sub/NameIdentifier claim.");
            await _next(context);
            return;
        }

        if (await _restrictionService.IsRestrictedAsync(userId, attribute.Scope, context.RequestAborted))
        {
            _logger.LogInformation(
                "Access denied: user {UserId} has an active processing restriction for scope '{Scope}'.",
                userId, attribute.Scope);

            context.Response.StatusCode  = StatusCodes.Status451UnavailableForLegalReasons;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new
            {
                error = "Processing of your data is currently restricted (GDPR Art. 18). This operation is unavailable while the restriction is active.",
                scope = attribute.Scope,
            });
            return;
        }

        await _next(context);
    }
}
