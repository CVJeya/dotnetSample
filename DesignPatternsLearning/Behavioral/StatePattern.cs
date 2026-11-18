// DESIGN PATTERN: State: changes an object's behavior when its internal state changes.
// Run this demo from the existing console menu in Program.cs.

namespace DesignPatternsLearning.Behavioral
{
    public interface IOrderState
    {
        void Process(Order order);
    }

    public class PendingState : IOrderState
    {
        public void Process(Order order)
        {
            Console.WriteLine("Order is pending, confirming...");

            order.SetState(new ConfirmedState());
        }
    }

    public class ConfirmedState : IOrderState
    {
        public void Process(Order order)
        {
            Console.WriteLine("Order confirmed, shipping...");

            order.SetState(new ShippedState());
        }
    }

    public class ShippedState : IOrderState
    {
        public void Process(Order order)
        {
            Console.WriteLine("Order shipped, delivering...");

            order.SetState(new DeliveredState());
        }
    }

    public class DeliveredState : IOrderState
    {
        public void Process(Order order)
        {
            Console.WriteLine("Order delivered!");
        }
    }

    public class Order
    {
        private IOrderState _state;

        public Order()
        {
            _state = new PendingState();
        }

        public void SetState(IOrderState state)
        {
            _state = state;
        }

        public void Process()
        {
            _state.Process(this);
        }
    }

    public class StatePattern
    {
        public static void Run()
        {
            var order = new Order();

            order.Process();
            order.Process();
            order.Process();
            order.Process();
        }
    }
}
