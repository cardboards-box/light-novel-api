using LightNovelCore.All;

var builder = WebApplication.CreateBuilder(args);

#if DEBUG
var appFile = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
builder.Configuration.AddJsonFile(appFile, false, true);
#endif

builder.Configuration.AddUserSecrets<Program>();

builder.Services.AddControllers()
	.AddJsonOptions(opts =>
	{
		opts.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
		opts.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
		opts.JsonSerializerOptions.AllowTrailingCommas = true;
		opts.JsonSerializerOptions.NumberHandling = JsonNumberHandling.AllowReadingFromString;
	});

builder.Services.AddOpenApi()
	.AddEndpointsApiExplorer()
	.AddScheduledTasks()
	.AddTelemetry()
	.AddCustomSwaggerGen();

await builder.Services.AddLnc(builder.Configuration);

var app = builder.Build();

app.RegisterBoxing();
app.AddScheduledTasks();

if (app.Environment.IsDevelopment() ||
	builder.Configuration[Constants.APPLICATION_NAME + ":EnableSwagger"]?.ToLower() == "true")
{
	app.MapOpenApi();
	app.UseSwagger();
	app.UseSwaggerUI(c =>
	{
		c.SwaggerEndpoint("/swagger/v1/swagger.json", "MangaBox V1");
	});
}

app.UseCors(c => c
   .AllowAnyHeader()
   .AllowAnyMethod()
   .AllowAnyOrigin()
   .WithExposedHeaders("Content-Disposition"));

app.UseAuthorization();

app.MapPrometheusScrapingEndpoint();
app.MapControllers();
app.UseResponseCaching();

app.Run();
