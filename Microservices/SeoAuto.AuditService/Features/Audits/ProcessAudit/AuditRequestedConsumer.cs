using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SeoAuto.AuditService.Domain.Enums;
using SeoAuto.AuditService.Infrastructure.Database;
using SeoAuto.AuditService.Infrastructure.ExternalService;
using SeoAuto.BuildingBlocks.Messaging;
using System.Threading.Tasks;

namespace SeoAuto.AuditService.Features.Audits.ProcessAudit
{
    public class AuditRequestedConsumer : IConsumer<AuditRequestedEvent>
    {
        private readonly ILogger<AuditRequestedConsumer> _logger;
        private readonly AuditDbContext _dbContext;
        private readonly IPageSpeedService _pageSpeedService;
        private readonly IHtmlService _htmlService;

        public AuditRequestedConsumer(ILogger<AuditRequestedConsumer> logger, AuditDbContext dbContext, IPageSpeedService pageSpeedService, IHtmlService htmlService)
        {
            _logger = logger;
            _dbContext = dbContext;
            _pageSpeedService = pageSpeedService;
            _htmlService = htmlService;
        }

        public async Task Consume(ConsumeContext<AuditRequestedEvent> context)
        {
            var message = context.Message;
            _logger.LogInformation("Received AuditRequestedEvent for AuditId: {AuditId}, Url: {Url}, Strategy: {Strategy}", message.AuditId, message.Url, message.Strategy);

            var auditRequest = await _dbContext.AuditRequests.FirstOrDefaultAsync(a => a.Id == message.AuditId);

            if (auditRequest == null)
            {
                _logger.LogWarning("AuditRequest with Id {AuditId} not found in the database.", message.AuditId);
                return;
            }

            try
            {
                auditRequest.Status = AuditStatus.Processing;
                await _dbContext.SaveChangesAsync();
                _logger.LogInformation("Updated AuditRequest status to Processing for Id {AuditId}.", message.AuditId);

                // Chạy song song cả Google PageSpeed API và cào HTML On-page bằng Task.WhenAll
                var pageSpeedTask = _pageSpeedService.GetPageSpeedMetricsAsync(auditRequest.Id, message.Strategy, message.Url, context.CancellationToken);
                var htmlTask = _htmlService.SeoAnalysisAsync(auditRequest.Id, message.Url, context.CancellationToken);

                await Task.WhenAll(pageSpeedTask, htmlTask);

                var rawMetrics = await pageSpeedTask;
                var seoAnalysis = await htmlTask;

                _dbContext.RawMetrics.Add(rawMetrics);
                _dbContext.SeoAnalyses.Add(seoAnalysis);

                auditRequest.Status = AuditStatus.Completed;
                auditRequest.CompletedAt = DateTime.UtcNow;
                await _dbContext.SaveChangesAsync();
                _logger.LogInformation("Updated AuditRequest status to Completed for Id {AuditId}.", message.AuditId);

                // Bắn event AuditCompletedEvent để ReportService và các service khác tiêu thụ
                var overallScore = (rawMetrics.PerformanceScore + rawMetrics.SeoScore + rawMetrics.AccessibilityScore + rawMetrics.BestPracticesScore) / 4;
                await context.Publish(new AuditCompletedEvent
                {
                    AuditId = auditRequest.Id,
                    UserId = auditRequest.UserId,
                    Url = auditRequest.Url,
                    Strategy = auditRequest.Strategy.ToString(),
                    CompletedAt = auditRequest.CompletedAt.Value,
                    OverallScore = overallScore,
                    PerformanceScore = rawMetrics.PerformanceScore,
                    SeoScore = rawMetrics.SeoScore,
                    AccessibilityScore = rawMetrics.AccessibilityScore,
                    BestPracticesScore = rawMetrics.BestPracticesScore,
                    PerformanceDataJson = System.Text.Json.JsonSerializer.Serialize(rawMetrics),
                    SeoDataJson = System.Text.Json.JsonSerializer.Serialize(seoAnalysis)
                }, context.CancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing AuditRequest for Id {AuditId}.", message.AuditId);

                // Dọn dẹp ChangeTracker để tránh lưu các entity thêm dở dang khi bị lỗi
                _dbContext.ChangeTracker.Clear();

                var failedAudit = await _dbContext.AuditRequests.FirstOrDefaultAsync(a => a.Id == message.AuditId);
                if (failedAudit != null)
                {
                    failedAudit.Status = AuditStatus.Failed;
                    failedAudit.ErrorMessage = ex.Message;
                    await _dbContext.SaveChangesAsync();
                }
            }
        }
    }
}
