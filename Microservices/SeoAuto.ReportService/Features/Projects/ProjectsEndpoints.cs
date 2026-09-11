using System.Security.Claims;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using SeoAuto.BuildingBlocks.Exceptions;
using SeoAuto.BuildingBlocks.Extensions;
using SeoAuto.ReportService.Domain.Entities;
using SeoAuto.ReportService.Infrastructure.Database;

namespace SeoAuto.ReportService.Features.Projects;

public record CreateProjectRequest(string Name, string? Description);
public record UpdateProjectRequest(string Name, string? Description);

public record ProjectSummaryDto(
    Guid Id,
    string Name,
    string? Description,
    DateTime CreatedAt,
    int WebsiteCount,
    int? LatestScore
);

public record WebsiteSummaryDto(
    Guid Id,
    Guid ProjectId,
    string Url,
    string Name,
    string? FaviconUrl,
    DateTime CreatedAt,
    int? LatestOverallScore,
    DateTime? LatestAuditDate
);

public record ProjectDetailDto(
    Guid Id,
    string Name,
    string? Description,
    DateTime CreatedAt,
    List<WebsiteSummaryDto> Websites
);

public static class ProjectsEndpoints
{
    public static void MapProjectsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/projects")
            .WithTags("Projects")
            .RequireAuthorization();

        // 1. POST /api/projects - Tạo dự án mới
        group.MapPost("", async (CreateProjectRequest request, ReportDbContext dbContext, ClaimsPrincipal user) =>
        {
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                throw new BadRequestException("Tên dự án không được để trống.");
            }

            var userId = user.GetUserId();

            var project = new Project
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Name = request.Name.Trim(),
                Description = request.Description?.Trim(),
                CreatedAt = DateTime.UtcNow
            };

            dbContext.Projects.Add(project);
            await dbContext.SaveChangesAsync();

            var result = new ProjectSummaryDto(
                project.Id,
                project.Name,
                project.Description,
                project.CreatedAt,
                0,
                null
            );

            return Results.Created($"/api/projects/{project.Id}", result);
        })
        .WithName("CreateProject");

        // 2. GET /api/projects - Lấy danh sách dự án của người dùng
        group.MapGet("", async (ReportDbContext dbContext, ClaimsPrincipal user) =>
        {
            var userId = user.GetUserId();

            var projects = await dbContext.Projects
                .AsNoTracking()
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => new ProjectSummaryDto(
                    p.Id,
                    p.Name,
                    p.Description,
                    p.CreatedAt,
                    p.Websites.Count,
                    p.Websites
                        .SelectMany(w => w.Reports)
                        .OrderByDescending(r => r.CreatedAt)
                        .Select(r => (int?)r.OverallScore)
                        .FirstOrDefault()
                ))
                .ToListAsync();

            return Results.Ok(projects);
        })
        .WithName("GetProjects");

        // 3. GET /api/projects/{id} - Lấy chi tiết dự án kèm danh sách website
        group.MapGet("{id:guid}", async (Guid id, ReportDbContext dbContext, ClaimsPrincipal user) =>
        {
            var userId = user.GetUserId();

            var project = await dbContext.Projects
                .AsNoTracking()
                .Include(p => p.Websites)
                    .ThenInclude(w => w.Reports.OrderByDescending(r => r.CreatedAt).Take(1))
                .FirstOrDefaultAsync(p => p.Id == id);

            if (project == null)
            {
                return Results.NotFound(new { Message = "Không tìm thấy dự án." });
            }

            if (project.UserId != userId)
            {
                return Results.Forbid();
            }

            var websiteDtos = project.Websites.Select(w =>
            {
                var latestReport = w.Reports.FirstOrDefault();
                return new WebsiteSummaryDto(
                    w.Id,
                    w.ProjectId,
                    w.Url,
                    w.Name,
                    w.FaviconUrl,
                    w.CreatedAt,
                    latestReport?.OverallScore,
                    latestReport?.CreatedAt
                );
            }).ToList();

            var result = new ProjectDetailDto(
                project.Id,
                project.Name,
                project.Description,
                project.CreatedAt,
                websiteDtos
            );

            return Results.Ok(result);
        })
        .WithName("GetProjectById");

        // 4. PUT /api/projects/{id} - Cập nhật dự án
        group.MapPut("{id:guid}", async (Guid id, UpdateProjectRequest request, ReportDbContext dbContext, ClaimsPrincipal user) =>
        {
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                throw new BadRequestException("Tên dự án không được để trống.");
            }

            var userId = user.GetUserId();
            var project = await dbContext.Projects.FirstOrDefaultAsync(p => p.Id == id);

            if (project == null)
            {
                return Results.NotFound(new { Message = "Không tìm thấy dự án." });
            }

            if (project.UserId != userId)
            {
                return Results.Forbid();
            }

            project.Name = request.Name.Trim();
            project.Description = request.Description?.Trim();

            await dbContext.SaveChangesAsync();

            return Results.Ok(new ProjectSummaryDto(
                project.Id,
                project.Name,
                project.Description,
                project.CreatedAt,
                await dbContext.Websites.CountAsync(w => w.ProjectId == project.Id),
                null
            ));
        })
        .WithName("UpdateProject");

        // 5. DELETE /api/projects/{id} - Xóa dự án
        group.MapDelete("{id:guid}", async (Guid id, ReportDbContext dbContext, ClaimsPrincipal user) =>
        {
            var userId = user.GetUserId();
            var project = await dbContext.Projects.FirstOrDefaultAsync(p => p.Id == id);

            if (project == null)
            {
                return Results.NotFound(new { Message = "Không tìm thấy dự án." });
            }

            if (project.UserId != userId)
            {
                return Results.Forbid();
            }

            dbContext.Projects.Remove(project);
            await dbContext.SaveChangesAsync();

            return Results.NoContent();
        })
        .WithName("DeleteProject");
    }
}
