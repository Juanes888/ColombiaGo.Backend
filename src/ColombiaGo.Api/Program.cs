using System.Text.Json;
using System.Text.Json.Serialization;
using ColombiaGo.Api.Extensions;
using ColombiaGo.Api.Middleware;
using ColombiaGo.Application;
using ColombiaGo.Infrastructure;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext());

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        // Enums travel over the wire as their names (e.g. "aprobado"), not raw integers —
        // readable payloads and a stable contract even if enum member order ever changes.
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
    });

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddSwaggerDocumentation();
builder.Services.AddFrontendCors(builder.Configuration);
builder.Services.AddApiHealthChecks(builder.Configuration);

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/swagger/v1/swagger.json", "ColombiaGO API v1"));
}

app.UseHttpsRedirection();

app.UseCors(CorsExtensions.FrontendPolicy);

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();

/// <summary>Exposed for WebApplicationFactory-based integration tests.</summary>
public partial class Program
{
}
