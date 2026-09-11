using System;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.Logging;
using SeoAuto.AiService.Features.Gemini;
using SeoAuto.BuildingBlocks.Messaging;

namespace SeoAuto.AiService.Features.Consumers;

public class AuditCompletedConsumer : IConsumer<AuditCompletedEvent>
{
    private readonly IGeminiClient _geminiClient;
    private readonly ILogger<AuditCompletedConsumer> _logger;

    public AuditCompletedConsumer(IGeminiClient geminiClient, ILogger<AuditCompletedConsumer> logger)
    {
        _geminiClient = geminiClient;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<AuditCompletedEvent> context)
    {
        var message = context.Message;
        _logger.LogInformation("AiService received AuditCompletedEvent for AuditId: {AuditId}, Url: {Url}", message.AuditId, message.Url);

        var input = new AuditAnalysisInput(
            message.AuditId,
            message.UserId,
            message.Url,
            message.Strategy,
            message.OverallScore,
            message.PerformanceScore,
            message.SeoScore,
            message.AccessibilityScore,
            message.BestPracticesScore,
            message.PerformanceDataJson,
            message.SeoDataJson
        );

        // Gọi Gemini Client để phân tích (hoặc sinh Smart Fallback nếu offline/lỗi)
        var aiResult = await _geminiClient.AnalyzeAuditAsync(input, context.CancellationToken);

        // Bắn sự kiện AiAnalysisCompletedEvent để ReportService và NotificationService tiếp nhận
        await context.Publish(new AiAnalysisCompletedEvent
        {
            AuditId = message.AuditId,
            UserId = message.UserId,
            Url = message.Url,
            Strategy = message.Strategy,
            AiSuggestionsJson = aiResult.MarkdownContent,
            SummaryAdvice = aiResult.SummaryAdvice,
            CompletedAt = DateTime.UtcNow,
            ModelUsed = aiResult.ModelUsed
        }, context.CancellationToken);

        _logger.LogInformation("AiService published AiAnalysisCompletedEvent for AuditId {AuditId} (Model: {Model}, Fallback: {IsFallback})",
            message.AuditId, aiResult.ModelUsed, aiResult.IsFallback);
    }
}
