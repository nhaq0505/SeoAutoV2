using System;

namespace SeoAuto.BuildingBlocks.Messaging
{
    public record AiAnalysisCompletedEvent
    {
        public Guid AuditId { get; init; }
        public Guid UserId { get; init; }
        public string Url { get; init; } = string.Empty;
        public string Strategy { get; init; } = string.Empty;
        public string AiSuggestionsJson { get; init; } = string.Empty;
        public string SummaryAdvice { get; init; } = string.Empty;
        public DateTime CompletedAt { get; init; } = DateTime.UtcNow;
        public string ModelUsed { get; init; } = string.Empty;
    }
}
