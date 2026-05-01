using BaseLibrary.DependencyInjection;
using System.Text;

namespace BaseLibrary;

public class FileTXTIO : IFileTXTIO
{
    protected IFileServices fileServices;

    private int _delay = 250;
    private int WriteTimeoutMs => 5 * _delay;
    private int ReadTimeoutMs => 5 * _delay / 2;
    private string? text;
    private string? _pathFile;
    private string? _fileBak => string.IsNullOrWhiteSpace(_pathFile) ? null : _pathFile + ".bak";

    private bool _isStayBak;
    private Task<bool>? tryWriteTask;
    private DateTime? lastWriteAttempt;
    public Exception eWrite { get; private set; }
    // BUG FIX: was static — shared across all instances causing concurrency issues
    private DateTime? lastReadAttempt;

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
    /// <summary>Sets the base retry delay in milliseconds. Primarily used in tests to reduce wait times.</summary>
    public void SetDelay(int milliseconds) => _delay = milliseconds;
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
        eWrite = null!;
        // BUG FIX: clear stale retry state so the previous failure timestamp cannot cause
        // an immediate timeout on the very first IOException of a new write call
        lastWriteAttempt = null;

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
        if (File.Exists(_pathFile))
        {
            bool bakSucess = false;
            while (!bakSucess)
            {
                // BUG FIX: declare hasAcess inside the outer loop so it resets each iteration.
                // Previously it was declared outside and never reset after deleting an invalid bak,
                // causing the inner copy loop to be skipped forever (infinite outer loop doing nothing).
                bool hasAcess = false;
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
                        else if ((DateTime.Now - lastWriteAttempt.Value).TotalMilliseconds > WriteTimeoutMs)
                        {
                            eWrite = e;
                            lastWriteAttempt = null;
                            return false;
                        }
                        else
                        {
                            Thread.Sleep(_delay); // BUG FIX: prevent busy-wait spin
                        }
                    }
                    catch (Exception e)
                    {
                        eWrite = e;
                        lastWriteAttempt = null;
                        return false;
                    }
                }
                //testa o arquivo bak
                try
                {
                    if (!fileServices.Check.CheckTextFile(_fileBak!) /*&& !fileServices.Check.CheckTextFileByChars(_fileBak!)*/)
                    {
                        try { File.Delete(_fileBak!); } catch { /* ignore — will be recreated next iteration */ }
                        // BUG FIX: track repeated validation failures so the outer loop can time out
                        if (lastWriteAttempt is null)
                        {
                            lastWriteAttempt = DateTime.Now;
                        }
                        else if ((DateTime.Now - lastWriteAttempt.Value).TotalMilliseconds > WriteTimeoutMs)
                        {
                            eWrite = new InvalidDataException("Bak file is consistently invalid: " + _fileBak);
                            lastWriteAttempt = null;
                            return false;
                        }
                        else
                        {
                            Thread.Sleep(_delay); // BUG FIX: prevent busy-wait spin
                        }
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
                    else if ((DateTime.Now - lastWriteAttempt.Value).TotalMilliseconds > WriteTimeoutMs)
                    {
                        eWrite = e;
                        lastWriteAttempt = null;
                        return false;
                    }
                    else
                    {
                        Thread.Sleep(_delay); // BUG FIX: prevent busy-wait spin
                    }
                }
            }
            lastWriteAttempt = null;
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
                        else if ((DateTime.Now - lastWriteAttempt.Value).TotalMilliseconds > WriteTimeoutMs)
                        {
                            lastWriteAttempt = null;
                            hasAcess = true;
                        }
                        else
                        {
                            Thread.Sleep(_delay); // BUG FIX: prevent busy-wait spin
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
        else if ((DateTime.Now - lastWriteAttempt.Value).TotalMilliseconds > WriteTimeoutMs)
        {
            eWrite = ex;
            lastWriteAttempt = null;
            return false;
        }
        Thread.Sleep(_delay); // BUG FIX: Task.Delay was not awaited so had no effect
        return true; // BUG FIX: return true to signal WriteTXT to retry (was recursively calling WriteTXT causing stack overflow risk)
    }
    public async Task<bool> WriteTXTAsync(string parTXT)
    {
        // BUG FIX: new Task<bool>(() => ...).Start() is an outdated pattern; use Task.Run for thread-pool execution
        tryWriteTask = Task.Run(() => WriteTXT(parTXT));
        return await tryWriteTask.ConfigureAwait(false);
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
        // BUG FIX: retry loop replaces the recursive ProcessWriterException → WriteTXT → ProcessWriterException
        // chain that risked a stack overflow on many retries
        try
        {
            while (true)
            {
                try
                {
                    using (StreamWriter sw = GetStreamWriter())
                    {
                        sw.Write(parTXT); // use the local parameter, not the instance field, to avoid data races
                        lastWriteAttempt = null;
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    if (!ProcessWriterException(parTXT, ex))
                        return false;
                }
            }
        }
        finally
        {
            AfterWriteOrReader();
        }
    }
    #endregion
    #region Read
    public bool BeforeReader()
    {
        eRead = null!;
        // BUG FIX: clear stale retry state so the previous failure timestamp cannot cause
        // an immediate timeout on the very first exception of a new read call
        lastReadAttempt = null;

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
            File.Copy(_pathFile, _fileBak!, true);
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
        //Verifica a codificação do arquivo
        Encoding? encoding = fileServices.Check.DetectTextFileEncoding(_pathFile!);
#if DEBUG
        if (encoding != Encoding.UTF8 && Path.GetExtension(_pathFile)?.ToUpper() != ".XML")
        {
        }
        Encoding temp2;
        using (var temp = new StreamReader(_pathFile!, detectEncodingFromByteOrderMarks: true))
        {
            temp2 = temp.CurrentEncoding;
        }
        Encoding? temp3;
        if (encoding is null)
        {
            temp3 = fileServices.Check.DetectTextFileEncodingGOS(_pathFile!);
        }
#endif
        if (encoding is null && Path.GetExtension(_pathFile)?.ToUpper() != ".XML")
        {
            encoding = fileServices.Check.DetectTextFileEncodingGOS(_pathFile!);
        }
        if (encoding is null)
        {
            return new StreamReader(_pathFile!, detectEncodingFromByteOrderMarks: true);
        }
        else
        {
            return new StreamReader(_pathFile!, encoding ?? Encoding.UTF8);
        }
    }
    public bool ProcessReaderException(Exception ex)
    {
        if (lastReadAttempt is null)
        {
            lastReadAttempt = DateTime.Now;
        }
        else if ((DateTime.Now - lastReadAttempt.Value).TotalMilliseconds > ReadTimeoutMs)
        {
            string? fileBak = _fileBak; // capture once — _fileBak is a computed property that can return null
            if (fileBak is not null && File.Exists(fileBak))
            {
                // BUG FIX: wrap bak read in try/catch — StreamReader creation was outside any try block
                try
                {
                    TextResult = File.ReadAllText(fileBak);
                    // BUG FIX: use File.Replace for atomic restore instead of Delete+Copy which can lose
                    // both files if the Copy fails after the Delete
                    try
                    {
                        if (File.Exists(_pathFile))
                            File.Replace(fileBak, _pathFile!, null);
                        else
                            File.Move(fileBak, _pathFile!);
                    }
                    catch (Exception restoreEx)
                    {
                        // Restoration failed: preserve the bak so AfterWriteOrReader() does not delete the
                        // last good copy, and surface the error so the caller knows the original is not restored.
                        _isStayBak = true;
                        eRead = restoreEx;
                        lastReadAttempt = null;
                        return false; // stop retrying — backup was read but original could not be restored
                    }
                    lastReadAttempt = null;
                    return false; // stop retrying — TextResult is set and original was restored from bak
                }
                catch (Exception recoveryEx)
                {
                    eRead = recoveryEx;
                    lastReadAttempt = null;
                    return false; // stop retrying — recovery from bak also failed
                }
            }
            eRead = ex;
            lastReadAttempt = null;
            return false; // stop retrying — no bak available
        }
        Thread.Sleep(_delay); // BUG FIX: Task.Delay was not awaited so had no effect
        return true; // BUG FIX: return true to signal ReadTXT to retry (was recursively calling ReadTXT causing stack overflow risk)
    }
    public async Task<bool> ReadTXTAsync()
    {
        // BUG FIX: new Task<bool>(() => ...).Start() is an outdated pattern; use Task.Run for thread-pool execution
        return await Task.Run(() => ReadTXT()).ConfigureAwait(false);
    }
    public bool ReadTXT()
    {
        TextResult = null!;

        if (!BeforeReader())
        {
            return false;
        }

        //Lê de fato o arquivo
        // BUG FIX: retry loop replaces the recursive ProcessReaderException → ReadTXT → ProcessReaderException
        // chain that risked a stack overflow on many retries
        try
        {
            while (true)
            {
                try
                {
                    using (StreamReader sr = GetStreamReader())
                    {
                        TextResult = sr.ReadToEnd();
                        lastReadAttempt = null;
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    if (!ProcessReaderException(ex))
                        // false means stop: if eRead is null recovery succeeded (TextResult set), otherwise failed
                        return eRead is null;
                }
            }
        }
        finally
        {
            AfterWriteOrReader();
        }
    }
    // BUG FIX: was static — shared across all instances causing concurrency issues with retry timing
    private DateTime? lastAcessAttempt;
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
                // BUG FIX: for write, this was silently ignored → infinite loop.
                // A missing directory is always fatal regardless of direction.
                if (isFileRead)
                    eRead = e;
                else
                    eWrite = e;
                lastAcessAttempt = null;
                return false;
            }
            catch (FileNotFoundException e)
            {
                if (isFileRead)
                {
                    eRead = e;
                    lastAcessAttempt = null;
                    return false;
                }
                else
                {
                    // BUG FIX: for write, a missing file is OK — we will create it.
                    // Previously this fell through without setting hasAcess → infinite loop.
                    hasAcess = true;
                }
            }
            catch (IOException e)
            {
                if (lastAcessAttempt is null)
                {
                    lastAcessAttempt = DateTime.Now;
                }
                else if ((DateTime.Now - lastAcessAttempt.Value).TotalMilliseconds > WriteTimeoutMs)
                {
                    //TODO return a error
                    if (isFileRead)
                        eRead = e;
                    else
                        eWrite = e;
                    lastAcessAttempt = null;
                    return false;
                }
                else
                {
                    Thread.Sleep(_delay); // BUG FIX: prevent busy-wait spin
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
                    else if ((DateTime.Now - lastAcessAttempt.Value).TotalMilliseconds > WriteTimeoutMs)
                    {
                        if (isFileRead)
                            eRead = e;
                        else
                            eWrite = e;
                        lastAcessAttempt = null;
                        return false;
                    }
                    else
                    {
                        Thread.Sleep(_delay); // BUG FIX: prevent busy-wait spin
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