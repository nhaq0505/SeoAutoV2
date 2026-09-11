using System.Threading.Tasks;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SeoAuto.BuildingBlocks.Messaging;
using SeoAuto.ReportService.Infrastructure.Database;

namespace SeoAuto.ReportService.Features.Consumers;

public class AiAnalysisCompletedConsumer : IConsumer<AiAnalysisCompletedEvent>
{
    private readonly ReportDbContext _dbContext;
    private readonly ILogger<AiAnalysisCompletedConsumer> _logger;

    public AiAnalysisCompletedConsumer(ReportDbContext dbContext, ILogger<AiAnalysisCompletedConsumer> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<AiAnalysisCompletedEvent> context)
    {
        var message = context.Message;
        _logger.LogInformation("ReportService received AiAnalysisCompletedEvent for AuditId {AuditId}", message.AuditId);

        var report = await _dbContext.Reports
            .FirstOrDefaultAsync(r => r.AuditRequestId == message.AuditId, context.CancellationToken);

        if (report == null)
        {
            _logger.LogWarning("Report not found for AuditRequestId {AuditId} when updating AI suggestions.", message.AuditId);
            return;
        }

        // Cập nhật trường AiSuggestions (JSONB / Markdown) vào CSDL theo đúng quy trình tuần tự SRS
        report.AiSuggestions = message.AiSuggestionsJson;
        await _dbContext.SaveChangesAsync(context.CancellationToken);

        _logger.LogInformation("Successfully updated Report {ReportId} with AI suggestions from model {Model}",
            report.Id, message.ModelUsed);
    }
}
