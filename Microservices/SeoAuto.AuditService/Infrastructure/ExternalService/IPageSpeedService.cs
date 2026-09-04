using SeoAuto.AuditService.Domain.Entities;

namespace SeoAuto.AuditService.Infrastructure.ExternalService
{
    public interface IPageSpeedService
    {
        Task<RawMetrics> GetPageSpeedMetricsAsync(Guid auditRequestId, string strategy, string url, CancellationToken cancellationToken = default);
    }
}
