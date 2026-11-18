// DESIGN PATTERN: Singleton: ensures a type exposes one shared instance; useful for carefully managed shared resources.
// Run this demo from the existing console menu in Program.cs.


using System;

namespace DesignPatternsLearning.Creational
{
    public sealed class GoodSingleton
    {
        private static readonly Lazy<GoodSingleton> _instance =
            new Lazy<GoodSingleton>(() => new GoodSingleton());
static T GetFirst<T>(List<T> items)
{
    return items[0];
} 
 Func<int, int> doubleNumber = number => number * 2;

        public static GoodSingleton Instance => _instance.Value;

        private GoodSingleton()
        {
            Console.WriteLine("Singleton instance created.");
        }

        public void DoWork()
        {
            Console.WriteLine("Singleton is doing work.");
            string name = GetFirst(new List<string> { "Alice", "Bob" });
            int number = GetFirst(new List<int> { 10, 20 });
            int doubled = doubleNumber(number);
            Console.WriteLine($"Name: {name}, Number: {number}, Doubled: {doubled}");
        }
    }

    public static class SingletonDemo
    {
        public static void Run()
        {
            Console.WriteLine("\n--- Singleton Pattern ---");

            var first = GoodSingleton.Instance;
            var second = GoodSingleton.Instance;

            first.DoWork();

            Console.WriteLine(
                $"Same instance? {ReferenceEquals(first, second)}");
            second.DoWork();
            Console.WriteLine(
                $"Same instance? {ReferenceEquals(first, second)}");
        }
    }
}
