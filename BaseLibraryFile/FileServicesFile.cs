using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseLibrary;

public class FileServicesFile : IFileServicesFile
{
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
}
