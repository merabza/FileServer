using System;
using System.Reflection;
using Figgle.Fonts;
using FileServerApi.Endpoints.V1;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Serilog;
using WebSystemTools.ApiExceptionHandler.DependencyInjection;
using WebSystemTools.ConfigurationEncrypt;
using WebSystemTools.SerilogLogger;
using WebSystemTools.StaticFilesTools.DependencyInjection;
using WebSystemTools.SwaggerTools.DependencyInjection;
using WebSystemTools.TestToolsApi.DependencyInjection;
using WebSystemTools.WindowsServiceTools;

try
{
    Console.WriteLine("Loading...");

    const string appName = "File Server";
    const string appKey = "29ab6e4bcd1a40d8a37ad141d59a575e";
    const int versionCount = 1;

    string header = $"{appName} {Assembly.GetEntryAssembly()?.GetName().Version}";
    Console.WriteLine(FiggleFonts.Standard.Render(header));

    var builder = WebApplication.CreateBuilder(new WebApplicationOptions
    {
        ContentRootPath = AppContext.BaseDirectory, Args = args
    });

    bool debugMode = builder.Environment.IsDevelopment();

    var logger = builder.Host.UseSerilogLogger(debugMode, builder.Configuration);
    ILogger? debugLogger = debugMode ? logger : null;
    builder.Host.UseWindowsServiceOnWindows(debugLogger, args);

    builder.Configuration.AddConfigurationEncryption(debugLogger, appKey);

    // @formatter:off
    builder.Services
        //WebSystemTools
        .AddSwagger(debugLogger, true, versionCount, appName)
        .AddFileServer(debugLogger, builder.WebHost);
    // @formatter:on

    //ReSharper disable once using

    await using var app = builder.Build();

    //WebSystemTools
    // ReSharper disable once RedundantArgumentDefaultValue
    app.UseSwaggerServices(debugLogger, versionCount);
    app.UseApiExceptionHandler(debugLogger);
    app.UseDefaultAndStaticFiles(debugLogger);

    app.UseTestToolsApiEndpoints(debugLogger);
    app.UseFileServerEndpoints(debugLogger);
    app.UseAntiforgery();

    await app.RunAsync();
    return 0;
}
catch (Exception e)
{
    Log.Fatal(e, "Host terminated unexpectedly");
    return 1;
}
finally
{
    await Log.CloseAndFlushAsync();
}
