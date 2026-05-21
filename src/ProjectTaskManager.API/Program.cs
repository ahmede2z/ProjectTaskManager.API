using ProjectTaskManager.Application;
using ProjectTaskManager.Infrastructure;
using ProjectTaskManager.API;
using Serilog;

// Two-stage init: bootstrap logger captures startup failures before host configuration runs
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting ProjectTaskManager.API");

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext());

    builder.Services
        .AddApplication()
        .AddInfrastructure(builder.Configuration)
        .AddJwtAuthentication(builder.Configuration)
        .AddPresentation(builder.Configuration);

    var app = builder.Build();

    app.UseExceptionHandler();
    app.UseSerilogRequestLogging();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapGet("/health", () => Results.Ok(new { status = "ok" }));
    app.MapControllers();

    app.Run();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "ProjectTaskManager.API terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
