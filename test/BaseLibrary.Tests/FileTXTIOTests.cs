using FluentAssertions;
using Moq;
using System.Text;

namespace BaseLibrary.Tests;

/// <summary>
/// Unit tests for <see cref="FileTXTIO"/> covering success paths, access failures,
/// missing files, corrupted backup, IO exceptions, retry logic and recovery.
/// </summary>
public class FileTXTIOTests : IDisposable
{
    // Temporary directory used for all test files; cleaned up in Dispose.
    private readonly string _tmpDir;
    private readonly Mock<IFileServicesCheck> _mockCheck;
    private readonly Mock<IFileServices> _mockServices;

    public FileTXTIOTests()
    {
        _tmpDir = Path.Combine(Path.GetTempPath(), "FileTXTIOTests_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tmpDir);

        _mockCheck = new Mock<IFileServicesCheck>();
        // Default: CheckTextFile returns true (valid text file)
        _mockCheck.Setup(c => c.CheckTextFile(It.IsAny<string>())).Returns(true);
        // Default: encoding detection returns null (use BOM detection)
        _mockCheck.Setup(c => c.DetectTextFileEncoding(It.IsAny<string>())).Returns((Encoding?)null);
        _mockCheck.Setup(c => c.DetectTextFileEncodingGOS(It.IsAny<string>())).Returns((Encoding?)null);

        _mockServices = new Mock<IFileServices>();
        _mockServices.Setup(s => s.Check).Returns(_mockCheck.Object);
    }

    public void Dispose()
    {
        try { Directory.Delete(_tmpDir, recursive: true); } catch { /* best effort */ }
    }

    private FileTXTIO CreateSut() => new(_mockServices.Object);

    private string TmpPath(string name) => Path.Combine(_tmpDir, name);

    // -------------------------------------------------------------------------
    // GetPathFile / SetPathFile
    // -------------------------------------------------------------------------

    [Fact]
    public void GetPathFile_AfterSetPathFile_ShouldReturnSetPath()
    {
        var sut = CreateSut();
        var path = TmpPath("file.txt");

        sut.SetPathFile(path);

        sut.GetPathFile().Should().Be(path);
    }

    [Fact]
    public void SetPathFile_WhenSamePath_ShouldNotChangeState()
    {
        var sut = CreateSut();
        var path = TmpPath("file.txt");
        sut.SetPathFile(path);

        sut.SetPathFile(path); // same path — no-op

        sut.GetPathFile().Should().Be(path);
    }

    [Fact]
    public void SetPathFile_WhenChangingPath_ShouldUpdatePath()
    {
        var sut = CreateSut();
        var path1 = TmpPath("file1.txt");
        var path2 = TmpPath("file2.txt");
        sut.SetPathFile(path1);

        sut.SetPathFile(path2);

        sut.GetPathFile().Should().Be(path2);
    }

    [Fact]
    public void SetPathFile_WhenIsStayBakAndBakExists_ShouldDeleteBak()
    {
        var sut = CreateSut();
        var path = TmpPath("stayBak.txt");
        var bakPath = path + ".bak";
        File.WriteAllText(path, "data");
        File.WriteAllText(bakPath, "bak");
        sut.SetPathFile(path);
        sut.SetStayBak(true);

        // Change path — should delete the previous bak
        sut.SetPathFile(TmpPath("other.txt"));

        File.Exists(bakPath).Should().BeFalse();
    }

    // -------------------------------------------------------------------------
    // Closing
    // -------------------------------------------------------------------------

    [Fact]
    public void Closing_WhenIsStayBakAndBakExists_ShouldDeleteBak()
    {
        var sut = CreateSut();
        var path = TmpPath("closing.txt");
        var bakPath = path + ".bak";
        File.WriteAllText(path, "data");
        File.WriteAllText(bakPath, "bak");
        sut.SetPathFile(path);
        sut.SetStayBak(true);

        sut.Closing();

        File.Exists(bakPath).Should().BeFalse();
    }

    [Fact]
    public void Closing_WhenIsStayBakFalse_ShouldNotDeleteBak()
    {
        var sut = CreateSut();
        var path = TmpPath("closing2.txt");
        var bakPath = path + ".bak";
        File.WriteAllText(path, "data");
        File.WriteAllText(bakPath, "bak");
        sut.SetPathFile(path);
        sut.SetStayBak(false);

        sut.Closing(); // bak exists but _isStayBak is false, so Closing() does not delete it

        File.Exists(bakPath).Should().BeTrue();
    }

    // -------------------------------------------------------------------------
    // WriteTXT — success scenarios
    // -------------------------------------------------------------------------

    [Fact]
    public void WriteTXT_WhenPathIsNull_ShouldReturnFalseWithArgumentNullException()
    {
        var sut = CreateSut();
        // _pathFile is not set → null

        var result = sut.WriteTXT("hello");

        result.Should().BeFalse();
        sut.eWrite.Should().BeOfType<ArgumentNullException>();
    }

    [Fact]
    public void WriteTXT_WhenNewFile_ShouldCreateFileWithContentAndReturnTrue()
    {
        var sut = CreateSut();
        var path = TmpPath("new.txt");
        sut.SetPathFile(path);

        var result = sut.WriteTXT("hello world");

        result.Should().BeTrue();
        sut.eWrite.Should().BeNull();
        File.ReadAllText(path).Should().Be("hello world");
    }

    [Fact]
    public void WriteTXT_WhenExistingFile_ShouldOverwriteContentAndReturnTrue()
    {
        var sut = CreateSut();
        var path = TmpPath("existing.txt");
        File.WriteAllText(path, "old content");
        sut.SetPathFile(path);

        var result = sut.WriteTXT("new content");

        result.Should().BeTrue();
        File.ReadAllText(path).Should().Be("new content");
    }

    [Fact]
    public void WriteTXT_WhenIsStayBakFalse_ShouldDeleteBakAfterWrite()
    {
        var sut = CreateSut();
        var path = TmpPath("bak_delete.txt");
        var bakPath = path + ".bak";
        File.WriteAllText(path, "data");
        sut.SetPathFile(path);
        sut.SetStayBak(false);

        var result = sut.WriteTXT("updated");

        result.Should().BeTrue();
        File.Exists(bakPath).Should().BeFalse("bak should be removed when _isStayBak is false");
    }

    [Fact]
    public void WriteTXT_WhenIsStayBakTrue_ShouldPreserveBakAfterWrite()
    {
        var sut = CreateSut();
        var path = TmpPath("bak_keep.txt");
        var bakPath = path + ".bak";
        File.WriteAllText(path, "original");
        sut.SetPathFile(path);
        sut.SetStayBak(true);

        var result = sut.WriteTXT("updated");

        result.Should().BeTrue();
        File.Exists(bakPath).Should().BeTrue("bak should be kept when _isStayBak is true");
        File.ReadAllText(bakPath).Should().Be("original", "bak should contain the pre-write snapshot");
    }

    // -------------------------------------------------------------------------
    // WriteTXT — integrity check failure
    // -------------------------------------------------------------------------

    [Fact]
    public void WriteTXT_WhenOriginalFileFailsIntegrityCheck_ShouldReturnFalseWithFileLoadException()
    {
        var sut = CreateSut();
        var path = TmpPath("corrupt.txt");
        File.WriteAllText(path, "bad");
        sut.SetPathFile(path);
        // Simulate corrupted file
        _mockCheck.Setup(c => c.CheckTextFile(path)).Returns(false);

        var result = sut.WriteTXT("new");

        result.Should().BeFalse();
        sut.eWrite.Should().BeOfType<FileLoadException>();
    }

    // -------------------------------------------------------------------------
    // WriteTXTAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task WriteTXTAsync_WhenNewFile_ShouldCreateFileAndReturnTrue()
    {
        var sut = CreateSut();
        var path = TmpPath("async_write.txt");
        sut.SetPathFile(path);

        var result = await sut.WriteTXTAsync("async content");

        result.Should().BeTrue();
        File.ReadAllText(path).Should().Be("async content");
    }

    [Fact]
    public async Task WriteTXTAsync_WhenPathIsNull_ShouldReturnFalse()
    {
        var sut = CreateSut();

        var result = await sut.WriteTXTAsync("text");

        result.Should().BeFalse();
        sut.eWrite.Should().BeOfType<ArgumentNullException>();
    }

    // -------------------------------------------------------------------------
    // ReadTXT — success scenarios
    // -------------------------------------------------------------------------

    [Fact]
    public void ReadTXT_WhenPathIsNull_ShouldReturnFalseWithFileNotFoundException()
    {
        var sut = CreateSut();

        var result = sut.ReadTXT();

        result.Should().BeFalse();
        sut.eRead.Should().BeOfType<FileNotFoundException>();
    }

    [Fact]
    public void ReadTXT_WhenFileDoesNotExist_ShouldReturnFalseWithFileNotFoundException()
    {
        var sut = CreateSut();
        sut.SetPathFile(TmpPath("nonexistent.txt"));

        var result = sut.ReadTXT();

        result.Should().BeFalse();
        sut.eRead.Should().BeOfType<FileNotFoundException>();
    }

    [Fact]
    public void ReadTXT_WhenFileExists_ShouldReadContentAndReturnTrue()
    {
        var sut = CreateSut();
        var path = TmpPath("read.txt");
        File.WriteAllText(path, "file content");
        sut.SetPathFile(path);

        var result = sut.ReadTXT();

        result.Should().BeTrue();
        sut.eRead.Should().BeNull();
        sut.TextResult.Should().Be("file content");
    }

    [Fact]
    public void ReadTXT_WhenIsStayBakFalse_ShouldDeleteBakAfterRead()
    {
        var sut = CreateSut();
        var path = TmpPath("read_bak_del.txt");
        var bakPath = path + ".bak";
        File.WriteAllText(path, "data");
        sut.SetPathFile(path);
        sut.SetStayBak(false);

        var result = sut.ReadTXT();

        result.Should().BeTrue();
        File.Exists(bakPath).Should().BeFalse("bak should be removed after read when _isStayBak is false");
    }

    [Fact]
    public void ReadTXT_WhenIsStayBakTrue_ShouldPreserveBakAfterRead()
    {
        var sut = CreateSut();
        var path = TmpPath("read_bak_keep.txt");
        var bakPath = path + ".bak";
        File.WriteAllText(path, "data");
        sut.SetPathFile(path);
        sut.SetStayBak(true);

        var result = sut.ReadTXT();

        result.Should().BeTrue();
        File.Exists(bakPath).Should().BeTrue("bak should be kept when _isStayBak is true");
    }

    // -------------------------------------------------------------------------
    // ReadTXTAsync
    // -------------------------------------------------------------------------

    [Fact]
    public async Task ReadTXTAsync_WhenFileExists_ShouldReadContentAndReturnTrue()
    {
        var sut = CreateSut();
        var path = TmpPath("async_read.txt");
        File.WriteAllText(path, "async text");
        sut.SetPathFile(path);

        var result = await sut.ReadTXTAsync();

        result.Should().BeTrue();
        sut.TextResult.Should().Be("async text");
    }

    [Fact]
    public async Task ReadTXTAsync_WhenPathIsNull_ShouldReturnFalse()
    {
        var sut = CreateSut();

        var result = await sut.ReadTXTAsync();

        result.Should().BeFalse();
        sut.eRead.Should().BeOfType<FileNotFoundException>();
    }

    // -------------------------------------------------------------------------
    // Write + Read round-trip
    // -------------------------------------------------------------------------

    [Fact]
    public void WriteThenReadTXT_ShouldPreserveContent()
    {
        var sut = CreateSut();
        var path = TmpPath("roundtrip.txt");
        sut.SetPathFile(path);
        var content = "Round-trip content — with accents: ção";

        sut.WriteTXT(content).Should().BeTrue();
        sut.ReadTXT().Should().BeTrue();

        sut.TextResult.Should().Be(content);
    }

    // -------------------------------------------------------------------------
    // ProcessWriterException — retry and timeout
    // -------------------------------------------------------------------------

    [Fact]
    public void ProcessWriterException_FirstCall_ShouldSetLastWriteAttemptAndReturnTrue()
    {
        var sut = CreateSut();
        var path = TmpPath("pwe.txt");
        sut.SetPathFile(path);

        // First exception — retry state not yet started
        var result = sut.ProcessWriterException("text", new IOException("locked"));

        result.Should().BeTrue("first call should signal retry");
        sut.eWrite.Should().BeNull("error should not be recorded yet");
    }

    [Fact]
    public async Task ProcessWriterException_AfterTimeout_ShouldSetEWriteAndReturnFalse()
    {
        var sut = CreateSut();
        var path = TmpPath("pwe_timeout.txt");
        sut.SetPathFile(path);
        var ioEx = new IOException("still locked");

        // First call starts the timer
        sut.ProcessWriterException("text", ioEx);

        // Wait longer than the 5 * 250 ms timeout
        await Task.Delay(1400);

        var result = sut.ProcessWriterException("text", ioEx);

        result.Should().BeFalse("timeout reached — should stop retrying");
        sut.eWrite.Should().BeSameAs(ioEx);
    }

    // -------------------------------------------------------------------------
    // ProcessReaderException — retry, timeout and bak recovery
    // -------------------------------------------------------------------------

    [Fact]
    public void ProcessReaderException_FirstCall_ShouldSetLastReadAttemptAndReturnTrue()
    {
        var sut = CreateSut();
        var path = TmpPath("pre.txt");
        sut.SetPathFile(path);

        var result = sut.ProcessReaderException(new IOException("locked"));

        result.Should().BeTrue("first call should signal retry");
        sut.eRead.Should().BeNull();
    }

    [Fact]
    public async Task ProcessReaderException_AfterTimeoutWithNoBak_ShouldSetEReadAndReturnFalse()
    {
        var sut = CreateSut();
        var path = TmpPath("pre_timeout.txt");
        sut.SetPathFile(path);
        var ioEx = new IOException("still locked");

        // First call starts the timer
        sut.ProcessReaderException(ioEx);

        // Wait longer than the 5 * 250 / 2 = 625 ms timeout
        await Task.Delay(750);

        var result = sut.ProcessReaderException(ioEx);

        result.Should().BeFalse("timeout reached — should stop retrying");
        sut.eRead.Should().BeSameAs(ioEx, "original exception should be recorded when no bak exists");
    }

    [Fact]
    public async Task ProcessReaderException_AfterTimeoutWithBak_ShouldRecoverAndReturnFalse()
    {
        // Arrange
        var sut = CreateSut();
        var path = TmpPath("pre_recover.txt");
        var bakPath = path + ".bak";
        File.WriteAllText(path, "original");
        File.WriteAllText(bakPath, "backup content");
        sut.SetPathFile(path);

        // Start retry timer
        sut.ProcessReaderException(new IOException("locked"));

        // Wait for timeout
        await Task.Delay(750);

        // Act — this should now trigger bak recovery
        var result = sut.ProcessReaderException(new IOException("still locked"));

        // Assert
        result.Should().BeFalse("stop signal — but recovery succeeded");
        sut.eRead.Should().BeNull("no error when recovery from bak succeeds");
        sut.TextResult.Should().Be("backup content");
    }

    [Fact]
    public async Task ProcessReaderException_AfterRecovery_ShouldRestoreOriginalFromBak()
    {
        // Arrange
        var sut = CreateSut();
        var path = TmpPath("pre_restore.txt");
        var bakPath = path + ".bak";
        File.WriteAllText(path, "original");
        File.WriteAllText(bakPath, "restored content");
        sut.SetPathFile(path);

        sut.ProcessReaderException(new IOException("locked"));
        await Task.Delay(750);
        sut.ProcessReaderException(new IOException("still locked"));

        // After recovery the original should be restored from bak
        File.Exists(path).Should().BeTrue("original file should be restored");
        File.ReadAllText(path).Should().Be("restored content");
    }

    // -------------------------------------------------------------------------
    // BeforeWrite / BeforeReader state reset
    // -------------------------------------------------------------------------

    [Fact]
    public void BeforeWrite_ShouldClearEWriteOnEachCall()
    {
        var sut = CreateSut();
        var path = TmpPath("clear_ewrite.txt");
        sut.SetPathFile(path);

        // First call with no path to force an error — oops, path is set, so trigger
        // a different error: make CheckTextFile fail after file is created
        File.WriteAllText(path, "data");
        _mockCheck.Setup(c => c.CheckTextFile(path)).Returns(false);
        sut.WriteTXT("x");
        sut.eWrite.Should().NotBeNull();

        // Second call: restore check to succeed and use a new file
        _mockCheck.Setup(c => c.CheckTextFile(It.IsAny<string>())).Returns(true);
        var path2 = TmpPath("clear_ewrite2.txt");
        sut.SetPathFile(path2);
        sut.WriteTXT("ok");

        sut.eWrite.Should().BeNull("eWrite must be cleared on each BeforeWrite call");
    }

    [Fact]
    public void BeforeReader_ShouldClearEReadOnEachCall()
    {
        var sut = CreateSut();

        // Cause a read error (no path)
        sut.ReadTXT();
        sut.eRead.Should().NotBeNull();

        // Now set path to a real file
        var path = TmpPath("clear_eread.txt");
        File.WriteAllText(path, "ok");
        sut.SetPathFile(path);
        sut.ReadTXT();

        sut.eRead.Should().BeNull("eRead must be cleared on each BeforeReader call");
    }

    // -------------------------------------------------------------------------
    // Instance isolation — static-field regression tests
    // -------------------------------------------------------------------------

    [Fact]
    public async Task TwoInstances_ShouldNotShareRetryState()
    {
        // Regression test: lastReadAttempt and lastAcessAttempt were static.
        // Instance A starting a retry timer must not affect instance B's first read.

        var path1 = TmpPath("inst1.txt");
        var path2 = TmpPath("inst2.txt");
        File.WriteAllText(path1, "content1");
        File.WriteAllText(path2, "content2");

        var sut1 = CreateSut();
        var sut2 = CreateSut();
        sut1.SetPathFile(path1);
        sut2.SetPathFile(path2);

        // Kick off sut1 retry timer (no bak exists → ProcessReaderException sets lastReadAttempt)
        sut1.ProcessReaderException(new IOException("locked"));

        // sut2 should still be able to read without being affected by sut1's retry state
        var result2 = sut2.ReadTXT();

        result2.Should().BeTrue("sut2 should read successfully regardless of sut1's retry state");
        sut2.TextResult.Should().Be("content2");
        sut2.eRead.Should().BeNull();
    }

    // -------------------------------------------------------------------------
    // BeforeWrite — bak validation with consistently-invalid bak
    // -------------------------------------------------------------------------

    [Fact]
    public void WriteTXT_WhenBakIsConsistentlyInvalid_ShouldEventuallyTimeOutAndReturnFalse()
    {
        // Arrange: original file passes check but every bak created is considered invalid
        var sut = CreateSut();
        var path = TmpPath("bak_invalid.txt");
        File.WriteAllText(path, "original");
        sut.SetPathFile(path);

        // Original passes; bak always fails
        _mockCheck.Setup(c => c.CheckTextFile(path)).Returns(true);
        _mockCheck.Setup(c => c.CheckTextFile(path + ".bak")).Returns(false);

        var result = sut.WriteTXT("new content");

        result.Should().BeFalse("bak consistently invalid — should time out");
        sut.eWrite.Should().NotBeNull();
    }

    // -------------------------------------------------------------------------
    // AfterWriteOrReader — bak cleanup
    // -------------------------------------------------------------------------

    [Fact]
    public void AfterWriteOrReader_WhenIsStayBakFalseAndBakExists_ShouldDeleteBak()
    {
        var sut = CreateSut();
        var path = TmpPath("after.txt");
        var bakPath = path + ".bak";
        File.WriteAllText(path, "data");
        File.WriteAllText(bakPath, "bak");
        sut.SetPathFile(path);
        sut.SetStayBak(false);

        var result = sut.AfterWriteOrReader();

        result.Should().BeTrue();
        File.Exists(bakPath).Should().BeFalse();
    }

    [Fact]
    public void AfterWriteOrReader_WhenIsStayBakTrueAndBakExists_ShouldKeepBak()
    {
        var sut = CreateSut();
        var path = TmpPath("after_keep.txt");
        var bakPath = path + ".bak";
        File.WriteAllText(path, "data");
        File.WriteAllText(bakPath, "bak");
        sut.SetPathFile(path);
        sut.SetStayBak(true);

        var result = sut.AfterWriteOrReader();

        result.Should().BeTrue();
        File.Exists(bakPath).Should().BeTrue();
    }

    [Fact]
    public void AfterWriteOrReader_WhenBakDoesNotExist_ShouldReturnTrue()
    {
        var sut = CreateSut();
        var path = TmpPath("after_nobak.txt");
        sut.SetPathFile(path);

        var result = sut.AfterWriteOrReader();

        result.Should().BeTrue();
    }

    // -------------------------------------------------------------------------
    // BeforeWrite — bak is created from original
    // -------------------------------------------------------------------------

    [Fact]
    public void BeforeWrite_WhenOriginalExists_ShouldCreateBakFile()
    {
        var sut = CreateSut();
        var path = TmpPath("bw_bak.txt");
        var bakPath = path + ".bak";
        File.WriteAllText(path, "data");
        sut.SetPathFile(path);

        var result = sut.BeforeWrite();

        result.Should().BeTrue();
        File.Exists(bakPath).Should().BeTrue("bak should be created from original before writing");
        File.ReadAllText(bakPath).Should().Be("data");
    }

    [Fact]
    public void BeforeWrite_WhenOriginalDoesNotExist_ShouldReturnTrueWithoutCreatingBak()
    {
        var sut = CreateSut();
        var path = TmpPath("bw_nonexistent.txt");
        var bakPath = path + ".bak";
        sut.SetPathFile(path);

        var result = sut.BeforeWrite();

        result.Should().BeTrue();
        File.Exists(bakPath).Should().BeFalse("no bak created when original does not exist");
    }

    // -------------------------------------------------------------------------
    // BeforeReader — bak is created from original
    // -------------------------------------------------------------------------

    [Fact]
    public void BeforeReader_WhenFileExists_ShouldCreateBakFile()
    {
        var sut = CreateSut();
        var path = TmpPath("br_bak.txt");
        var bakPath = path + ".bak";
        File.WriteAllText(path, "read data");
        sut.SetPathFile(path);

        var result = sut.BeforeReader();

        result.Should().BeTrue();
        File.Exists(bakPath).Should().BeTrue("bak must be created before reading for recovery purposes");
        File.ReadAllText(bakPath).Should().Be("read data");
    }

    [Fact]
    public void BeforeReader_WhenFileDoesNotExist_ShouldReturnFalseWithFileNotFoundException()
    {
        var sut = CreateSut();
        sut.SetPathFile(TmpPath("missing.txt"));

        var result = sut.BeforeReader();

        result.Should().BeFalse();
        sut.eRead.Should().BeOfType<FileNotFoundException>();
    }
}
