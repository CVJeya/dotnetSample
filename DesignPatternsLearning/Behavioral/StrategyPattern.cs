// DESIGN PATTERN: Strategy: encapsulates interchangeable algorithms and selects one at runtime.
// Run this demo from the existing console menu in Program.cs.

namespace DesignPatternsLearning.Behavioral
{
    public interface IPaymentStrategy
    {
        void Pay(decimal amount);
    }

    public class CreditCardStrategy : IPaymentStrategy
    {
        private readonly string _cardNumber;

        public CreditCardStrategy(string cardNumber)
        {
            _cardNumber = cardNumber;
        }

        public void Pay(decimal amount)
        {
            Console.WriteLine(
                $"Paid ${amount} using Credit Card {_cardNumber}");
        }
    }

    public class PayPalStrategy : IPaymentStrategy
    {
        private readonly string _email;

        public PayPalStrategy(string email)
        {
            _email = email;
        }

        public void Pay(decimal amount)
        {
            Console.WriteLine(
                $"Paid ${amount} using PayPal {_email}");
        }
    }

    public class ShoppingCart
    {
        private IPaymentStrategy? _strategy;
        private decimal _total;

        public void SetPaymentStrategy(IPaymentStrategy strategy)
        {
            _strategy = strategy;
        }

        public void AddItem(decimal price)
        {
            _total += price;
        }

        public void Checkout()
        {
            if (_strategy == null)
                throw new InvalidOperationException(
                    "Payment strategy not set");

            _strategy.Pay(_total);
        }
    }

    public class StrategyPattern
    {
        public static void Run()
        {
            var cart = new ShoppingCart();

            cart.AddItem(29.99m);
            cart.AddItem(49.99m);

            // Select Credit Card strategy
            cart.SetPaymentStrategy(
                new CreditCardStrategy("123456789"));

            cart.Checkout();

            // Change strategy at runtime
            cart.SetPaymentStrategy(
                new PayPalStrategy("user@example.com"));

            cart.Checkout();
        }
    }
}
