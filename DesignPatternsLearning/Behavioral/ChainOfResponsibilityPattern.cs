// DESIGN PATTERN: Chain of Responsibility: passes a request through handlers until one handles it.
// Run this demo from the existing console menu in Program.cs.

namespace DesignPatternsLearning.Behavioral
{
    public class ExpenseRequest
    {
        public decimal Amount { get; set; }
    }

    public abstract class Approver
    {
        protected Approver? _nextApprover;
        protected decimal _approvalLimit;

        public void SetNext(Approver next)
        {
            _nextApprover = next;
        }

        public void Approve(ExpenseRequest request)
        {
            if (request.Amount <= _approvalLimit)
            {
                Console.WriteLine(
                    $"{GetType().Name} approved ${request.Amount}");
            }
            else if (_nextApprover != null)
            {
                _nextApprover.Approve(request);
            }
            else
            {
                Console.WriteLine(
                    $"Request for ${request.Amount} rejected");
            }
        }
    }

    public class Manager : Approver
    {
        public Manager()
        {
            _approvalLimit = 1000m;
        }
    }

    public class Director : Approver
    {
        public Director()
        {
            _approvalLimit = 10000m;
        }
    }

    public class CEO : Approver
    {
        public CEO()
        {
            _approvalLimit = decimal.MaxValue;
        }
    }

    public class ChainOfResponsibilityPattern
    {
        public static void Run()
        {
            var manager = new Manager();
            var director = new Director();
            var ceo = new CEO();

            // Create the chain
            manager.SetNext(director);
            director.SetNext(ceo);

            manager.Approve(
                new ExpenseRequest { Amount = 500m });

            manager.Approve(
                new ExpenseRequest { Amount = 5000m });

            manager.Approve(
                new ExpenseRequest { Amount = 50000m });
        }
    }
}