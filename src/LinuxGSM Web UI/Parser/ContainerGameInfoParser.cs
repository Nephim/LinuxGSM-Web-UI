namespace LinuxGSM.Web.UI.Parser;

using CsvHelper;
using CsvHelper.Configuration;
using LinuxGSM.Web.UI.Models;
using System.Globalization;

public class ContainerGameInfoParser
{
	public static List<ContainerGameInfo> ParseCsv(string csvContent)
	{
		using var reader = new StringReader(csvContent);
		var config = new CsvConfiguration(CultureInfo.InvariantCulture)
		{
			HasHeaderRecord = true,
			PrepareHeaderForMatch = (PrepareHeaderForMatchArgs args) => args.Header.ToLower(CultureInfo.InvariantCulture)
		};

		using var csv = new CsvReader(reader, config);
		// This will automatically map CSV columns to properties on GameServerInfo.
		var records = csv.GetRecords<ContainerGameInfo>().ToList();
		return records;
	}
}
