namespace WebsiteScraper.ExtractHelpers
{
    public class DownloadImageHelper
    {
        public static string DownloadImageAsync(string imageUrl, string localFileName, string targetFolderName)
        {
            var downloaderParams = new AssetDownloader.DownloaderParams
                                       {
                                           RemoteUrl = imageUrl,
                                           LocalFileName = localFileName,
                                           TargetFolderName = targetFolderName,
                                       };
            AssetDownloader.StartDownloadAsync(downloaderParams);

            return downloaderParams.FullLocalPath;
        }

        public static string DownloadImage(string imageUrl, string localFileName, string targetFolderName)
        {
            var downloaderParams = new AssetDownloader.DownloaderParams
            {
                RemoteUrl = imageUrl,
                LocalFileName = localFileName,
                TargetFolderName = targetFolderName,
            };
            AssetDownloader.Download(downloaderParams);

            return downloaderParams.FullLocalPath;
        }
    }
}
