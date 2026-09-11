using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using SeoAuto.BuildingBlocks.Extensions;
using SeoAuto.ReportService.Infrastructure.Database;

namespace SeoAuto.ReportService.Features.Reports;

public record ReportDetailResponse(
    Guid Id,
    Guid? WebsiteId,
    Guid AuditRequestId,
    Guid UserId,
    string Url,
    string Strategy,
    int OverallScore,
    int PerformanceScore,
    int SeoScore,
    int AccessibilityScore,
    int BestPracticesScore,
    object? PerformanceData,
    object? SeoData,
    object? AiSuggestions,
    DateTime CreatedAt
);

public record ScoreHistoryPoint(
    DateTime Date,
    int OverallScore,
    int PerformanceScore,
    int SeoScore,
    int AccessibilityScore,
    int BestPracticesScore
);

public record ReportHistoryResponse(
    Guid? WebsiteId,
    string? Url,
    List<ScoreHistoryPoint> History
);

public static class ReportsEndpoints
{
    public static void MapReportsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/reports")
            .WithTags("Reports")
            .RequireAuthorization();

        // 1. GET /api/reports/history - Lấy lịch sử điểm số để vẽ biểu đồ Core Web Vitals / Performance
        group.MapGet("/history", async (Guid? websiteId, string? url, DateTime? from, DateTime? to, ReportDbContext dbContext, ClaimsPrincipal user) =>
        {
            var userId = user.GetUserId();

            var query = dbContext.Reports
                .AsNoTracking()
                .Where(r => r.UserId == userId);

            if (websiteId.HasValue)
            {
                query = query.Where(r => r.WebsiteId == websiteId.Value);
            }
            else if (!string.IsNullOrWhiteSpace(url))
            {
                var normalized = url.Trim().TrimEnd('/');
                query = query.Where(r => r.Url == url || r.Url.TrimEnd('/') == normalized);
            }

            if (from.HasValue)
            {
                query = query.Where(r => r.CreatedAt >= from.Value);
            }

            if (to.HasValue)
            {
                query = query.Where(r => r.CreatedAt <= to.Value);
            }

            var points = await query
                .OrderBy(r => r.CreatedAt)
                .Select(r => new ScoreHistoryPoint(
                    r.CreatedAt,
                    r.OverallScore,
                    r.PerformanceScore,
                    r.SeoScore,
                    r.AccessibilityScore,
                    r.BestPracticesScore
                ))
                .ToListAsync();

            return Results.Ok(new ReportHistoryResponse(websiteId, url, points));
        })
        .WithName("GetReportHistory");

        // 2. GET /api/reports/{id} - Xem chi tiết bản báo cáo (theo report Id hoặc auditRequestId)
        group.MapGet("/{id:guid}", async (Guid id, ReportDbContext dbContext, ClaimsPrincipal user) =>
        {
            var userId = user.GetUserId();

            var report = await dbContext.Reports
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == id || r.AuditRequestId == id);

            if (report == null)
            {
                return Results.NotFound(new { Message = "Không tìm thấy báo cáo." });
            }

            if (report.UserId != userId)
            {
                return Results.Forbid();
            }

            var response = new ReportDetailResponse(
                report.Id,
                report.WebsiteId,
                report.AuditRequestId,
                report.UserId,
                report.Url,
                report.Strategy,
                report.OverallScore,
                report.PerformanceScore,
                report.SeoScore,
                report.AccessibilityScore,
                report.BestPracticesScore,
                ParseJsonOrRaw(report.PerformanceData),
                ParseJsonOrRaw(report.SeoData),
                ParseJsonOrRaw(report.AiSuggestions),
                report.CreatedAt
            );

            return Results.Ok(response);
        })
        .WithName("GetReportById");
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
