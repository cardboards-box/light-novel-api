namespace LightNovelCore.Database;

using Models;
using Models.Types;
using Services;

/// <summary>
/// Extensions for dependency injection
/// </summary>
public static class DiExtensions
{
	private static int _initialized;

	/// <summary>
	/// Adds the database services
	/// </summary>
	/// <param name="resolver">The resolver to add to</param>
	/// <returns>The resolver fluent method chaining</returns>
	public static IDependencyResolver AddDatabase(this IDependencyResolver resolver)
	{
		if (Interlocked.Exchange(ref _initialized, 1) == 1)
			return resolver; // already initialized

		DefaultTypeMap.MatchNamesWithUnderscores = true;

		return resolver
			.Logger(c =>
			{
				//c.WriteTo.Sink(new DbLoggerSink(), Serilog.Events.LogEventLevel.Information)
				// .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning);
			})

			.SetDefaultConvention(t => t.CaseConvention<NoChangeConvention>())

			.Transient<IDbService, DbService>()
			.Add<ILncCoverDbService, LncCoverDbService, LncCover>()
			.Add<ILncSeriesDbService, LncSeriesDbService, LncSeries>()
			.Add<ILncVolumeDbService, LncVolumeDbService, LncVolume>()
			.Add<ILncPublisherDbService, LncPublisherDbService, LncPublisher>()
			.Add<ILncPublicationDbService, LncPublicationDbService, LncPublication>()
			.Add<ILncNovelStagingDbService, LncNovelStagingDbService, LncNovelStaging>()

			.Mapping(c => c
				.Enum<LncFormat>()
				.PolyfillGuidArrays());
	}

	private static IDependencyResolver AddType<TModel>(this IDependencyResolver resolver, string type)
		where TModel : class, IDbType
	{
		return resolver
			.Type<TModel>(type)
			.Transient<IDbModel, TModel>();
	}

	private static IDependencyResolver AddType<TModel>(this IDependencyResolver resolver)
		where TModel : class, IDbType
	{
		var type = typeof(TModel).GetCustomAttribute<TypeAttribute>()?.Name;
		if (string.IsNullOrEmpty(type))
			throw new InvalidOperationException($"Type attribute not found on type {typeof(TModel).FullName}");

		return resolver.AddType<TModel>(type);
	}

	private static IDependencyResolver Add<TInterface, TConcrete, TModel>(this IDependencyResolver resolver)
		where TInterface : class
		where TConcrete : class, TInterface
		where TModel : DbObject, IDbTable
	{
		return resolver
			.Model<TModel>()
			.Transient<IDbModel, TModel>()
			.Transient<TInterface, TConcrete>();
	}

	private static ITypeMapBuilder Enum<T>(this ITypeMapBuilder builder)
		where T : struct, Enum
	{
		return builder.TypeHandler<T, EnumHandler<T>>();
	}

	/// <summary>
	/// This can be used to register models with the DI system so the `GenerateDatabaseScriptsVerb` or `GenerateOrmClassesVerb` can be run on them.
	/// However, this should be replaced with a call to <see cref="Add{TInterface, TConcrete, TModel}(IDependencyResolver)"/> once the ORM services are created.
	/// </summary>
	/// <typeparam name="T">The type of the table</typeparam>
	/// <param name="resolver">The dependency resolver</param>
	/// <returns>The dependency resolver for fluent method chaining</returns>
	[Obsolete("This is just a temporary solution to including models in the DI system, " +
		"for context generation. Be sure to remove references to this after generation.")]
	private static IDependencyResolver TempTable<T>(this IDependencyResolver resolver)
		where T : DbObject, IDbTable
	{
		return resolver.Transient<IDbModel, T>();
	}
}
