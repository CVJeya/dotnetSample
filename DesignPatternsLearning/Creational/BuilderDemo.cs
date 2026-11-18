// DESIGN PATTERN: Builder: constructs a complex object step by step and keeps construction separate from representation.
// Run this demo from the existing console menu in Program.cs.


using System;
using System.Collections.Generic;

namespace DesignPatternsLearning.Creational
{
    public class Report
    {
        public string Title { get; set; } = "";
        public List<string> Sections { get; set; } = new();
        public bool IncludeFooter { get; set; }

        public void Display()
        {
            Console.WriteLine($"Report: {Title}");

            foreach (var section in Sections)
                Console.WriteLine($"Section: {section}");

            Console.WriteLine($"Footer: {IncludeFooter}");
        }
    }

    public class ReportBuilder
    {
        private readonly Report _report = new();

        public ReportBuilder WithTitle(string title)
        {
            _report.Title = title;
            return this;
        }

        public ReportBuilder AddSection(string section)
        {
            _report.Sections.Add(section);
            return this;
        }

        public ReportBuilder WithFooter()
        {
            _report.IncludeFooter = true;
            return this;
        }

        public Report Build()
        {
            return _report;
        }
    }

    public static class BuilderDemo
    {
        public static void Run()
        {
            Console.WriteLine("\n--- Builder Pattern ---");

            var report = new ReportBuilder()
                .WithTitle("Sales Report")
                .AddSection("Revenue")
                .AddSection("Expenses")
                .WithFooter()
                .Build();

            report.Display();
        }
    }
}

