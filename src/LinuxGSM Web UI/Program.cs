using Docker.DotNet;
using LinuxGSM.Web.UI.Extensions;
using LinuxGSM.Web.UI.Service;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

await builder.ConfigureDocker();
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<ITaskQueueService, TaskQueueService>();
builder.Services.AddHostedService<TaskQueueHostedService>();
builder.Services.AddSingleton<ITaskStatusService, TaskStatusService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.MapOpenApi();
	app.MapScalarApiReference();
}

var summaries = new[]
{
	"Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
	var forecast = Enumerable.Range(1, 5).Select(index =>
		new WeatherForecast
		(
			DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
			Random.Shared.Next(-20, 55),
			summaries[Random.Shared.Next(summaries.Length)]
		))
		.ToArray();
	return forecast;
})
.WithName("GetWeatherForecast");

app.MapGet("container", async (IDockerClient dockerClient) =>
{
	var containers = await dockerClient.Containers.ListLGSMContainersAsync();

	return containers;
});

app.MapPost("container/stop/{id}", (string id, IDockerClient dockerClient, ITaskStatusService taskStatusService) =>
{
	var jobId = taskStatusService.EnqueueTask(async token => await dockerClient.Containers.StopContainerAsync(id, new(), token));

	return Results.Accepted($"/tasks/status/{jobId}", new { jobId });
});

app.MapPost("container/start/{id}", (string id, IDockerClient dockerClient, ITaskStatusService taskStatusService) =>
{
	var jobId = taskStatusService.EnqueueTask(async token => await dockerClient.Containers.StartContainerAsync(id, new(), token));

	return Results.Accepted($"/tasks/status/{jobId}", new { jobId });
});

app.MapGet("/tasks/status/{jobId:guid}", (Guid jobId, ITaskStatusService taskStatusService) =>
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

app.Run();

internal record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
	public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
