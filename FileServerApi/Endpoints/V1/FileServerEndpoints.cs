using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using FileServerApi.V1.Routes;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WebInstallers;

namespace FileServerApi.Endpoints.V1;

// ReSharper disable once UnusedType.Global
public sealed class FileServerEndpoints : IInstaller
{
    public int InstallPriority => 70;
    public int ServiceUsePriority => 70;

    public bool InstallServices(WebApplicationBuilder builder, bool debugMode, string[] args,
        Dictionary<string, string> parameters)
    {
        builder.WebHost.ConfigureKestrel((context, options) =>
        {
            options.Limits.MaxRequestBodySize =
                context.Configuration.GetValue<long>("Kestrel:Limits:MaxRequestBodySize");
        });

        builder.Services.Configure<FormOptions>(options => { options.MultipartBodyLengthLimit = 5545902080; });

        builder.Services.AddAntiforgery();
        return true;
    }

    public bool UseServices(WebApplication app, bool debugMode)
    {
        if (debugMode)
            Console.WriteLine($"{GetType().Name}.{nameof(UseServices)} Started");

        var downloadGroup = app.MapGroup(FileServerApiRoutes.ApiBase + FileServerApiRoutes.Download.DownloadBase);

        downloadGroup.MapGet(FileServerApiRoutes.Download.File, DownloadFile);

        var uploadGroup = app.MapGroup(FileServerApiRoutes.ApiBase + FileServerApiRoutes.Upload.UploadBase);

        uploadGroup.MapPost(FileServerApiRoutes.Upload.File, UploadFile).DisableAntiforgery()
            //.Accepts<IFormFile>("multipart/form-data")
            //.Accepts("multipart/form-data")
            .Produces(200);

        app.UseAntiforgery();

        if (debugMode)
            Console.WriteLine($"{GetType().Name}.{nameof(UseServices)} Finished");

        return true;
    }

    //შესასვლელი წერტილი (endpoint)
    //დანიშნულება -> კავშირის შემოწმების საშუალება
    //შემავალი ინფორმაცია -> არა
    //უფლება -> შემოწმება საჭირო არ არის
    //მოქმედება -> უბრალოდ აბრუნებს 200 კოდს. თუ ამ მეთოდმა იმუშავა, კლიენტი მიხვდება, რომ პროგრამა გაშვებულია
    // GET api/v1/download/file
    //[HttpGet(TestApiRoutes.Test.TestConnection)]
    private static IResult DownloadFile([FromRoute] string fileName, IConfiguration configuration)
    {
        const string mimeType = MediaTypeNames.Application.Octet;
        var path = FileServerLocalPathFromSettings(configuration);
        if (path is null)
            throw new ArgumentNullException(nameof(path));
        return Results.File(Path.Combine(path, fileName), mimeType);
    }

    //IFormFile? file, 
    //HttpContext context
    private static async Task<IResult> UploadFile(HttpContext context, IConfiguration configuration,
        CancellationToken cancellationToken)
    {
        var form = await context.Request.ReadFormAsync(cancellationToken);
        var file = form.Files.GetFile("file");

        var path = FileServerUploadLocalPathFromSettings(configuration);
        if (path is null)
            throw new ArgumentNullException(nameof(configuration), "File server upload path is not configured.");

        if (file == null || file.Length == 0)
            return Results.BadRequest("No file uploaded.");

        var filePath = Path.Combine(path, file.FileName);
        // ReSharper disable once DisposableConstructor
        await using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream, cancellationToken);
        }

        return Results.Ok();
    }

    private static string? FileServerLocalPathFromSettings(IConfiguration configuration)
    {
        var fileServerSettings = FileServerSettings.Create(configuration);
        return fileServerSettings?.FileServerLocalPath;
    }

    private static string? FileServerUploadLocalPathFromSettings(IConfiguration configuration)
    {
        var fileServerSettings = FileServerSettings.Create(configuration);
        return fileServerSettings?.FileServerUploadLocalPath;
    }
}