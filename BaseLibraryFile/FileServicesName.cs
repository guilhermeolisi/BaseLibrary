using BaseLibrary.Text;
using System.Text.RegularExpressions;

namespace BaseLibrary;

public class FileServicesName : IFileServicesName
{

    public void InfoFromFilePath(in string filepath, ref string fileName, ref string folder, ref string extension)
    {
        if (string.IsNullOrWhiteSpace(filepath)) return;

        string regexTemp = string.Empty;
        regexTemp = @"^(?<Folder>.+)\\(?<FileName>[^\\/|<>*:""?]+?)(\.(?<Extension>[^\\/|<>*:""?]+?))?$";
        Match m = Regex.Match(filepath, regexTemp);
        fileName = m.Groups["FileName"].Value;
        folder = m.Groups["Folder"].Value;
        extension = m.Groups["Extension"].Value;
    }
    public (string?, string?, string?) InfoFromFilePath(in string filepath)
    {
        if (string.IsNullOrWhiteSpace(filepath)) return (null, null, null);

        string regexTemp = string.Empty;
        regexTemp = @"^(?<Folder>.+)\\(?<FileName>[^\\/|<>*:""?]+)\.(?<Extension>[^\\/|<>*:""?]+?)$";
        Match m = Regex.Match(filepath, regexTemp);
        return (m.Groups["FileName"].Value, m.Groups["Folder"].Value, m.Groups["Extension"].Value);
    }
    public void InfoFromFolderPath(in string filepath, ref string actualFolder, ref string folderParent)
    {
        if (string.IsNullOrWhiteSpace(filepath)) return;

        string regexTemp = string.Empty;
        regexTemp = @"^(?<FolderParent>.+)\\(?<FolderName>[^\\/|<>*:""?]+)$";
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

        //string fileName = Path.GetFileNameWithoutExtension(fullPath).Trim();
        //string sufix = mode switch
        //{
        //    'C' => "Copied",
        //    'E' => "Exported",
        //    'I' => "Imported",
        //    'M' => "Moved",
        //    _ => ""
        //};

        //int ind = fileName.Length - 1;
        //if (fileName[ind] == ')')
        //{
        //    bool foundIndex = false;
        //    while (ind >= 0 && (fileName[ind] == ')' || char.IsDigit(fileName[ind]) || fileName[ind] == '(' || fileName[ind] == ' '))
        //    {
        //        if (fileName[ind] == '(')
        //        {
        //            foundIndex = true;
        //        }
        //        ind--;
        //    }
        //    ind++;
        //    if (foundIndex && ind > 0)
        //    {
        //        fileName = fileName[..ind];
        //    }
        //}
        //ind = fileName.Length - 1;
        //if (!string.IsNullOrWhiteSpace(sufix) && fileName.Contains(sufix))
        //{
        //    int indSurfix = sufix.Length - 1;
        //    while (ind >= 0 && ((indSurfix >= 0 && fileName[ind] == sufix[indSurfix]) || fileName[ind] == ' ' || fileName[ind] == '-'))
        //    {
        //        ind--;
        //        indSurfix--;
        //    }
        //    ind++;
        //    if (indSurfix < 0 && ind > 0)
        //    {
        //        fileName = fileName[..ind];
        //    }
        //}

        int index = 1;
        while (File.Exists(fullPath))
        {
            //fullPath = Path.Combine(dir, string.Format("{0}{1}{2}{3}", fileName, string.IsNullOrWhiteSpace(sufix) ? "" : " - " + sufix, index == 1 ? "" : " (" + index + ")", exten));
            fullPath = Path.Combine(dir, fileName.CombineNameAndSufix(sufix, index) + exten);
            index++;
        }
        return fullPath;
    }
    static readonly char[] forbiden = new char[] { '/', '\\', '>', '<', ':', '*', '?', '"', '|' };
    public string CutCharacterFileForbiden(string fileName, bool cut)
    {
        if (fileName is null)
            throw new ArgumentNullException(nameof(fileName));
        if (string.IsNullOrWhiteSpace(fileName))
        {
            fileName = "unnamed";
        }
        if (cut && fileName.Length > 30)
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
}
