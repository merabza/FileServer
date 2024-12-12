namespace FileServerApi.V1.Routes;

public static class FileServerApiRoutes
{
    private const string Api = "api";
    private const string Version = "v1";
    public const string ApiBase = Api + "/" + Version;

    public static class Download
    {
        public const string DownloadBase = "/download";

        // GET api/v1/download/file/{fileName}
        public const string File = "/file/{fileName}";
    }

    public static class Upload
    {
        public const string UploadBase = "/upload";

        // GET api/v1/download/file/{fileName}
        public const string File = "/file";
    }
}