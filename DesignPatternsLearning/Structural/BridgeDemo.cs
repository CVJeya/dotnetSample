// DESIGN PATTERN: Bridge: separates an abstraction from its implementation so both can evolve independently.
// Run this demo from the existing console menu in Program.cs.


using System;
using System.IO;

namespace DesignPatternsLearning.Structural
{
    public interface IRenderer
    {
        void Render(string content);
    }

    public class ConsoleRenderer : IRenderer
    {
        public void Render(string content)
        {
            Console.WriteLine($"Console: {content}");
        }
    }

    public class FileRenderer : IRenderer
    {
        private readonly string _filename;

        public FileRenderer(string filename)
        {
            _filename = filename;
        }

        public void Render(string content)
        {
            File.WriteAllText(_filename, content);
            Console.WriteLine($"Written to {_filename}");
        }
    }

    public abstract class Document
    {
        protected readonly IRenderer Renderer;

        protected Document(IRenderer renderer)
        {
            Renderer = renderer;
        }

        public abstract void Display();
    }

    public class PdfDocument : Document
    {
        private readonly string _content;

        public PdfDocument(string content, IRenderer renderer)
            : base(renderer)
        {
            _content = content;
        }

        public override void Display()
        {
            Renderer.Render($"PDF: {_content}");
        }
    }

    public class WordDocument : Document
    {
        private readonly string _content;

        public WordDocument(string content, IRenderer renderer)
            : base(renderer)
        {
            _content = content;
        }

        public override void Display()
        {
            Renderer.Render($"Word: {_content}");
        }
    }

    public static class BridgeDemo
    {
        public static void Run()
        {
            Console.WriteLine("\n--- Bridge Pattern ---");

            Document pdf = new PdfDocument(
                "Sales Report", new ConsoleRenderer());
            pdf.Display();

            Document word = new WordDocument(
                "Welcome Letter", new ConsoleRenderer());
            word.Display();

            Document pdfToFile = new PdfDocument(
                "Saved Report", new FileRenderer("output.txt"));
            pdfToFile.Display();
        }
    }
}
