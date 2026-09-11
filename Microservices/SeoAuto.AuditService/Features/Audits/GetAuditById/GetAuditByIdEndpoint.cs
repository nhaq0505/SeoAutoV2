using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using SeoAuto.AuditService.Infrastructure.Database;
using SeoAuto.BuildingBlocks.Extensions;

namespace SeoAuto.AuditService.Features.Audits.GetAuditById;

public record RawMetricsDto(
    int PerformanceScore,
    int AccessibilityScore,
    int BestPracticesScore,
    int SeoScore,
    int LcpMs,
    int InpMs,
    float Cls,
    int TtfbMs,
    int FcpMs,
    int SpeedIndexMs
);

public record SeoAnalysisDto(
    string Title,
    string MetaDescription,
    string CanonicalUrl,
    bool HasRobotsTxt,
    bool HasSitemap,
    int H1Count,
    int ImagesWithoutAlt,
    object? OpenGraphData,
    object? StructuredData
);

public record AuditDetailResponse(
    Guid Id,
    Guid UserId,
    string Url,
    string Status,
    string Strategy,
    DateTime CreatedAt,
    DateTime? CompletedAt,
    string? ErrorMessage,
    RawMetricsDto? RawMetrics,
    SeoAnalysisDto? SeoAnalysis
);

public static class GetAuditByIdEndpoint
{
    public static void MapGetAuditByIdEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/audits/{id:guid}", async (Guid id, AuditDbContext dbContext, ClaimsPrincipal user) =>
        {
            // 1. Xác thực UserId từ JWT Claims (dùng extension chuẩn)
            var userId = user.GetUserId();

            // 2. Tìm kiếm AuditRequest kèm RawMetrics và SeoAnalysis
            var audit = await dbContext.AuditRequests
                .AsNoTracking()
                .Include(a => a.RawMetrics)
                .Include(a => a.SeoAnalysis)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (audit == null)
            {
                return Results.NotFound(new { Message = "Không tìm thấy yêu cầu Audit." });
            }

            // 3. Kiểm tra quyền sở hữu (Bảo mật: User A không được xem audit của User B - SRS TC-011)
            if (audit.UserId != userId)
            {
                return Results.Forbid();
            }

            // 4. Bóc tách RawMetricsDto nếu có
            RawMetricsDto? rawMetricsDto = null;
            if (audit.RawMetrics != null)
            {
                var rm = audit.RawMetrics;
                rawMetricsDto = new RawMetricsDto(
                    rm.PerformanceScore,
                    rm.AccessibilityScore,
                    rm.BestPracticesScore,
                    rm.SeoScore,
                    rm.LCP_ms,
                    rm.INP_ms,
                    rm.CLS,
                    rm.TTFB_ms,
                    rm.FCP_ms,
                    rm.SpeedIndex_ms
                );
            }

            // 5. Bóc tách SeoAnalysisDto và parse JSON data nếu có
            SeoAnalysisDto? seoAnalysisDto = null;
            if (audit.SeoAnalysis != null)
            {
                var sa = audit.SeoAnalysis;
                seoAnalysisDto = new SeoAnalysisDto(
                    sa.Title,
                    sa.MetaDescription,
                    sa.CanonicalUrl,
                    sa.HasRobotsTxt,
                    sa.HasSitemap,
                    sa.H1Count,
                    sa.ImagesWithoutAlt,
                    ParseJsonOrRaw(sa.OpenGraphData),
                    ParseJsonOrRaw(sa.StructuredData)
                );
            }

            var response = new AuditDetailResponse(
                audit.Id,
                audit.UserId,
                audit.Url,
                audit.Status.ToString(),
                audit.Strategy.ToString(),
                audit.CreatedAt,
                audit.CompletedAt,
                audit.ErrorMessage,
                rawMetricsDto,
                seoAnalysisDto
            );

            return Results.Ok(response);
        })
        .WithName("GetAuditById")
        .WithTags("Audits")
        .RequireAuthorization();
    }

    private static object? ParseJsonOrRaw(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return null;
        try
        {
            return JsonSerializer.Deserialize<JsonElement>(json);
        }
        catch
        {
            return json;
        }
    }
}
