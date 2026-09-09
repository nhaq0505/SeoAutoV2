using SeoAuto.AuditService.Domain.Entities;

namespace SeoAuto.AuditService.Infrastructure.ExternalService
{
    public interface IHtmlService
    {
        Task<SeoAnalysis> SeoAnalysisAsync(Guid auditRequestId, string url, CancellationToken cancellationToken = default);

    }
}
