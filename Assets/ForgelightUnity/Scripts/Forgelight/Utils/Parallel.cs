using System;
using System.Collections.Generic;

public class Parallel
{
    public static int NumberOfParallelTasks;

    static Parallel()
    {
        NumberOfParallelTasks = Environment.ProcessorCount;
    }



    public delegate System.Threading.Tasks.ParallelLoopResult AsyncForEach<T>(IEnumerable<T> enumerable, Action<T> action);
}