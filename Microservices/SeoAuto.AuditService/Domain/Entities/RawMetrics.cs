using MassTransit;

namespace SeoAuto.AuditService.Domain.Entities;

    public class RawMetrics
    {
        public Guid Id { get; set; }
        public Guid AuditRequestId { get; set; }
        public int PerformanceScore { get; set; }
        public int AccessibilityScore { get; set; }
        public int BestPracticesScore { get; set; }
        public int SeoScore { get; set; }
        public int LCP_ms { get; set; } 
        public int INP_ms { get; set; }
        public float CLS { get; set; }
        public int TTFB_ms { get; set; }
        public int FCP_ms { get; set; }
        public int SpeedIndex_ms { get; set; }

    //Navigation Property
    public AuditRequest AuditRequest { get; set; } = null!;
    }

