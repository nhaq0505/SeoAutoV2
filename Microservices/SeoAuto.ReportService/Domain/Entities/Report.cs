using System;

namespace SeoAuto.ReportService.Domain.Entities
{
    public class Report
    {
        public Guid Id { get; set; }
        public Guid? WebsiteId { get; set; }
        public Guid AuditRequestId { get; set; }
        public Guid UserId { get; set; }
        public string Url { get; set; } = string.Empty;
        public string Strategy { get; set; } = string.Empty;
        
        public int OverallScore { get; set; }
        public int PerformanceScore { get; set; }
        public int SeoScore { get; set; }
        public int AccessibilityScore { get; set; }
        public int BestPracticesScore { get; set; }

        public string PerformanceData { get; set; } = "{}";
        public string SeoData { get; set; } = "{}";
        public string? AiSuggestions { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Website? Website { get; set; }
    }
}
