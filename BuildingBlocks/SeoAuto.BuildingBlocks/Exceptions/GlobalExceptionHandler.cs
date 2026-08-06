using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace SeoAuto.BuildingBlocks.Exceptions;

public class GlobalExceptionHandler : IExceptionHandler
{
    public ILogger<GlobalExceptionHandler> Logger { get; }

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        Logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        // 1. Ghi log chi tiết lỗi ra Console/Log system
        Logger.LogError(exception, "Unhandled Exception: {Message}", exception.Message);

        // 2. Xác định mã lỗi HTTP (StatusCode) dựa trên kiểu Exception
        var statusCode = exception switch
        {
            CustomException customEx => customEx.StatusCode,
            BadHttpRequestException => StatusCodes.Status400BadRequest,
            UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
            KeyNotFoundException => StatusCodes.Status404NotFound,
            _ => StatusCodes.Status500InternalServerError
        };

        // 3. Đóng gói lỗi thành chuẩn ProblemDetails (Tiêu chuẩn báo lỗi API toàn cầu RFC 7807)
        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = exception switch
            {
                CustomException => "Application Exception",
                _ => "Internal Server Error"
            },
            Detail = exception.Message,
            Instance = httpContext.Request.Path
        };

        // 4. Thiết lập StatusCode cho HTTP Response & Ghi JSON trả về Client
        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        // 5. Trả về true báo cho ASP.NET Core biết lỗi đã được xử lý xong
        return true;
    }
}