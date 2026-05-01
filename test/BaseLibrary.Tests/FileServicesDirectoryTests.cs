using FluentAssertions;

namespace BaseLibrary.Tests;

/// <summary>
/// Unit tests for <see cref="FileServicesDirectory"/> covering all public methods,
/// edge cases, and the logic bugs that were fixed.
/// </summary>
public class FileServicesDirectoryTests : IDisposable
{
    private readonly string _tmpDir;
    private readonly FileServicesDirectory _sut;

    public FileServicesDirectoryTests()
    {
        _tmpDir = Path.Combine(Path.GetTempPath(), "FileServicesDirectoryTests_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tmpDir);
        _sut = new FileServicesDirectory();
    }

    public void Dispose()
    {
        // Remove read-only attributes before deleting, to avoid UnauthorizedAccessException
        if (Directory.Exists(_tmpDir))
        {
            foreach (var f in Directory.GetFiles(_tmpDir, "*", SearchOption.AllDirectories))
                File.SetAttributes(f, FileAttributes.Normal);
            foreach (var d in Directory.GetDirectories(_tmpDir, "*", SearchOption.AllDirectories))
                File.SetAttributes(d, FileAttributes.Normal);
            Directory.Delete(_tmpDir, recursive: true);
        }
    }

    private string TmpPath(string name) => Path.Combine(_tmpDir, name);

    private string CreateFile(string relativePath, string content = "test")
    {
        var fullPath = TmpPath(relativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        File.WriteAllText(fullPath, content);
        return fullPath;
    }

    // -------------------------------------------------------------------------
    // GetDirectorySize
    // -------------------------------------------------------------------------

    [Fact]
    public void GetDirectorySize_NonExistentDirectory_ReturnsZero()
    {
        var result = _sut.GetDirectorySize(TmpPath("does_not_exist"), recursive: false);

        result.Should().Be(0);
    }

    [Fact]
    public void GetDirectorySize_NullOrWhitespace_ReturnsZero()
    {
        _sut.GetDirectorySize(null!, recursive: false).Should().Be(0);
        _sut.GetDirectorySize("   ", recursive: false).Should().Be(0);
    }

    [Fact]
    public void GetDirectorySize_EmptyDirectory_ReturnsZero()
    {
        var dir = TmpPath("empty");
        Directory.CreateDirectory(dir);

        var result = _sut.GetDirectorySize(dir, recursive: false);

        result.Should().Be(0);
    }

    [Fact]
    public void GetDirectorySize_DirectoryWithFiles_ReturnsSumOfFileSizes()
    {
        var dir = TmpPath("sized");
        Directory.CreateDirectory(dir);
        File.WriteAllText(Path.Combine(dir, "a.txt"), "hello");     // 5 bytes
        File.WriteAllText(Path.Combine(dir, "b.txt"), "world!");    // 6 bytes

        var result = _sut.GetDirectorySize(dir, recursive: false);

        result.Should().Be(11);
    }

    [Fact]
    public void GetDirectorySize_Recursive_AccumulatesSubdirectorySizes()
    {
        // Previously broken: recursive call result was discarded.
        var root = TmpPath("recursive_size");
        Directory.CreateDirectory(root);
        File.WriteAllText(Path.Combine(root, "root.txt"), "ab");        // 2 bytes

        var sub = Path.Combine(root, "sub");
        Directory.CreateDirectory(sub);
        File.WriteAllText(Path.Combine(sub, "sub.txt"), "abcde");       // 5 bytes

        var result = _sut.GetDirectorySize(root, recursive: true);

        result.Should().Be(7);
    }

    [Fact]
    public void GetDirectorySize_NonRecursive_DoesNotCountSubdirectoryFiles()
    {
        var root = TmpPath("nonrec_size");
        Directory.CreateDirectory(root);
        File.WriteAllText(Path.Combine(root, "root.txt"), "ab");        // 2 bytes

        var sub = Path.Combine(root, "sub");
        Directory.CreateDirectory(sub);
        File.WriteAllText(Path.Combine(sub, "sub.txt"), "abcde");       // 5 bytes — should be ignored

        var result = _sut.GetDirectorySize(root, recursive: false);

        result.Should().Be(2);
    }

    // -------------------------------------------------------------------------
    // DirectoryCopy
    // -------------------------------------------------------------------------

    [Fact]
    public void DirectoryCopy_NonExistentSource_ThrowsDirectoryNotFoundException()
    {
        var act = () => _sut.DirectoryCopy(TmpPath("no_source"), TmpPath("dest"), copySubDirs: false, preserveTime: false);

        act.Should().Throw<DirectoryNotFoundException>();
    }

    [Fact]
    public void DirectoryCopy_CopiesFilesToDestination()
    {
        var src = TmpPath("copy_src");
        Directory.CreateDirectory(src);
        File.WriteAllText(Path.Combine(src, "file.txt"), "content");

        var dest = TmpPath("copy_dest");
        _sut.DirectoryCopy(src, dest, copySubDirs: false, preserveTime: false);

        File.Exists(Path.Combine(dest, "file.txt")).Should().BeTrue();
        File.ReadAllText(Path.Combine(dest, "file.txt")).Should().Be("content");
    }

    [Fact]
    public void DirectoryCopy_PreserveTime_True_KeepsOriginalTimestamps()
    {
        var src = TmpPath("ptime_src");
        Directory.CreateDirectory(src);
        var filePath = Path.Combine(src, "file.txt");
        File.WriteAllText(filePath, "data");
        var originalWrite = new DateTime(2020, 6, 15, 10, 0, 0, DateTimeKind.Local);
        File.SetLastWriteTime(filePath, originalWrite);

        var dest = TmpPath("ptime_dest");
        _sut.DirectoryCopy(src, dest, copySubDirs: false, preserveTime: true);

        File.GetLastWriteTime(Path.Combine(dest, "file.txt")).Should().BeCloseTo(originalWrite, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void DirectoryCopy_CopySubDirs_True_CopiesSubdirectoriesRecursively()
    {
        var src = TmpPath("subdir_src");
        Directory.CreateDirectory(src);
        File.WriteAllText(Path.Combine(src, "root.txt"), "root");

        var sub = Path.Combine(src, "sub");
        Directory.CreateDirectory(sub);
        File.WriteAllText(Path.Combine(sub, "sub.txt"), "sub");

        var dest = TmpPath("subdir_dest");
        _sut.DirectoryCopy(src, dest, copySubDirs: true, preserveTime: false);

        File.Exists(Path.Combine(dest, "root.txt")).Should().BeTrue();
        File.Exists(Path.Combine(dest, "sub", "sub.txt")).Should().BeTrue();
    }

    [Fact]
    public void DirectoryCopy_CopySubDirs_False_DoesNotCopySubdirectories()
    {
        var src = TmpPath("nosubdir_src");
        Directory.CreateDirectory(src);
        File.WriteAllText(Path.Combine(src, "root.txt"), "root");

        var sub = Path.Combine(src, "sub");
        Directory.CreateDirectory(sub);
        File.WriteAllText(Path.Combine(sub, "sub.txt"), "sub");

        var dest = TmpPath("nosubdir_dest");
        _sut.DirectoryCopy(src, dest, copySubDirs: false, preserveTime: false);

        File.Exists(Path.Combine(dest, "root.txt")).Should().BeTrue();
        Directory.Exists(Path.Combine(dest, "sub")).Should().BeFalse();
    }

    [Fact]
    public void DirectoryCopy_DestinationDoesNotExist_CreatesDestination()
    {
        var src = TmpPath("mkdest_src");
        Directory.CreateDirectory(src);
        File.WriteAllText(Path.Combine(src, "file.txt"), "x");

        var dest = TmpPath("mkdest_dest");
        _sut.DirectoryCopy(src, dest, copySubDirs: false, preserveTime: false);

        Directory.Exists(dest).Should().BeTrue();
    }

    // -------------------------------------------------------------------------
    // DirectoryClear
    // -------------------------------------------------------------------------

    [Fact]
    public void DirectoryClear_NonExistentDirectory_DoesNotThrow()
    {
        var act = () => _sut.DirectoryClear(TmpPath("ghost_dir"));

        act.Should().NotThrow();
    }

    [Fact]
    public void DirectoryClear_EmptyDirectory_DoesNotThrow()
    {
        var dir = TmpPath("clear_empty");
        Directory.CreateDirectory(dir);

        var act = () => _sut.DirectoryClear(dir);

        act.Should().NotThrow();
    }

    [Fact]
    public void DirectoryClear_DirectoryWithFiles_DeletesAllFiles()
    {
        var dir = TmpPath("clear_files");
        Directory.CreateDirectory(dir);
        File.WriteAllText(Path.Combine(dir, "a.txt"), "a");
        File.WriteAllText(Path.Combine(dir, "b.txt"), "b");

        _sut.DirectoryClear(dir);

        Directory.GetFiles(dir).Should().BeEmpty();
    }

    [Fact]
    public void DirectoryClear_DirectoryWithSubdirectories_DeletesAllSubdirectories()
    {
        var dir = TmpPath("clear_subdirs");
        Directory.CreateDirectory(dir);
        Directory.CreateDirectory(Path.Combine(dir, "sub1"));
        Directory.CreateDirectory(Path.Combine(dir, "sub2"));

        _sut.DirectoryClear(dir);

        Directory.GetDirectories(dir).Should().BeEmpty();
    }

    [Fact]
    public void DirectoryClear_DirectoryItself_IsKeptAfterClear()
    {
        var dir = TmpPath("clear_keep");
        Directory.CreateDirectory(dir);
        File.WriteAllText(Path.Combine(dir, "f.txt"), "f");

        _sut.DirectoryClear(dir);

        Directory.Exists(dir).Should().BeTrue();
    }

    // -------------------------------------------------------------------------
    // CreatAllPath
    // -------------------------------------------------------------------------

    [Fact]
    public void CreatAllPath_NullGoal_ThrowsArgumentNullException()
    {
        var act = () => _sut.CreatAllPath(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void CreatAllPath_WhitespaceGoal_ThrowsArgumentException()
    {
        var act = () => _sut.CreatAllPath("   ");

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void CreatAllPath_NestedPath_CreatesAllDirectories()
    {
        var path = TmpPath(Path.Combine("level1", "level2", "level3"));

        _sut.CreatAllPath(path);

        Directory.Exists(path).Should().BeTrue();
    }

    [Fact]
    public void CreatAllPath_AlreadyExistingPath_DoesNotThrow()
    {
        var path = TmpPath("existing_path");
        Directory.CreateDirectory(path);

        var act = () => _sut.CreatAllPath(path);

        act.Should().NotThrow();
    }

    // -------------------------------------------------------------------------
    // RenameAllWhithoutSpaces
    // -------------------------------------------------------------------------

    [Fact]
    public void RenameAllWhithoutSpaces_RenamesFilesWithSpaces()
    {
        var dir = TmpPath("rename_files");
        Directory.CreateDirectory(dir);
        File.WriteAllText(Path.Combine(dir, "my file.txt"), "x");

        _sut.RenameAllWhithoutSpaces(dir);

        File.Exists(Path.Combine(dir, "my%20file.txt")).Should().BeTrue();
        File.Exists(Path.Combine(dir, "my file.txt")).Should().BeFalse();
    }

    [Fact]
    public void RenameAllWhithoutSpaces_FilesWithoutSpaces_AreNotRenamed()
    {
        var dir = TmpPath("rename_noop");
        Directory.CreateDirectory(dir);
        File.WriteAllText(Path.Combine(dir, "nospacefile.txt"), "x");

        _sut.RenameAllWhithoutSpaces(dir);

        File.Exists(Path.Combine(dir, "nospacefile.txt")).Should().BeTrue();
    }

    [Fact]
    public void RenameAllWhithoutSpaces_RenamesFoldersWithSpaces()
    {
        var parent = TmpPath("rename_folders");
        Directory.CreateDirectory(parent);
        var spacedFolder = Path.Combine(parent, "my folder");
        Directory.CreateDirectory(spacedFolder);
        File.WriteAllText(Path.Combine(spacedFolder, "inner.txt"), "x");

        _sut.RenameAllWhithoutSpaces(parent);

        Directory.Exists(Path.Combine(parent, "my%20folder")).Should().BeTrue();
        Directory.Exists(spacedFolder).Should().BeFalse();
    }

    [Fact]
    public void RenameAllWhithoutSpaces_Recursive_RenamesFilesInSubdirectories()
    {
        var root = TmpPath("rename_recursive");
        Directory.CreateDirectory(root);
        var sub = Path.Combine(root, "sub");
        Directory.CreateDirectory(sub);
        File.WriteAllText(Path.Combine(sub, "inner file.txt"), "x");

        _sut.RenameAllWhithoutSpaces(root);

        File.Exists(Path.Combine(sub, "inner%20file.txt")).Should().BeTrue();
        File.Exists(Path.Combine(sub, "inner file.txt")).Should().BeFalse();
    }

    // -------------------------------------------------------------------------
    // CountTotalFiles
    // -------------------------------------------------------------------------

    [Fact]
    public void CountTotalFiles_NonExistentFolder_ReturnsZero()
    {
        var result = _sut.CountTotalFiles(TmpPath("does_not_exist"));

        result.Should().Be(0);
    }

    [Fact]
    public void CountTotalFiles_EmptyFolder_ReturnsZero()
    {
        var dir = TmpPath("count_empty");
        Directory.CreateDirectory(dir);

        var result = _sut.CountTotalFiles(dir);

        result.Should().Be(0);
    }

    [Fact]
    public void CountTotalFiles_FlatFolder_ReturnsCorrectCount()
    {
        var dir = TmpPath("count_flat");
        Directory.CreateDirectory(dir);
        File.WriteAllText(Path.Combine(dir, "a.txt"), "a");
        File.WriteAllText(Path.Combine(dir, "b.txt"), "b");
        File.WriteAllText(Path.Combine(dir, "c.txt"), "c");

        var result = _sut.CountTotalFiles(dir);

        result.Should().Be(3);
    }

    [Fact]
    public void CountTotalFiles_Recursive_CountsFilesInSubdirectories()
    {
        var root = TmpPath("count_recursive");
        Directory.CreateDirectory(root);
        File.WriteAllText(Path.Combine(root, "root.txt"), "r");

        var sub = Path.Combine(root, "sub");
        Directory.CreateDirectory(sub);
        File.WriteAllText(Path.Combine(sub, "sub1.txt"), "s1");
        File.WriteAllText(Path.Combine(sub, "sub2.txt"), "s2");

        var result = _sut.CountTotalFiles(root);

        result.Should().Be(3);
    }

    [Fact]
    public void CountTotalFiles_WithPattern_CountsOnlyMatchingFiles()
    {
        var dir = TmpPath("count_pattern");
        Directory.CreateDirectory(dir);
        File.WriteAllText(Path.Combine(dir, "a.txt"), "a");
        File.WriteAllText(Path.Combine(dir, "b.csv"), "b");
        File.WriteAllText(Path.Combine(dir, "c.txt"), "c");

        var result = _sut.CountTotalFiles(dir, "*.txt");

        result.Should().Be(2);
    }

    [Fact]
    public void CountTotalFiles_WithExcludePattern_ExcludesMatchingFiles()
    {
        var dir = TmpPath("count_exclude");
        Directory.CreateDirectory(dir);
        File.WriteAllText(Path.Combine(dir, "keep.txt"), "k");
        File.WriteAllText(Path.Combine(dir, "skip.log"), "s");

        var result = _sut.CountTotalFiles(dir, excludePattern: [".log"]);

        result.Should().Be(1);
    }

    // -------------------------------------------------------------------------
    // SetAttributesNormal
    // -------------------------------------------------------------------------

    [Fact]
    public void SetAttributesNormal_String_NonExistentFolder_DoesNotThrow()
    {
        var act = () => _sut.SetAttributesNormal(TmpPath("no_such_dir"));

        act.Should().NotThrow();
    }

    [Fact]
    public void SetAttributesNormal_DirectoryInfo_NonExistentDir_DoesNotThrow()
    {
        var dir = new DirectoryInfo(TmpPath("no_such_dir2"));

        var act = () => _sut.SetAttributesNormal(dir);

        act.Should().NotThrow();
    }

    [Fact]
    public void SetAttributesNormal_ReadOnlyFiles_SetsNormalAttributes()
    {
        var dir = TmpPath("attrs_dir");
        Directory.CreateDirectory(dir);
        var filePath = Path.Combine(dir, "readonly.txt");
        File.WriteAllText(filePath, "x");
        File.SetAttributes(filePath, FileAttributes.ReadOnly);

        _sut.SetAttributesNormal(dir);

        File.GetAttributes(filePath).Should().Be(FileAttributes.Normal);
    }

    [Fact]
    public void SetAttributesNormal_Recursive_ClearsAttributesInSubdirectories()
    {
        var root = TmpPath("attrs_recursive");
        Directory.CreateDirectory(root);
        var sub = Path.Combine(root, "sub");
        Directory.CreateDirectory(sub);
        var filePath = Path.Combine(sub, "readonly.txt");
        File.WriteAllText(filePath, "x");
        File.SetAttributes(filePath, FileAttributes.ReadOnly);

        _sut.SetAttributesNormal(root);

        File.GetAttributes(filePath).Should().Be(FileAttributes.Normal);
    }
}
