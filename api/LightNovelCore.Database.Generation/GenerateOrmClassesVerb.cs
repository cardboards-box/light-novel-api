namespace LightNovelCore.Database.Generation;

using Services;

/// <summary>
/// Options for generating ORM classes
/// </summary>
[Verb("generate-orm-classes", HelpText = "Generate ORM classes for the database")]
public class GenerateOrmClassesOptions
{
    /// <summary>
    /// The default directory to generate the ORM classes in
    /// </summary>
    public const string DEFAULT_DIRECTORY = "Generated\\OrmServices";

    /// <summary>
    /// The default namespace to use for the generated ORM classes
    /// </summary>
    public const string DEFAULT_NAMESAPCE = Constants.APPLICATION_NAME + ".Database.Services";

    /// <summary>
    /// The default application name to use as the type prefix
    /// </summary>
    public const string DEFAULT_APPNAME = Constants.APPLICATION_NAME;

    /// <summary>
    /// The directory to generate the ORM classes in
    /// </summary>
    [Option('d', "directory", HelpText = "The directory to generate the ORM classes in", Default = DEFAULT_DIRECTORY)]
    public string Directory { get; set; } = DEFAULT_DIRECTORY;

    /// <summary>
    /// The namespace to use for the generated ORM classes
    /// </summary>
    [Option('n', "namespace", HelpText = "The namespace to use for the generated ORM classes", Default = DEFAULT_NAMESAPCE)]
    public string Namespace { get; set; } = DEFAULT_NAMESAPCE;

    /// <summary>
    /// The version of the application to generate the scripts for (default includes all columns)
    /// </summary>
    [Option('v', "as-of-version", HelpText = " The version of the application to generate the scripts for (default includes all columns)")]
    public int? AsOfVersion { get; set; }

    /// <summary>
    /// The application name to use for type prefixes
    /// </summary>
    [Option('a', "app-name", HelpText = "The application name to use as the type prefix", Default = DEFAULT_APPNAME)]
    public string AppName { get; set; } = DEFAULT_APPNAME;
}

internal class GenerateOrmClassesVerb(
    ILogger<GenerateOrmClassesVerb> logger,
    IDatabaseMetadataService _metadata,
    IFileGenerationService _generation) : BooleanVerb<GenerateOrmClassesOptions>(logger)
{
    public static string GetPrefix(GenerateOrmClassesOptions options)
    {
        var acronym = new string([..options.AppName.Where(char.IsUpper)]);
        if (string.IsNullOrEmpty(acronym)) return options.AppName;

        var fc = acronym[0];
        return char.ToUpper(fc) + acronym[1..].ToLower();
	}

    public override async Task<bool> Execute(GenerateOrmClassesOptions options, CancellationToken token)
    {
        var directory = Path.GetFullPath(options.Directory);
        if (string.IsNullOrEmpty(directory))
        {
            _logger.LogError("Invalid directory specified");
            return false;
        }

        if (!Directory.Exists(directory))
        {
            _logger.LogInformation("Creating ORM services directory: {dir}", directory);
            Directory.CreateDirectory(directory);
        }

        var prefix = GetPrefix(options);
        var asOf = options.AsOfVersion ?? int.MaxValue;
        var entities = _metadata.GetEntities(directory, directory, directory, null);

        foreach (var table in entities.Tables)
        {
            _logger.LogInformation("Writing ORM service: {table}", table.Type.Name);
            var path = Path.Combine(directory, $"{table.Type.Name}DbService.cs");
            await using var writer = new StreamWriter(path);
            await _generation.OrmClass(table, writer, asOf, 
                options.Namespace, entities, prefix);
        }

        var dbServices = Path.Combine(directory, "DbServices.cs");
        await using var dbWriter = new StreamWriter(dbServices);
		await _generation.DbServiceClass(entities, dbWriter, options.Namespace, prefix);

        var extPath = Path.Combine(directory, "DiExtensions.cs");
        await using var extWriter = new StreamWriter(extPath);
        await _generation.ResolverExtensions(entities, extWriter);

        _logger.LogInformation("Finished creating ORM classes");
        return true;
    }
}
