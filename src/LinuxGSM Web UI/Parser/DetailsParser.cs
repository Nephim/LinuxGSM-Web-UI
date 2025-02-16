namespace LinuxGSM.Web.UI.Parser;

using System.Text.RegularExpressions;

/// <summary>
/// A robust parser that splits the raw output into sections and key/value pairs.
/// </summary>
public class DetailsParser
{
	private static readonly string[] _separator = ["\r\n", "\r", "\n"];

	/// <summary>
	/// Parses the raw output into a dictionary where each key is a section name and
	/// the value is a dictionary of key/value pairs from that section.
	/// </summary>
	public static Dictionary<string, Dictionary<string, string>> Parse(string input)
	{
		// Remove ANSI escape sequences.
		var cleaned = StripAnsiCodes(input);

		var data = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
		string currentSection = "General";

		// Regex patterns.
		var sectionHeaderPattern = new Regex(@"^(?<section>[A-Za-z ]+(?:Details|Usage|Resource(?:s)?|Parameters|Ports))\s*$", RegexOptions.Compiled);
		var separatorPattern = new Regex(@"^[-=]{5,}$", RegexOptions.Compiled);
		var keyValuePattern = new Regex(@"^(?<key>[^:]+):\s*(?<value>.+)$", RegexOptions.Compiled);

		// Split the cleaned output into lines.
		var lines = cleaned.Split(_separator, StringSplitOptions.None);

		foreach (var rawLine in lines)
		{
			var line = rawLine.Trim();
			if (string.IsNullOrEmpty(line))
			{
				continue;
			}

			// Skip separator lines.
			if (separatorPattern.IsMatch(line))
			{
				continue;
			}

			// If the line matches a section header, update the current section.
			var sectionMatch = sectionHeaderPattern.Match(line);
			if (sectionMatch.Success)
			{
				currentSection = sectionMatch.Groups["section"].Value.Trim();
				data.TryAdd(currentSection, new(StringComparer.OrdinalIgnoreCase));

				continue;
			}

			// Try to match a key/value pair.
			var kvMatch = keyValuePattern.Match(line);
			if (kvMatch.Success)
			{
				var key = kvMatch.Groups["key"].Value.Trim();
				var value = kvMatch.Groups["value"].Value.Trim();
				data.TryAdd(currentSection, new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase));
				// Only add if not already present
				data[currentSection].TryAdd(key, value);
			}
		}

		return data;
	}

	/// <summary>
	/// Removes ANSI escape sequences from the provided text.
	/// </summary>
	public static string StripAnsiCodes(string input)
	{
		var ansiPattern = @"(?:\x1B[@-_][0-?]*[ -/]*[@-~])";
		return Regex.Replace(input, ansiPattern, "");
	}
}
