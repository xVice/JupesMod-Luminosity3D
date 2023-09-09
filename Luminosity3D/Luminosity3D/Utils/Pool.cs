using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Luminosity3D.Utils
{
    public static class Pools
    {
        private static List<IPoolable> AllPools = new List<IPoolable>();

        public static void RegisterPool<T>(Pool<T> pool)
        {
            AllPools.Add(pool);
        }

        public static void MergeAllPools()
        {
            foreach (var pool in AllPools)
            {
                if (pool is IPoolable poolable)
                {
                    poolable.MergePool();
                }
            }
        }
    }

    public interface IPoolable
    {
        void MergePool();
    }

    public class Pool<T> : IPoolable
    {
        private Queue<T> queue;
        private List<T> list;

        public Pool()
        {
            queue = new Queue<T>();
            list = new List<T>();
            Pools.RegisterPool(this);
        }

        public List<T> GetContent()
        {
            return list;
        }

        public void Enqueue(T item)
        {
            queue.Enqueue(item);
        }

        public List<T> Update(Func<List<T>, List<T>> updateAction = null)
        {
            // Perform operations on the merged list here
            if (updateAction != null)
            {
                list = updateAction(list);
            }

            return list;
        }

        public void MergePool()
        {

                while (queue.TryDequeue(out T item))
                {
                    list.Add(item);
                }
            
        }

    }
}