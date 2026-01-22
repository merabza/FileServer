using Microsoft.Extensions.Configuration;

namespace FileServerApi;

public sealed class FileServerSettings
{
    public string? FileServerLocalPath { get; set; }
    public string? FileServerUploadLocalPath { get; set; }

    public static FileServerSettings? Create(IConfiguration configuration)
    {
        IConfigurationSection projectSettingsSection = configuration.GetSection("FileServerSettings");
        return projectSettingsSection.Get<FileServerSettings>();
    }
}
