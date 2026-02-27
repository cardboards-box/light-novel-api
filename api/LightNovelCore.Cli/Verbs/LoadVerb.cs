using CardboardBox.Json;

namespace LightNovelCore.Cli.Verbs;

using Services;

[Verb("load", HelpText = "Load a novels from the API")]
internal class LoadOptions
{

}

internal class LoadVerb(
	IJsonService _json,
	ILogger<LoadVerb> logger,
	IDataRefreshService _refresh) : BooleanVerb<LoadOptions>(logger)
{
	public override async Task<bool> Execute(LoadOptions options, CancellationToken token)
	{
		var result = await _refresh.Load(token);
		_logger.LogInformation("Finished - {Code} - {Success}", result.Code, result.Success);
		using var io = File.Create("results.json");
		await _json.Serialize(result, io, token);
		return result.Success;
	}
}
