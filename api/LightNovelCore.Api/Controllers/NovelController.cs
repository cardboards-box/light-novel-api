namespace LightNovelCore.Api.Controllers;

/// <summary>
/// The controller for novel endpoints
/// </summary>
public class NovelController(
	IDbService _db,
	ICalendarService _calendar,
	IDataRefreshService _refresh,
	ILogger<NovelController> logger) : BaseController(logger)
{
	/// <summary>
	/// Searches for novels based on the given filters
	/// </summary>
	/// <param name="filter">The filter to search with</param>
	/// <returns>The paginated search results</returns>
	[HttpPost, Route("novels")]
	[ProducesPaged<LncEntity<LncPublication>>]
	public Task<IActionResult> Search([FromBody] SearchFilter filter) => Box(async () =>
	{
		var results = await _db.Publication.Search(filter, false);
		return Boxed.Ok(results.Pages, results.Count, results.Results);
	});

	/// <summary>
	/// Searches for novels based on the given filters
	/// </summary>
	/// <param name="filter">The filter to search with</param>
	/// <returns>The paginated search results</returns>
	[HttpGet, Route("novels")]
	[ProducesPaged<LncEntity<LncPublication>>]
	public Task<IActionResult> SearchQuery([FromQuery] SearchFilter filter) => Search(filter);

	/// <summary>
	/// Returns a weeks worth of novels centered around the given date
	/// </summary>
	/// <param name="date">The date to search by</param>
	/// <param name="filter">The filter to search by</param>
	/// <returns>The calendar results for the week</returns>
	[HttpGet, Route("novels/calendar/{date}/week")]
	[ProducesBox<CalendarResults<LncEntity<LncPublication>>>, ProducesError]
	public Task<IActionResult> CalendarWeek([FromRoute] DateOnly date, [FromQuery] SearchFilter filter) => Box(async () =>
	{
		var results = await _calendar.CalendarizeWeek(filter, date);
		return Boxed.Ok(results);
	});

	/// <summary>
	/// Returns a months worth of novels centered around the given date
	/// </summary>
	/// <param name="date">The date to search by</param>
	/// <param name="filter">The filter to search by</param>
	/// <returns>The calendar results for the month</returns>
	[HttpGet, Route("novels/calendar/{date}/month")]
	[ProducesBox<CalendarResults<LncEntity<LncPublication>>>, ProducesError]
	public Task<IActionResult> CalendarMonth([FromRoute] DateOnly date, [FromQuery] SearchFilter filter) => Box(async () =>
	{
		var results = await _calendar.CalendarizeMonth(filter, date);
		return Boxed.Ok(results);
	});

	/// <summary>
	/// Refreshes the current novel data
	/// </summary>
	/// <param name="token">The cancellation token for the request</param>
	/// <returns>The results of the refresh</returns>
	[HttpGet, Route("novels/refresh")]
	[ProducesBox<SeriesUpsertResult>, ProducesError]
	public Task<IActionResult> Refresh(CancellationToken token) => Box(() => _refresh.Load(token));
}
