using Serilog;
using HexagonalArchitecture.Api.Modules;

try
{
    var builder = WebApplication.CreateBuilder(args);

    HexagonalArchitectureApiModule.ConfigureServices(builder);

    var app = builder.Build();

    HexagonalArchitectureApiModule.ConfigurePipeline(app);

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
