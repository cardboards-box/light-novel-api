namespace LightNovelCore.Services;

using CalRes = CalendarResults<LncEntity<LncPublication>>;

/// <summary>
/// A service for handling calendar searches
/// </summary>
public interface ICalendarService
{
	/// <summary>
	/// Searches by the given filter and returns the calendar entries
	/// </summary>
	/// <param name="filter">The filter to search by</param>
	/// <param name="start">The start date of the calendar range</param>
	/// <param name="end">The end date of the calendar range</param>
	/// <param name="chunk">The size of each calendar chunk</param>
	/// <returns>The calendar results</returns>
	Task<CalRes> Calendarize(SearchFilter filter, DateOnly start, DateOnly end, int chunk);

	/// <summary>
	/// Gets a weeks worth of data surrounding the given date
	/// </summary>
	/// <param name="filter">The filter to search by</param>
	/// <param name="date">The date to get the week for</param>
	/// <returns>The calendar results for the week</returns>
	Task<CalRes> CalendarizeWeek(SearchFilter filter, DateOnly date);

	/// <summary>
	/// Gets a months worth of data surrounding the given date
	/// </summary>
	/// <param name="filter">The filter to search by</param>
	/// <param name="date">The date to get the month for</param>
	/// <returns>The calendar results for the month</returns>
	Task<CalRes> CalendarizeMonth(SearchFilter filter, DateOnly date);
}

internal class CalendarService(IDbService _db) : ICalendarService
{
	public static IEnumerable<CalendarChunk> Chunk(
		Dictionary<DateOnly, List<Guid>> source,
		DateOnly start, DateOnly end, int chunk)
	{
		var current = start;
		var days = new List<CalendarDay>();

		while (current <= end)
		{
			var items = source.TryGetValue(current, out var list)
				? list.ToArray() : [];
			days.Add(new(current, items));
			current = current.AddDays(1);

			if (days.Count < chunk) continue;

			yield return new(
				days.First().Date, 
				days.Last().Date, 
				[.. days]);
			days.Clear();
		}

		if (days.Count == 0) yield break;

		var chunkStart = days.First().Date;
		if (days.Count == chunk)
		{
			yield return new(chunkStart, end, [.. days]);
			yield break;
		}

		current = chunkStart.AddDays(days.Count);
		while(current <= end)
			days.Add(new(current, []));

		yield return new(chunkStart, end, [.. days]);
	}

	public static CalendarResults<T> Calendarize<T>(
		IEnumerable<T> source, Func<T, Guid> id, Func<T, DateTime> date,
		int chunk, DateOnly start, DateOnly end)
	{
		var entities = new Dictionary<Guid, T>();
		var sortedSource = new Dictionary<DateOnly, List<Guid>>();

		foreach (var item in source)
		{
			var itemId = id(item);
			entities.TryAdd(itemId, item);
			var datekey = DateOnly.FromDateTime(date(item));
			if (!sortedSource.ContainsKey(datekey))
				sortedSource[datekey] = [];
			sortedSource[datekey].Add(itemId);
		}

		var chunks = Chunk(sortedSource, start, end, chunk).ToArray();
		return new(start, end, chunk, entities, chunks);
	}

	public async Task<CalRes> Calendarize(SearchFilter filter, DateOnly start, DateOnly end, int chunk)
	{
		filter.Start = start.ToDateTime(TimeOnly.MinValue);
		filter.End = end.ToDateTime(TimeOnly.MaxValue);

		var search = await _db.Publication.Search(filter, true);
		return Calendarize(
			search.Results, 
			item => item.Entity.Id, 
			item => item.Entity.ReleaseDate, 
			chunk, start, end);
	}

	public Task<CalRes> CalendarizeWeek(SearchFilter filter, DateOnly date)
	{
		var start = date.StartOfWeek();
		var end = start.EndOfWeek();
		return Calendarize(filter, start, end, 7);
	}

	public Task<CalRes> CalendarizeMonth(SearchFilter filter, DateOnly date)
	{
		var start = date.StartOfMonth().StartOfWeek();
		var end = start.EndOfMonth().EndOfWeek();
		return Calendarize(filter, start, end, 7);
	}
}
