using System;
using System.Reflection;
using ApiExceptionHandler.DependencyInjection;
using ConfigurationEncrypt;
using Figgle.Fonts;
using FileServerApi.Endpoints.V1;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Serilog;
using SerilogLogger;
using StaticFilesTools.DependencyInjection;
using SwaggerTools.DependencyInjection;
using TestToolsApi.DependencyInjection;
using WindowsServiceTools;

try
{
    Console.WriteLine("Loading...");

    const string appName = "File Server";
    const int versionCount = 1;

    var header = $"{appName} {Assembly.GetEntryAssembly()?.GetName().Version}";
    Console.WriteLine(FiggleFonts.Standard.Render(header));

    var builder = WebApplication.CreateBuilder(new WebApplicationOptions
    {
        ContentRootPath = AppContext.BaseDirectory, Args = args
    });

    var debugMode = builder.Environment.IsDevelopment();

    builder.Host.UseSerilogLogger(builder.Configuration, debugMode);
    builder.Host.UseWindowsServiceOnWindows(debugMode, args);

    builder.Configuration.AddConfigurationEncryption(debugMode, "29ab6e4bcd1a40d8a37ad141d59a575e");

    // @formatter:off
    builder.Services
        //WebSystemTools
        .AddSwagger(debugMode, true, versionCount, appName)
        .AddFileServer(builder.WebHost, debugMode);
    // @formatter:on

    //ReSharper disable once using

    using var app = builder.Build();

    //WebSystemTools
    // ReSharper disable once RedundantArgumentDefaultValue
    app.UseSwaggerServices(debugMode, versionCount);
    app.UseApiExceptionHandler(debugMode);
    app.UseDefaultAndStaticFiles(debugMode);

    app.UseTestToolsApiEndpoints(debugMode);
    app.UseFileServerEndpoints(debugMode);
    app.UseAntiforgery();

    app.Run();
    return 0;
}
catch (Exception e)
{
    Log.Fatal(e, "Host terminated unexpectedly");
    return 1;
}
finally
{
    Log.CloseAndFlush();
}