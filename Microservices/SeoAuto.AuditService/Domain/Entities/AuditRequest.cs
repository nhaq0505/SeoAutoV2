using SeoAuto.AuditService.Domain.Enums;

namespace SeoAuto.AuditService.Domain.Entities
{
    public class AuditRequest
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Url { get; set; } = string.Empty;
        public AuditStatus Status { get; set; } = AuditStatus.Pending;
        public AuditStrategy Strategy { get; set; } = AuditStrategy.Desktop;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedAt { get; set; }
        public string? ErrorMessage { get; set; }

        //Navigation Property
        public RawMetrics? RawMetrics { get; set; }
        public SeoAnalysis? SeoAnalysis { get; set; }

    }
}
