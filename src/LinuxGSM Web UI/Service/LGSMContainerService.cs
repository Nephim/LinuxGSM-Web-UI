namespace LinuxGSM.Web.UI.Service;

using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Docker.DotNet;
using LinuxGSM.Web.UI.Extensions;
using LinuxGSM.Web.UI.LGSMControl;
using LinuxGSM.Web.UI.Models;
using LinuxGSM.Web.UI.Parser;

public class LGSMContainerService : BackgroundService
{
	private readonly IGameServerInfoService _gameServerInfoService;
	private readonly IDockerClient _dockerClient;
	private readonly LGSMDockerController _lGSMDockerController;
	private readonly ILogger<LGSMContainerService> _logger;

	public LGSMContainerService(IGameServerInfoService gameServerInfoService, IDockerClient dockerClient, LGSMDockerController lGSMDockerController, ILogger<LGSMContainerService> logger)
	{
		_gameServerInfoService = gameServerInfoService;
		_dockerClient = dockerClient;
		_lGSMDockerController = lGSMDockerController;
		_logger = logger;
	}

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		while (!stoppingToken.IsCancellationRequested)
		{
			try
			{
				var infos = await _gameServerInfoService.GetGameServerInfos();
				var containers = await _dockerClient.Containers.ListLGSMContainersAsync();

				var lgsmContainersTasks = containers.Select(async x =>
				{
					var containerGameInfo = infos.First(y => y.ShortName == x.Image[(x.Image.IndexOf(':') + 1)..]);
					var details = _lGSMDockerController.Details(x.ID, containerGameInfo.GameServerName, stoppingToken);

					var stringBuilder = new StringBuilder();
					await foreach (var line in details)
					{
						stringBuilder.AppendLine(line);
					}
					var gameServer = ServerInfoParser.ExtractServerInfo(DetailsParser.Parse(stringBuilder.ToString()));
					return new LGSMContainer
					{
						ContainerId = x.ID,
						GameServer = gameServer,
						ContainerGameInfo = containerGameInfo,
						ContainerListResponse = x,
					};
				}).ToList();

				var lgsmContainers = await Task.WhenAll(lgsmContainersTasks);


			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Something went wrong updating lgsmContainers");
			}
			finally
			{
				await Task.Delay(10000, stoppingToken);
			}
		}
	}
}
