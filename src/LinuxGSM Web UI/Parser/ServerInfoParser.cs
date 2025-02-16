namespace LinuxGSM.Web.UI.Parser;

using LinuxGSM.Web.UI.Models;

public static class ServerInfoParser
{
	public static GameServer ExtractServerInfo(Dictionary<string, Dictionary<string, string>> parsedData)
	{
		var info = new GameServer();

		// Try to locate the preferred section: one containing "Minecraft Server Details"
		Dictionary<string, string>? preferredSection = null;

		var preferredKey = parsedData.Keys.FirstOrDefault(x => x.Contains(" Server Details"));

		if (preferredKey is not null)
		{
			parsedData.TryGetValue(preferredKey, out preferredSection);
		}

		// Helper: search through all sections for a key if not found in the preferred section.
		string? FindValue(string key)
		{
			if (preferredSection != null && preferredSection.TryGetValue(key, out var val))
			{
				return val;
			}

			foreach (var section in parsedData.Values)
			{
				if (section.TryGetValue(key, out val))
				{
					return val;
				}
			}
			return null;
		}

		// Populate the model, preferring values from the preferred section.
		info.ServerName = FindValue("Server name") ?? info.ServerName;
		info.Status = FindValue("Status") ?? info.ServerName;
		info.GameServerDetails = preferredSection;

		return info;
	}
}
