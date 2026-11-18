// DESIGN PATTERN: Object Pool: reuses a bounded set of expensive objects instead of repeatedly creating and destroying them.
// Run this demo from the existing console menu in Program.cs.


using System;
using System.Collections.Generic;

namespace DesignPatternsLearning.Creational
{
    public class PooledObject
    {
        public Guid Id { get; } = Guid.NewGuid();

        public void Reset()
        {
            Console.WriteLine($"Resetting {Id}");
        }
    }

    public class ObjectPool
    {
        private readonly Stack<PooledObject> _available = new();

        public ObjectPool(int size)
        {
            for (int i = 0; i < size; i++)
                _available.Push(new PooledObject());
        }

        public PooledObject Acquire()
        {
            if (_available.Count == 0)
                throw new InvalidOperationException(
                    "No objects available.");

            return _available.Pop();
        }

        public void Release(PooledObject obj)
        {
            obj.Reset();
            _available.Push(obj);
        }
    }

    public static class ObjectPoolDemo
    {
        public static void Run()
        {
            Console.WriteLine("\n--- Object Pool Pattern ---");

            var pool = new ObjectPool(2);

            var first = pool.Acquire();
            Console.WriteLine($"First ID: {first.Id}");

            pool.Release(first);

            var reused = pool.Acquire();
            Console.WriteLine($"Reused ID: {reused.Id}");

            Console.WriteLine(
                $"Same object? {ReferenceEquals(first, reused)}");
        }
    }
}
