namespace LightNovelCore.Api.Middleware.ScheduledTasks;

/// <summary>
/// Registers the scheduled tasks for the application
/// </summary>
public static class ScheduleExtensions
{
	/// <summary>
	/// Registers the services for the scheduled tasks in the application
	/// </summary>
	/// <param name="services">The service collection to add the scheduled tasks to</param>
	/// <returns>The updated service collection</returns>
	public static IServiceCollection AddScheduledTasks(this IServiceCollection services)
	{
		return services
			.AddScheduler()
			.AddTransient<RefreshData>();
	}

	/// <summary>
	/// Adds the scheduled tasks to the application
	/// </summary>
	/// <param name="app">The web applciation to add the scheduled tasks to</param>
	/// <returns>The updated web application</returns>
	public static WebApplication AddScheduledTasks(this WebApplication app)
	{
		app.Services.UseScheduler(schedule =>
		{
			schedule.Schedule<RefreshData>()
				.EveryThirtyMinutes()
				.PreventOverlapping(nameof(RefreshData));
		});
		return app;
	}
}
