namespace LinuxGSM.Web.UI.Extensions;

using Docker.DotNet;
using Docker.DotNet.Models;

public static class ContainerOperationsExtensions
{
	public static Task<IList<ContainerListResponse>> ListLGSMContainersAsync(this IContainerOperations containers)
	{
		return containers.ListContainersAsync(new ContainersListParameters
		{
			All = true,
			Filters = new Dictionary<string, IDictionary<string, bool>>
			{
				{ "label", new Dictionary<string, bool>
					{
						{ "org.opencontainers.image.url=https://github.com/GameServerManagers/docker-gameserver", true }
					}
				}
			}
		});
	}
}
