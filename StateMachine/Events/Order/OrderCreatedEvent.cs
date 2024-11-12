using Stock.API.Messages;

namespace Stock.API.Events;

public class OrderCreatedEvent
{
    public OrderCreatedEvent(Guid correlationId)
    {
        CorrelationId = correlationId;
    }

    public Guid CorrelationId { get; }
    public List<OrderItemMessage> OrderItems { get; set; }
}