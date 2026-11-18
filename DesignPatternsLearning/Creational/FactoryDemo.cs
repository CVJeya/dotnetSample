// DESIGN PATTERN: Factory: centralizes object creation so callers depend on an abstraction rather than concrete construction details.
// Run this demo from the existing console menu in Program.cs.


using System;

namespace DesignPatternsLearning.Creational
{
    public interface IPaymentGateway
    {
        void ProcessPayment(decimal amount);
    }

    public class CreditCardGateway : IPaymentGateway
    {
        public void ProcessPayment(decimal amount)
        {
            Console.WriteLine($"Credit card payment: {amount:C}");
        }
    }

    public class PayPalGateway : IPaymentGateway
    {
        public void ProcessPayment(decimal amount)
        {
            Console.WriteLine($"PayPal payment: {amount:C}");
        }
    }

    public static class PaymentGatewayFactory
    {
        public static IPaymentGateway Create(string type)
        {
            return type.ToLowerInvariant() switch
            {
                "creditcard" => new CreditCardGateway(),
                "paypal" => new PayPalGateway(),
                _ => throw new ArgumentException(
                    $"Unknown gateway: {type}")
            };
        }
    }

    public static class FactoryDemo
    {
        public static void Run()
        {
            Console.WriteLine("\n--- Factory Pattern ---");

            IPaymentGateway gateway =
                PaymentGatewayFactory.Create("paypal");

            gateway.ProcessPayment(99.99m);

            gateway = PaymentGatewayFactory.Create("creditcard");
            gateway.ProcessPayment(49.99m);
        }
    }
}

