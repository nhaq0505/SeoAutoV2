using System;
using System.Collections.Generic;
using System.Text;

namespace SeoAuto.BuildingBlocks.Messaging
{
    public interface IEvent
    {
        Guid EventId { get; }
        DateTime OccurredOn { get; }
    }

    public record IntegrationEvent : IEvent
    {
        public Guid EventId { get; init; } = Guid.NewGuid();
        public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
    }
}
