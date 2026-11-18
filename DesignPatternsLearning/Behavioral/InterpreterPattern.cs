// DESIGN PATTERN: Interpreter: represents a small grammar and evaluates expressions in that grammar.
// Run this demo from the existing console menu in Program.cs.

namespace DesignPatternsLearning.Behavioral
{
    // Expression interface
    public interface IExpression
    {
        int Interpret();
    }

    // Represents a number
    public class Number : IExpression
    {
        private readonly int _value;

        public Number(int value)
        {
            _value = value;
        }

        public int Interpret()
        {
            return _value;
        }
    }

    // Addition expression
    public class Add : IExpression
    {
        private readonly IExpression _left;
        private readonly IExpression _right;

        public Add(IExpression left, IExpression right)
        {
            _left = left;
            _right = right;
        }

        public int Interpret()
        {
            return _left.Interpret() + _right.Interpret();
        }
    }

    // Subtraction expression
    public class Subtract : IExpression
    {
        private readonly IExpression _left;
        private readonly IExpression _right;

        public Subtract(IExpression left, IExpression right)
        {
            _left = left;
            _right = right;
        }

        public int Interpret()
        {
            return _left.Interpret() - _right.Interpret();
        }
    }

    public class InterpreterPattern
    {
        public static void Run()
        {
            // Expression: (5 + 3) - 2

            IExpression expression = new Subtract(
                new Add(
                    new Number(5),
                    new Number(3)
                ),
                new Number(2)
            );

            Console.WriteLine(
                $"Result: {expression.Interpret()}");
        }
    }
}