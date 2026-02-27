namespace LightNovelCore.Services;

/// <summary>
/// Dependency injection extensions
/// </summary>
public static class DiExtensions
{
	/// <summary>
	/// Adds the LNC services to the specified service collection
	/// </summary>
	/// <param name="services">The service collection to append to</param>
	/// <returns>The service collection for fluent method chaining</returns>
	public static IServiceCollection AddLncServices(this IServiceCollection services)
	{
		return services
			.AddTransient<IDataRefreshService, DataRefreshService>()
			.AddTransient<ICalendarService, CalendarService>();
	}
}
