namespace ForgelightUnity.Editor.Forgelight.Utils
{
    using System.Collections.Concurrent;
    using System.Threading;

    public interface IPoolable
    {
        /// <summary>
        /// Called when the object is returned to the pool.
        /// </summary>
        void Reset();
    }

    /// <summary>
    /// A simple generic object pool. Thread Safe.
    /// </summary>
    /// <typeparam name="T">The type of object pool.</typeparam>
    public class ObjectPool<T> where T : IPoolable, new()
    {
        public int TotalObjects
        {
            get { return ActiveInstances + PooledItems.Count; }
        }
        public int ActiveInstances;
        public ConcurrentBag<T> PooledItems = new ConcurrentBag<T>();

        private bool capacityDefined;
        private int capacity;

        public int Capacity
        {
            get { return capacity; }
            set
            {
                capacityDefined = true;
                capacity = value;
            }
        }

        /// <param name="startAmount">The number of instances to prefill the pool.</param>
        public ObjectPool(int startAmount)
        {
            InitializePool(startAmount);
        }

        /// <param name="startAmount">The number of instances to prefill the pool.</param>
        /// <param name="capacity">The capacity of the pool.</param>
        public ObjectPool(int startAmount, int capacity)
        {
            this.Capacity = capacity;
            InitializePool(startAmount);
        }

        public ObjectPool() {}

        private void InitializePool(int startAmount)
        {
            for (int i = 0; i < startAmount; i++)
            {
                PooledItems.Add(new T());
            }
        }

        /// <summary>
        /// Finds an existing pooled object, or creates a new one if none are available
        /// </summary>
        /// <returns>A pooled object, or the default value if no pooled objects are available, and the pool has reached capacity.</returns>
        public T GetPooledObject()
        {
            T pooledItem;
            PooledItems.TryTake(out pooledItem);

            if (pooledItem == null)
            {
                if (capacityDefined && capacity <= TotalObjects)
                {
                    return default(T);
                }

                pooledItem = new T();
            }

            Interlocked.Increment(ref ActiveInstances);
            return pooledItem;
        }

        /// <summary>
        /// Returns an existing active instance back to the pool.
        /// </summary>
        /// <param name="instance"></param>
        public void ReturnObjectToPool(T instance)
        {
            instance.Reset();
            PooledItems.Add(instance);
            Interlocked.Decrement(ref ActiveInstances);
        }
    }
}