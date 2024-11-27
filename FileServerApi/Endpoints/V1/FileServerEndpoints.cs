using FileServerApi.V1.Routes;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
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
        return true;
    }

    public bool UseServices(WebApplication app, bool debugMode)
    {
        if (debugMode)
            Console.WriteLine($"{GetType().Name}.{nameof(UseServices)} Started");

        var group = app.MapGroup(FileServerApiRoutes.ApiBase + FileServerApiRoutes.Download.DownloadBase);

        group.MapGet(FileServerApiRoutes.Download.File, DownloadFile);

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
        const string mimeType = "application/octet-stream";
        var path = FileServerLocalPathFromSettings(configuration);
        if (path is null)
            throw new ArgumentNullException(nameof(path));
        return Results.File(Path.Combine(path,fileName), mimeType);
    }


    private static string? FileServerLocalPathFromSettings(IConfiguration configuration)
    {
        var fileServerSettings = FileServerSettings.Create(configuration);
        return fileServerSettings?.FileServerLocalPath;
    }
}