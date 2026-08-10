using System;
using System.Collections.Generic;
using System.Text;

namespace SeoAuto.BuildingBlocks.Messaging
{
    public record AuditRequestedEvent
    {
        public Guid AuditId {  get; init; }
        public string Url { get; init; } = string.Empty;
        public string Strategy { get; init; } = string.Empty;
    }
}
