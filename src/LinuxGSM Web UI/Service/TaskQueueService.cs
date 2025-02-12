namespace LinuxGSM.Web.UI.Service;

using System.Collections.Concurrent;

public class TaskQueueService : ITaskQueueService, IDisposable
{
	private readonly ConcurrentQueue<Func<CancellationToken, Task>> _workItems = new();
	private readonly SemaphoreSlim _signal = new(0);

	public void QueueBackgroundWorkItem(Func<CancellationToken, Task> workItem)
	{
		ArgumentNullException.ThrowIfNull(workItem);

		_workItems.Enqueue(workItem);
		_signal.Release();
	}

	public async Task<Func<CancellationToken, Task>> DequeueAsync(CancellationToken cancellationToken)
	{
		await _signal.WaitAsync(cancellationToken);
		return !_workItems.TryDequeue(out var workItem)
			? throw new InvalidOperationException("Semaphore signaled, but no work item was found in the queue.")
			: workItem;
	}

	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (disposing)
		{
			_signal?.Dispose();
		}
	}
}
