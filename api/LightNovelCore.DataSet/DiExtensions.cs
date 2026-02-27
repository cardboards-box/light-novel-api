namespace LightNovelCore.DataSet;

/// <summary>
/// Dependency injection extensions
/// </summary>
public static class DiExtensions
{
	/// <summary>
	/// Adds the LnCoreDataset services to the specified service collection
	/// </summary>
	/// <param name="services">The service collection to append to</param>
	/// <returns>The service collection for fluent method chaining</returns>
	public static IServiceCollection AddLnCoreDataset(this IServiceCollection services)
	{
		return services
			.AddTransient<INovelApiService, NovelApiService>()
			.AddTransient<ICoverApiService, CoverApiService>();
	}
}
