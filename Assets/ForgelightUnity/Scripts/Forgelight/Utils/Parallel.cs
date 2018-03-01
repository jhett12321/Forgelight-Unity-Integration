namespace ForgelightUnity.Forgelight.Utils
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public class Parallel
    {
        public static int NumberOfParallelTasks;

        static Parallel()
        {
            NumberOfParallelTasks = Environment.ProcessorCount;
        }



        public delegate ParallelLoopResult AsyncForEach<T>(IEnumerable<T> enumerable, Action<T> action);
    }
}