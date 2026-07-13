using AccountTransferService.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace AccountTransferService.Api.Middleware;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly IProblemDetailsService _problemDetailsService;
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(IProblemDetailsService problemDetailsService, ILogger<GlobalExceptionHandler> logger)
    {
        _problemDetailsService = problemDetailsService;
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var (statusCode, title, errorCode) = exception switch
        {
            AccountNotFoundException accountNotFound => (StatusCodes.Status404NotFound, "Account not found", accountNotFound.ErrorCode),
            InsufficientFundsException insufficientFunds => (StatusCodes.Status422UnprocessableEntity, "Insufficient funds", insufficientFunds.ErrorCode),
            _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred", "INTERNAL_ERROR")
        };

        if (statusCode == StatusCodes.Status500InternalServerError)
        {
            _logger.LogError(exception, "Unhandled exception processing request {Path}", httpContext.Request.Path);
        }

        httpContext.Response.StatusCode = statusCode;

        return await _problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Detail = exception.Message,
                Type = $"https://httpstatuses.com/{statusCode}",
                Extensions =
                {
                    ["errorCode"] = errorCode,
                    ["traceId"] = httpContext.TraceIdentifier
                }
            }
        });
    }
}
