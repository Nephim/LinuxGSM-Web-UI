namespace LinuxGSM.Web.UI.Endpoints;

using Docker.DotNet;
using LinuxGSM.Web.UI.Extensions;
using LinuxGSM.Web.UI.Service;

public static class ContainerEndpoints
{
	public static IEndpointRouteBuilder MapContainerEndpoints(this IEndpointRouteBuilder builder)
	{
		builder.MapGet("", async (IDockerClient dockerClient) =>
		{
			var containers = await dockerClient.Containers.ListLGSMContainersAsync();

			return containers;
		});

		builder.MapPost("stop/{id}", (string id, IDockerClient dockerClient, ITaskStatusService taskStatusService) =>
		{
			var jobId = taskStatusService.EnqueueTask(async token => await dockerClient.Containers.StopContainerAsync(id, new(), token));

			return Results.Accepted($"/tasks/status/{jobId}", new { jobId });
		});

		builder.MapPost("start/{id}", (string id, IDockerClient dockerClient, ITaskStatusService taskStatusService) =>
		{
			var jobId = taskStatusService.EnqueueTask(async token => await dockerClient.Containers.StartContainerAsync(id, new(), token));

			return Results.Accepted($"/tasks/status/{jobId}", new { jobId });
		});

		return builder;
	}
}
