namespace LinuxGSM.Web.UI.Service;

public class TaskQueueHostedService : BackgroundService
{
	private readonly ITaskQueueService _taskQueue;
	private readonly ILogger<TaskQueueHostedService> _logger;

	public TaskQueueHostedService(ITaskQueueService taskQueue, ILogger<TaskQueueHostedService> logger)
	{
		_taskQueue = taskQueue;
		_logger = logger;
	}

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		while (!stoppingToken.IsCancellationRequested)
		{
			var workItem = await _taskQueue.DequeueAsync(stoppingToken);
			try
			{
				await workItem(stoppingToken);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error occurred executing background work item.");
			}
		}
	}
}
