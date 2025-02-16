namespace LinuxGSM.Web.UI.Handler;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LinuxGSM.Web.UI.Models;

public interface IGameServerCSVHandler
{
	Task<List<ContainerGameInfo>> FetchAndParseCsvAsync(CancellationToken cancellationToken = default);
}