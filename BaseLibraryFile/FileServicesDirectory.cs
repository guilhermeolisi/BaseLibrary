using FileTypeChecker;
using FileTypeChecker.Abstracts;
using FileTypeChecker.Extensions;
using Splat;
using System.Drawing;
using System.IO.Compression;
using System.Text;
using System.Text.RegularExpressions;

namespace BaseLibrary;

public class FileServicesDirectory : IFileServicesDirectory
{
    public void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs, bool preserveTime)
    {
        // Get the subdirectories for the specified directory.
        DirectoryInfo dir = new DirectoryInfo(sourceDirName);

        if (!dir.Exists)
        {
            throw new DirectoryNotFoundException(
                "Source directory does not exist or could not be found: "
                + sourceDirName);
        }

        // If the destination directory doesn't exist, create it.       
        //if (!Directory.Exists(destDirName))
        Directory.CreateDirectory(destDirName);
        if (preserveTime)
        {
            Directory.SetCreationTime(destDirName, dir.CreationTime);
            Directory.SetLastWriteTime(destDirName, dir.LastWriteTime);
        }
        //else
        //{
        //    Directory.SetCreationTime(destDirName, DateTime.Now);
        //    Directory.SetLastWriteTime(destDirName, DateTime.Now);
        //}

        // Get the files in the directory and copy them to the new location.
        FileInfo[] files = dir.GetFiles();
        foreach (FileInfo file in files)
        {
            string tempPath = Path.Combine(destDirName, file.Name);
            file.CopyTo(tempPath, true);
            if (preserveTime)
            {
                File.SetLastWriteTime(Path.Combine(destDirName, file.Name), file.LastWriteTime);
                File.SetCreationTime(Path.Combine(destDirName, file.Name), file.CreationTime);
                //file.LastWriteTime = File.GetLastWriteTime(Path.Combine(destDirName, file.Name));
                //file.CreationTime = File.GetCreationTime(Path.Combine(destDirName, file.Name));
            }
            //else
            //{
            //    File.SetLastWriteTime(Path.Combine(destDirName, file.Name), DateTime.Now);
            //    File.SetCreationTime(Path.Combine(destDirName, file.Name), DateTime.Now);
            //}
            //#if DEBUG
            //var trash = File.GetLastWriteTime(Path.Combine(destDirName, file.Name));
            //#endif
        }

        // If copying subdirectories, copy them and their contents to new location.
        if (copySubDirs)
        {
            DirectoryInfo[] dirs = dir.GetDirectories();

            foreach (DirectoryInfo subdir in dirs)
            {
                string tempPath = Path.Combine(destDirName, subdir.Name);
                DirectoryCopy(subdir.FullName, tempPath, copySubDirs, preserveTime);
            }
        }
    }
    public double GetDirectorySize(string directory)
    {
        if (string.IsNullOrWhiteSpace(directory) || !Directory.Exists(directory))
            return 0;

        double result = 0;
        foreach (string dir in Directory.GetDirectories(directory))
        {
            GetDirectorySize(dir);
        }

        foreach (FileInfo file in new DirectoryInfo(directory).GetFiles())
        {
            result += file.Length;
        }

        return result;
    }
    public void DirectoryClear(string sourceDirName)
    {
        // Get the subdirectories for the specified directory.
        string[] files = Directory.GetFiles(sourceDirName);
        for (int i = 0; i < files.Length; i++)
        {
            File.Delete(files[i]);
        }
        string[] directories = Directory.GetDirectories(sourceDirName);
        for (int i = 0; i < directories.Length; i++)
        {
            Directory.Delete(directories[i], true);
        }
    }
    public void CreatAllPath(string goal)
    {
        if (goal is null)
            return;
        string? tempPath = goal[..];
        Stack<string> folderToCreate = new();
        while (!Directory.Exists(tempPath))
        {
            folderToCreate.Push(tempPath![..]);
            tempPath = Path.GetDirectoryName(tempPath);
        }
        if (string.IsNullOrWhiteSpace(tempPath))
            return;
        while (folderToCreate.Count > 0)
            Directory.CreateDirectory(folderToCreate.Pop());
    }
    public void RenameAllWhithoutSpaces(string folder)
    {
        string[] files = Directory.GetFiles(folder);
        string parentFolder = null;
        for (int i = 0; i < files.Length; i++)
        {
            string fileName = Path.GetFileName(files[i]);
            if (fileName.Contains(' '))
            {
#if DEBUG
                var trash  = files[i].Replace(" ", "%20");
#endif
                parentFolder ??= Path.GetDirectoryName(files[i]);
                fileName = fileName.Replace(" ", "%20");
                File.Move(files[i], Path.Combine(parentFolder, fileName));
            }
        }
        string[] folders = Directory.GetDirectories(folder);
        for (int i = 0; i < folders.Length; i++)
        {
            string folderPath = folders[i].Substring(0);
            string folderName = Path.GetFileName(folderPath);
            if (folderName.Contains(' '))
            {
                parentFolder ??= Path.GetDirectoryName(folderPath);
                
                folderName = folderName.Replace(" ", "%20");
                folderPath = Path.Combine(parentFolder, folderName);
                Directory.Move(folders[i], folderPath);
            }
            RenameAllWhithoutSpaces(folderPath);
        }
    }
}
