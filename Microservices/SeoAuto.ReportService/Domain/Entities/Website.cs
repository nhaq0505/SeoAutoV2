using System;
using System.Collections.Generic;

namespace SeoAuto.ReportService.Domain.Entities
{
    public class Website
    {
        public Guid Id { get; set; }
        public Guid ProjectId { get; set; }
        public string Url { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? FaviconUrl { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Project? Project { get; set; }
        public ICollection<Report> Reports { get; set; } = new List<Report>();
    }
}
