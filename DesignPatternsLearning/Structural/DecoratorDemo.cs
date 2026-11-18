// DESIGN PATTERN: Decorator: wraps an object to add behavior without changing the wrapped class.
// Run this demo from the existing console menu in Program.cs.



using System;

namespace DesignPatternsLearning.Structural
{
    public interface ICoffee
    {
        decimal GetCost();
        string GetDescription();
    }

    public class SimpleCoffee : ICoffee
    {
        public decimal GetCost() => 2.00m;

        public string GetDescription() => "Simple Coffee";
    }

    public abstract class CoffeeDecorator : ICoffee
    {
        protected readonly ICoffee Coffee;

        protected CoffeeDecorator(ICoffee coffee)
        {
            Coffee = coffee;
        }

        public virtual decimal GetCost() => Coffee.GetCost();

        public virtual string GetDescription() =>
            Coffee.GetDescription();
    }

    public class MilkDecorator : CoffeeDecorator
    {
        public MilkDecorator(ICoffee coffee) : base(coffee) { }

        public override decimal GetCost() =>
            Coffee.GetCost() + 0.50m;

        public override string GetDescription() =>
            Coffee.GetDescription() + ", Milk";
    }

    public class ChocolateDecorator : CoffeeDecorator
    {
        public ChocolateDecorator(ICoffee coffee) : base(coffee) { }

        public override decimal GetCost() =>
            Coffee.GetCost() + 0.75m;

        public override string GetDescription() =>
            Coffee.GetDescription() + ", Chocolate";
    }

    public class WhippedCreamDecorator : CoffeeDecorator
    {
        public WhippedCreamDecorator(ICoffee coffee) : base(coffee) { }

        public override decimal GetCost() =>
            Coffee.GetCost() + 0.60m;

        public override string GetDescription() =>
            Coffee.GetDescription() + ", Whipped Cream";
    }

    public static class DecoratorDemo
    {
        public static void Run()
        {
            Console.WriteLine("\n--- Decorator Pattern ---");

            ICoffee coffee = new SimpleCoffee();
            Show(coffee);

            coffee = new MilkDecorator(coffee);
            Show(coffee);

            coffee = new ChocolateDecorator(coffee);
            Show(coffee);

            coffee = new WhippedCreamDecorator(coffee);
            Show(coffee);
        }

        private static void Show(ICoffee coffee)
        {
            Console.WriteLine(
                $"{coffee.GetDescription()}: ${coffee.GetCost():F2}");
        }
    }
}

