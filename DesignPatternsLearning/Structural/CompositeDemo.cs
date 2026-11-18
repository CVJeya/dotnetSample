// DESIGN PATTERN: Composite: lets clients treat individual objects and groups of objects uniformly.
// Run this demo from the existing console menu in Program.cs.


using System;
using System.Collections.Generic;
using System.Linq;

namespace DesignPatternsLearning.Structural
{
    public interface IFileSystemItem
    {
        void Display(int indent = 0);
        long GetSize();
    }

    public class FileItem : IFileSystemItem
    {
        public string Name { get; }
        public long Size { get; }

        public FileItem(string name, long size)
        {
            Name = name;
            Size = size;
        }

        public void Display(int indent = 0)
        {
            Console.WriteLine(
                new string(' ', indent) + $"{Name} ({Size} bytes)");
        }

        public long GetSize() => Size;
    }

    public class DirectoryItem : IFileSystemItem
    {
        public string Name { get; }

        private readonly List<IFileSystemItem> _items = new();

        public DirectoryItem(string name)
        {
            Name = name;
        }

        public void Add(IFileSystemItem item)
        {
            _items.Add(item);
        }

        public void Display(int indent = 0)
        {
            Console.WriteLine(
                new string(' ', indent) + $"{Name}/");

            foreach (var item in _items)
            {
                item.Display(indent + 2);
            }
        }

        public long GetSize()
        {
            return _items.Sum(item => item.GetSize());
        }
    }

    public static class CompositeDemo
    {
        public static void Run()
        {
            Console.WriteLine("\n--- Composite Pattern ---");

            var root = new DirectoryItem("root");

            root.Add(new FileItem("readme.txt", 1024));
            root.Add(new FileItem("config.json", 512));

            var documents = new DirectoryItem("Documents");
            documents.Add(new FileItem("report.pdf", 2048));
            documents.Add(new FileItem("budget.xlsx", 1536));

            root.Add(documents);

            root.Display();

            Console.WriteLine(
                $"Total size: {root.GetSize()} bytes");
        }
    }
}
