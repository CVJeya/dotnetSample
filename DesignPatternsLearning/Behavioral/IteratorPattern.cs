// DESIGN PATTERN: Iterator: traverses a collection without exposing its internal representation.
// Run this demo from the existing console menu in Program.cs.

namespace DesignPatternsLearning.Behavioral
{
    public interface IIterator<T>
    {
        T Next();

        bool HasNext();
    }

    public interface IIterable<T>
    {
        IIterator<T> CreateIterator();
    }

    public class ArrayIterator<T> : IIterator<T>
    {
        private readonly T[] _array;
        private int _index = 0;

        public ArrayIterator(T[] array)
        {
            _array = array;
        }

        public bool HasNext()
        {
            return _index < _array.Length;
        }

        public T Next()
        {
            if (!HasNext())
                throw new InvalidOperationException(
                    "No more elements");

            return _array[_index++];
        }
    }

    public class Collection<T> : IIterable<T>
    {
        private readonly T[] _items;

        public Collection(T[] items)
        {
            _items = items;
        }

        public IIterator<T> CreateIterator()
        {
            return new ArrayIterator<T>(_items);
        }
    }

    public class IteratorPattern
    {
        public static void Run()
        {
            var collection = new Collection<string>(
                new[] { "Apple", "Banana", "Orange" });

            var iterator = collection.CreateIterator();

            while (iterator.HasNext())
            {
                Console.WriteLine(iterator.Next());
            }
        }
    }
}