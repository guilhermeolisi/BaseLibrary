namespace BaseLibrary;

public interface IFileServicesFile
{
    /// <summary>
    /// Copies a file from the specified source path to the specified destination path, with an option to preserve the
    /// original file's timestamp. Careful, this method can be used in the folder of the application in MSIX.
    /// </summary>
    /// <remarks>If <paramref name="preservetime"/> is set to <see langword="true"/>, the copied file will
    /// retain the original file's creation and modification timestamps. If <paramref name="preservetime"/> is set to
    /// <see langword="false"/>, the copied file will have new timestamps based on the time of the copy
    /// operation.</remarks>
    /// <param name="original">The full path of the file to copy. This cannot be null or empty.</param>
    /// <param name="destination">The full path where the file should be copied. This cannot be null or empty.</param>
    /// <param name="preservetime">A boolean value indicating whether to preserve the original file's timestamp.  <see langword="true"/> to
    /// preserve the timestamp; otherwise, <see langword="false"/>.</param>
    void CopyConserveTime(string original, string destination, bool preservetime = true);
    void CopyDecriptingFile(string sourcePath, string destinationFolder, string appFolder);
    void CopyFileCarefulEncription(string sourcePath, string destinationFolder, string anEFSFolder);
    void ExtractZipConserveTime(string zipPath, string extractPath);
    DateTime FileLastModification(string filePath);
    /// <summary>
    /// Get file size in bytes
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns>file size in bytes</returns>
    long FileSize(string filePath);
    bool IsFileLocked(string pathFile);

    /// <summary>
    /// Open file in a external program
    /// </summary>
    /// <param name="filePath">File Path to open</param>
    /// <param name="OS">0: Windows, 1: Linux, 2: macOS</param>
    void OpenFile(string filePath, sbyte OS);
    void TryDeleteFile(string filePath);
    bool VerifyIfEncrypted(string filePath);
}