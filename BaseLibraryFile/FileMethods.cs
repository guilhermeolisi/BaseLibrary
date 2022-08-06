using FileTypeChecker;
using FileTypeChecker.Abstracts;
using FileTypeChecker.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BaseLibrary;

public static class FileMethods
{
    public static bool WriteTXT(string pathFile, in string parTXT)
    {
        if (string.IsNullOrWhiteSpace(pathFile))
            return false;
        short count = 0;
        bool isCont = true;
        bool returnBak = false;
        try
        {
            BakWriteBegin(pathFile);
            while (isCont && count < 3)
            {
                count++;
                isCont = false;
                try
                {
                    using (StreamWriter sw = new StreamWriter(pathFile, false, Encoding.UTF8))
                    {
                        sw.Write(parTXT);
                    }
                    return true;
                }
                catch (IOException e)
                {
                    isCont = true;
                    Thread.Sleep(100);
                    if (!returnBak)
                        returnBak = true;
                }
                catch (Exception e)
                {
                    if (!returnBak)
                        returnBak = true;
                }
            }
        }
        finally
        {
            BakWriteEnd(pathFile, returnBak);
        }

        return false;
    }
    public static async Task<bool> WriteTXTAsync(string pathFile, string parTXT)
    {
        if (string.IsNullOrWhiteSpace(pathFile))
            return false;
        short count = 0;
        bool isCont = true;
        bool returnBak = false;
        try
        {
            BakWriteBegin(pathFile);
            while (isCont && count < 10)
            {
                count++;
                isCont = false;
                try
                {
                    using (StreamWriter sw = new StreamWriter(pathFile, false, Encoding.UTF8))
                    {
                        await sw.WriteAsync(parTXT);
                        return true;
                    }
                }
                catch (IOException e)
                {
                    isCont = true;
                    await Task.Delay(200);
                    if (!returnBak)
                        returnBak = true;
                }
                catch (Exception e)
                {
                    if (!returnBak)
                        returnBak = true;
                }
            }
        }
        finally
        {
            BakWriteEnd(pathFile, returnBak);
        }
        return false;
    }
    private static string tmpExt = ".tmp";
    private static void BakWriteBegin(string pathFile)
    {
        if (File.Exists(pathFile))
        {
            File.Copy(pathFile, pathFile + tmpExt, true);
        }
    }
    private static void BakWriteEnd(string pathFile, bool returnBak)
    {
        if (returnBak)
        {
            if (File.Exists(pathFile + tmpExt))
            {
                try
                {
                    File.Copy(pathFile + tmpExt, pathFile, true);
                }
                catch (Exception e)
                {

                }
            }
        }
        if (File.Exists(pathFile + tmpExt) && File.Exists(pathFile) && File.GetCreationTime(pathFile) > File.GetCreationTime(pathFile + tmpExt))
        {
            try
            {
                File.Delete(pathFile + tmpExt);
            }
            catch (Exception e)
            {

            }
        }
    }
    public static string? ReadTXT(string pathFile)
    {
        if (string.IsNullOrWhiteSpace(pathFile) || !File.Exists(pathFile))
            return null;
        short count = 0;
        bool isCont = true;
        string fileTemp = BakReadBegin(pathFile);
        try
        {
            while (isCont && count < 3)
            {
                count++;
                isCont = false;
                try
                {
                    using (StreamReader sr = new StreamReader(fileTemp))
                    {
                        return sr.ReadToEnd();
                    }
                }
                catch (IOException e)
                {
                    isCont = true;
                    Thread.Sleep(100);
                }
                catch (Exception e)
                {
                    return null;
                }
            }
        }
        finally
        {
            BakReadEnd(pathFile);
        }
        return null;
    }
    public static async Task<string?> ReadTXTAsync(string pathFile)
    {
        if (string.IsNullOrWhiteSpace(pathFile) || !File.Exists(pathFile))
            return null;
        short count = 0;
        bool isCont = true;
        string fileTemp = BakReadBegin(pathFile);
        try
        {
            while (isCont && count < 10)
            {
                count++;
                isCont = false;
                try
                {
                    using (StreamReader sr = new StreamReader(fileTemp))
                    {
                        return await sr.ReadToEndAsync();
                    }
                }
                catch (IOException e)
                {
                    isCont = true;
                    await Task.Delay(200);
                }
                catch (Exception e)
                {
                    return null;
                }
            }
        }
        finally
        {
            BakReadEnd(pathFile);
        }
        return null;
    }
    private static string BakReadBegin(string pathFile)
    {

#if DEBUG
        if ((File.Exists(pathFile + tmpExt) && !File.Exists(pathFile)) || (File.Exists(pathFile + tmpExt) && File.Exists(pathFile) && File.GetCreationTime(pathFile + tmpExt) > File.GetCreationTime(pathFile)))
        {

        }
        var trash = File.GetLastWriteTime(pathFile + tmpExt);
        DateTime? trash2 = (File.Exists(pathFile) ? File.GetLastWriteTime(pathFile) : null);
        var trash3 = File.GetCreationTime(pathFile + tmpExt);
        DateTime? trash4 = (File.Exists(pathFile) ? File.GetCreationTime(pathFile) : null);
#endif

        if ((File.Exists(pathFile + tmpExt) && !File.Exists(pathFile)) || (File.Exists(pathFile + tmpExt) && File.Exists(pathFile) && File.GetCreationTime(pathFile + tmpExt) > File.GetLastWriteTime(pathFile)))
        {
            try
            {
                File.Move(pathFile + tmpExt, pathFile, true);
            }
            catch (Exception e)
            {
                return pathFile + tmpExt;
            }
        }
        return pathFile;
    }
    private static void BakReadEnd(string pathFile)
    {
        if ((File.Exists(pathFile + tmpExt) && !File.Exists(pathFile)) || (File.Exists(pathFile + tmpExt) && File.Exists(pathFile) && File.GetCreationTime(pathFile + tmpExt) > File.GetCreationTime(pathFile)))
        {
            try
            {
                File.Move(pathFile + tmpExt, pathFile, true);
            }
            catch (Exception e)
            {

            }
        }
    }
    public static void InfoFromFilePath(in string filepath, ref string fileName, ref string folder, ref string extension)
    {
        if (string.IsNullOrWhiteSpace(filepath)) return;

        string regexTemp = string.Empty;
        regexTemp = @"^(?<Folder>.+)\\(?<FileName>[^\\/|<>*:""?]+?)(\.(?<Extension>[^\\/|<>*:""?]+?))?$";
        Match m = Regex.Match(filepath, regexTemp);
        fileName = m.Groups["FileName"].Value;
        folder = m.Groups["Folder"].Value;
        extension = m.Groups["Extension"].Value;
    }
    public static (string, string, string) InfoFromFilePath(in string filepath)
    {
        if (string.IsNullOrWhiteSpace(filepath)) return (null, null, null);

        string regexTemp = string.Empty;
        regexTemp = @"^(?<Folder>.+)\\(?<FileName>[^\\/|<>*:""?]+)\.(?<Extension>[^\\/|<>*:""?]+?)$";
        Match m = Regex.Match(filepath, regexTemp);
        return (m.Groups["FileName"].Value, m.Groups["Folder"].Value, m.Groups["Extension"].Value);
    }
    public static void InfoFromFolderPath(in string filepath, ref string actualFolder, ref string folderParent)
    {
        if (string.IsNullOrWhiteSpace(filepath)) return;

        string regexTemp = string.Empty;
        regexTemp = @"^(?<FolderParent>.+)\\(?<FolderName>[^\\/|<>*:""?]+)$";
        Match m = Regex.Match(filepath, regexTemp);
        actualFolder = m.Groups["FolderName"].Value;
        folderParent = m.Groups["FolderParent"].Value;
    }
    public static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs, bool preserveTime)
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
    public static void DirectoryClear(string sourceDirName)
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
    public static void CreatAllPath(string goal)
    {
        string tempPath = goal[..];
        Stack<string> folderToCreate = new();
        while (!Directory.Exists(tempPath))
        {
            folderToCreate.Push(tempPath[..]);
            tempPath = Path.GetDirectoryName(tempPath);
        }
        if (string.IsNullOrWhiteSpace(tempPath))
            return;
        while (folderToCreate.Count > 0)
            Directory.CreateDirectory(folderToCreate.Pop());
    }
    /// <summary>
    /// Verifica se é um arquivo zip
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static bool CheckZipFile(string path)
    {
        try
        {
            using (var zipFile = ZipFile.OpenRead(path))
            {
                var entries = zipFile.Entries;
                return true;
            }
        }
        catch (InvalidDataException)
        {
            return false;
        }
    }
    /// <summary>
    /// Verifica se é um arquivo binário ou de texto
    /// </summary>
    /// <returns>text return true, binary return false</returns>
    public static bool CheckTextFile(string filePath)
    {
        if (!File.Exists(filePath)) return false;
        const int charsToCheck = 8000;
        const char nulChar = '\0';

        int nulCount = 0;

        using (var streamReader = new StreamReader(filePath))
        {
            for (var i = 0; i < charsToCheck; i++)
            {
                if (streamReader.EndOfStream)
                    return true;

                if ((char)streamReader.Read() == nulChar)
                {
                    nulCount++;

                    if (nulCount >= 1)
                        return false;
                }
                else
                {
                    nulCount = 0;
                }
            }
        }

        return true;
    }
    /// <summary>
    /// Verifica se é um arquivo imagem
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static bool CheckImageFile(string path)
    {
        try
        {
            using (var fileStream = File.OpenRead(path))
            {
                var isRecognizableType = FileTypeValidator.IsTypeRecognizable(fileStream);

                if (!isRecognizableType)
                {
                    // Do something ...
                    return false;
                }

                //IFileType fileType = FileTypeValidator.GetFileType(fileStream);
                return fileStream.IsImage();
                //Console.WriteLine("Type Name: {0}", fileType.Name);
                //Console.WriteLine("Type Extension: {0}", fileType.Extension);
                //Console.WriteLine("Is Image?: {0}", fileStream.IsImage());
                //Console.WriteLine("Is Bitmap?: {0}", fileStream.Is<Bitmap>());
            }
        }
        catch (InvalidDataException)
        {
            return false;
        }
    }
    /// <summary>
    /// Verifica se é um arquivo XML
    /// </summary>
    /// <param name="fullPath"></param>
    /// <param name="mode">C copied; E exported; I imported; M moved</param>
    /// <returns></returns>
    public static string FileNameAvailable(this string fullPath, char? mode)
    {
        string? dir = Path.GetDirectoryName(fullPath);
        string fileName = Path.GetFileNameWithoutExtension(fullPath);
        string exten = Path.GetExtension(fullPath);
        string sufix = mode switch
        {
            'C' => "Copied",
            'E' => "Exported",
            'I' => "Imported",
            'M' => "Moved",
            _ => ""
        };
        if (!Directory.Exists(dir))
            return fullPath;
        int index = 1;
        while (File.Exists(fullPath))
        {
            fullPath = Path.Combine(dir, string.Format("{0}{1}{2}{3}", fileName, string.IsNullOrWhiteSpace(sufix) ? "" : " - " + sufix, index == 1 ? "" : " (" + index + ")", exten));
            index++;
        }
        return fullPath;
    }
    static char[] forbiden = new char[] { '/', '\\', '>', '<', ':', '*', '?', '"', '|' };
    public static string CutCharacterFileForbiden(this string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            return filePath;
        if (filePath.Length > 30)
            filePath = filePath[0..29];
        int ind = 0;

#if DEBUG
        for (int i = 0; i < forbiden.Length; i++)
        {
            if (forbiden[i] < 31)
            { }
        }

#endif

        while (ind < filePath.Length)
        {
            bool isForbiden = false;
            for (int i = 0; i < forbiden.Length; i++)
            {
                if (filePath[ind] == forbiden[i])
                {
                    isForbiden = true;
                    break;
                }
            }
            if (!isForbiden)
            {
                for (int i = 0; i < 32; i++)
                {
                    if (filePath[ind] == i)
                    {
                        isForbiden = true;
                        break;
                    }
                }
            }
            if (isForbiden)
            {
#if DEBUG
                var tasrh = filePath[ind];
                var trash2 = filePath.Insert(ind, " ");
                var trash3 = filePath.Remove(ind, 1);
#endif
                filePath = filePath.Remove(ind, 1).Insert(ind, " ");
            }
            ind++;
        }
        return filePath;
    }
}
