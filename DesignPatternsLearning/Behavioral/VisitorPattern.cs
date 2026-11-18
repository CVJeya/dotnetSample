// DESIGN PATTERN: Visitor: adds operations to an object structure while keeping those operations separate from element classes.
// Run this demo from the existing console menu in Program.cs.

namespace DesignPatternsLearning.Behavioral
{
    // Element interface
    public interface IElement
    {
        void Accept(IVisitor visitor);
    }

    // Visitor interface
    public interface IVisitor
    {
        void Visit(Circle circle);

        void Visit(Rectangle rectangle);
    }

    public class Circle : IElement
    {
        public double Radius { get; set; }

        public void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    public class Rectangle : IElement
    {
        public double Width { get; set; }

        public double Height { get; set; }

        public void Accept(IVisitor visitor)
        {
            visitor.Visit(this);
        }
    }

    // Operation 1: Calculate area
    public class AreaCalculator : IVisitor
    {
        public void Visit(Circle circle)
        {
            double area = Math.PI *
                          circle.Radius *
                          circle.Radius;

            Console.WriteLine($"Circle area: {area:F2}");
        }

        public void Visit(Rectangle rectangle)
        {
            double area = rectangle.Width *
                          rectangle.Height;

            Console.WriteLine($"Rectangle area: {area:F2}");
        }
    }

    // Operation 2: Calculate perimeter
    public class PerimeterCalculator : IVisitor
    {
        public void Visit(Circle circle)
        {
            double perimeter = 2 * Math.PI * circle.Radius;

            Console.WriteLine(
                $"Circle perimeter: {perimeter:F2}");
        }

        public void Visit(Rectangle rectangle)
        {
            double perimeter = 2 *
                (rectangle.Width + rectangle.Height);

            Console.WriteLine(
                $"Rectangle perimeter: {perimeter:F2}");
        }
    }

    public class VisitorPattern
    {
        public static void Run()
        {
            IElement[] shapes =
            {
                new Circle { Radius = 5 },
                new Rectangle { Width = 4, Height = 6 }
            };

            var areaCalculator = new AreaCalculator();
            var perimeterCalculator = new PerimeterCalculator();

            foreach (var shape in shapes)
            {
                shape.Accept(areaCalculator);
                shape.Accept(perimeterCalculator);

                Console.WriteLine();
            }
        }
    }
}
