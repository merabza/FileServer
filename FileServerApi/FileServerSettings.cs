using Microsoft.Extensions.Configuration;

namespace FileServerApi;

public class FileServerSettings
{
    public string? FileServerLocalPath { get; set; }
    public string? FileServerUploadLocalPath { get; set; }

    public static FileServerSettings? Create(IConfiguration configuration)
    {
        var projectSettingsSection = configuration.GetSection("FileServerSettings");
        return projectSettingsSection.Get<FileServerSettings>();
    }
}