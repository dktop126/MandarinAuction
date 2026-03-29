using MandarinAuction.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace MandarinAuction.API.Middleware;

/// <summary>
/// Middleware для глобальной обработки исключений.
/// Перехватывает исключения и возвращает соответствующие HTTP коды с информацией об ошибке.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var response = context.Response;
        response.ContentType = "application/json";

        var errorResponse = new ProblemDetails();

        switch (exception)
        {
            case AuctionClosedException auctionClosedException:
                response.StatusCode = StatusCodes.Status400BadRequest;
                errorResponse.Title = "Аукцион закрыт";
                errorResponse.Detail = auctionClosedException.Message;
                errorResponse.Status = response.StatusCode;
                break;
            
            case BidTooLowException bidTooLowException:
                response.StatusCode = StatusCodes.Status400BadRequest;
                errorResponse.Title = "Некорректная ставка";
                errorResponse.Detail = bidTooLowException.Message;
                errorResponse.Status = response.StatusCode;
                break;
            
            case NotFoundException notFoundException:
                response.StatusCode = StatusCodes.Status404NotFound;
                errorResponse.Title = "Ресурс не найден";
                errorResponse.Detail = notFoundException.Message;
                errorResponse.Status = response.StatusCode;
                break;
            
            default:
                _logger.LogError(exception, "Необработанное исключение");
                response.StatusCode = StatusCodes.Status500InternalServerError;
                errorResponse.Title = "Внутренняя ошибка сервера";
                errorResponse.Status = response.StatusCode;
                break;
            
        }
        await response.WriteAsJsonAsync(errorResponse);
    }
}