﻿namespace BaseLibrary;

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
    public double GetDirectorySize(string directory, bool recursive)
    {
        if (string.IsNullOrWhiteSpace(directory) || !Directory.Exists(directory))
            return 0;

        double result = 0;
        if (recursive)
        {
            foreach (string dir in Directory.GetDirectories(directory))
            {
                GetDirectorySize(dir, recursive);
            }
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
            throw new ArgumentNullException(nameof(goal));
        if (string.IsNullOrWhiteSpace(goal))
            throw new ArgumentException(nameof(goal) + " parameter is empty");

        //TODO fazer isto aqui de maneira recursiva com CreateAllPath
        string? tempPath = goal.Substring(0);
        Stack<string> folderToCreate = new();
        while (!string.IsNullOrWhiteSpace(tempPath) && !Directory.Exists(tempPath))
        {
            folderToCreate.Push(tempPath!.Substring(0));
            tempPath = Path.GetDirectoryName(tempPath);
        }
        //if (string.IsNullOrWhiteSpace(tempPath))
        //    return;
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
                var trash = files[i].Replace(" ", "%20");
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
    public int CountTotalFiles(string folder, string? pattern = null, string[]? excludePattern = null)
    {
        int result = 0;

        string[] files = pattern is null ? Directory.GetFiles(folder) : Directory.GetFiles(folder, pattern);

        result += files.Length;

        int excludeP = 0;
        for (int j = 0; j < excludePattern?.Length; j++)
        {
            if (!string.IsNullOrEmpty(excludePattern[j]))
            {
                for (int i = 0; i < files.Length; i++)
                {
                    if (files[i].Contains(excludePattern[j]))
                        excludeP++;
                }
            }

        }
        result -= excludeP;
        string[] directories = Directory.GetDirectories(folder);

        for (int i = 0; i < directories.Length; i++)
        {
            result += CountTotalFiles(directories[i], pattern, excludePattern);
        }
        return result;
    }
    public void SetAttributesNormal(string folder)
    {
        DirectoryInfo dir = new DirectoryInfo(folder);
        SetAttributesNormal(dir);
    }
    public void SetAttributesNormal(DirectoryInfo dir)
    {
        if (!dir.Exists)
            return;
        foreach (var sub in dir.GetDirectories())
            SetAttributesNormal(sub);
        foreach (var f in dir.GetFiles())
            f.Attributes = FileAttributes.Normal;
        dir.Attributes = FileAttributes.Normal;
    }
}
