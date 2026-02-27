namespace LightNovelCore.Models.Composites;

/// <summary>
/// Represents the results of a calendar'd query
/// </summary>
/// <typeparam name="T">The type of entities in the calendar</typeparam>
/// <param name="Start">The start date of the calendar range</param>
/// <param name="End">The end date of the calendar range</param>
/// <param name="Chunk">The chunking factor for dividing the calendar range</param>
/// <param name="Entities">The entities that are included in the calendar</param>
/// <param name="Entries">The chunks in the calendar range</param>
public record class CalendarResults<T>(
	[property: JsonPropertyName("start")] DateOnly Start,
	[property: JsonPropertyName("end")] DateOnly End,
	[property: JsonPropertyName("chunk")] int Chunk,
	[property: JsonPropertyName("publications")] Dictionary<Guid, T> Entities,
	[property: JsonPropertyName("entries")] CalendarChunk[] Entries);

/// <summary>
/// A chunk of the calendar results
/// </summary>
/// <param name="Start">The start date of the calendar range</param>
/// <param name="End">The end date of the calendar range</param>
/// <param name="Entries">The entities in the calendar range</param>
public record class CalendarChunk(
	[property: JsonPropertyName("start")] DateOnly Start,
	[property: JsonPropertyName("end")] DateOnly End,
	[property: JsonPropertyName("entries")] CalendarDay[] Entries);

/// <summary>
/// Represents a day on a calendar
/// </summary>
/// <param name="Date">The date of the calendar entry</param>
/// <param name="Entities">The entities for this day</param>
public record class CalendarDay(
	[property: JsonPropertyName("date")] DateOnly Date,
	[property: JsonPropertyName("entities")] Guid[] Entities);
