using LightNovelCore.All;
using LightNovelCore.Cli.Verbs;
using LightNovelCore.Database.Generation;

var services = new ServiceCollection()
	.AddConfig(c => c
		.AddFile("appsettings.json")
		.AddUserSecrets<Program>(), out var config)
	.AddDatabaseGeneration();

await services.AddLnc(config);

return await services.Cli(args, c => c
	.Add<InitVerb>()
	.Add<LoadVerb>()
	.Add<TestVerb>()
	.AddDatabaseGeneration());