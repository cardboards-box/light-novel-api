using CardboardBox.Database.Postgres.Standard;
using CardboardBox.Http;
using CardboardBox.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Net;

namespace LightNovelCore.All;

using Core;
using Database;
using DataSet;
using Services;

/// <summary>
/// Dependency injection services
/// </summary>
public static class DiExtensions
{
	/// <summary>
	/// Adds all of the services to the service collection
	/// </summary>
	/// <param name="services">The service collection to add to</param>
	/// <param name="config">The configuration to use</param>
	public static async Task AddLnc(this IServiceCollection services, IConfiguration config)
	{
		services
			.AddJson()
			.AddLncServices()
			.AddCoreServices()
			.AddCardboardHttp()
			.AddLnCoreDataset()
			.ConfigureHttpClientDefaults(c =>
			{
				c.ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
				{
					AutomaticDecompression = DecompressionMethods.All
				});
			});

		await services.AddServices(config, c => c.AddDatabase());
	}
}
