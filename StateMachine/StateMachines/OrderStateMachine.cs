using MassTransit;
using Order.API.Events;
using Payment.API.Events;
using StateMachine.Events;
using StateMachine.Events.Order;
using StateMachine.Settings;
using StateMachine.StateInstances;
using Stock.API.Events;
using Stock.API.Messages;

namespace StateMachine.StateMachines;

public class OrderStateMachine : MassTransitStateMachine<OrderStateInstance>
{
    public Event<OrderStartedEvent> OrderStartedEvent { get; set; }
    public Event<StockReservedEvent> StockReservedEvent { get; set; }
    public Event<PaymentCompletedEvent> PaymentCompletedEvent { get; set; }
    public Event<PaymentFailedEvent> PaymentFailedEvent { get; set; }
    public Event<StockNotReservedEvent> StockNotReservedEvent { get; set; }

    public State OrderCreated { get; set; }
    public State StockReserved { get; set; }
    public State PaymentCompleted { get; set; }
    public State PaymentFailed { get; set; }
    public State StockNotReserved { get; set; }

    public OrderStateMachine()
    {
        //State Instance'da ki hangi property'nin sipariş sürecindeki state'i tutacağı bildiriliyor.
        //Yani artık tüm event'ler CurrentState property'sin de tutulacaktır!
        InstanceState(instance => instance.CurrentState);

        Event(() => OrderStartedEvent,
            orderStateInstance =>
                orderStateInstance.CorrelateBy<int>(database => database.OrderId, @event => @event.Message.OrderId)
                    .SelectId(e => Guid.NewGuid()));

        Event(() => StockReservedEvent,
            orderStateInstance =>
                orderStateInstance.CorrelateById(@event => @event.Message.CorrelationId));

        Event(() => StockNotReservedEvent,
            orderStateInstance =>
                orderStateInstance.CorrelateById(@event => @event.Message.CorrelationId));

        Event(() => PaymentCompletedEvent,
            orderStateInstance =>
                orderStateInstance.CorrelateById(@event => @event.Message.CorrelationId));

        Event(() => PaymentFailedEvent,
            orderStateInstance =>
                orderStateInstance.CorrelateById(@event => @event.Message.CorrelationId));

        Initially(When(OrderStartedEvent)
            .Then(context =>
            {
                context.Instance.BuyerId = context.Data.BuyerId;
                context.Instance.OrderId = context.Data.OrderId;
                context.Instance.TotalPrice = context.Data.TotalPrice;
                context.Instance.CreatedDate = DateTime.Now;
            })
            .Then(context => Console.WriteLine("Ara işlem 1"))
            .Then(context => Console.WriteLine("Ara işlem 2"))
            .TransitionTo(OrderCreated)
            .Then(context => Console.WriteLine("Ara işlem 3"))
            .Send(new Uri($"queue:{RabbitMQSettings.Stock_OrderCreatedEventQueue}"),
                context => new OrderCreatedEvent(context.Instance.CorrelationId)
                {
                    OrderItems = context.Data.OrderItems
                }));
        
        During(OrderCreated,
            When(StockReservedEvent)
                .TransitionTo(StockReserved)
                .Send(new Uri($"queue:{RabbitMQSettings.Payment_StartedEventQueue}"),
                    context => new PaymentStartedEvent(context.Instance.CorrelationId)
                    {
                        OrderItems = context.Data.OrderItems,
                        TotalPrice = context.Instance.TotalPrice
                    }),

            When(StockNotReservedEvent)
                .TransitionTo(StockNotReserved)
                .Send(new Uri($"queue:{RabbitMQSettings.Order_OrderFailedEventQueue}"),
                    context => new OrderFailedEvent()
                    {
                        OrderId = context.Instance.OrderId,
                        Message = context.Data.Message
                    }));
        
        During(StockReserved,
            When(PaymentCompletedEvent)
                .TransitionTo(PaymentCompleted)
                .Send(new Uri($"queue:{RabbitMQSettings.Order_OrderCompletedEventQueue}"),
                    context => new OrderSuccessEvent()
                    {
                        OrderId = context.Instance.OrderId
                    })
                .Finalize(),

            When(PaymentFailedEvent)
                .TransitionTo(PaymentFailed)
                .Send(new Uri($"queue:{RabbitMQSettings.Order_OrderFailedEventQueue}"),
                    context => new OrderFailedEvent()
                    {
                        OrderId = context.Instance.OrderId,
                        Message = context.Data.Message
                    })
                .Send(new Uri($"queue:{RabbitMQSettings.Stock_RollbackMessageQueue}"),
                    context => new StockRollBackMessage
                    {
                        OrderItems = context.Data.OrderItems
                    }));

        //Finalize  instance deleted!
        SetCompletedWhenFinalized();
    }
}