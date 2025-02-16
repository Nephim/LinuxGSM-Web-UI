namespace LinuxGSM.Web.UI.Endpoints;

using LinuxGSM.Web.UI.Service;

public static class StatusEndpoints
{
	public static IEndpointRouteBuilder MapStatusEndpoints(this IEndpointRouteBuilder builder)
	{
		builder.MapGet("/tasks/status/{jobId:guid}", (Guid jobId, ITaskStatusService taskStatusService) =>
		{
			var task = taskStatusService.GetTaskStatus(jobId);
			if (task is null)
			{
				return Results.NotFound(new { jobId, status = "NotFound" });
			}

			// For example, report the task status.
			return Results.Ok(new
			{
				jobId,
				status = task.Status.ToString(),
				isCompleted = task.IsCompleted,
				isFaulted = task.IsFaulted,
				isCanceled = task.IsCanceled
			});
		});

		return builder;
	}
}
