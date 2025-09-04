using BaseLibrary.DependencyInjection;
using System.Text;

namespace BaseLibrary;

public class FileTXTIO : IFileTXTIO
{
    protected IFileServices fileServices;

    private int _delay = 250;
    private string? text;
    private string? _pathFile;
    private string? _fileBak => string.IsNullOrWhiteSpace(_pathFile) ? null : _pathFile + ".bak";

    private bool _isStayBak;
    private Task<bool>? tryWriteTask;
    private DateTime? lastWriteAttempt;
    public Exception eWrite { get; private set; }
    private static DateTime? lastReadAttempt;

    public string TextResult { get; private set; }
    public Exception eRead { get; private set; }

    public FileTXTIO(/*string? pathFile = null, bool isStayBak = false, int delay = 250, */IFileServices? fileServices = null)
    {
        this.fileServices = fileServices ?? Locator.ConstantContainer.Resolve<IFileServices>()!;

        //_delay = delay;
        //_pathFile = pathFile;
        //_isStayBak = isStayBak;
        eRead = null!;
        TextResult = null!;
        eWrite = null!;
    }
    public void SetStayBak(bool value) => _isStayBak = value;
    public void SetPathFile(string? pathFile)
    {
        if (pathFile == _pathFile)
            return;

        if (!string.IsNullOrWhiteSpace(_pathFile) && _isStayBak && !string.IsNullOrWhiteSpace(_fileBak))
        {
            if (File.Exists(_fileBak))
            {
                try
                {
                    File.Delete(_fileBak);
                }
                catch (Exception)
                {

                }
            }
        }
        _pathFile = pathFile;
    }
    public string? GetPathFile() => _pathFile;

    public void Closing()
    {
        if (_isStayBak && !string.IsNullOrWhiteSpace(_fileBak))
        {
            if (File.Exists(_fileBak))
            {
                try
                {
                    File.Delete(_fileBak);
                }
                catch (Exception)
                {

                }
            }
        }

        if (tryWriteTask is null || tryWriteTask.IsCompleted)
            return;

        tryWriteTask.Wait();

    }
    #region Write

    public bool BeforeWrite()
    {
        if (eWrite is not null)
        {
            eWrite = null!;
        }

        if (_pathFile is null)
        {
            eWrite = new ArgumentNullException("File name");
            return false;
        }
        // Verifica o ascesso ao arquivo original
        if (!VerifyFilesAcess(false))
        {
            return false;
        }

        //Verificar se o arquivo original está intacto
        try
        {
            if (File.Exists(_pathFile))
            {
                if (!fileServices.Check.CheckTextFile(_pathFile) /*&& !fileServices.Check.CheckTextFileByChars(_pathFile)*/)
                {
                    eWrite = new FileLoadException("Wrong file formart or violated file: " + _pathFile);
                    return false;
                }
                else
                {
                    using (StreamReader sr = new(_pathFile))
                    {
                        sr.ReadToEnd();
                    }
                }
            }
        }
        catch (Exception ex)
        {
            eWrite = ex;
            return false;
        }

        //Cria o arquivo bak
        bool hasAcess = false;
        if (File.Exists(_pathFile))
        {
            hasAcess = false;
            bool bakSucess = false;
            while (!bakSucess)
            {
                while (!hasAcess)
                {
                    try
                    {
                        if (File.Exists(_fileBak))
                        {
                            File.Delete(_fileBak);
                        }
                        File.Copy(_pathFile, _fileBak!);
                        hasAcess = true;
                    }
                    catch (IOException e)
                    {
                        if (lastWriteAttempt is null)
                        {
                            lastWriteAttempt = DateTime.Now;
                        }
                        else if ((DateTime.Now - ((DateTime)lastWriteAttempt)).TotalMilliseconds > 5 * _delay)
                        {
                            eWrite = e;
                            lastWriteAttempt = null;
                            return false;
                        }
                    }
                    catch (Exception e)
                    {
                        eWrite = e;
                        lastWriteAttempt = null;
                        return false;
                    }
                }
                //texta o arquivo bak
                try
                {
                    if (!fileServices.Check.CheckTextFile(_fileBak!) /*&& !fileServices.Check.CheckTextFileByChars(_fileBak!)*/)
                    {
                        File.Delete(_fileBak!);
                    }
                    else
                    {
                        using (StreamReader sr = new(_fileBak!))
                        {
                            sr.ReadToEnd();
                        }
                        bakSucess = true;
                    }
                }
                catch (Exception e)
                {
                    if (lastWriteAttempt is null)
                    {
                        lastWriteAttempt = DateTime.Now;
                    }
                    else if ((DateTime.Now - ((DateTime)lastWriteAttempt)).TotalMilliseconds > 5 * _delay)
                    {
                        eWrite = e;
                        lastWriteAttempt = null;
                        return false;
                    }
                }
            }
        }
        return true;
    }
    public StreamWriter GetStreamWriter()
    {
        return new StreamWriter(_pathFile, false, Encoding.UTF8);
    }
    public bool AfterWriteOrReader()
    {
        bool hasAcess = false;
        if (File.Exists(_fileBak))
        {
            if (!_isStayBak)
            {
                while (!hasAcess)
                {
                    try
                    {
                        File.Delete(_fileBak);
                        hasAcess = true;
                    }
                    catch (IOException)
                    {
                        if (lastWriteAttempt is null)
                        {
                            lastWriteAttempt = DateTime.Now;
                        }
                        else if ((DateTime.Now - ((DateTime)lastWriteAttempt)).TotalMilliseconds > 5 * _delay)
                        {
                            lastWriteAttempt = null;
                            hasAcess = true;
                        }
                    }
                    catch (Exception)
                    {
                        lastWriteAttempt = null;
                        hasAcess = true;
                    }
                }
            }
        }
        return true;
    }
    public bool ProcessWriterException(string parTXT, Exception ex)
    {
        if (lastWriteAttempt is null)
        {
            lastWriteAttempt = DateTime.Now;
        }
        else if ((DateTime.Now - ((DateTime)lastWriteAttempt)).TotalMilliseconds > 5 * _delay)
        {
            eWrite = ex;
            lastWriteAttempt = null;
            return false;
        }
        Task.Delay(_delay);
        return WriteTXT(parTXT);
    }
    public async Task<bool> WriteTXTAsync(string parTXT)
    {
        tryWriteTask = new Task<bool>(() => WriteTXT(parTXT));
        tryWriteTask.Start();
        await tryWriteTask;
        return tryWriteTask.Result;
    }
    public bool WriteTXT(string parTXT)
    {
        lock (this)
        {
            text = parTXT;
        }

        if (!BeforeWrite())
        {
            return false;
        }
        bool hasAcess = false;
        //Escreve o arquivo de fato
        try
        {
            using (StreamWriter sw = GetStreamWriter())
            {
                sw.Write(text);
                if (lastReadAttempt is not null)
                    lastReadAttempt = null;
            }
        }
        catch (Exception ex)
        {
            return ProcessWriterException(parTXT, ex);
        }
        finally
        {
            if (!AfterWriteOrReader())
            {

            }
        }
        return true;
    }
    #endregion
    #region Read
    public bool BeforeReader()
    {
        if (eRead is not null)
        {
            eRead = null!;
        }
        if (_pathFile is null)
        {
            eRead = new FileNotFoundException("Path file is null");
            return false;
        }
        if (!File.Exists(_pathFile))
        {
            eRead = new FileNotFoundException("Path file: " + _pathFile);
            return false;
        }

        //TODO Verificar o ascesso ao arquivo original e bak
        if (!VerifyFilesAcess(true))
        {
            return false;
        }
        //faz cópia do arquivo bak
        if (_isStayBak)
        {
            if (File.Exists(_fileBak))
            {
                try
                {
                    File.Delete(_fileBak);
                }
                catch (Exception ex)
                {
                    eRead = ex;
                    return false;
                }
            }
        }
        try
        {
            File.Copy(_pathFile, _fileBak, true);
        }
        catch (Exception ex)
        {
            eRead = ex;
            return false;
        }
        return true;
    }
    public StreamReader GetStreamReader()
    {
        return new StreamReader(_pathFile);
    }
    public bool ProcessReaderException(Exception ex)
    {
        if (lastReadAttempt is null)
        {
            lastReadAttempt = DateTime.Now;
        }
        else if ((DateTime.Now - ((DateTime)lastReadAttempt)).TotalMilliseconds > 5 * _delay / 2)
        {
            if (File.Exists(_fileBak))
            {
                bool hasAcess = false;
                while (!hasAcess)
                {
                    using (StreamReader sr = new(_fileBak))
                    {
                        TextResult = sr.ReadToEnd();

#pragma warning disable CS0168 // Variable is declared but never used
                        try
                        {
                            File.Delete(_pathFile);
                            File.Copy(_fileBak, _pathFile);
                            File.Delete(_fileBak);
                            hasAcess = true;
                            return true;
                        }
                        catch (IOException e)
                        {
                            if (lastWriteAttempt is null)
                            {
                                lastWriteAttempt = DateTime.Now;
                            }
                            else if ((DateTime.Now - ((DateTime)lastWriteAttempt)).TotalMilliseconds > 5 * _delay)
                            {
                                lastWriteAttempt = null;
                                hasAcess = true;
                            }
                        }
                        catch (Exception e)
                        {
                            lastWriteAttempt = null;
                            hasAcess = true;
                        }
#pragma warning restore CS0168 // Variable is declared but never used
                    }
                }

            }
            eRead = ex;
            lastReadAttempt = null;
            return false;
        }
        Task.Delay(_delay);
        return ReadTXT();
    }
    public async Task<bool> ReadTXTAsync()
    {
        Task<bool> task = new Task<bool>(() => ReadTXT());
        task.Start();
        await task;
        return task.Result;
    }
    public bool ReadTXT()
    {
        TextResult = null;

        if (!BeforeReader())
        {
            return false;
        }

        //Lê de fato o arquivo
        try
        {
            using (StreamReader sr = GetStreamReader())
            {
                if (sr is null)
                    return false;
                TextResult = sr.ReadToEnd();

                if (lastReadAttempt is not null)
                    lastReadAttempt = null;
                return true;
            }
        }
        catch (Exception ex)
        {
            return ProcessReaderException(ex);
        }
        finally
        {
            if (!AfterWriteOrReader())
            {

            }
        }
    }
    private static DateTime? lastAcessAttempt;
    //private Exception
    private bool VerifyFilesAcess(bool isFileRead)
    {
        //TODO Verificar o ascesso ao arquivo original
        bool hasAcess = false;
        while (!hasAcess)
        {
            try
            {
                if (File.Exists(_pathFile))
                {
                    using (StreamReader sr = new(_pathFile))
                    {
                        sr.ReadToEnd();
                    }
                }
                hasAcess = true;
            }
            catch (DirectoryNotFoundException e)
            {
                if (isFileRead)
                {
                    eRead = e;
                    lastAcessAttempt = null;
                    return false;
                }
            }
            catch (FileNotFoundException e)
            {
                if (isFileRead)
                {
                    eRead = e;
                    lastAcessAttempt = null;
                    return false;
                }
            }
            catch (IOException e)
            {
                if (lastAcessAttempt is null)
                {
                    lastAcessAttempt = DateTime.Now;
                }
                else if ((DateTime.Now - ((DateTime)lastAcessAttempt)).TotalMilliseconds > 5 * _delay)
                {
                    //TODO return a error
                    if (isFileRead)
                        eRead = e;
                    else
                        eWrite = e;
                    lastAcessAttempt = null;
                    return false;
                }
            }
            catch (Exception e)
            {
                if (isFileRead)
                    eRead = e;
                else
                    eWrite = e;
                lastAcessAttempt = null;
                return false;
            }
        }
        //verifica o acesso ao arquivo bak
        if (File.Exists(_fileBak))
        {
            hasAcess = false;
            while (!hasAcess)
            {
                try
                {
                    using (StreamReader sr = new(_fileBak))
                    {
                        sr.ReadToEnd();
                    }
                    hasAcess = true;
                }
                catch (IOException e)
                {
                    if (lastAcessAttempt is null)
                    {
                        lastAcessAttempt = DateTime.Now;
                    }
                    else if ((DateTime.Now - ((DateTime)lastAcessAttempt)).TotalMilliseconds > 5 * _delay)
                    {
                        if (isFileRead)
                            eRead = e;
                        else
                            eWrite = e;
                        lastAcessAttempt = null;
                        return false;
                    }
                }
                catch (Exception e)
                {
                    if (isFileRead)
                        eRead = e;
                    else
                        eWrite = e;
                    lastAcessAttempt = null;
                    return false;
                }
            }
        }
        lastAcessAttempt = null;
        return true;
    }
    #endregion
}