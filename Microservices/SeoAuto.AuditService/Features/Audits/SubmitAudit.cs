using MassTransit;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using SeoAuto.AuditService.Domain.Entities;
using SeoAuto.AuditService.Domain.Enums;
using System.Security.Claims;
using SeoAuto.AuditService.Infrastructure.Database;
using SeoAuto.BuildingBlocks.Messaging;
using SeoAuto.BuildingBlocks.Exceptions;
using SeoAuto.BuildingBlocks.Extensions;
using System;

namespace SeoAuto.AuditService.Features.Audits.SubmitAudit;

// Khai báo kiểu Dữ liệu Nhận vào và Trả về
public record SubmitAuditRequest(string Url, string Strategy = "Desktop");
public record SubmitAuditResponse(Guid AuditId, string Message);

public static class SubmitAuditEndpoint
{
    public static void MapSubmitAuditEndpoint(this IEndpointRouteBuilder app)
    {
        // Chú ý: Dùng AuditDbContext
        app.MapPost("/api/audits/submit", async (SubmitAuditRequest request, AuditDbContext dbContext, IPublishEndpoint publishEndpoint, ClaimsPrincipal user) =>
        {
            // 1. Kiểm tra tính hợp lệ của URL (Bao gồm chống SSRF - Yêu cầu FR-301)
            if (!IsValidPublicUrl(request.Url))
            {
                return Results.BadRequest(new { Message = "URL không hợp lệ hoặc bị chặn vì lý do bảo mật (SSRF)." });
            }

            // 2. Ép kiểu Strategy từ string sang Enum (Mặc định là Desktop nếu truyền bậy)
            var strategy = Enum.TryParse<AuditStrategy>(request.Strategy, true, out var parsedStrategy)
                ? parsedStrategy
                : AuditStrategy.Desktop;


            // 3. Lấy UserId từ ClaimsPrincipal (dùng extension chuẩn)
            var userId = user.GetUserId();

            // 4. Tạo bản ghi AuditRequest mới với trạng thái Pending
            var newRequest = new AuditRequest
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Url = request.Url,
                Status = AuditStatus.Pending,
                Strategy = strategy,
                CreatedAt = DateTime.UtcNow
            };

            dbContext.AuditRequests.Add(newRequest);

            // 4. Bắn tin nhắn (Event) lên RabbitMQ để Worker đi làm việc
            await publishEndpoint.Publish(new AuditRequestedEvent
            {
                AuditId = newRequest.Id,
                Url = newRequest.Url,
                Strategy = newRequest.Strategy.ToString()
            });

            // 5. Lưu vào Database
            await dbContext.SaveChangesAsync();

            // 6. Trả về mã 202 Accepted (Báo hiệu: "Đã tiếp nhận yêu cầu, đang xử lý ngầm")
            return Results.Accepted($"/api/audits/{newRequest.Id}", new SubmitAuditResponse(newRequest.Id, "Yêu cầu Audit đã được đưa vào hàng đợi!"));
        })
        .WithName("SubmitAudit")
        .WithTags("Audits")
        .RequireAuthorization(); // Yêu cầu người dùng phải đăng nhập để Submit Audit
    }

    // --- Hàm Helper: Xác thực URL và Chống SSRF ---
    private static bool IsValidPublicUrl(string urlString)
    {
        if (string.IsNullOrWhiteSpace(urlString)) return false;

        if (!Uri.TryCreate(urlString, UriKind.Absolute, out var uri))
            return false;

        // Bắt buộc phải là HTTP hoặc HTTPS
        if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
            return false;

        // CHỐNG SSRF: Chặn các dải IP nội bộ và localhost
        if (uri.IsLoopback) return false; // Chặn các địa chỉ vòng lặp (127.0.0.1)

        // Chặn thêm các từ khóa nhạy cảm trỏ về server nội bộ
        if (uri.Host.Contains("localhost") || uri.Host.StartsWith("192.168") || uri.Host.StartsWith("10."))
            return false;

        return true;
    }
}