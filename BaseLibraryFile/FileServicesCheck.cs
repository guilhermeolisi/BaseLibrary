using FileTypeChecker;
using FileTypeChecker.Abstracts;
using FileTypeChecker.Extensions;
using System.IO.Compression;

namespace BaseLibrary;

public class FileServicesCheck : IFileServicesCheck
{
    public bool CheckZipFile(string path)
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
    public bool CheckTextFileByChars(string filePath)
    {
        //https://stackoverflow.com/questions/4744890/c-sharp-check-if-file-is-text-based
        if (!File.Exists(filePath))
            return false;
        const int charsToCheck = 8000;
        const char nulChar = '\0';

        int nulCount = 0;

        using (var streamReader = new StreamReader(filePath))
        {
            for (var i = 0; i < charsToCheck; i++)
            {
                if (streamReader.EndOfStream)
                    return true;

#if DEBUG
                var char1 = streamReader.Read();
                var char2 = (char)char1;
                if (char2 == nulChar)
#else
                if ((char)streamReader.Read() == nulChar)
#endif

                {

                    nulCount++;

                    if (nulCount >= 1) //verificar quantos characteres diferentes consecutivos devem ser consideraods um binário
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
    public bool CheckTextFileByEncoding(string filePath)
    {
        if (!File.Exists(filePath))
            return false;
        var result = TextFileEncodingDetector.DetectTextFileEncoding(filePath);

        return result is not null;
    }
    public bool CheckTextFile(string filePath)
    {
        return CheckTextFileByEncoding(filePath) || CheckTextFileByChars(filePath);
    }
    public bool CheckImageFile(string path)
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

                IFileType fileType = FileTypeValidator.GetFileType(fileStream);
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
}
