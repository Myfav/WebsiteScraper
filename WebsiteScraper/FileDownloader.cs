using System;
using System.IO;

namespace WebsiteScraper
{
    public static class FileSystem
    {
        public static bool FileExists(string file)
        {
            return File.Exists(file);
        }

        public static bool DirectoryExists(string directory)
        {
            return Directory.Exists(directory);
        }

        public static void CreateDirectory(string directory)
        {
            Directory.CreateDirectory(directory);
        }

        public static DateTime LastWriteTime(string file)
        {
            if (FileExists(file))
            {
                var info = new FileInfo(file);
                return info.LastWriteTime;
            }
            return DateTime.MinValue;
        }

        public static void SetLastWriteTime(string file, DateTime time)
        {
            if (FileExists(file))
            {
                File.SetLastWriteTime(file, time);
            }
        }

        public static IStream CreateStream(string file, FileMode mode)
        {
            return new IOStream(new FileStream(file, mode));
        }

        public static void MoveFile(string source, string destination, bool overwrite)
        {
            if (overwrite && File.Exists(destination))
                File.Delete(destination);

            File.Move(source, destination);
        }

        public static void DeleteDirectory(string path, bool recursive)
        {
            if (Directory.Exists(path))
                Directory.Delete(path, recursive);
        }
    }

    public class FileDownloader
    {
    }
}