using System.Threading.RateLimiting;

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
			.AddTransient<ICalendarService, CalendarService>()
			.AddTransient<IHttpService, HttpService>()
			.AddTransient<ICoverCacheService, CoverCacheService>()
			
			.AddKeyedSingleton<RateLimiter>(CoverCacheService.LIMITER_KEY, (s, _) => 
			{
				var config = s.GetRequiredService<IConfiguration>();
				var tokens = int.TryParse(config["Imaging:TokensPerPeriod"], out var tkn) ? tkn : 30;
				var period = double.TryParse(config["Imaging:PeriodSeconds"], out var sec) ? sec : 5;

				return new TokenBucketRateLimiter(new()
				{
					TokenLimit = tokens,
					TokensPerPeriod = tokens,
					QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
					QueueLimit = int.MaxValue,
					ReplenishmentPeriod = TimeSpan.FromSeconds(period),
					AutoReplenishment = true
				});
			});
	}
}
