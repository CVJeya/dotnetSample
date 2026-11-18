// DESIGN PATTERN: Prototype: creates a new object by copying an existing configured instance.
// Run this demo from the existing console menu in Program.cs.


using System;
using System.Collections.Generic;

namespace DesignPatternsLearning.Creational
{
    public class User
    {
        public string Name { get; set; } = "";
        public List<string> Roles { get; set; } = new();

        public User Clone()
        {
            return new User
            {
                Name = Name,
                Roles = new List<string>(Roles)
            };
        }
    }

    public static class PrototypeDemo
    {
        public static void Run()
        {
            Console.WriteLine("\n--- Prototype Pattern ---");

            var original = new User
            {
                Name = "Jeya",
                Roles = new List<string> { "Admin", "User" }
            };

            var copy = original.Clone();
            copy.Name = "Alex";
            copy.Roles.Add("Manager");

            Console.WriteLine($"Original: {original.Name}");
            Console.WriteLine($"Copy: {copy.Name}");
            Console.WriteLine(
                $"Original roles: {original.Roles.Count}");
            Console.WriteLine(
                $"Copy roles: {copy.Roles.Count}");
        }
    }
}
