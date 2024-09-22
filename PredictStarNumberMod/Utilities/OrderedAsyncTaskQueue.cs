using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace PredictStarNumberMod.Utilities
{
    public class OrderedAsyncTaskQueue<T>
    {
        private readonly SemaphoreSlim semaphore = new SemaphoreSlim(1);
        private readonly SemaphoreSlim semaphore_waiting_until_queue_empty = new SemaphoreSlim(1);
        private readonly ConcurrentQueue<Func<Task<T>>> queue = new ConcurrentQueue<Func<Task<T>>>();
        private readonly ConcurrentQueue<Func<Task<T>>> queue_waiting_until_queue_empty = new ConcurrentQueue<Func<Task<T>>>();

        internal async Task<T> StartTaskAsync(Func<Task<T>> action)
        {
            queue.Enqueue(async () =>
            {
                return await action();
            });

            return await ProcessQueue();
        }

        private async Task<T> ProcessQueue()
        {
            while (queue.TryPeek(out Func<Task<T>> action))
            {
#if DEBUG
                Plugin.Log.Info("Start ProcessQueue : " + queue.Count.ToString() + "left");
#endif
                await semaphore.WaitAsync();
                T result = default;
                try
                {
                    return result = await action(); // キュー内の非同期処理を実行
                }
                finally
                {
                    queue.TryDequeue(out _);
#if DEBUG
                    Plugin.Log.Info("Finish ProcessQueue : " + queue.Count.ToString() + "left");
#endif
                    semaphore.Release();
                }
            }
            return default;
        }

        internal async Task WaitUntilQueueEmptyAsync()
        {
            await StartWaitingUntilQueueEmptyAsync(async () =>
            {
                while (!queue.IsEmpty)
                {
                    await Task.Delay(100);
                }
                return default;
            });
        }

        private async Task<T> StartWaitingUntilQueueEmptyAsync(Func<Task<T>> action)
        {
            queue_waiting_until_queue_empty.Enqueue(async () =>
            {
                return await action();
            });

            return await ProcessQueueForWaitingUntilQueueEmptyAsync();
        }

        private async Task<T> ProcessQueueForWaitingUntilQueueEmptyAsync()
        {
            while (queue_waiting_until_queue_empty.TryPeek(out Func<Task<T>> action))
            {
                await semaphore_waiting_until_queue_empty.WaitAsync();
                T result = default;
                try
                {
                    return result = await action(); // キュー内の非同期処理を実行
                }
                finally
                {
                    queue_waiting_until_queue_empty.TryDequeue(out _);
                    semaphore_waiting_until_queue_empty.Release();
                }
            }
            return default;
        }
    }
}
