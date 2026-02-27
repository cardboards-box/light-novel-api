namespace LightNovelCore.Api.Middleware.ScheduledTasks;

/// <summary>
/// A scheduled task for caching missing cover images
/// </summary>
public class CacheCovers(
	ICoverCacheService _cache,
	ILogger<CacheCovers> _logger) : IInvocable, ICancellableInvocable
{
	/// <inheritdoc />
	public CancellationToken CancellationToken { get; set; } = CancellationToken.None;

	/// <inheritdoc />
	public async Task Invoke()
	{
		try
		{
			_logger.LogInformation("Starting caching missing covers");
			await _cache.ProcessMissing(CancellationToken);
			_logger.LogInformation("Finished caching missing covers");
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "An error occurred while caching missing covers");
		}
	}
}
