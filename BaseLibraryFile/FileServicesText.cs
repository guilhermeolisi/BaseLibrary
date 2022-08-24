using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseLibrary;

public class FileServicesText : IFileServicesText
{
    public bool WriteTXT(string pathFile, in string parTXT)
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
    public async Task<bool> WriteTXTAsync(string pathFile, string parTXT)
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
    private const string tmpExt = ".tmp";
    private void BakWriteBegin(string pathFile)
    {
        if (File.Exists(pathFile))
        {
            File.Copy(pathFile, pathFile + tmpExt, true);
        }
    }
    private void BakWriteEnd(string pathFile, bool returnBak)
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
        if (File.Exists(pathFile + tmpExt) && File.Exists(pathFile) && File.GetLastWriteTime(pathFile) > File.GetCreationTime(pathFile + tmpExt))
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
    public string? ReadTXT(string pathFile)
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
    public async Task<string?> ReadTXTAsync(string pathFile)
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
    private string BakReadBegin(string pathFile)
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
    private void BakReadEnd(string pathFile)
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
}
