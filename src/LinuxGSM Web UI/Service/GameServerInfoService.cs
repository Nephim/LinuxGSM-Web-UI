namespace LinuxGSM.Web.UI.Service;

using LinuxGSM.Web.UI.Handler;
using LinuxGSM.Web.UI.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

public class GameServerInfoService : IGameServerInfoService, IDisposable
{
	private readonly IMemoryCache _cache;
	private readonly ILogger<GameServerInfoService> _logger;
	private readonly IGameServerCSVHandler _gameServerCSVHandler;
	private const string _cacheKey = "GameServerInfoCollection";
	private readonly SemaphoreSlim _semaphore = new(1, 1);

	public GameServerInfoService(IMemoryCache cache, ILogger<GameServerInfoService> logger, IGameServerCSVHandler gameServerCSVHandler)
	{
		_cache = cache;
		_logger = logger;
		_gameServerCSVHandler = gameServerCSVHandler;
	}

	public async Task<IReadOnlyCollection<ContainerGameInfo>> GetGameServerInfos()
	{
		if (_cache.TryGetValue(_cacheKey, out ConcurrentBag<ContainerGameInfo>? serverInfos) && serverInfos is not null)
		{
			// Return a read-only snapshot.
			return serverInfos.ToList().AsReadOnly();
		}

		await RefreshGameServerInfosAsync();

		if (_cache.TryGetValue(_cacheKey, out serverInfos) && serverInfos is not null)
		{
			// Return a read-only snapshot.
			return serverInfos.ToList().AsReadOnly();
		}

		return [];
	}

	public async Task RefreshGameServerInfosAsync(CancellationToken cancellationToken = default)
	{
		var fetchedServers = await _gameServerCSVHandler.FetchAndParseCsvAsync(cancellationToken);
		var bag = new ConcurrentBag<ContainerGameInfo>(fetchedServers);

		await _semaphore.WaitAsync(cancellationToken);
		try
		{
			var options = new MemoryCacheEntryOptions()
				.SetAbsoluteExpiration(TimeSpan.FromHours(1));


			_cache.Set(_cacheKey, bag, options);
		}
		finally
		{
			_semaphore.Release();
		}


		_logger.LogInformation("GameServerInfo refreshed. Count: {Count}", bag.Count);
	}

	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (disposing)
		{
			_semaphore?.Dispose();
		}
	}
}
