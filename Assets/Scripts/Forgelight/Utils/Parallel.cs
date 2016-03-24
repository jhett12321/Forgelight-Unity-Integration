using System;
using System.Collections.Generic;
using System.ComponentModel;

public class Parallel
{
    public static int NumberOfParallelTasks;

    static Parallel()
    {
        NumberOfParallelTasks = Environment.ProcessorCount;
    }

    public static BackgroundWorker AsyncForEach<T>(bool autoDispose, IEnumerable<T> enumerable, Action<T> action)
    {
        BackgroundWorker backgroundWorker = new BackgroundWorker();

        backgroundWorker.DoWork += delegate
        {
            System.Threading.Tasks.Parallel.ForEach(enumerable, action);
        };

        if (autoDispose)
        {
            backgroundWorker.RunWorkerCompleted += delegate
            {
                backgroundWorker.Dispose();
            };
        }

        backgroundWorker.RunWorkerAsync();

        return backgroundWorker;
    }
}