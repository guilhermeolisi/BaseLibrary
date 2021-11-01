using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BaseLibrary
{
    public static class FileProcess
    {
        public static void WriteTXT(string pathFile, in string parTXT)
        {
            if (pathFile == null) return;
            try
            {
                using (StreamWriter sw = new StreamWriter(pathFile, false, Encoding.UTF8))
                {
                    sw.Write(parTXT);
                }
            }
            //TODO implementar o catch
            catch (Exception e) { }
        }
        private static DateTime? lastAttempt;
        public static async Task WriteTXTAsync(string pathFile, string parTXT)
        {
            if (pathFile == null) return;
            try
            {
                using (StreamWriter sw = new StreamWriter(pathFile, false, Encoding.UTF8))
                {
                    //TODO está dando erro de acesso, que outro processo está no controle do arquivo, provavelmente é por causa de vário processos ao mesmo tempo
                    await sw.WriteAsync(parTXT);
                    if (lastAttempt != null)
                        lastAttempt = null;
                }
            }
            //TODO implementar o catch
            catch (Exception e)
            {
                if (lastAttempt == null)
                {
                    lastAttempt = new();
                    lastAttempt = DateTime.Now;
                }
                if ((DateTime.Now - ((DateTime)lastAttempt)).TotalSeconds > 5)
                {
                    //TODO return a error
                    return;
                }
                await Task.Delay(500);
                WriteTXTAsync(pathFile, parTXT);
            }
        }
        public static string ReadTXT(string pathfile, bool isMandatory = true)
        {
            if (pathfile == null) return string.Empty;
            try
            {
                using (StreamReader sr = new StreamReader(pathfile))
                {
                    return sr.ReadToEnd();
                    //return File.ReadAllText(pathfile);
                }
            }
            catch (DirectoryNotFoundException e)
            {
                return null;
            }
            catch (FileNotFoundException e)
            {
                return null;
            }
            catch (NotSupportedException e)
            {
                return null;
            }
            catch (Exception e)
            {
                return null;
            }
        }
        public static async Task<string> ReadTXTAsync(string pathfile, bool isMandatory = true)
        {
            if (pathfile == null) return string.Empty;
            try
            {
                using (StreamReader sr = new StreamReader(pathfile))
                {
                    return await sr.ReadToEndAsync();
                }
            }
            catch (DirectoryNotFoundException dirEx)
            {
                return null;
            }
            catch (FileNotFoundException fileEx)
            {
                return null;
            }
            catch (NotSupportedException e)
            {
                return null;
            }
            catch (Exception e)
            {
                return null;
            }
        }
        public static void InfoFromFilePath(in string filepath, ref string fileName, ref string folder, ref string extension)
        {
            if (string.IsNullOrWhiteSpace(filepath)) return;

            string regexTemp = string.Empty;
            regexTemp = @"^(?<Folder>.+)\\(?<FileName>[^\\/|<>*:""?]+)\.(?<Extension>[^\\/|<>*:""?]+?)$";
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

            DirectoryInfo[] dirs = dir.GetDirectories();

            // If the destination directory doesn't exist, create it.       
            //if (!Directory.Exists(destDirName))
            Directory.CreateDirectory(destDirName);
            if (preserveTime)
            {
                Directory.SetCreationTime(destDirName, dir.CreationTime);
                Directory.SetLastWriteTime(destDirName, dir.LastWriteTime);
            }

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string tempPath = Path.Combine(destDirName, file.Name);
                file.CopyTo(tempPath, true);
                if (preserveTime)
                {
                    file.LastWriteTime = File.GetLastWriteTime(Path.Combine(destDirName, file.Name));
                    file.CreationTime = File.GetCreationTime(Path.Combine(destDirName, file.Name));
                }
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string tempPath = Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, tempPath, copySubDirs, preserveTime);
                }
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
         public static void FileNameAvailable(ref string fullPath)
        {
            string dir = Path.GetDirectoryName(fullPath);
            string fileName = Path.GetFileNameWithoutExtension(fullPath);
            string exten = Path.GetExtension(fullPath);
            if (string.IsNullOrWhiteSpace(exten))
            {
                fullPath = null;
                return;
            }
            if (!Directory.Exists(dir))
                return;
            int index = 1;
            while (File.Exists(fullPath))
            {
                fullPath = Path.Combine(dir, string.Format("{0} ({1}){2}", fileName, index, exten));
                index++;
            }
        }
    }
}
