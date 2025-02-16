using System.Text;
using LinuxGSM.Web.UI.Endpoints;
using LinuxGSM.Web.UI.Extensions;
using LinuxGSM.Web.UI.Handler;
using LinuxGSM.Web.UI.LGSMControl;
using LinuxGSM.Web.UI.Parser;
using LinuxGSM.Web.UI.Service;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddHttpClient();

await builder.ConfigureDocker();
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<ITaskQueueService, TaskQueueService>();
builder.Services.AddHostedService<TaskQueueHostedService>();
builder.Services.AddSingleton<ITaskStatusService, TaskStatusService>();
builder.Services.AddSingleton<IGameServerInfoService, GameServerInfoService>();
builder.Services.AddSingleton<IGameServerCSVHandler, GameServerCSVHandler>();

builder.Services.AddSingleton<DockerExecutor>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.MapOpenApi();
	app.MapScalarApiReference();
}

app.MapGroup("container").MapContainerEndpoints();
app.MapStatusEndpoints();

app.MapPost("execute/{id}", async (DockerExecutor executor, string id, HttpResponse response, CancellationToken ct) =>
{

	var stringBuilder = new StringBuilder();

	await foreach (var line in executor.ExecuteCommand(id, ["./vhserver", "details"], ct))
	{
		stringBuilder.AppendLine(line);
	}
	var raw = stringBuilder.ToString();
	var parsed = DetailsParser.Parse(raw);

	var model = ServerInfoParser.ExtractServerInfo(parsed);

	return Results.Ok(new { raw, model });
});

app.MapGet("GameServers", async (IGameServerInfoService service) =>
{
	return Results.Ok(await service.GetGameServerInfos());
});

app.Run();
