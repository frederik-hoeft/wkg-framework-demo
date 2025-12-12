using Cloudbb.Web.Data;
using Cloudbb.Web.Services.Auth;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.CodeAnalysis;
using Wkg.AspNetCore.Abstractions.Controllers;
using Wkg.AspNetCore.Transactions;

namespace Cloudbb.Web.Api.V1.Controllers;

/// <summary>
/// Base controller for Cloudbb API endpoints that provides common functionality for authenticated operations.
/// Extends DatabaseController with user authentication context and request validation patterns specific to the Cloudbb forum system.
/// </summary>
/// <param name="transactionService">Database transaction service for managing data consistency across operations.</param>
/// <param name="userClaims">Service for extracting and validating authenticated user information from HTTP context.</param>
public abstract class CloudbbControllerBase(ITransactionServiceHandle transactionService, IUserClaimIndex userClaims) : DatabaseController<CloudbbDbContext>(transactionService)
{
    /// <summary>
    /// Provides access to authenticated user claims and identity information from the current HTTP request context.
    /// Used throughout controller actions to determine user permissions and ownership of forum content.
    /// </summary>
    protected IUserClaimIndex UserClaims => userClaims;

    /// <summary>
    /// Performs comprehensive validation of incoming API requests including null checks, model state validation, and user authentication.
    /// This method provides a standardized validation pattern used across all Cloudbb controllers to ensure consistent error handling.
    /// </summary>
    /// <typeparam name="TRequest">The type of request object being validated, typically a data transfer object with validation attributes.</typeparam>
    /// <param name="request">The incoming request object to validate. Can be null, which will result in a BadRequest response.</param>
    /// <param name="errorResult">Output parameter containing the appropriate error response if validation fails, or null if validation succeeds.</param>
    /// <param name="userId">Output parameter containing the authenticated user's unique identifier if validation succeeds, or Guid.Empty if validation fails.</param>
    /// <returns>True if all validation passes (request is valid, model state is valid, and user is authenticated), false otherwise.</returns>
    protected bool TryValidateContext<TRequest>([NotNullWhen(true)] TRequest? request, [NotNullWhen(false)] out IActionResult? errorResult, out Guid userId)
    {
        errorResult = null;
        userId = Guid.Empty;
        if (request is null)
        {
            errorResult = BadRequest("Request body cannot be null");
            return false;
        }
        if (!ModelState.IsValid)
        {
            errorResult = BadRequest(ModelState);
            return false;
        }
        if (!userClaims.TryGetUserId(out userId))
        {
            // honestly, should never happen. the auth middleware should catch this
            errorResult = Unauthorized();
            return false;
        }
        return true;
    }
}
