using Splat;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    public long FileSize(string fileName) => new FileInfo(fileName).Length;
    public DateTime FileLastModification(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            throw new ArgumentException(nameof(fileName));

        DateTime created = File.GetCreationTime(fileName);
        DateTime writed = File.GetLastWriteTime(fileName);

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
}
