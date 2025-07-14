using System.Text;

namespace BaseLibrary;

public class FileServicesText : IFileServicesText
{
    public bool WriteTXT(string pathFile, string parTXT)
    {
        if (string.IsNullOrWhiteSpace(pathFile))
            throw new ArgumentNullException(nameof(pathFile));
        if (parTXT is null)
            throw new ArgumentNullException(nameof(parTXT));

        if (!Directory.Exists(Path.GetDirectoryName(pathFile)))
        {
            return false;
        }

        short count = 0;
        bool isCont = true;
        bool returnBak = false;


        bool isnew = !File.Exists(pathFile);

        try
        {
            BakWriteBegin(pathFile);
            while (isCont && count < 3)
            {
                count++;
                isCont = false;
#pragma warning disable CS0168 // Variable is declared but never used
                try
                {
                    using (StreamWriter sw = new StreamWriter(pathFile, false, Encoding.UTF8))
                    {
                        sw.Write(parTXT);
                    }
                    DateTime now = DateTime.Now;
                    if (isnew)
                    {
                        File.SetCreationTime(pathFile, now);
                    }
                    File.SetLastWriteTime(pathFile, now);
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
#pragma warning restore CS0168 // Variable is declared but never used
            }
        }
        finally
        {
            BakWriteEnd(pathFile, returnBak);
        }

#if DEBUG
        if (File.Exists(pathFile + tmpExt))
        {

        }
#endif

        return false;
    }
    public async Task<bool> WriteTXTAsync(string pathFile, string parTXT)
    {
        if (string.IsNullOrWhiteSpace(pathFile))
            throw new ArgumentNullException(nameof(pathFile));
        if (parTXT is null)
            throw new ArgumentNullException(nameof(parTXT));

        short count = 0;
        bool isCont = true;
        bool returnBak = false;

        bool isnew = !File.Exists(pathFile);

        try
        {
            BakWriteBegin(pathFile);
            while (isCont && count < 10)
            {
                count++;
                isCont = false;
#pragma warning disable CS0168 // Variable is declared but never used
                try
                {
                    using (StreamWriter sw = new StreamWriter(pathFile, false, Encoding.UTF8))
                    {
                        await sw.WriteAsync(parTXT);
                        returnBak = false;
                    }
                    DateTime now = DateTime.Now;
                    if (isnew)
                    {
                        File.SetCreationTime(pathFile, now);
                    }
                    File.SetLastWriteTime(pathFile, now);
                    return true;
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
#pragma warning restore CS0168 // Variable is declared but never used
            }
        }
        finally
        {
            BakWriteEnd(pathFile, returnBak);
        }

#if DEBUG
        if (File.Exists(pathFile + tmpExt))
        {

        }
#endif

        return false;
    }
    private const string tmpExt = ".tmp";
    private void BakWriteBegin(string pathFile)
    {
        if (File.Exists(pathFile))
        {
            File.Copy(pathFile, pathFile + tmpExt, true);
            File.SetCreationTime(pathFile + tmpExt, DateTime.Now);
        }
    }
    private void BakWriteEnd(string pathFile, bool returnBak)
    {
        if (returnBak)
        {
            if (File.Exists(pathFile + tmpExt))
            {
#pragma warning disable CS0168 // Variable is declared but never used
                try
                {
                    DateTime creation = File.GetCreationTime(pathFile + tmpExt);
                    DateTime lastWrite = File.GetLastWriteTime(pathFile + tmpExt);
                    File.Copy(pathFile + tmpExt, pathFile, true);
#if DEBUG
                    var trash = File.GetCreationTime(pathFile);
                    var trash2 = File.GetLastWriteTime(pathFile);
#endif

                    if (File.GetCreationTime(pathFile) != creation)
                    {
                        File.SetCreationTime(pathFile, creation);
                    }
                    if (File.GetLastWriteTime(pathFile) != creation)
                    {
                        File.SetLastWriteTime(pathFile, creation);
                    }
                }
                catch (Exception e)
                {
                    //TODO verificar o que fazer
                }
#pragma warning restore CS0168 // Variable is declared but never used
            }
        }
        if (File.Exists(pathFile + tmpExt) && File.Exists(pathFile) && File.GetLastWriteTime(pathFile) > File.GetCreationTime(pathFile + tmpExt))
        {
#pragma warning disable CS0168 // Variable is declared but never used
            try
            {
                File.Delete(pathFile + tmpExt);
            }
            catch (Exception e)
            {

            }
#pragma warning restore CS0168 // Variable is declared but never used
        }
    }
    public string? ReadTXT(string pathFile)
    {
        if (string.IsNullOrWhiteSpace(pathFile))
            throw new ArgumentNullException(nameof(pathFile));


        short count = 0;
        bool isCont = true;
        string fileTemp = BakReadBegin(pathFile);
        if (fileTemp == pathFile + tmpExt && !File.Exists(Path.GetDirectoryName(pathFile)))
        {
            return null;
        }
        try
        {
            while (isCont && count < 3)
            {
                count++;
                isCont = false;
#pragma warning disable CS0168 // Variable is declared but never used
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
#pragma warning restore CS0168 // Variable is declared but never used
            }
        }
        finally
        {
            BakReadEnd(pathFile);
        }

#if DEBUG
        if (File.Exists(pathFile + tmpExt))
        {

        }
#endif

        return null;
    }
    public async Task<string?> ReadTXTAsync(string pathFile)
    {
        if (string.IsNullOrWhiteSpace(pathFile))
            throw new ArgumentNullException(nameof(pathFile));

        short count = 0;
        bool isCont = true;
        string fileTemp = BakReadBegin(pathFile);
        if (fileTemp == pathFile + tmpExt && !File.Exists(Path.GetDirectoryName(pathFile)))
        {
            return null;
        }
        try
        {
            while (isCont && count < 10)
            {
                count++;
                isCont = false;
#pragma warning disable CS0168 // Variable is declared but never used
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
#pragma warning restore CS0168 // Variable is declared but never used
            }
        }
        finally
        {
            BakReadEnd(pathFile);
        }

#if DEBUG
        if (File.Exists(pathFile + tmpExt))
        {

        }
#endif
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

        if (File.Exists(pathFile + tmpExt) && (!File.Exists(pathFile)) || File.GetCreationTime(pathFile + tmpExt) > File.GetLastWriteTime(pathFile))
        {
#pragma warning disable CS0168 // Variable is declared but never used
            try
            {
                DateTime creation = File.GetCreationTime(pathFile + tmpExt);
                DateTime lastWrite = File.GetLastWriteTime(pathFile + tmpExt);

                File.Move(pathFile + tmpExt, pathFile, true);
#if DEBUG
                var trash10 = File.GetCreationTime(pathFile);
                var trash12 = File.GetLastWriteTime(pathFile);
#endif

                if (File.GetCreationTime(pathFile) != creation)
                {
                    File.SetCreationTime(pathFile, creation);
                }
                if (File.GetLastWriteTime(pathFile) != creation)
                {
                    File.SetLastWriteTime(pathFile, creation);
                }

            }
            catch (Exception e)
            {
                return pathFile + tmpExt;
            }
#pragma warning restore CS0168 // Variable is declared but never used
        }
        return pathFile;
    }
    private void BakReadEnd(string pathFile)
    {
        if (File.Exists(pathFile + tmpExt) && (!File.Exists(pathFile)) || File.GetCreationTime(pathFile + tmpExt) > File.GetLastWriteTime(pathFile))
        {
#pragma warning disable CS0168 // Variable is declared but never used
            try
            {
                DateTime creation = File.GetCreationTime(pathFile + tmpExt);
                DateTime lastWrite = File.GetLastWriteTime(pathFile + tmpExt);

                File.Move(pathFile + tmpExt, pathFile, true);

#if DEBUG
                var trash10 = File.GetCreationTime(pathFile);
                var trash12 = File.GetLastWriteTime(pathFile);
#endif

                if (File.GetCreationTime(pathFile) != creation)
                {
                    File.SetCreationTime(pathFile, creation);
                }
                if (File.GetLastWriteTime(pathFile) != creation)
                {
                    File.SetLastWriteTime(pathFile, creation);
                }

            }
            catch (Exception e)
            {
                return;
            }
#pragma warning restore CS0168 // Variable is declared but never used
        }
        if (File.Exists(pathFile + tmpExt) && File.Exists(pathFile) && File.GetCreationTime(pathFile + tmpExt) < File.GetLastWriteTime(pathFile))
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
}
