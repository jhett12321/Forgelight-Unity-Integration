using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;

public class Parallel
{
    public static int NumberOfParallelTasks;

    static Parallel()
    {
        NumberOfParallelTasks = Environment.ProcessorCount;
    }

    public static void AsyncForEach<T>(IEnumerable<T> enumerable, Action<T> action)
    {
        BackgroundWorker backgroundWorker = new BackgroundWorker();

        backgroundWorker.DoWork += delegate
        {
            ForEach(enumerable, action);
        };

        backgroundWorker.RunWorkerCompleted += delegate
        {
            backgroundWorker.Dispose();
        };

        backgroundWorker.RunWorkerAsync();
    }

    public static void ForEach<T>(IEnumerable<T> enumerable, Action<T> action)
    {
        var syncRoot = new object();

        if (enumerable == null) return;

        var enumerator = enumerable.GetEnumerator();

        InvokeAsync<T> del = InvokeAction;

        var seedItemArray = new T[NumberOfParallelTasks];
        var resultList = new List<IAsyncResult>(NumberOfParallelTasks);

        for (int i = 0; i < NumberOfParallelTasks; i++)
        {
            bool moveNext;

            lock (syncRoot)
            {
                moveNext = enumerator.MoveNext();
                seedItemArray[i] = enumerator.Current;
            }

            if (moveNext)
            {
                var iAsyncResult = del.BeginInvoke(enumerator, action, seedItemArray[i], syncRoot, i, null, null);
                resultList.Add(iAsyncResult);
            }
        }

        foreach (var iAsyncResult in resultList)
        {
            del.EndInvoke(iAsyncResult);
            iAsyncResult.AsyncWaitHandle.Close();
        }
    }

    delegate void InvokeAsync<T>(IEnumerator<T> enumerator, Action<T> achtion, T item, object syncRoot, int i);

    static void InvokeAction<T>(IEnumerator<T> enumerator, Action<T> action, T item, object syncRoot, int i)
    {
        if (String.IsNullOrEmpty(Thread.CurrentThread.Name))
        {
            Thread.CurrentThread.Name = String.Format("Parallel.ForEach Worker Thread No:{0}", i);
        }

        bool moveNext = true;

        while (moveNext)
        {
            action.Invoke(item);

            lock (syncRoot)
            {
                moveNext = enumerator.MoveNext();
                item = enumerator.Current;
            }
        }
    }
}