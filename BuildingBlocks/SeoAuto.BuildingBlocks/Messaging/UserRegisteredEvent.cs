namespace SeoAuto.BuildingBlocks.Messaging;

// Kế thừa IntegrationEvent để lấy EventId và OccurredOn tự động
public record UserRegisteredEvent : IntegrationEvent
{
    public Guid UserId { get; init; }
    public string Email { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
}

