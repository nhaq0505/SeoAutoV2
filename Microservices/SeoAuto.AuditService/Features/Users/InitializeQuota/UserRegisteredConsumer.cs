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

        _logger.LogInformation("UserRegisteredEvent received for UserId: {UserId}, Email: {Email}, FullName: {FullName}. Initial free quota initialized.",
            eventData.UserId, eventData.Email, eventData.FullName);

        return Task.CompletedTask;
    }
}