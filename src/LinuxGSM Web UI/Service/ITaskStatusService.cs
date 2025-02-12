namespace LinuxGSM.Web.UI.Service;

public interface ITaskStatusService
{
	/// <summary>
	/// Enqueues a work item and returns a unique job identifier.
	/// </summary>
	/// <param name="workItem">The background work item to enqueue.</param>
	/// <returns>A GUID representing the job.</returns>
	Guid EnqueueTask(Func<CancellationToken, Task> workItem);

	/// <summary>
	/// Retrieves the task associated with a given job identifier.
	/// </summary>
	/// <param name="jobId">The job identifier.</param>
	/// <returns>The Task representing the work item, or null if not found.</returns>
	Task? GetTaskStatus(Guid jobId);
}
