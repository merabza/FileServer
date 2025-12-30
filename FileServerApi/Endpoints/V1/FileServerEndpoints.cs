using System;
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
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FileServerApi.Endpoints.V1;

// ReSharper disable once UnusedType.Global
public static class FileServerEndpoints
{
    public static IServiceCollection AddFileServer(this IServiceCollection services, IWebHostBuilder webHostBuilder,
        bool debugMode)
    {
        if (debugMode)
            Console.WriteLine($"{nameof(AddFileServer)} Started");

        webHostBuilder.ConfigureKestrel((context, options) =>
        {
            options.Limits.MaxRequestBodySize =
                context.Configuration.GetValue<long>("Kestrel:Limits:MaxRequestBodySize");
        });

        services.Configure<FormOptions>(options => { options.MultipartBodyLengthLimit = 5545902080; });

        services.AddAntiforgery();

        if (debugMode)
            Console.WriteLine($"{nameof(AddFileServer)} Finished");

        return services;
    }

    public static bool UseFileServerEndpoints(this IEndpointRouteBuilder endpoints, bool debugMode)
    {
        if (debugMode)
            Console.WriteLine($"{nameof(UseFileServerEndpoints)} Started");

        var downloadGroup = endpoints.MapGroup(FileServerApiRoutes.ApiBase + FileServerApiRoutes.Download.DownloadBase);

        downloadGroup.MapGet(FileServerApiRoutes.Download.File, DownloadFile);

        var uploadGroup = endpoints.MapGroup(FileServerApiRoutes.ApiBase + FileServerApiRoutes.Upload.UploadBase);

        uploadGroup.MapPost(FileServerApiRoutes.Upload.File, UploadFile).DisableAntiforgery()
            //.Accepts<IFormFile>("multipart/form-data")
            //.Accepts("multipart/form-data")
            .Produces(200);

        if (debugMode)
            Console.WriteLine($"{nameof(UseFileServerEndpoints)} Finished");

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
        return path is null
            ? throw new ArgumentNullException(nameof(path))
            : Results.File(Path.Combine(path, fileName), mimeType);
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