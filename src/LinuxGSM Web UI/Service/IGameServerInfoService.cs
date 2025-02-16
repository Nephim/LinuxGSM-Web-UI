namespace LinuxGSM.Web.UI.Service;

using LinuxGSM.Web.UI.Models;

public interface IGameServerInfoService
{
	Task<IReadOnlyCollection<ContainerGameInfo>> GetGameServerInfos();
	Task RefreshGameServerInfosAsync(CancellationToken cancellationToken = default);
}
