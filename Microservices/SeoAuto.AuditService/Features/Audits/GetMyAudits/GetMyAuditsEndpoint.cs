using System.Security.Claims;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using SeoAuto.AuditService.Infrastructure.Database;
using SeoAuto.BuildingBlocks.Extensions;

namespace SeoAuto.AuditService.Features.Audits.GetMyAudits;

public record AuditSummaryDto(
    Guid Id,
    string Url,
    string Status,
    string Strategy,
    DateTime CreatedAt,
    DateTime? CompletedAt,
    string? ErrorMessage,
    int? PerformanceScore,
    int? SeoScore
);

public record PagedResult<T>(
    IReadOnlyList<T> Items,
    int TotalCount,
    int Page,
    int PageSize
)
{
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}

public static class GetMyAuditsEndpoint
{
    public static void MapGetMyAuditsEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/audits/my", async (AuditDbContext dbContext, ClaimsPrincipal user, int page = 1, int pageSize = 10) =>
        {
            // 1. Xác thực UserId từ JWT Claims (dùng extension chuẩn)
            var userId = user.GetUserId();

            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 50) pageSize = 10;

            var query = dbContext.AuditRequests
                .AsNoTracking()
                .Where(a => a.UserId == userId);

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(a => a.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(a => new AuditSummaryDto(
                    a.Id,
                    a.Url,
                    a.Status.ToString(),
                    a.Strategy.ToString(),
                    a.CreatedAt,
                    a.CompletedAt,
                    a.ErrorMessage,
                    a.RawMetrics != null ? a.RawMetrics.PerformanceScore : (int?)null,
                    a.RawMetrics != null ? a.RawMetrics.SeoScore : (int?)null
                ))
                .ToListAsync();

            return Results.Ok(new PagedResult<AuditSummaryDto>(items, totalCount, page, pageSize));
        })
        .WithName("GetMyAudits")
        .WithTags("Audits")
        .RequireAuthorization();
    }
}
