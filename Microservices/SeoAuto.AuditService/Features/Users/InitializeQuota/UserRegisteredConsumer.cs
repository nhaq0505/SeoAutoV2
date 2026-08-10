using MassTransit;
using SeoAuto.BuildingBlocks.Messaging;


namespace SeoAuto.AuditService.Features.Users.InitializeQuota;

public class UserRegisteredConsumer : IConsumer<UserRegisteredEvent>
{
    private readonly ILogger<UserRegisteredConsumer> _logger;

    public UserRegisteredConsumer(ILogger<UserRegisteredConsumer> logger)
    {
        _logger = logger;
    }

    public Task Consume(ConsumeContext<UserRegisteredEvent> context)
    {
        var eventData = context.Message;

        _logger.LogInformation("\n==============================================");
        _logger.LogInformation("🎉 AUDIT SERVICE DA BAT DUOC EVENT!");
        _logger.LogInformation("   => Chao Mung User: {FullName}", eventData.FullName);
        _logger.LogInformation("   => Email: {Email}", eventData.Email);
        _logger.LogInformation("   => Da Cap San 10 luot Audit Free");
        _logger.LogInformation("==============================================\n");

        return Task.CompletedTask;
    }
}