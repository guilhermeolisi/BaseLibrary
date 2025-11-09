using BaseLibrary.DependencyInjection;
using System.Diagnostics;
using System.IO.Compression;

namespace BaseLibrary;

public class FileServicesFile : IFileServicesFile
{
    IFileServicesDirectory directory;
    public FileServicesFile(IFileServicesDirectory? directory = null)
    {
        this.directory = directory ?? Locator.ConstantContainer.Resolve<IFileServicesDirectory>()! ?? new FileServicesDirectory();
    }
    public void CopyConserveTime(string original, string destination, bool preservetime = true)
    {
        File.Copy(original, destination);
        File.SetCreationTime(destination, File.GetCreationTime(original));
        File.SetLastWriteTime(destination, File.GetLastWriteTime(original));
        File.SetLastAccessTime(destination, File.GetLastAccessTime(original));
    }
    /// <summary>
    /// Get the file size in bytes
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns>size in bytes</returns>
    public long FileSize(string filePath) => new FileInfo(filePath).Length;
    public DateTime FileLastModification(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException(nameof(filePath));

        DateTime created = File.GetCreationTime(filePath);
        DateTime writed = File.GetLastWriteTime(filePath);

        return created > writed ? created : writed;
    }
    public void ExtractZipConserveTime(string zipPath, string extractPath)
    {
        using (ZipArchive archive = ZipFile.OpenRead(zipPath))
        {
            foreach (ZipArchiveEntry entry in archive.Entries)
            {
                // Gets the full path to ensure that relative segments are removed.
                string destinationPath = Path.GetFullPath(Path.Combine(extractPath, entry.FullName));

                string folder = Path.GetDirectoryName(destinationPath);
                directory.CreatAllPath(folder);

                // Ordinal match is safest, case-sensitive volumes can be mounted within volumes that
                // are case-insensitive.
                if (destinationPath.StartsWith(extractPath, StringComparison.Ordinal))
                    entry.ExtractToFile(destinationPath);
            }
        }
    }
    /// <summary>
    /// Open a file in external application
    /// </summary>
    /// <param name="filePath">File path to open</param>
    /// <param name="OS">0: Windows, 1: Linux, 2: Mac</param>
    /// <exception cref="ArgumentException"></exception>
    public void OpenFile(string filePath, sbyte OS)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("The string can not be null or empty", nameof(filePath));
        if (!File.Exists(filePath))
            throw new ArgumentException("The file does not exist", nameof(filePath));

        //System.Diagnostics.Process.Start(filePath);

        using (Process process = new())
        {
            string commandProcess = null;
            string argsProcess = "";

            switch (OS)
            {
                case 0:
                    commandProcess = "cmd.exe";
                    argsProcess = "/C \"" + filePath + "\"";
                    break;
                case 1:
                    commandProcess = "xdg-open";
                    argsProcess = "\"" + filePath + "\"";
                    break;
                case 2:
                    commandProcess = "open";
                    argsProcess = "\"" + filePath + "\"";
                    break;
                default:
                    break;
            }

            process.StartInfo = new()
            {
                FileName = commandProcess,
                UseShellExecute = true,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                //WorkingDirectory = sindarinFolder,
                Arguments = argsProcess
            };

            process.Start();
        }
    }
    public bool VerifyIfEncrypted(string filePath)
    {
        FileAttributes attrs = File.GetAttributes(filePath);

        bool fileIsEncrypted = (attrs & FileAttributes.Encrypted) != 0;
        return fileIsEncrypted;

    }
    /// <summary>
    /// Tem que ser usado dentro de um Try Catch
    /// </summary>
    /// <param name="sourcePath"></param>
    /// <param name="destinationFolder"></param>
    /// <param name="anEFSFolder"></param>
    /// <exception cref="InvalidOperationException"></exception>
    public void CopyDecriptingFile(string sourcePath, string destinationFolder, string anEFSFolder)
    {
        if (string.IsNullOrWhiteSpace(anEFSFolder))
            throw new ArgumentException("The EFS folder path cannot be null or empty.", nameof(anEFSFolder));

        string fileName = Path.GetFileName(sourcePath);
        string filePathApp = Path.Combine(anEFSFolder, fileName);
        string filePathDestination = Path.Combine(destinationFolder, fileName);
        //Copia para a pasta App que supostamente seria no disco encriptografado
        try
        {
            File.Copy(sourcePath, filePathApp, true);
        }
        catch (Exception ex)
        {

        }
        // Descriptografa o arquivo copiado
        try
        {
            File.Decrypt(filePathApp);
        }
        catch (Exception ex)
        {
            // Handle exception if decryption fails
            throw new InvalidOperationException("Failed to decrypt the file after copying.", ex);
        }

        // Copia o arquivo descriptografado para a pasta de destino
        try
        {
            File.Copy(filePathApp, filePathDestination, true);
        }
        catch (Exception ex)
        {
            // Handle exception if copy fails
            throw new InvalidOperationException("Failed to copy the decrypted file to the destination folder.", ex);
        }

        // Remove o arquivo da pasta App
        try
        {
            File.Delete(filePathApp);
        }
        catch (Exception ex)
        {
            // Handle exception if deletion fails
            throw new InvalidOperationException("Failed to delete the temporary file in the App folder.", ex);
        }
    }
    /// <summary>
    /// Precisa estar dentro de um Try Catch
    /// </summary>
    /// <param name="sourcePath"></param>
    /// <param name="destinationFolder"></param>
    /// <param name="anEFSFolder"></param>
    /// <exception cref="InvalidOperationException"></exception>
    public void CopyFileCarefulEncription(string sourcePath, string destinationFolder, string anEFSFolder)
    {
        bool isEncrypted = false;
        try
        {
            File.Copy(sourcePath, destinationFolder, true);
        }
        catch (IOException ex)
        {
            isEncrypted = VerifyIfEncrypted(sourcePath);
            if (!isEncrypted)
            {
                throw new IOException("Failed to copy the file to the destination folder.", ex);
            }
        }
        if (isEncrypted)
        {
            try
            {
                CopyDecriptingFile(sourcePath, destinationFolder, anEFSFolder);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to copy the encrypted file carefully.", ex);
            }
        }
    }
    private static readonly TimeSpan delayVerification = TimeSpan.FromMilliseconds(300);
    /// <summary>
    /// Attempts to delete the specified file, retrying up to three times waiting 500 milliseconds if an <see cref="IOException"/> occurs.
    /// </summary>
    /// <remarks>If the file does not exist, the method completes without performing any action. If an <see
    /// cref="IOException"/> occurs during the deletion, the method retries up to three times before rethrowing the
    /// exception.</remarks>
    /// <param name="filePath">The full path of the file to delete. The path must not be null or empty.</param>
    public void TryDeleteFile(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            throw new ArgumentException("The string can not be null or empty", nameof(filePath));

        int retryCount = 3;
        TimeSpan delay = delayVerification;
        while (IsFileLocked(filePath))
        {
            Thread.Sleep(delay);
            delay = delay * 2;
            retryCount--;
            if (retryCount <= 0)
                throw new IOException("The file is locked and cannot be deleted: " + filePath);
        }

        // Acho que não se pode usar o if (File.Exists(filePath)) aqui por que pode causar IOException em condições de corrida:
        // https://learn.microsoft.com/en-us/dotnet/standard/io/common-i-o-tasks
        // https://learn.microsoft.com/en-us/dotnet/api/system.io.file.exists?view=net-9.0
        File.Delete(filePath); //Se o arquivo não existir, não lança exception: see https://learn.microsoft.com/en-us/dotnet/api/system.io.file.delete?view=net-9.0

        //int retryCount = 3;
        //while (retryCount > 0)
        //{
        //    try
        //    {
        //        // Acho que não se pode usar o if (File.Exists(filePath)) aqui por que pode causar IOException em condições de corrida:
        //        // https://learn.microsoft.com/en-us/dotnet/standard/io/common-i-o-tasks
        //        // https://learn.microsoft.com/en-us/dotnet/api/system.io.file.exists?view=net-9.0
        //        File.Delete(filePath); //Se o arquivo não existir, não lança exception: see https://learn.microsoft.com/en-us/dotnet/api/system.io.file.delete?view=net-9.0
        //    }
        //    catch (IOException) when (retryCount > 1) // tem que ser maior que 1 se não o exception não throw por causa do while
        //    {
        //        Thread.Sleep(500);
        //        retryCount--;
        //    }
        //}
    }
    public bool IsFileLocked(string pathFile)
    {
        FileStream stream = null;

        try
        {
            stream = new FileStream(pathFile, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
        }
        catch (IOException)
        {
            //the file is unavailable because it is:
            //still being written to
            //or being processed by another thread
            //or does not exist (has already been processed)
            return true;
        }
        finally
        {
            stream?.Close();
        }

        //file is not locked
        return false;
    }
}
