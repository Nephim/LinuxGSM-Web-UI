namespace LinuxGSM.Web.UI.LGSMControl;

using System.Runtime.CompilerServices;
using System.Text;
using Docker.DotNet;
using Docker.DotNet.Models;

public class DockerExecutor
{
	private readonly IDockerClient _client;
	private readonly ILogger<DockerExecutor> _logger;

	public DockerExecutor(IDockerClient client, ILogger<DockerExecutor> logger)
	{
		_client = client;
		_logger = logger;
	}

	/// <summary>
	/// Executes a command inside a container and yields stdout lines as they become available.
	/// Stderr is logged.
	/// </summary>
	public async IAsyncEnumerable<string> ExecuteCommand(
		string containerId,
		string[] command,
		[EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		var execCreateParameters = new ContainerExecCreateParameters
		{
			Cmd = command,
			AttachStdout = true,
			AttachStderr = true,
			User = "linuxgsm"
		};

		var execCreateResponse = await _client.Exec.ExecCreateContainerAsync(containerId, execCreateParameters, cancellationToken);
		var multiplexedStreamNullable = await _client.Exec.StartAndAttachContainerExecAsync(execCreateResponse.ID, false, cancellationToken);
		if (multiplexedStreamNullable is null)
		{
			yield break;
		}
		var multiplexedStream = multiplexedStreamNullable;

		// We'll use a buffer to hold incoming data and build complete lines.
		var buffer = new byte[8192];
		var lineBuilder = new StringBuilder();

		while (!cancellationToken.IsCancellationRequested)
		{
			var result = await multiplexedStream.ReadOutputAsync(buffer, 0, buffer.Length, cancellationToken);
			if (result.EOF)
			{
				// Flush any remaining text.
				if (lineBuilder.Length > 0)
				{
					yield return lineBuilder.ToString();
					lineBuilder.Clear();
				}
				break;
			}

			// Convert bytes to text.
			var textChunk = Encoding.UTF8.GetString(buffer, 0, result.Count);

			if (result.Target == MultiplexedStream.TargetStream.StandardOut)
			{
				// Append and split into complete lines.
				lineBuilder.Append(textChunk);
				var allText = lineBuilder.ToString();
				int newLineIndex;
				while ((newLineIndex = allText.IndexOf('\n')) >= 0)
				{
					var line = allText[..newLineIndex].TrimEnd('\r');
					yield return line;
					allText = allText[(newLineIndex + 1)..];
				}
				lineBuilder.Clear();
				lineBuilder.Append(allText);
			}
			else if (result.Target == MultiplexedStream.TargetStream.StandardError)
			{
				// Log stderr output immediately.
				var errorText = Encoding.UTF8.GetString(buffer, 0, result.Count);
				_logger.LogError("Docker Exec Error: {ErrorText}", errorText);
			}
		}
	}
}
