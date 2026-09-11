using System.Security.Claims;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using SeoAuto.BuildingBlocks.Exceptions;
using SeoAuto.BuildingBlocks.Extensions;
using SeoAuto.ReportService.Domain.Entities;
using SeoAuto.ReportService.Features.Projects;
using SeoAuto.ReportService.Infrastructure.Database;

namespace SeoAuto.ReportService.Features.Websites;

public record CreateWebsiteRequest(
    Guid ProjectId,
    string Url,
    string Name,
    string? FaviconUrl
);

public record UpdateWebsiteRequest(
    string Name,
    string? FaviconUrl
);

public record WebsiteDetailDto(
    Guid Id,
    Guid ProjectId,
    string ProjectName,
    string Url,
    string Name,
    string? FaviconUrl,
    DateTime CreatedAt,
    int? LatestOverallScore,
    DateTime? LatestAuditDate,
    List<WebsiteReportItemDto> RecentReports
);

public record WebsiteReportItemDto(
    Guid Id,
    Guid AuditRequestId,
    string Strategy,
    int OverallScore,
    int PerformanceScore,
    int SeoScore,
    int AccessibilityScore,
    int BestPracticesScore,
    DateTime CreatedAt
);

public static class WebsitesEndpoints
{
    public static void MapWebsitesEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/websites")
            .WithTags("Websites")
            .RequireAuthorization();

        // 1. POST /api/websites - Thêm website mới vào một dự án
        group.MapPost("", async (CreateWebsiteRequest request, ReportDbContext dbContext, ClaimsPrincipal user) =>
        {
            if (string.IsNullOrWhiteSpace(request.Url))
            {
                throw new BadRequestException("URL website không được để trống.");
            }

            if (string.IsNullOrWhiteSpace(request.Name))
            {
                throw new BadRequestException("Tên website không được để trống.");
            }

            var userId = user.GetUserId();

            var project = await dbContext.Projects
                .FirstOrDefaultAsync(p => p.Id == request.ProjectId && p.UserId == userId);

            if (project == null)
            {
                throw new NotFoundException("Không tìm thấy dự án hợp lệ để thêm website.");
            }

            var website = new Website
            {
                Id = Guid.NewGuid(),
                ProjectId = request.ProjectId,
                Url = request.Url.Trim(),
                Name = request.Name.Trim(),
                FaviconUrl = request.FaviconUrl?.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            dbContext.Websites.Add(website);

            // Tự động liên kết các bản báo cáo audit trước đây của URL này với website mới tạo
            var normalizedUrl = website.Url.TrimEnd('/');
            var unlinkedReports = await dbContext.Reports
                .Where(r => r.UserId == userId && r.WebsiteId == null &&
                           (r.Url == website.Url || r.Url.TrimEnd('/') == normalizedUrl))
                .ToListAsync();

            foreach (var rep in unlinkedReports)
            {
                rep.WebsiteId = website.Id;
            }

            await dbContext.SaveChangesAsync();

            var result = new WebsiteSummaryDto(
                website.Id,
                website.ProjectId,
                website.Url,
                website.Name,
                website.FaviconUrl,
                website.CreatedAt,
                unlinkedReports.OrderByDescending(r => r.CreatedAt).Select(r => (int?)r.OverallScore).FirstOrDefault(),
                unlinkedReports.OrderByDescending(r => r.CreatedAt).Select(r => (DateTime?)r.CreatedAt).FirstOrDefault()
            );

            return Results.Created($"/api/websites/{website.Id}", result);
        })
        .WithName("CreateWebsite");

        // 2. GET /api/websites - Lấy danh sách website (có thể lọc theo projectId)
        group.MapGet("", async (Guid? projectId, ReportDbContext dbContext, ClaimsPrincipal user) =>
        {
            var userId = user.GetUserId();

            var query = dbContext.Websites
                .AsNoTracking()
                .Include(w => w.Project)
                .Where(w => w.Project != null && w.Project.UserId == userId);

            if (projectId.HasValue)
            {
                query = query.Where(w => w.ProjectId == projectId.Value);
            }

            var websites = await query
                .OrderByDescending(w => w.CreatedAt)
                .Select(w => new WebsiteSummaryDto(
                    w.Id,
                    w.ProjectId,
                    w.Url,
                    w.Name,
                    w.FaviconUrl,
                    w.CreatedAt,
                    w.Reports.OrderByDescending(r => r.CreatedAt).Select(r => (int?)r.OverallScore).FirstOrDefault(),
                    w.Reports.OrderByDescending(r => r.CreatedAt).Select(r => (DateTime?)r.CreatedAt).FirstOrDefault()
                ))
                .ToListAsync();

            return Results.Ok(websites);
        })
        .WithName("GetWebsites");

        // 3. GET /api/websites/{id} - Lấy chi tiết website kèm các báo cáo gần đây
        group.MapGet("{id:guid}", async (Guid id, ReportDbContext dbContext, ClaimsPrincipal user) =>
        {
            var userId = user.GetUserId();

            var website = await dbContext.Websites
                .AsNoTracking()
                .Include(w => w.Project)
                .Include(w => w.Reports.OrderByDescending(r => r.CreatedAt).Take(20))
                .FirstOrDefaultAsync(w => w.Id == id);

            if (website == null || website.Project == null)
            {
                return Results.NotFound(new { Message = "Không tìm thấy website." });
            }

            if (website.Project.UserId != userId)
            {
                return Results.Forbid();
            }

            var recentReports = website.Reports.Select(r => new WebsiteReportItemDto(
                r.Id,
                r.AuditRequestId,
                r.Strategy,
                r.OverallScore,
                r.PerformanceScore,
                r.SeoScore,
                r.AccessibilityScore,
                r.BestPracticesScore,
                r.CreatedAt
            )).ToList();

            var latest = recentReports.FirstOrDefault();

            var result = new WebsiteDetailDto(
                website.Id,
                website.ProjectId,
                website.Project.Name,
                website.Url,
                website.Name,
                website.FaviconUrl,
                website.CreatedAt,
                latest?.OverallScore,
                latest?.CreatedAt,
                recentReports
            );

            return Results.Ok(result);
        })
        .WithName("GetWebsiteById");

        // 4. PUT /api/websites/{id} - Cập nhật website
        group.MapPut("{id:guid}", async (Guid id, UpdateWebsiteRequest request, ReportDbContext dbContext, ClaimsPrincipal user) =>
        {
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                throw new BadRequestException("Tên website không được để trống.");
            }

            var userId = user.GetUserId();
            var website = await dbContext.Websites
                .Include(w => w.Project)
                .FirstOrDefaultAsync(w => w.Id == id);

            if (website == null || website.Project == null)
            {
                return Results.NotFound(new { Message = "Không tìm thấy website." });
            }

            if (website.Project.UserId != userId)
            {
                return Results.Forbid();
            }

            website.Name = request.Name.Trim();
            website.FaviconUrl = request.FaviconUrl?.Trim();

            await dbContext.SaveChangesAsync();

            return Results.Ok(new WebsiteSummaryDto(
                website.Id,
                website.ProjectId,
                website.Url,
                website.Name,
                website.FaviconUrl,
                website.CreatedAt,
                null,
                null
            ));
        })
        .WithName("UpdateWebsite");

        // 5. DELETE /api/websites/{id} - Xóa website
        group.MapDelete("{id:guid}", async (Guid id, ReportDbContext dbContext, ClaimsPrincipal user) =>
        {
            var userId = user.GetUserId();
            var website = await dbContext.Websites
                .Include(w => w.Project)
                .FirstOrDefaultAsync(w => w.Id == id);

            if (website == null || website.Project == null)
            {
                return Results.NotFound(new { Message = "Không tìm thấy website." });
            }

            if (website.Project.UserId != userId)
            {
                return Results.Forbid();
            }

            dbContext.Websites.Remove(website);
            await dbContext.SaveChangesAsync();

            return Results.NoContent();
        })
        .WithName("DeleteWebsite");
    }
}
