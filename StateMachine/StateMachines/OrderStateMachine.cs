using MassTransit;
using StateMachine.StateInstances;

namespace StateMachine.StateMachines;

public class OrderStateMachine: MassTransitStateMachine<OrderStateInstance>
{
    public OrderStateMachine()
    {
        InstanceState(x => x.CurrentState);
    }
}