﻿using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace BaseLibrary;

public class FileTXTIO
{
    private int _delay = 500;
    private string text;
    private string? _pathFile;
    private string _fileBak => string.IsNullOrWhiteSpace(_pathFile) ? null : _pathFile + ".bak";
    private bool _isStayBak;
    public bool? isOK;
    public FileTXTIO(string pathFile, bool isStayBak, int delay = 250)
    {
        _delay = delay;
        _pathFile = pathFile;
        _isStayBak = isStayBak;
    }
    public void SetPathFile(string? pathFile)
    {
        if (_isStayBak && !string.IsNullOrWhiteSpace(_fileBak))
        {
            if (File.Exists(_fileBak))
            {
                try
                {
                    File.Delete(_fileBak);
                }
                catch (Exception ex)
                {

                }
            }
        }
        _pathFile = pathFile;
    }
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
                catch (Exception ex)
                {

                }
            }
        }

        if (tryWriteTask is null || tryWriteTask.IsCompleted)
            return;

        tryWriteTask.Wait();

    }
    #region Write
    private Task tryWriteTask;
    private static DateTime? lastWriteAttempt;
    public Exception eWrite;
    public async Task<bool> WriteTXTAsync(string parTXT)
    {
        Task<bool> task = new Task<bool>(() => WriteTXT(parTXT));
        task.Start();
        await task;
        return task.Result;
    }
    public bool WriteTXT(string parTXT)
    {
        if (_pathFile is null)
        {
            eWrite = new ArgumentNullException("File name");
            return false;
        }
        lock (this)
        {
            text = parTXT;
        }

        //TODO Verificar o ascesso ao arquivo original
        if (!VerifyFilesAcess(false))
        {
            return false;
        }

        //Verificar se o arquivo original está intacto
        try
        {
            if (File.Exists(_pathFile))
            {
                if (!FileMethods.CheckTextFile(_pathFile))
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
                        File.Copy(_pathFile, _fileBak);
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
                    if (!FileMethods.CheckTextFile(_fileBak))
                    {
                        File.Delete(_fileBak);
                    }
                    else
                    {
                        using (StreamReader sr = new(_fileBak))
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

        //Escreve o arquivo de fato
        try
        {
            using (StreamWriter sw = new StreamWriter(_pathFile, false, Encoding.UTF8))
            {
                sw.Write(text);
                if (File.Exists(_fileBak))
                {
                    if (!_isStayBak)
                    {
                        hasAcess = false;
                        while (!hasAcess)
                        {
                            try
                            {
                                File.Delete(_fileBak);
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
                                    lastWriteAttempt = null;
                                    hasAcess = true;
                                }
                            }
                            catch (Exception e)
                            {
                                lastWriteAttempt = null;
                                hasAcess = true;
                            }
                        }
                    }
                }
                return true;
            }
        }
        catch (Exception ex)
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
    }
    #endregion
    #region Read
    private static DateTime? lastReadAttempt;
    public string TextResult;
    public Exception eRead;
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

        if (_isStayBak)
        {
            try
            {
                if (File.Exists(_fileBak))
                {
                    File.Delete(_fileBak);
                }
                File.Copy(_pathFile, _fileBak);
            }
            catch (Exception ex)
            {
                eRead = ex;
                return false;
            }
        }
        try
        {
            using (StreamReader sr = new(_pathFile))
            {
                TextResult = sr.ReadToEnd();

                if (lastReadAttempt is not null)
                    lastReadAttempt = null;
                return true;
            }
        }
        catch (Exception ex)
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