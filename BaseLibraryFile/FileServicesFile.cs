using Splat;
using System.Diagnostics;
using System.IO.Compression;

namespace BaseLibrary;

public class FileServicesFile : IFileServicesFile
{
    IFileServicesDirectory directory;
    public FileServicesFile(IFileServicesDirectory? directory = null)
    {
        this.directory = directory ?? Locator.Current!.GetService<IFileServicesDirectory>()! ?? new FileServicesDirectory();
    }
    public void CopyConserveTime(string original, string destination, bool preservetime = true)
    {
        File.Copy(original, destination);
        File.SetCreationTime(destination, File.GetCreationTime(original));
        File.SetLastWriteTime(destination, File.GetLastWriteTime(original));
        File.SetLastAccessTime(destination, File.GetLastAccessTime(original));
    }
    /// <summary>
    /// Get the file size in bytes
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns>size in bytes</returns>
    public long FileSize(string filePath) => new FileInfo(filePath).Length;
    public DateTime FileLastModification(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException(nameof(filePath));

        DateTime created = File.GetCreationTime(filePath);
        DateTime writed = File.GetLastWriteTime(filePath);

        return created > writed ? created : writed;
    }
    public void ExtractZipConserveTime(string zipPath, string extractPath)
    {
        using (ZipArchive archive = ZipFile.OpenRead(zipPath))
        {
            foreach (ZipArchiveEntry entry in archive.Entries)
            {
                // Gets the full path to ensure that relative segments are removed.
                string destinationPath = Path.GetFullPath(Path.Combine(extractPath, entry.FullName));

                string folder = Path.GetDirectoryName(destinationPath);
                directory.CreatAllPath(folder);

                // Ordinal match is safest, case-sensitive volumes can be mounted within volumes that
                // are case-insensitive.
                if (destinationPath.StartsWith(extractPath, StringComparison.Ordinal))
                    entry.ExtractToFile(destinationPath);
            }
        }
    }
    /// <summary>
    /// Open a file in external application
    /// </summary>
    /// <param name="filePath">File path to open</param>
    /// <param name="OS">0: Windows, 1: Linux, 2: Mac</param>
    /// <exception cref="ArgumentException"></exception>
    public void OpenFile(string filePath, sbyte OS)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("The string can not be null or empty", nameof(filePath));
        if (!File.Exists(filePath))
            throw new ArgumentException("The file does not exist", nameof(filePath));

        //System.Diagnostics.Process.Start(filePath);

        using (Process process = new())
        {
            string commandProcess = null;
            string argsProcess = "";

            switch (OS)
            {
                case 0:
                    commandProcess = "cmd.exe";
                    argsProcess = "/C \"" + filePath + "\"";
                    //commandProcess = "\"" + filePath + "\"";
                    break;
                case 1:
                    commandProcess = "xdg-open";
                    argsProcess = filePath;
                    break;
                case 2:
                    commandProcess = "open";
                    argsProcess = filePath;
                    break;
                default:
                    break;
            }
            //if (OS == 0)
            //{
            //    //commandProcess = "\"" + fileManual + "\"";// Path.Combine(sindarinFolder, "Sindarin manual.pdf");
            //    commandProcess = "\"" + filePathManual + "\"";
            //}
            //else if (OS == 1)
            //{
            //    commandProcess = "xdg-open";
            //    argsProcess = filePathManual;//fileManual;
            //}
            //else if (OS == 2)
            //{
            //    commandProcess = "open";
            //    argsProcess = filePathManual;//fileManual;
            //}
            process.StartInfo = new()
            {
                FileName = commandProcess,
                UseShellExecute = true,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                //WorkingDirectory = sindarinFolder,
                Arguments = argsProcess
            };

            process.Start();
        }
    }
}
