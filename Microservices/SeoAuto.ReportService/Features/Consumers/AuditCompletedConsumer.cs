using MassTransit;
using Microsoft.EntityFrameworkCore;
using SeoAuto.BuildingBlocks.Messaging;
using SeoAuto.ReportService.Domain.Entities;
using SeoAuto.ReportService.Infrastructure.Database;

namespace SeoAuto.ReportService.Features.Consumers;

public class AuditCompletedConsumer : IConsumer<AuditCompletedEvent>
{
    private readonly ReportDbContext _dbContext;
    private readonly ILogger<AuditCompletedConsumer> _logger;

    public AuditCompletedConsumer(ReportDbContext dbContext, ILogger<AuditCompletedConsumer> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<AuditCompletedEvent> context)
    {
        var message = context.Message;
        _logger.LogInformation("Processing AuditCompletedEvent for AuditId: {AuditId}, Url: {Url}", message.AuditId, message.Url);

        var existingReport = await _dbContext.Reports
            .FirstOrDefaultAsync(r => r.AuditRequestId == message.AuditId, context.CancellationToken);

        if (existingReport != null)
        {
            _logger.LogWarning("Report already exists for AuditId: {AuditId}. Skipping duplicate event.", message.AuditId);
            return;
        }

        var normalizedUrl = message.Url.Trim().TrimEnd('/');
        var website = await _dbContext.Websites
            .Include(w => w.Project)
            .FirstOrDefaultAsync(w => w.Project != null && w.Project.UserId == message.UserId &&
                                      (w.Url == message.Url || w.Url.TrimEnd('/') == normalizedUrl),
                                 context.CancellationToken);

        var report = new Report
        {
            Id = Guid.NewGuid(),
            WebsiteId = website?.Id,
            AuditRequestId = message.AuditId,
            UserId = message.UserId,
            Url = message.Url,
            Strategy = message.Strategy,
            OverallScore = message.OverallScore,
            PerformanceScore = message.PerformanceScore,
            SeoScore = message.SeoScore,
            AccessibilityScore = message.AccessibilityScore,
            BestPracticesScore = message.BestPracticesScore,
            PerformanceData = string.IsNullOrWhiteSpace(message.PerformanceDataJson) ? "{}" : message.PerformanceDataJson,
            SeoData = string.IsNullOrWhiteSpace(message.SeoDataJson) ? "{}" : message.SeoDataJson,
            AiSuggestions = "{}",
            CreatedAt = message.CompletedAt
        };

        _dbContext.Reports.Add(report);
        await _dbContext.SaveChangesAsync(context.CancellationToken);

        _logger.LogInformation("Successfully created Report {ReportId} for AuditId {AuditId}, WebsiteId {WebsiteId}",
            report.Id, message.AuditId, report.WebsiteId);
    }
}
