namespace SeoAuto.AuditService.Domain.Entities
{
    public class SeoAnalysis
    {
        public Guid Id { get; set; }
        public Guid AuditRequestId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string MetaDescription { get; set; } = string.Empty;
        public string CanonicalUrl { get; set; } = string.Empty;
        public bool HasRobotsTxt { get; set; }
        public bool HasSitemap { get; set; }
        public int H1Count { get; set; }
        public int ImagesWithoutAlt { get; set; }
        public string? OpenGraphData { get; set; }
        public string? StructuredData { get; set; }
        public AuditRequest AuditRequest { get; set; } = null!;

    }
}
