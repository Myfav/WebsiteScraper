using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.AccessControl;
using System.Security.Principal;

namespace WebsiteScraper
{
    public interface IWebClient
    {
        string DownloadString(string url);
    }

    public class WebClient : IWebClient
    {
        private readonly System.Net.WebClient client;

        public WebClient()
        {
            client = new System.Net.WebClient();
        }

        #region IWebClient Members

        public string DownloadString(string url)
        {
            return client.DownloadString(url);
        }

        #endregion
    }

    public interface IFileSystem
    {
        bool FileExists(string file);
        DateTime LastWriteTime(string file);
        void SetLastWriteTime(string file, DateTime time);
        bool DirectoryExists(string directory);
        void CreateDirectory(string directory);
        IStream CreateStream(string file, FileMode mode);

        void MoveFile(string source, string destination, bool overwrite);

        void DeleteDirectory(string directory, bool recursive);
    }

    public interface IStream : IDisposable
    {
        int Read(byte[] byteBuffer, int offset, int count);

        void Write(byte[] byteBuffer, int offset, int count);

        void Close();
    }

    internal class IOStream : IStream
    {
        private readonly Stream _stream;

        public IOStream(Stream stream)
        {
            _stream = stream;
        }

        #region IStream Members

        public void Dispose()
        {
            _stream.Dispose();
        }

        public int Read(byte[] byteBuffer, int offset, int count)
        {
            return _stream.Read(byteBuffer, offset, count);
        }

        public void Write(byte[] byteBuffer, int offset, int count)
        {
            _stream.Write(byteBuffer, offset, count);
        }

        public void Close()
        {
            _stream.Close();
        }

        #endregion
    }

    public interface IWebRequest
    {
        IHttpWebRequest Create(Uri url);
    }

    public interface IHttpWebRequest
    {
        DateTime IfModifiedSince { get; set; }
        WebHeaderCollection Headers { get; set; }
        IHttpWebResponse GetResponse();
    }

    public interface IHttpWebResponse
    {
        HttpStatusCode StatusCode { get; }

        IStream GetResponseStream();

        void Close();

        string GetResponseHeader(string p);
    }

    internal class WebRequest : IWebRequest
    {
        #region IWebRequest Members

        public IHttpWebRequest Create(Uri url)
        {
            return new HttpWebRequest(System.Net.WebRequest.Create(url) as System.Net.HttpWebRequest);
        }

        #endregion
    }

    internal class HttpWebRequest : IHttpWebRequest
    {
        private readonly System.Net.HttpWebRequest _request;

        public HttpWebRequest(System.Net.HttpWebRequest request)
        {
            _request = request;
        }

        #region IHttpWebRequest Members

        public IHttpWebResponse GetResponse()
        {
            var response = _request.GetResponse() as System.Net.HttpWebResponse;
            if (response == null)
                return null;

            return new HttpWebResponse(response);
        }

        public DateTime IfModifiedSince
        {
            get { return _request.IfModifiedSince; }
            set { _request.IfModifiedSince = value; }
        }

        public WebHeaderCollection Headers
        {
            get { return _request.Headers; }
            set { _request.Headers = value; }
        }

        #endregion
    }

    internal class HttpWebResponse : IHttpWebResponse
    {
        private readonly System.Net.HttpWebResponse _response;

        public HttpWebResponse(System.Net.HttpWebResponse response)
        {
            if (response == null)
                throw new ArgumentNullException("response", "HttpWebResponse constructed with a null argument");

            _response = response;
        }

        #region IHttpWebResponse Members

        public HttpStatusCode StatusCode
        {
            get { return _response.StatusCode; }
        }

        public IStream GetResponseStream()
        {
            return new IOStream(_response.GetResponseStream());
        }

        public void Close()
        {
            _response.Close();
        }

        public string GetResponseHeader(string p)
        {
            return _response.GetResponseHeader(p);
        }

        #endregion
    }

    public static class AssetDownloader
    {
        private const int MaxPath = 260;
        private static readonly List<BackgroundWorker> _activeWorkers = new List<BackgroundWorker>();

        public static bool IsIdle
        {
            get { return (_activeWorkers.Count <= 0); }
        }

        public static void Download(DownloaderParams downloadInfo)
        {
            string localPath = GetFile(downloadInfo);
        }

        public static void StartDownloadAsync(DownloaderParams downloadInfo)
        {
            var worker = new BackgroundWorker();
            worker.DoWork += DownloadDoWork;
            worker.RunWorkerCompleted += DownloadComplete;
            _activeWorkers.Add(worker);
            worker.RunWorkerAsync(downloadInfo);
        }

        private static void DownloadComplete(object sender, RunWorkerCompletedEventArgs e)
        {
            _activeWorkers.Remove((BackgroundWorker) sender);
        }

        private static void DownloadDoWork(object sender, DoWorkEventArgs e)
        {
            string localPath = GetFile((DownloaderParams) e.Argument);
            e.Result = localPath;
        }

        private static bool IsValidPath(string path)
        {
            char[] invalidChars = Path.GetInvalidPathChars();
            if (invalidChars.Any(invalidChar => path.Contains(invalidChar)))
            {
                return false;
            }

            return path.Length < MaxPath;
        }

        /// <summary> Checks for write access for the given file.
        /// </summary>
        /// <param name="fileName">The filename.</param>
        /// <returns>true, if write access is allowed, otherwise false</returns>
        public static bool WriteAccess(string fileName)
        {
            if ((File.GetAttributes(fileName) & FileAttributes.ReadOnly) != 0)
                return false;

            // Get the access rules of the specified files (user groups and user names that have access to the file)
            AuthorizationRuleCollection rules = File.GetAccessControl(fileName).GetAccessRules(true, true,
                                                                                               typeof (
                                                                                                   SecurityIdentifier));

            // Get the identity of the current user and the groups that the user is in.
            IdentityReferenceCollection groups = WindowsIdentity.GetCurrent().Groups;
            string sidCurrentUser = WindowsIdentity.GetCurrent().User.Value;

            // Check if writing to the file is explicitly denied for this user or a group the user is in.
            if (
                rules.OfType<FileSystemAccessRule>().Any(
                    r =>
                    (groups.Contains(r.IdentityReference) || r.IdentityReference.Value == sidCurrentUser) &&
                    r.AccessControlType == AccessControlType.Deny &&
                    (r.FileSystemRights & FileSystemRights.WriteData) == FileSystemRights.WriteData))
                return false;

            // Check if writing is allowed
            return
                rules.OfType<FileSystemAccessRule>().Any(
                    r =>
                    (groups.Contains(r.IdentityReference) || r.IdentityReference.Value == sidCurrentUser) &&
                    r.AccessControlType == AccessControlType.Allow &&
                    (r.FileSystemRights & FileSystemRights.WriteData) == FileSystemRights.WriteData);
        }

        private static string GetFile(DownloaderParams info)
        {
            string writePath = info.FullLocalPath;

            if (!IsValidPath(writePath))
            {
                Debug.Fail("Generated invalid paths.");
                return null;
            }

            string directory = Path.GetDirectoryName(writePath);

            //if (WriteAccess(writePath))
            //{
            //    Debug.WriteLine("I have write access");
            //}
            //else
            //{
            //    Debug.WriteLine("I don't have write access");
            //}

            if (!FileSystem.DirectoryExists(directory))
            {
                FileSystem.CreateDirectory(directory);
            }
            //check for existence of local file
            bool localFileExists = FileSystem.FileExists(writePath);
            if (localFileExists)
            {
                return writePath;
            }
            var webRequest = new WebRequest();
            IHttpWebResponse response;
            IHttpWebRequest request = webRequest.Create(new Uri(info.RemoteUrl));
            try
            {
                response = request.GetResponse();
            }
            catch (WebException ex)
            {
                response = ex.Response as System.Net.HttpWebResponse == null
                               ? null
                               : new HttpWebResponse(ex.Response as System.Net.HttpWebResponse);
            }
            if (response == null)
            {
                return null;
            }
            if (response.StatusCode == HttpStatusCode.OK)
            {
                ReadResponseToFileStream(writePath, response);

                return writePath;
            }
            return null;
        }

        private static void ReadResponseToFileStream(string localFilePath, IHttpWebResponse response)
        {
            IStream data = response.GetResponseStream();
            var byteBuffer = new byte[4096];
            using (IStream output = FileSystem.CreateStream(localFilePath, FileMode.Create))
            {
                int bytesRead;
                do
                {
                    bytesRead = data.Read(byteBuffer, 0, byteBuffer.Length);
                    if (bytesRead > 0)
                    {
                        output.Write(byteBuffer, 0, bytesRead);
                    }
                } while (bytesRead > 0);
            }

            data.Close();
            response.Close();
        }

        #region Nested type: DownloaderParams

        public class DownloaderParams
        {
            public string RemoteUrl;
            public string LocalFileName;
            public string TargetFolderName;

            public string FullLocalPath
            {
                get
                {
                    string localPath = Path.Combine(Directory.GetCurrentDirectory(),
                                                    TargetFolderName + Path.DirectorySeparatorChar + LocalFileName);
                    return localPath;
                }
            }
        }

        #endregion
    }
}