using BaseLibrary.Text;
using System.Text.RegularExpressions;

namespace BaseLibrary;

public class FileServicesName : IFileServicesName
{
    static readonly string regexTemp = @"^(?<Folder>.+)\\(?<FileName>[^\\/|<>*:""?]+?)(\.(?<Extension>[^\\/|<>*:""?]+?))?$";
    public void InfoFromFilePath(in string filepath, ref string fileName, ref string folder, ref string extension)
    {
        if (string.IsNullOrWhiteSpace(filepath)) 
            //return;
            throw new ArgumentNullException(nameof(filepath));

        //string regexTemp = string.Empty;
        //regexTemp = @"^(?<Folder>.+)\\(?<FileName>[^\\/|<>*:""?]+?)(\.(?<Extension>[^\\/|<>*:""?]+?))?$";

        //Por algum motivo desconhecido, o regex não está funcionando no Linux
        // Match m = Regex.Match(filepath, regexTemp);
        // fileName = m.Groups["FileName"].Value;
        // folder = m.Groups["Folder"].Value;
        // extension = m.Groups["Extension"].Value;

        fileName = Path.GetFileNameWithoutExtension(filepath);
        folder = Path.GetDirectoryName(filepath);
        extension = Path.GetExtension(filepath);
    }
    public (string?, string?, string?) InfoFromFilePath(in string filepath)
    {
        if (string.IsNullOrWhiteSpace(filepath)) return (null, null, null);

        //string regexTemp = string.Empty;
        //regexTemp = @"^(?<Folder>.+)\\(?<FileName>[^\\/|<>*:""?]+)\.(?<Extension>[^\\/|<>*:""?]+?)$";
        Match m = Regex.Match(filepath, regexTemp);
        return (m.Groups["FileName"].Value, m.Groups["Folder"].Value, m.Groups["Extension"].Value);
    }
    public void InfoFromFolderPath(in string filepath, ref string actualFolder, ref string folderParent)
    {
        if (string.IsNullOrWhiteSpace(filepath)) return;

        //string regexTemp = string.Empty;
        //regexTemp = @"^(?<FolderParent>.+)\\(?<FolderName>[^\\/|<>*:""?]+)$";
        Match m = Regex.Match(filepath, regexTemp);
        actualFolder = m.Groups["FolderName"].Value;
        folderParent = m.Groups["FolderParent"].Value;
    }
    public string FileNameAvailable(string fullPath, char? mode = null)
    {
        if (string.IsNullOrWhiteSpace(fullPath))
            throw new ArgumentNullException(nameof(fullPath));
        string? dir = Path.GetDirectoryName(fullPath);
        if (!Directory.Exists(dir))
            return fullPath;

        string exten = Path.GetExtension(fullPath);

        (string fileName, string sufix) = Path.GetFileNameWithoutExtension(fullPath).GetNameAndSufixAvailable(mode);


        int index = 1;
        while (File.Exists(fullPath))
        {
            //fullPath = Path.Combine(dir, string.Format("{0}{1}{2}{3}", fileName, string.IsNullOrWhiteSpace(sufix) ? "" : " - " + sufix, index == 1 ? "" : " (" + index + ")", exten));
            fullPath = Path.Combine(dir, fileName.CombineNameAndSufix(sufix, index) + exten);
            index++;
        }
        return fullPath;
    }
    static readonly char[] forbiden = ['/', '\\', '>', '<', ':', '*', '?', '"', '|'];
    public string CutCharacterFileForbiden(string fileName, bool cut)
    {
        if (fileName is null)
            throw new ArgumentNullException(nameof(fileName));
        if (string.IsNullOrWhiteSpace(fileName))
        {
            fileName = "unnamed";
        }
        if (cut && fileName.Length > 40) //por que este número? 40
            fileName = fileName[0..39];
        int ind = 0;

#if DEBUG
        for (int i = 0; i < forbiden.Length; i++)
        {
            if (forbiden[i] < 31)
            { }
        }

#endif

        while (ind < fileName.Length)
        {
            bool isForbiden = false;
            for (int i = 0; i < forbiden.Length; i++)
            {
                if (fileName[ind] == forbiden[i])
                {
                    isForbiden = true;
                    break;
                }
            }
            if (!isForbiden)
            {
                for (int i = 0; i < 32; i++)
                {
                    if (fileName[ind] == i)
                    {
                        isForbiden = true;
                        break;
                    }
                }
            }
            if (isForbiden)
            {
#if DEBUG
                var tasrh = fileName[ind];
                var trash2 = fileName.Insert(ind, " ");
                var trash3 = fileName.Remove(ind, 1);
#endif
                fileName = fileName.Remove(ind, 1).Insert(ind, " ");
            }
            ind++;
        }
        return fileName;
    }
    public bool IsFilePathValid(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            return false;

        try
        {
            Path.Combine(filePath);
        }
        catch (Exception)
        {
            return false;
        }
        string? fileNameWithoutExtenstion;
        try
        {
            fileNameWithoutExtenstion = Path.GetFileNameWithoutExtension(filePath);
        }
        catch (Exception)
        {
            return false;
        }
        if (string.IsNullOrWhiteSpace(fileNameWithoutExtenstion))
            return false;

        FileAttributes? attr = null;
        //https://stackoverflow.com/questions/1395205/better-way-to-check-if-a-path-is-a-file-or-a-directory
        // get the file attributes for file or directory
        // o arquivo precisa existir
        try
        {
            attr = File.GetAttributes(filePath);
        }
        catch (Exception ex)
        {

        }
        if (attr is not null)
            return !attr.Value.HasFlag(FileAttributes.Directory);// || attr.HasFlag(FileAttributes.Normal);


        // if has trailing slash then it's a directory
        if (filePath.EndsWith(Path.DirectorySeparatorChar))
            return false; // ends with slash

        // if has extension then its a file; directory otherwise
        return !string.IsNullOrWhiteSpace(Path.GetExtension(filePath));
    }
    public bool IsDirectoryPathValid(string folderPath)
    {
        if (string.IsNullOrWhiteSpace(folderPath))
            return false;
        try
        {
            Path.Combine(folderPath);
        }
        catch (Exception)
        {
            return false;
        }


        FileAttributes? attr = null;
        //https://stackoverflow.com/questions/1395205/better-way-to-check-if-a-path-is-a-file-or-a-directory
        // get the file attributes for file or directory
        // o arquivo precisa existir
        try
        {
            attr = File.GetAttributes(folderPath);
        }
        catch (Exception ex)
        {

        }

        if (attr is not null)
            return attr.Value.HasFlag(FileAttributes.Directory);// || attr.HasFlag(FileAttributes.Normal);

        // if has trailing slash then it's a directory
        if (folderPath.EndsWith(Path.DirectorySeparatorChar))
            return true; // ends with slash

        // if has extension then its a file; directory otherwise
        return string.IsNullOrWhiteSpace(Path.GetExtension(folderPath));
    }
}
