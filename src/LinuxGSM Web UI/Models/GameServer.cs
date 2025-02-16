namespace LinuxGSM.Web.UI.Models;

/// <summary>
/// Represents the fields we care about.
/// </summary>
public class GameServer
{
	public string ServerName { get; set; } = "Unknown";
	public string Status { get; set; } = "Unknown";
	public Dictionary<string, string>? GameServerDetails { get; set; }
}
