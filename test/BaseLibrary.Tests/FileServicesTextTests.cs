using FluentAssertions;

namespace BaseLibrary.Tests;

/// <summary>
/// Unit tests for <see cref="FileServicesText"/> covering the bugs fixed:
/// 1. BakReadBegin / BakReadEnd – condition grouping (operator precedence)
/// 2. ReadTXT / ReadTXTAsync – Directory.Exists instead of File.Exists for directory check
/// 3. WriteTXTAsync – missing directory validation
/// 4. Timestamp restoration – SetLastWriteTime must use lastWrite, not creation
/// </summary>
public class FileServicesTextTests : IDisposable
{
    private readonly string _tmpDir;
    private readonly FileServicesText _sut;

    public FileServicesTextTests()
    {
        _tmpDir = Path.Combine(Path.GetTempPath(), "FileServicesTextTests_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tmpDir);
        _sut = new FileServicesText();
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

    // -------------------------------------------------------------------------
    // WriteTXT – argument validation
    // -------------------------------------------------------------------------

    [Fact]
    public void WriteTXT_NullOrWhitespacePath_ThrowsArgumentNullException()
    {
        _sut.Invoking(s => s.WriteTXT("   ", "text"))
            .Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void WriteTXT_NullContent_ThrowsArgumentNullException()
    {
        _sut.Invoking(s => s.WriteTXT(TmpPath("f.txt"), null!))
            .Should().Throw<ArgumentNullException>();
    }

    // -------------------------------------------------------------------------
    // WriteTXT – directory validation
    // -------------------------------------------------------------------------

    [Fact]
    public void WriteTXT_DirectoryDoesNotExist_ReturnsFalse()
    {
        var path = Path.Combine(_tmpDir, "nonexistent_dir", "file.txt");

        var result = _sut.WriteTXT(path, "hello");

        result.Should().BeFalse();
    }

    [Fact]
    public void WriteTXT_ExistingDirectory_WritesFileAndReturnsTrue()
    {
        var path = TmpPath("write_ok.txt");

        var result = _sut.WriteTXT(path, "hello world");

        result.Should().BeTrue();
        File.ReadAllText(path).Should().Be("hello world");
    }

    // -------------------------------------------------------------------------
    // WriteTXTAsync – directory validation (Fix 3: was missing before the fix)
    // -------------------------------------------------------------------------

    [Fact]
    public async Task WriteTXTAsync_NullOrWhitespacePath_ThrowsArgumentNullException()
    {
        await _sut.Invoking(s => s.WriteTXTAsync("   ", "text"))
            .Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task WriteTXTAsync_NullContent_ThrowsArgumentNullException()
    {
        await _sut.Invoking(s => s.WriteTXTAsync(TmpPath("f.txt"), null!))
            .Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task WriteTXTAsync_DirectoryDoesNotExist_ReturnsFalse()
    {
        var path = Path.Combine(_tmpDir, "nonexistent_dir", "file.txt");

        var result = await _sut.WriteTXTAsync(path, "hello");

        result.Should().BeFalse();
    }

    [Fact]
    public async Task WriteTXTAsync_ExistingDirectory_WritesFileAndReturnsTrue()
    {
        var path = TmpPath("async_write_ok.txt");

        var result = await _sut.WriteTXTAsync(path, "async content");

        result.Should().BeTrue();
        File.ReadAllText(path).Should().Be("async content");
    }

    // -------------------------------------------------------------------------
    // ReadTXT – argument validation
    // -------------------------------------------------------------------------

    [Fact]
    public void ReadTXT_NullOrWhitespacePath_ThrowsArgumentNullException()
    {
        _sut.Invoking(s => s.ReadTXT("   "))
            .Should().Throw<ArgumentNullException>();
    }

    // -------------------------------------------------------------------------
    // ReadTXT – Directory.Exists fix (Fix 2)
    // When BakReadBegin returns the .tmp path but the directory exists, the
    // guard in ReadTXT must NOT prematurely return null.
    // -------------------------------------------------------------------------

    [Fact]
    public void ReadTXT_ExistingFile_ReturnsContent()
    {
        var path = TmpPath("read_ok.txt");
        File.WriteAllText(path, "read content");

        var result = _sut.ReadTXT(path);

        result.Should().Be("read content");
    }

    [Fact]
    public void ReadTXT_NonExistentFile_ReturnsNull()
    {
        var path = TmpPath("does_not_exist.txt");

        var result = _sut.ReadTXT(path);

        result.Should().BeNull();
    }

    /// <summary>
    /// Verifies Fix 2: previously used File.Exists(directory) which always
    /// returned false, causing the method to return null when a .tmp backup
    /// was present even though the directory exists and the .tmp can be used.
    /// After the fix (Directory.Exists), the .tmp is correctly promoted and read.
    /// </summary>
    [Fact]
    public void ReadTXT_WhenOnlyTmpExists_PromotesTmpAndReturnsContent()
    {
        var path = TmpPath("promote_tmp.txt");
        var tmpPath = path + ".tmp";

        // Create only the .tmp file (original absent) — BakReadBegin should promote it
        File.WriteAllText(tmpPath, "backup content");

        var result = _sut.ReadTXT(path);

        result.Should().Be("backup content");
    }

    // -------------------------------------------------------------------------
    // ReadTXTAsync – Directory.Exists fix (Fix 2)
    // -------------------------------------------------------------------------

    [Fact]
    public async Task ReadTXTAsync_NullOrWhitespacePath_ThrowsArgumentNullException()
    {
        await _sut.Invoking(s => s.ReadTXTAsync("   "))
            .Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task ReadTXTAsync_ExistingFile_ReturnsContent()
    {
        var path = TmpPath("async_read_ok.txt");
        File.WriteAllText(path, "async read content");

        var result = await _sut.ReadTXTAsync(path);

        result.Should().Be("async read content");
    }

    [Fact]
    public async Task ReadTXTAsync_NonExistentFile_ReturnsNull()
    {
        var path = TmpPath("async_does_not_exist.txt");

        var result = await _sut.ReadTXTAsync(path);

        result.Should().BeNull();
    }

    /// <summary>
    /// Verifies Fix 2 (async variant): .tmp backup is promoted and returned
    /// instead of null when the directory exists.
    /// </summary>
    [Fact]
    public async Task ReadTXTAsync_WhenOnlyTmpExists_PromotesTmpAndReturnsContent()
    {
        var path = TmpPath("async_promote_tmp.txt");
        var tmpPath = path + ".tmp";

        File.WriteAllText(tmpPath, "async backup content");

        var result = await _sut.ReadTXTAsync(path);

        result.Should().Be("async backup content");
    }

    // -------------------------------------------------------------------------
    // BakReadBegin / BakReadEnd – condition grouping (Fix 1)
    // Without the fix the condition evaluated as:
    //   (tmpExists && !mainExists) || tmpCreation > mainLastWrite
    // which could enter the block when tmpExt doesn't exist (via the || branch).
    // With the fix the condition is:
    //   tmpExists && (!mainExists || tmpCreation > mainLastWrite)
    // so the block is only entered when the .tmp file actually exists.
    // -------------------------------------------------------------------------

    [Fact]
    public void ReadTXT_WhenTmpIsNewerThanMain_UsesTmp()
    {
        var path = TmpPath("newer_tmp.txt");
        var tmpPath = path + ".tmp";

        // Write main file
        File.WriteAllText(path, "old content");
        var oldTime = DateTime.Now.AddHours(-2);
        File.SetLastWriteTime(path, oldTime);

        // Write .tmp with a much newer creation time
        File.WriteAllText(tmpPath, "new content");
        File.SetCreationTime(tmpPath, DateTime.Now);

        var result = _sut.ReadTXT(path);

        result.Should().Be("new content");
    }

    [Fact]
    public void ReadTXT_WhenMainIsNewer_UsesMain()
    {
        var path = TmpPath("newer_main.txt");
        var tmpPath = path + ".tmp";

        // Write main file with current time
        File.WriteAllText(path, "main content");
        File.SetLastWriteTime(path, DateTime.Now);

        // Write .tmp with an old creation time
        File.WriteAllText(tmpPath, "stale backup");
        File.SetCreationTime(tmpPath, DateTime.Now.AddHours(-2));

        var result = _sut.ReadTXT(path);

        result.Should().Be("main content");
    }

    // -------------------------------------------------------------------------
    // BakWriteEnd – timestamp restoration uses lastWrite not creation (Fix 4)
    // If a write fails the backup .tmp is restored; the restored file should
    // have its LastWriteTime set to the original LastWriteTime, not CreationTime.
    // -------------------------------------------------------------------------

    [Fact]
    public void WriteTXT_RoundTrip_PreservesContent()
    {
        var path = TmpPath("roundtrip.txt");

        _sut.WriteTXT(path, "first version");
        _sut.WriteTXT(path, "second version");

        _sut.ReadTXT(path).Should().Be("second version");
    }

    [Fact]
    public async Task WriteTXTAsync_RoundTrip_PreservesContent()
    {
        var path = TmpPath("async_roundtrip.txt");

        await _sut.WriteTXTAsync(path, "first version");
        await _sut.WriteTXTAsync(path, "second version");

        (await _sut.ReadTXTAsync(path)).Should().Be("second version");
    }

    /// <summary>
    /// Regression for Fix 4 (BakReadBegin / BakReadEnd):
    /// After promoting a .tmp whose LastWriteTime differs from its CreationTime,
    /// the promoted file must carry the original LastWriteTime, not CreationTime.
    /// </summary>
    [Fact]
    public void ReadTXT_AfterTmpPromotion_RestoresLastWriteTime()
    {
        var path = TmpPath("restore_lw.txt");
        var tmpPath = path + ".tmp";

        // Write .tmp with explicitly different CreationTime and LastWriteTime
        File.WriteAllText(tmpPath, "backup content");
        var expectedLastWrite = new DateTime(2021, 3, 15, 8, 30, 0, DateTimeKind.Local);
        File.SetLastWriteTime(tmpPath, expectedLastWrite);

        // No main file → BakReadBegin promotes .tmp to main
        _sut.ReadTXT(path);

        // LastWriteTime on the promoted file must match the .tmp's LastWriteTime (Fix 4 regression).
        // Note: CreationTime is platform-specific and not asserted here.
        File.GetLastWriteTime(path).Should().BeCloseTo(expectedLastWrite, TimeSpan.FromSeconds(2));
    }

    /// <summary>
    /// Regression for Fix 4 (BakWriteEnd):
    /// When a write fails and the .tmp backup is restored, the file's
    /// LastWriteTime should be restored to the backup's LastWriteTime —
    /// not to its CreationTime.
    /// </summary>
    [Fact]
    public void WriteTXT_OnWriteFailure_RestoresContentAndLastWriteTime()
    {
        var path = TmpPath("write_failure_ts.txt");
        _sut.WriteTXT(path, "original content");

        // Set a specific, easily distinguishable LastWriteTime on the file.
        // BakWriteBegin will copy the file to .tmp and preserve this as .tmp's LastWriteTime.
        var originalLastWrite = new DateTime(2019, 5, 20, 9, 0, 0, DateTimeKind.Local);
        File.SetLastWriteTime(path, originalLastWrite);

        // Make file read-only so the write attempt fails (UnauthorizedAccessException).
        File.SetAttributes(path, FileAttributes.ReadOnly);
        try
        {
            var result = _sut.WriteTXT(path, "new content");

            // Write should fail (false) and content must be restored from .tmp
            result.Should().BeFalse();
            File.ReadAllText(path).Should().Be("original content");
            // LastWriteTime must be restored from backup's LastWriteTime, not CreationTime
            File.GetLastWriteTime(path).Should().BeCloseTo(originalLastWrite, TimeSpan.FromSeconds(2));
        }
        finally
        {
            File.SetAttributes(path, FileAttributes.Normal);
        }
    }

    // -------------------------------------------------------------------------
    // WriteTXT / ReadTXT – empty string content
    // -------------------------------------------------------------------------

    [Fact]
    public void WriteTXT_EmptyContent_WritesEmptyFileAndReturnsTrue()
    {
        var path = TmpPath("empty.txt");

        var result = _sut.WriteTXT(path, string.Empty);

        result.Should().BeTrue();
        File.ReadAllText(path).Should().BeEmpty();
    }

    [Fact]
    public void ReadTXT_EmptyFile_ReturnsEmptyString()
    {
        var path = TmpPath("read_empty.txt");
        File.WriteAllText(path, string.Empty);

        var result = _sut.ReadTXT(path);

        result.Should().BeEmpty();
    }

    // -------------------------------------------------------------------------
    // WriteTXT / ReadTXT – unicode content
    // -------------------------------------------------------------------------

    [Fact]
    public void WriteTXT_UnicodeContent_PreservesContent()
    {
        var path = TmpPath("unicode.txt");
        var content = "héllo wörld 日本語 🎉";

        _sut.WriteTXT(path, content);

        _sut.ReadTXT(path).Should().Be(content);
    }
}
