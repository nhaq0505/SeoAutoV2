using System;

namespace SeoAuto.BuildingBlocks.Messaging
{
    public record AuditCompletedEvent
    {
        public Guid AuditId { get; init; }
        public Guid UserId { get; init; }
        public string Url { get; init; } = string.Empty;
        public string Strategy { get; init; } = string.Empty;
        public DateTime CompletedAt { get; init; }
        public int OverallScore { get; init; }
        public int PerformanceScore { get; init; }
        public int SeoScore { get; init; }
        public int AccessibilityScore { get; init; }
        public int BestPracticesScore { get; init; }
        public string PerformanceDataJson { get; init; } = string.Empty;
        public string SeoDataJson { get; init; } = string.Empty;
    }
}
