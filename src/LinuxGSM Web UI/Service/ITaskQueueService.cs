namespace LinuxGSM.Web.UI.Service;

public interface ITaskQueueService
{
	void QueueBackgroundWorkItem(Func<CancellationToken, Task> workItem);
	Task<Func<CancellationToken, Task>> DequeueAsync(CancellationToken cancellationToken);
}
