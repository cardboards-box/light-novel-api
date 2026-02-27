namespace LightNovelCore.Cli.Verbs;

using Database;
using Models.Composites;
using Models.Types;

[Verb("test", HelpText = "Run tests.")]
internal class TestOptions
{
	[Value(0, Required = true, HelpText = "The test method to run.")]
	public string Method { get; set; } = string.Empty;
}

internal class TestVerb(
	IDbService _db,
	ILogger<TestVerb> logger) : BooleanVerb<TestOptions>(logger)
{
	private static readonly JsonSerializerOptions _options = new()
	{
		WriteIndented = true,
		AllowTrailingCommas = true,
	};

	public static string Serialize<T>(T item)
	{
		return JsonSerializer.Serialize(item, _options);
	}

	public async Task Search()
	{
		var filter = new SearchFilter
		{
			Start = DateTime.Now.AddMonths(-1),
			End = DateTime.Now.AddMonths(1),
			Publisher = [Guid.Parse("ac0ab6e9-1c9b-46e6-a487-454c34a632b8"), Guid.Parse("58c437fe-1174-4071-b503-a2c76c1719d9")],
			Released = true,
			Format = [LncFormat.Physical, LncFormat.Digital],
			Search = "Villainess",
			Isbn = ["9798855407150", "9798895618684", "9798895614532"],
			Asc = true,
			Size = 100,
			Page = 1,
		};
		var search = await _db.Publication.Search(filter, false);
		_logger.LogInformation("Search Results: {Search}", Serialize(search));
	}

	public override async Task<bool> Execute(TestOptions options, CancellationToken token)
	{
		var methods = GetType().GetMethods();
		var method = methods.FirstOrDefault(t => t.Name.EqualsIc(options.Method));

		if (method is null)
		{
			_logger.LogError("The method {Method} does not exist", options.Method);
			return false;
		}

		object[] parameters = method.GetParameters().Length <= 0 ? [] : [token];
		var result = method.Invoke(this, parameters);
		if (result is null) { }
		else if (result is Task task)
			await task;
		else if (result is ValueTask vTask)
			await vTask;

		_logger.LogInformation("Method execution complete");
		return true;
	}
}
