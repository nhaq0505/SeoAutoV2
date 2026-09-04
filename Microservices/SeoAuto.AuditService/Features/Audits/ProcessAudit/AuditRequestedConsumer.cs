using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SeoAuto.AuditService.Domain.Enums;
using SeoAuto.AuditService.Infrastructure.Database;
using SeoAuto.BuildingBlocks.Messaging;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;


namespace SeoAuto.AuditService.Features.Audits.ProcessAudit
{
    public class AuditRequestedConsumer : IConsumer<AuditRequestedEvent>
    {
        private readonly ILogger<AuditRequestedConsumer> _logger;
        private readonly AuditDbContext _dbContext;

        public AuditRequestedConsumer(ILogger<AuditRequestedConsumer> logger, AuditDbContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }



        public async Task Consume(ConsumeContext<AuditRequestedEvent> context)
        {
            var message = context.Message;
            _logger.LogInformation("Received AuditRequestedEvent for AuditId: {AuditId}, Url: {Url}, Strategy: {Strategy}", message.AuditId, message.Url, message.Strategy);

            var auditRequest = await _dbContext.AuditRequests.FirstOrDefaultAsync(a => a.Id == message.AuditId);

            if (auditRequest == null)
            {
                _logger.LogWarning("AuditRequest with Id {AuditId} not found in the database.", message.AuditId);
                return;
            }

            try
            {
                auditRequest.Status = AuditStatus.Processing;
                await _dbContext.SaveChangesAsync();
                _logger.LogInformation("Updated AuditRequest status to Processing for Id {AuditId}.", message.AuditId);

                //TODO Gọi API GooglePageSpeed , Cào HTML phân tích SEO Onpage

                auditRequest.Status = AuditStatus.Completed;
                auditRequest.CompletedAt = DateTime.UtcNow;
                await _dbContext.SaveChangesAsync();
                _logger.LogInformation("Updated AuditRequest status to Completed for Id {AuditId}.", message.AuditId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating AuditRequest status for Id {AuditId}.", message.AuditId);

                auditRequest.Status = AuditStatus.Failed;
                auditRequest.ErrorMessage = ex.Message;
                await _dbContext.SaveChangesAsync();
            }
        }
    }
}
