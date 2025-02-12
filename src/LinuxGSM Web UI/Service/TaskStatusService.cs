namespace LinuxGSM.Web.UI.Service;

using Microsoft.Extensions.Caching.Memory;

public class TaskStatusService : ITaskStatusService
{
	private readonly ITaskQueueService _taskQueue;
	private readonly IMemoryCache _cache;

	public TaskStatusService(ITaskQueueService taskQueue, IMemoryCache cache)
	{
		_taskQueue = taskQueue;
		_cache = cache;
	}

	public Guid EnqueueTask(Func<CancellationToken, Task> workItem)
	{
		// Create a new job ID.
		var jobId = Guid.NewGuid();

		// Create a TaskCompletionSource to track the work item's completion.
		var tcs = new TaskCompletionSource<bool>();

		// Wrap the original work item to signal the tcs when done.
		_taskQueue.QueueBackgroundWorkItem(async token =>
		{
			try
			{
				await workItem(token);
				tcs.SetResult(true);
			}
			catch (Exception ex)
			{
				tcs.SetException(ex);
			}
		});

		// Store the task (which contains its status) in the cache.
		_cache.Set(jobId, tcs.Task, new MemoryCacheEntryOptions
		{
			AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
		});

		return jobId;
	}

	public Task? GetTaskStatus(Guid jobId)
	{
		return _cache.TryGetValue(jobId, out Task? task) ? task : null;
	}
}
