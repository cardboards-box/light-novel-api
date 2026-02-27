namespace LightNovelCore.Api.Middleware.ScheduledTasks;

/// <summary>
/// A scheduled task for refreshing novel data
/// </summary>
public class RefreshData(
	IDataRefreshService _refresh,
	ILogger<RefreshData> _logger) : ICancellableInvocable, IInvocable
{
	/// <inheritdoc />
	public CancellationToken CancellationToken { get; set; } = CancellationToken.None;

	/// <inheritdoc />
	public async Task Invoke()
	{
		try
		{
			_logger.LogInformation("Starting refresh of data");
			await _refresh.Load(CancellationToken);
			_logger.LogInformation("Finished refreshing data");
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "An error occurred while refreshing data");
		}
	}
}
