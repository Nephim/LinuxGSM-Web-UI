namespace LinuxGSM.Web.UI.Extensions;

using System.Diagnostics.CodeAnalysis;
using Docker.DotNet;
using Docker.DotNet.Models;

/// <summary>
/// Configures the Docker client for the application.  Attempts to connect to the Docker daemon
/// using default locations and falls back to the DockerHost environment variable if provided.
/// Registers the IDockerClient as a singleton in the dependency injection container.
/// </summary>
public static class DockerSocketSetup
{
	/// <summary>
	/// Configures the Docker client and registers it with dependency injection.
	/// </summary>
	/// <param name="builder">The WebApplicationBuilder instance.</param>
	/// <returns>The updated WebApplicationBuilder instance.</returns>
	[SuppressMessage("Performance", "CA1848:Use the LoggerMessage delegates", Justification = "<Pending>")]
	public static async Task<WebApplicationBuilder> ConfigureDocker(this WebApplicationBuilder builder)
	{
		var loggerFactory = builder.Services.BuildServiceProvider().GetRequiredService<ILoggerFactory>();
		var logger = loggerFactory.CreateLogger("DockerConfiguration");
		var dockerHost = builder.Configuration["DockerHost"];

		if (string.IsNullOrEmpty(dockerHost))
		{
			logger.LogInformation("No DockerHost env variable provided. Falling back to default implementation.");
		}

		// Try default locations
		string[] defaultLocations = [
			"unix:///var/run/docker.sock",  // Linux
            "npipe://./pipe/docker_engine", // Windows (Docker Desktop) - Adjust if needed
            ];

		foreach (var location in defaultLocations)
		{
			try
			{
				var client = new DockerClientConfiguration(new Uri(location)).CreateClient();

				try
				{
					await client.Containers.ListContainersAsync(new ContainersListParameters()); // Simple test
					dockerHost = location; // Success!
					logger.LogInformation("Connected to Docker daemon at {Location}", location);
					break; // Exit the loop if a connection is successful
				}
				catch
				{
					logger.LogInformation("Failed to connect to Docker daemon at {Location}", location);
				}

			}
			catch (Exception ex)
			{
				logger.LogInformation(ex, "Error creating Docker client for {Location}", location);
			}
		}

		if (string.IsNullOrEmpty(dockerHost))
		{
			logger.LogInformation("Could not connect to Docker daemon at any of the default locations.");
			throw new ArgumentException("Could not connect to Docker daemon at any of the default locations and no explicit location was provided. Use DockerHost environment variable.");
		}

		builder.Services.AddSingleton<IDockerClient>(sp => new DockerClientConfiguration(new Uri(dockerHost)).CreateClient());

		return builder;
	}
}
