using BaseLibrary;
using FluentAssertions;
using System.IO.Compression;

namespace BaseLibrary.Tests;

/// <summary>
/// Unit tests for <see cref="FileServicesFile"/> covering all public methods,
/// including success scenarios, invalid inputs, non-existent files, locked files,
/// copy with overwrite, decryption validation, and ZIP extraction.
/// </summary>
public class FileServicesFileTests : IDisposable
{
    private readonly string _tmpDir;
    private readonly FileServicesFile _sut;

    public FileServicesFileTests()
    {
        _tmpDir = Path.Combine(Path.GetTempPath(), "FileServicesFileTests_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tmpDir);
        _sut = new FileServicesFile(new FileServicesDirectory());
    }

    public void Dispose()
    {
        if (Directory.Exists(_tmpDir))
        {
            foreach (var f in Directory.GetFiles(_tmpDir, "*", SearchOption.AllDirectories))
                File.SetAttributes(f, FileAttributes.Normal);
            Directory.Delete(_tmpDir, recursive: true);
        }
    }

    private string TmpPath(string name) => Path.Combine(_tmpDir, name);

    private string CreateFile(string relativeName, string content = "test content")
    {
        var fullPath = TmpPath(relativeName);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        File.WriteAllText(fullPath, content);
        return fullPath;
    }

    // -------------------------------------------------------------------------
    // CopyConserveTime
    // -------------------------------------------------------------------------

    [Fact]
    public void CopyConserveTime_NullSource_ThrowsArgumentException()
    {
        var act = () => _sut.CopyConserveTime(null!, TmpPath("dest.txt"));

        act.Should().Throw<ArgumentException>().WithParameterName("original");
    }

    [Fact]
    public void CopyConserveTime_WhitespaceSource_ThrowsArgumentException()
    {
        var act = () => _sut.CopyConserveTime("   ", TmpPath("dest.txt"));

        act.Should().Throw<ArgumentException>().WithParameterName("original");
    }

    [Fact]
    public void CopyConserveTime_NullDestination_ThrowsArgumentException()
    {
        var src = CreateFile("src_null_dest.txt");

        var act = () => _sut.CopyConserveTime(src, null!);

        act.Should().Throw<ArgumentException>().WithParameterName("destination");
    }

    [Fact]
    public void CopyConserveTime_WhitespaceDestination_ThrowsArgumentException()
    {
        var src = CreateFile("src_ws_dest.txt");

        var act = () => _sut.CopyConserveTime(src, "   ");

        act.Should().Throw<ArgumentException>().WithParameterName("destination");
    }

    [Fact]
    public void CopyConserveTime_NonExistentSource_ThrowsFileNotFoundException()
    {
        var act = () => _sut.CopyConserveTime(TmpPath("no_such_file.txt"), TmpPath("dest.txt"));

        act.Should().Throw<FileNotFoundException>();
    }

    [Fact]
    public void CopyConserveTime_Success_CopiesFileContent()
    {
        var src = CreateFile("copy_src.txt", "hello world");
        var dest = TmpPath("copy_dest.txt");

        _sut.CopyConserveTime(src, dest);

        File.Exists(dest).Should().BeTrue();
        File.ReadAllText(dest).Should().Be("hello world");
    }

    [Fact]
    public void CopyConserveTime_PreserveTimeTrue_KeepsOriginalTimestamps()
    {
        var src = CreateFile("ptime_src.txt", "data");
        var originalWrite = new DateTime(2020, 3, 15, 10, 0, 0, DateTimeKind.Local);
        File.SetLastWriteTime(src, originalWrite);
        var dest = TmpPath("ptime_dest.txt");

        _sut.CopyConserveTime(src, dest, preservetime: true);

        File.GetLastWriteTime(dest).Should().BeCloseTo(originalWrite, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void CopyConserveTime_PreserveTimeFalse_DoesNotPreserveOriginalTimestamps()
    {
        var src = CreateFile("ntime_src.txt", "data");
        var oldWrite = new DateTime(2015, 1, 1, 0, 0, 0, DateTimeKind.Local);
        File.SetLastWriteTime(src, oldWrite);
        var dest = TmpPath("ntime_dest.txt");

        var beforeCopy = DateTime.Now;
        _sut.CopyConserveTime(src, dest, preservetime: false);

        // The destination's write time should be close to "now", not the old source timestamp
        File.GetLastWriteTime(dest).Should().BeOnOrAfter(beforeCopy - TimeSpan.FromSeconds(2));
        File.GetLastWriteTime(dest).Should().NotBeCloseTo(oldWrite, TimeSpan.FromDays(1));
    }

    // -------------------------------------------------------------------------
    // FileSize
    // -------------------------------------------------------------------------

    [Fact]
    public void FileSize_NullPath_ThrowsArgumentException()
    {
        var act = () => _sut.FileSize(null!);

        act.Should().Throw<ArgumentException>().WithParameterName("filePath");
    }

    [Fact]
    public void FileSize_WhitespacePath_ThrowsArgumentException()
    {
        var act = () => _sut.FileSize("   ");

        act.Should().Throw<ArgumentException>().WithParameterName("filePath");
    }

    [Fact]
    public void FileSize_ValidFile_ReturnsCorrectSize()
    {
        var content = "12345";   // 5 ASCII bytes
        var file = CreateFile("size_test.txt", content);

        var result = _sut.FileSize(file);

        result.Should().Be(content.Length);
    }

    // -------------------------------------------------------------------------
    // FileLastModification
    // -------------------------------------------------------------------------

    [Fact]
    public void FileLastModification_NullPath_ThrowsArgumentException()
    {
        var act = () => _sut.FileLastModification(null!);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void FileLastModification_WhitespacePath_ThrowsArgumentException()
    {
        var act = () => _sut.FileLastModification("   ");

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void FileLastModification_ReturnsLatestOfCreationAndWriteTime()
    {
        var file = CreateFile("mod_test.txt", "data");
        var writeTime = new DateTime(2022, 6, 1, 12, 0, 0, DateTimeKind.Local);
        var creationTime = new DateTime(2021, 1, 1, 0, 0, 0, DateTimeKind.Local);
        File.SetCreationTime(file, creationTime);
        File.SetLastWriteTime(file, writeTime);

        var result = _sut.FileLastModification(file);

        result.Should().BeCloseTo(writeTime, TimeSpan.FromSeconds(2));
    }

    // -------------------------------------------------------------------------
    // ExtractZipConserveTime
    // -------------------------------------------------------------------------

    [Fact]
    public void ExtractZipConserveTime_NullZipPath_ThrowsArgumentException()
    {
        var act = () => _sut.ExtractZipConserveTime(null!, TmpPath("extract"));

        act.Should().Throw<ArgumentException>().WithParameterName("zipPath");
    }

    [Fact]
    public void ExtractZipConserveTime_WhitespaceZipPath_ThrowsArgumentException()
    {
        var act = () => _sut.ExtractZipConserveTime("   ", TmpPath("extract"));

        act.Should().Throw<ArgumentException>().WithParameterName("zipPath");
    }

    [Fact]
    public void ExtractZipConserveTime_NonExistentZip_ThrowsFileNotFoundException()
    {
        var act = () => _sut.ExtractZipConserveTime(TmpPath("no_such.zip"), TmpPath("extract"));

        act.Should().Throw<FileNotFoundException>();
    }

    [Fact]
    public void ExtractZipConserveTime_NullExtractPath_ThrowsArgumentException()
    {
        var zip = TmpPath("dummy.zip");
        CreateMinimalZip(zip, "file.txt", "content");

        var act = () => _sut.ExtractZipConserveTime(zip, null!);

        act.Should().Throw<ArgumentException>().WithParameterName("extractPath");
    }

    [Fact]
    public void ExtractZipConserveTime_WhitespaceExtractPath_ThrowsArgumentException()
    {
        var zip = TmpPath("dummy2.zip");
        CreateMinimalZip(zip, "file.txt", "content");

        var act = () => _sut.ExtractZipConserveTime(zip, "   ");

        act.Should().Throw<ArgumentException>().WithParameterName("extractPath");
    }

    [Fact]
    public void ExtractZipConserveTime_ValidZip_ExtractsFile()
    {
        var zip = TmpPath("valid.zip");
        CreateMinimalZip(zip, "hello.txt", "hello!");
        var extractDir = TmpPath("extract_valid");
        Directory.CreateDirectory(extractDir);

        _sut.ExtractZipConserveTime(zip, extractDir);

        File.Exists(Path.Combine(extractDir, "hello.txt")).Should().BeTrue();
        File.ReadAllText(Path.Combine(extractDir, "hello.txt")).Should().Be("hello!");
    }

    [Fact]
    public void ExtractZipConserveTime_DirectoryEntries_AreSkipped()
    {
        // Build a ZIP that contains only a directory entry (Name is empty)
        var zip = TmpPath("dir_entry.zip");
        using (var archive = ZipFile.Open(zip, ZipArchiveMode.Create))
        {
            // A directory entry has FullName ending with '/' and empty Name
            archive.CreateEntry("subdir/");
        }
        var extractDir = TmpPath("extract_dir_entry");
        Directory.CreateDirectory(extractDir);

        // Should not throw (directory entry is skipped gracefully)
        var act = () => _sut.ExtractZipConserveTime(zip, extractDir);

        act.Should().NotThrow();
    }

    [Fact]
    public void ExtractZipConserveTime_PathTraversalEntry_IsSkipped()
    {
        // Build a ZIP containing a path-traversal entry
        var zip = TmpPath("traversal.zip");
        using (var archive = ZipFile.Open(zip, ZipArchiveMode.Create))
        {
            var entry = archive.CreateEntry("../escape.txt");
            using var writer = new StreamWriter(entry.Open());
            writer.Write("should not appear");
        }
        var extractDir = TmpPath("extract_traversal");
        Directory.CreateDirectory(extractDir);
        var escapedFile = Path.GetFullPath(Path.Combine(extractDir, "../escape.txt"));

        _sut.ExtractZipConserveTime(zip, extractDir);

        // The file outside the extract directory must not have been created
        File.Exists(escapedFile).Should().BeFalse();
    }

    // -------------------------------------------------------------------------
    // OpenFile
    // -------------------------------------------------------------------------

    [Fact]
    public void OpenFile_NullFilePath_ThrowsArgumentException()
    {
        var act = () => _sut.OpenFile(null!, 0);

        act.Should().Throw<ArgumentException>().WithParameterName("filePath");
    }

    [Fact]
    public void OpenFile_WhitespaceFilePath_ThrowsArgumentException()
    {
        var act = () => _sut.OpenFile("   ", 0);

        act.Should().Throw<ArgumentException>().WithParameterName("filePath");
    }

    [Fact]
    public void OpenFile_NonExistentFile_ThrowsArgumentException()
    {
        var act = () => _sut.OpenFile(TmpPath("ghost.txt"), 0);

        act.Should().Throw<ArgumentException>().WithParameterName("filePath");
    }

    [Fact]
    public void OpenFile_UnsupportedOsValue_ThrowsArgumentException()
    {
        var file = CreateFile("open_test.txt");

        var act = () => _sut.OpenFile(file, 99);

        act.Should().Throw<ArgumentException>().WithParameterName("OS");
    }

    // -------------------------------------------------------------------------
    // VerifyIfEncrypted
    // -------------------------------------------------------------------------

    [Fact]
    public void VerifyIfEncrypted_NullPath_ThrowsArgumentException()
    {
        var act = () => _sut.VerifyIfEncrypted(null!);

        act.Should().Throw<ArgumentException>().WithParameterName("filePath");
    }

    [Fact]
    public void VerifyIfEncrypted_WhitespacePath_ThrowsArgumentException()
    {
        var act = () => _sut.VerifyIfEncrypted("   ");

        act.Should().Throw<ArgumentException>().WithParameterName("filePath");
    }

    [Fact]
    public void VerifyIfEncrypted_NormalFile_ReturnsFalse()
    {
        var file = CreateFile("not_encrypted.txt");

        var result = _sut.VerifyIfEncrypted(file);

        result.Should().BeFalse();
    }

    // -------------------------------------------------------------------------
    // TryDeleteFile
    // -------------------------------------------------------------------------

    [Fact]
    public void TryDeleteFile_NullPath_ThrowsArgumentException()
    {
        var act = () => _sut.TryDeleteFile(null!);

        act.Should().Throw<ArgumentException>().WithParameterName("filePath");
    }

    [Fact]
    public void TryDeleteFile_WhitespacePath_ThrowsArgumentException()
    {
        var act = () => _sut.TryDeleteFile("   ");

        act.Should().Throw<ArgumentException>().WithParameterName("filePath");
    }

    [Fact]
    public void TryDeleteFile_NonExistentFile_DoesNotThrow()
    {
        // Per documentation: if the file does not exist, the method completes without action
        var act = () => _sut.TryDeleteFile(TmpPath("does_not_exist.txt"));

        act.Should().NotThrow();
    }

    [Fact]
    public void TryDeleteFile_ExistingFile_DeletesFile()
    {
        var file = CreateFile("to_delete.txt");

        _sut.TryDeleteFile(file);

        File.Exists(file).Should().BeFalse();
    }

    [Fact]
    public void TryDeleteFile_LockedFile_ThrowsIOException()
    {
        var file = CreateFile("locked.txt");

        // Keep an exclusive lock on the file
        using var lockStream = new FileStream(file, FileMode.Open, FileAccess.ReadWrite, FileShare.None);

        var act = () => _sut.TryDeleteFile(file);

        act.Should().Throw<IOException>();
    }

    // -------------------------------------------------------------------------
    // IsFileLocked
    // -------------------------------------------------------------------------

    [Fact]
    public void IsFileLocked_UnlockedFile_ReturnsFalse()
    {
        var file = CreateFile("unlocked.txt");

        var result = _sut.IsFileLocked(file);

        result.Should().BeFalse();
    }

    [Fact]
    public void IsFileLocked_LockedFile_ReturnsTrue()
    {
        var file = CreateFile("is_locked.txt");
        using var lockStream = new FileStream(file, FileMode.Open, FileAccess.ReadWrite, FileShare.None);

        var result = _sut.IsFileLocked(file);

        result.Should().BeTrue();
    }

    // -------------------------------------------------------------------------
    // CopyFileSafeLinux / CopyFileSafeLinuxAsync
    // -------------------------------------------------------------------------

    [Fact]
    public void CopyFileSafeLinux_Success_CopiesFileContent()
    {
        var src = CreateFile("linux_src.txt", "linux content");
        var dest = TmpPath("linux_dest.txt");

        _sut.CopyFileSafeLinux(src, dest, overwrite: false);

        File.Exists(dest).Should().BeTrue();
        File.ReadAllText(dest).Should().Be("linux content");
    }

    [Fact]
    public void CopyFileSafeLinux_OverwriteTrue_OverwritesExistingFile()
    {
        var src = CreateFile("linux_ow_src.txt", "new content");
        var dest = CreateFile("linux_ow_dest.txt", "old content");

        _sut.CopyFileSafeLinux(src, dest, overwrite: true);

        File.ReadAllText(dest).Should().Be("new content");
    }

    [Fact]
    public void CopyFileSafeLinux_OverwriteFalse_ThrowsWhenDestinationExists()
    {
        var src = CreateFile("linux_noow_src.txt", "data");
        var dest = CreateFile("linux_noow_dest.txt", "existing");

        var act = () => _sut.CopyFileSafeLinux(src, dest, overwrite: false);

        act.Should().Throw<IOException>();
    }

    [Fact]
    public async Task CopyFileSafeLinuxAsync_Success_CopiesFileContent()
    {
        var src = CreateFile("async_src.txt", "async content");
        var dest = TmpPath("async_dest.txt");

        await _sut.CopyFileSafeLinuxAsync(src, dest, overwrite: false);

        File.Exists(dest).Should().BeTrue();
        File.ReadAllText(dest).Should().Be("async content");
    }

    [Fact]
    public async Task CopyFileSafeLinuxAsync_OverwriteTrue_OverwritesExistingFile()
    {
        var src = CreateFile("async_ow_src.txt", "new async");
        var dest = CreateFile("async_ow_dest.txt", "old async");

        await _sut.CopyFileSafeLinuxAsync(src, dest, overwrite: true);

        File.ReadAllText(dest).Should().Be("new async");
    }

    // -------------------------------------------------------------------------
    // CopyDecriptingFile — validation paths (note: method name spelling preserved from existing API)
    // -------------------------------------------------------------------------

    [Fact]
    public void CopyDecriptingFile_NullSourcePath_ThrowsArgumentException()
    {
        var act = () => _sut.CopyDecriptingFile(null!, TmpPath("dest_folder"), TmpPath("efs_folder"));

        act.Should().Throw<ArgumentException>().WithParameterName("sourcePath");
    }

    [Fact]
    public void CopyDecriptingFile_WhitespaceSourcePath_ThrowsArgumentException()
    {
        var act = () => _sut.CopyDecriptingFile("   ", TmpPath("dest_folder"), TmpPath("efs_folder"));

        act.Should().Throw<ArgumentException>().WithParameterName("sourcePath");
    }

    [Fact]
    public void CopyDecriptingFile_NonExistentSourceFile_ThrowsFileNotFoundException()
    {
        var act = () => _sut.CopyDecriptingFile(
            TmpPath("ghost_source.txt"),
            TmpPath("dest_folder"),
            TmpPath("efs_folder"));

        act.Should().Throw<FileNotFoundException>();
    }

    [Fact]
    public void CopyDecriptingFile_NullEfsFolder_ThrowsArgumentException()
    {
        var src = CreateFile("decrypt_src.txt");

        var act = () => _sut.CopyDecriptingFile(src, TmpPath("dest_folder"), null!);

        act.Should().Throw<ArgumentException>().WithParameterName("anEFSFolder");
    }

    [Fact]
    public void CopyDecriptingFile_WhitespaceEfsFolder_ThrowsArgumentException()
    {
        var src = CreateFile("decrypt_ws_src.txt");

        var act = () => _sut.CopyDecriptingFile(src, TmpPath("dest_folder"), "   ");

        act.Should().Throw<ArgumentException>().WithParameterName("anEFSFolder");
    }

    // -------------------------------------------------------------------------
    // Helper
    // -------------------------------------------------------------------------

    private static void CreateMinimalZip(string zipPath, string entryName, string entryContent)
    {
        using var archive = ZipFile.Open(zipPath, ZipArchiveMode.Create);
        var entry = archive.CreateEntry(entryName);
        using var writer = new StreamWriter(entry.Open());
        writer.Write(entryContent);
    }
}
