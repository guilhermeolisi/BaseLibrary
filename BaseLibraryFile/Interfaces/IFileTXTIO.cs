﻿namespace BaseLibrary;

public interface IFileTXTIO
{
    void SetStayBak(bool value);
    void SetPathFile(string? pathFile);
    string? GetPathFile();
    void Closing();
    Exception eWrite { get; }
    //bool BeforeWrite();
    //StreamWriter GetStreamWriter();
    //bool AfterWriteOrReader();
    //bool ProcessWriterException(string parTXT, Exception ex);
    Task<bool> WriteTXTAsync(string parTXT);
    bool WriteTXT(string parTXT);
    string TextResult { get; }
    Exception eRead { get; }
    //bool BeforeReader();
    //StreamReader GetStreamReader();
    //bool ProcessReaderException(Exception ex);
    Task<bool> ReadTXTAsync();
    bool ReadTXT();
    //bool VerifyFilesAcess(bool isFileRead);
}