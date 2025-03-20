//Created by ApiProgramClassCreator at 11/24/2024 11:14:23 PM

using System;
using System.Collections.Generic;
using ConfigurationEncrypt;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Serilog;
using SwaggerTools;
using WebInstallers;
using AssemblyReference = ApiExceptionHandler.AssemblyReference;

try
{
    var parameters = new Dictionary<string, string>
    {
        { ConfigurationEncryptInstaller.AppKeyKey, "29ab6e4bcd1a40d8a37ad141d59a575e" },
        { SwaggerInstaller.AppNameKey, "File Server" },
        { SwaggerInstaller.VersionCountKey, 1.ToString() },
        { SwaggerInstaller.UseSwaggerWithJwtBearerKey, string.Empty } //Allow Swagger
    };

    var builder = WebApplication.CreateBuilder(new WebApplicationOptions
    {
        ContentRootPath = AppContext.BaseDirectory, Args = args
    });

    var debugMode = builder.Environment.IsDevelopment();

    if (!builder.InstallServices(debugMode, args, parameters,

//WebSystemTools
            AssemblyReference.Assembly, ConfigurationEncrypt.AssemblyReference.Assembly,
            SerilogLogger.AssemblyReference.Assembly, StaticFilesTools.AssemblyReference.Assembly,
            SwaggerTools.AssemblyReference.Assembly, TestToolsApi.AssemblyReference.Assembly,
            WindowsServiceTools.AssemblyReference.Assembly,

//FileServer
            FileServerApi.AssemblyReference.Assembly))
        return 2;

    //ReSharper disable once using

    using var app = builder.Build();

    if (!app.UseServices(debugMode)) return 1;

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