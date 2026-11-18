// DESIGN PATTERN: Facade: offers a simple entry point over a set of more complex subsystem classes.
// Run this demo from the existing console menu in Program.cs.



using System;

namespace DesignPatternsLearning.Structural
{
    public class PaymentProcessor
    {
        public bool ValidateCard(string cardNumber)
        {
            Console.WriteLine("Validating card...");
            return cardNumber.Length == 16;
        }
    }

    public class FraudDetectionService
    {
        public bool CheckForFraud(
            string cardNumber, decimal amount)
        {
            Console.WriteLine("Checking for fraud...");
            return false;
        }
    }

    public class PaymentGateway
    {
        public bool ProcessPayment(decimal amount)
        {
            Console.WriteLine($"Processing payment: ${amount:F2}");
            return true;
        }
    }

    public class NotificationService
    {
        public void SendConfirmation(
            string email, decimal amount)
        {
            Console.WriteLine(
                $"Confirmation sent to {email} for ${amount:F2}");
        }
    }

    public class PaymentFacade
    {
        private readonly PaymentProcessor _processor = new();
        private readonly FraudDetectionService _fraudService = new();
        private readonly PaymentGateway _gateway = new();
        private readonly NotificationService _notification = new();

        public bool ProcessPayment(
            string cardNumber, string email, decimal amount)
        {
            if (!_processor.ValidateCard(cardNumber))
            {
                Console.WriteLine("Payment rejected: invalid card.");
                return false;
            }

            if (_fraudService.CheckForFraud(cardNumber, amount))
            {
                Console.WriteLine("Payment rejected: fraud detected.");
                return false;
            }

            if (!_gateway.ProcessPayment(amount))
            {
                Console.WriteLine("Payment failed.");
                return false;
            }

            _notification.SendConfirmation(email, amount);
            return true;
        }
    }

    public static class FacadeDemo
    {
        public static void Run()
        {
            Console.WriteLine("\n--- Facade Pattern ---");

            var facade = new PaymentFacade();

            bool success = facade.ProcessPayment(
                "4111111111111111",
                "user@example.com",
                99.99m);

            Console.WriteLine($"Payment successful: {success}");
        }
    }
}

