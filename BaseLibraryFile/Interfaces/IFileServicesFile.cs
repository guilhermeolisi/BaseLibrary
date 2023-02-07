namespace BaseLibrary
{
    public interface IFileServicesFile
    {
        void CopyConserveTime(string original, string destination, bool preservetime = true);
        void ExtractZipConserveTime(string zipPath, string extractPath);
        DateTime FileLastModification(string fileName);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns>file size in bytes</returns>
        long FileSize(string fileName);
    }
}