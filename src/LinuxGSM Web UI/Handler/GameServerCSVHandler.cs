namespace LinuxGSM.Web.UI.Handler;

using LinuxGSM.Web.UI.Models;
using LinuxGSM.Web.UI.Parser;

public class GameServerCSVHandler : IGameServerCSVHandler
{
	private const string _csvUrl = "https://raw.githubusercontent.com/GameServerManagers/LinuxGSM/refs/heads/master/lgsm/data/serverlist.csv";

	private readonly IHttpClientFactory _httpClientFactory;

	public GameServerCSVHandler(IHttpClientFactory httpClientFactory) => _httpClientFactory = httpClientFactory;

	public async Task<List<ContainerGameInfo>> FetchAndParseCsvAsync(CancellationToken cancellationToken = default)
	{
		var httpClient = _httpClientFactory.CreateClient();
		var csvContent = await httpClient.GetStringAsync(_csvUrl, cancellationToken);

		// Use the parser to convert the CSV content into a list of GameServerInfo objects.
		var gameServerInfos = ContainerGameInfoParser.ParseCsv(csvContent);
		return gameServerInfos;
	}
}
