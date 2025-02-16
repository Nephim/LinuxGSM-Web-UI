namespace LinuxGSM.Web.UI.LGSMControl;

using System.Text.RegularExpressions;

public partial class LGSMDockerController
{
	private readonly DockerExecutor _dockerExecutor;

	public LGSMDockerController(DockerExecutor dockerExecutor) => _dockerExecutor = dockerExecutor;

	public IAsyncEnumerable<string> Start(string containerId, string lgsmFileName, CancellationToken cancellationToken = default)
	{
		if (!IsValidFileName().IsMatch(lgsmFileName))
		{
			throw new ArgumentException("Invalid file name", nameof(lgsmFileName));
		}

		lgsmFileName = lgsmFileName.Trim();

		return _dockerExecutor.ExecuteCommand(containerId, [$"./{lgsmFileName}", "start"], cancellationToken);
	}

	public IAsyncEnumerable<string> Details(string containerId, string lgsmFileName, CancellationToken cancellationToken = default)
	{
		if (!IsValidFileName().IsMatch(lgsmFileName))
		{
			throw new ArgumentException("Invalid file name", nameof(lgsmFileName));
		}

		lgsmFileName = lgsmFileName.Trim();

		return _dockerExecutor.ExecuteCommand(containerId, [$"./{lgsmFileName}", "details"], cancellationToken);
	}

	[GeneratedRegex(@"^[a-zA-Z0-9_\-]+$")]
	private static partial Regex IsValidFileName();
}
