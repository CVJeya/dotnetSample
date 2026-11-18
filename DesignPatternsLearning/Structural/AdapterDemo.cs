// DESIGN PATTERN: Adapter: translates one interface into another interface expected by the client.
// Run this demo from the existing console menu in Program.cs.


using System;
using System.Threading.Tasks;

namespace DesignPatternsLearning.Structural
{
    // Existing system: cannot change its interface.
    public class LegacyPaymentSystem
    {
        public decimal GetBalance(string accountId) => 1000m;

        public bool TransferFunds(
            string from, string to, decimal amount)
        {
            Console.WriteLine(
                $"Legacy system: Transferred {amount:C} from {from} to {to}");
            return true;
        }
    }

    // Interface expected by the new application.
    public interface IModernPaymentGateway
    {
        Task<decimal> GetBalanceAsync(string accountId);

        Task<bool> SendMoneyAsync(
            string from, string to, decimal amount);
    }

    // Adapter translates the new interface into legacy calls.
    public class LegacyPaymentAdapter : IModernPaymentGateway
    {
        private readonly LegacyPaymentSystem _legacy;

        public LegacyPaymentAdapter(LegacyPaymentSystem legacy)
        {
            _legacy = legacy;
        }

        public Task<decimal> GetBalanceAsync(string accountId)
        {
            return Task.FromResult(_legacy.GetBalance(accountId));
        }

        public Task<bool> SendMoneyAsync(
            string from, string to, decimal amount)
        {
            return Task.FromResult(
                _legacy.TransferFunds(from, to, amount));
        }
    }

    public static class AdapterDemo
    {
        public static void Run()
        {
            Console.WriteLine("\n--- Adapter Pattern ---");

            IModernPaymentGateway gateway =
                new LegacyPaymentAdapter(new LegacyPaymentSystem());

            bool success = gateway.SendMoneyAsync(
                "ACC001", "ACC002", 500m).GetAwaiter().GetResult();

            decimal balance = gateway.GetBalanceAsync(
                "ACC001").GetAwaiter().GetResult();

            Console.WriteLine($"Success: {success}");
            Console.WriteLine($"Balance: {balance:C}");
        }
    }
}
