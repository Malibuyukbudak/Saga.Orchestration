using Stock.API.Messages;

namespace StateMachine.Events;

public class PaymentFailedEvent
{
    public PaymentFailedEvent(Guid correlationId)
    {
        CorrelationId = correlationId;
    }
    public Guid CorrelationId { get; }
    public List<OrderItemMessage> OrderItems { get; set; }
    public string Message { get; set; }
}