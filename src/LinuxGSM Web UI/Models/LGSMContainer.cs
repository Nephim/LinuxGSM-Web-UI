namespace LinuxGSM.Web.UI.Models;

using Docker.DotNet.Models;

public class LGSMContainer
{
	public string ContainerId { get; set; } = string.Empty;
	public ContainerGameInfo ContainerGameInfo { get; set; } = new();
	public GameServer GameServer { get; set; } = new();
	public ContainerListResponse ContainerListResponse { get; set; } = new();
}
