namespace BaseLibrary
{
    public interface IFileServicesFile
    {
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
        /// <summary>
        /// Open file in a external program
        /// </summary>
        /// <param name="filePath">File Path to open</param>
        /// <param name="OS">0: Windows, 1: Linux, 2: macOS</param>
        void OpenFile(string filePath, sbyte OS);
        bool VerifyIfEncrypted(string filePath);
    }
}