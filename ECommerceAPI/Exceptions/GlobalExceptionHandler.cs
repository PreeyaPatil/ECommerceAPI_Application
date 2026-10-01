using ECommerceAPI.Models;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;

namespace ECommerceAPI.Exceptions
{
    // GlobalExceptionHandler provides centralized exception handling
    // for the entire ASP.NET Core Web API application.
    // Instead of writing try-catch blocks inside every controller or service,
    // exceptions can be handled here in one common place.
    public sealed class GlobalExceptionHandler : IExceptionHandler
    {
        // ILogger is used to record exception details in the application logs.
        private readonly ILogger<GlobalExceptionHandler> _logger;

        // ILogger is injected through Dependency Injection.
        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger;
        }

        // TryHandleAsync is automatically called by ASP.NET Core
        // whenever an unhandled exception occurs while processing a request.
        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            // These variables will store the HTTP status code
            // and user-friendly error message returned to the client.
            int statusCode;
            string message;

            // Check the type of exception and convert it
            // into an appropriate HTTP status code and message.
            switch (exception)
            {
                // Used when the requested resource does not exist.
                // Example: Product with the given ProductId was not found.
                case NotFoundException:
                    statusCode = StatusCodes.Status404NotFound;
                    message = exception.Message;
                    break;

                // Used when a business rule is violated.
                // Example: Trying to order a product that is out of stock.
                case BusinessException:
                    statusCode = StatusCodes.Status400BadRequest;
                    message = exception.Message;
                    break;

                // Used when the request conflicts with existing data.
                // Example: Registering with an email address that already exists.
                case ConflictException:
                    statusCode = StatusCodes.Status409Conflict;
                    message = exception.Message;
                    break;

                // Used when the current user is not authorized
                // to perform the requested operation.
                case UnauthorizedAccessException:
                    statusCode = StatusCodes.Status401Unauthorized;
                    message = exception.Message;
                    break;

                // Occurs when Entity Framework Core detects that
                // another request has already modified the same database record.
                case DbUpdateConcurrencyException:
                    statusCode = StatusCodes.Status409Conflict;
                    message = "The data was modified by another request. Please try again.";
                    break;

                // Handles all other unexpected exceptions
                // that are not specifically handled above.
                default:
                    statusCode = StatusCodes.Status500InternalServerError;

                    // Do not expose the actual exception message to the client
                    // because it may contain sensitive implementation details.
                    message = "An unexpected error occurred while processing the request.";
                    break;
            }

            // Server-side errors are logged as Error because
            // they normally indicate an unexpected application problem.
            if (statusCode == StatusCodes.Status500InternalServerError)
            {
                _logger.LogError(
                    exception,
                    "An unhandled exception occurred. TraceId: {TraceId}",
                    httpContext.TraceIdentifier);
            }
            else
            {
                // Known or expected failures are logged as Warning.
                // We also include the TraceId so that the client's error response
                // can be matched with the corresponding log entry.
                _logger.LogWarning(
                    exception,
                    "Request failed with StatusCode {StatusCode}. TraceId: {TraceId}",
                    statusCode,
                    httpContext.TraceIdentifier);
            }

            // Create a standard API failure response.
            // TraceIdentifier uniquely identifies the current HTTP request
            // and is useful when troubleshooting errors using application logs.
            var response = ApiResponse<object?>.FailureResponse(
                message,
                traceId: httpContext.TraceIdentifier);

            // Set the HTTP status code that will be returned to the client.
            httpContext.Response.StatusCode = statusCode;

            // Convert the ApiResponse object into JSON
            // and write it to the HTTP response body.
            await httpContext.Response.WriteAsJsonAsync(response, cancellationToken);

            // Returning true tells ASP.NET Core that
            // this exception has been successfully handled.
            return true;
        }
    }
}
