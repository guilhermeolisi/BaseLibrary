using FluentAssertions;
using System.Text;

namespace BaseLibrary.Tests;

/// <summary>
/// Unit tests for <see cref="TextFileEncodingDetector"/>.
/// </summary>
public class TextFileEncodingDetectorTests : IDisposable
{
    private readonly string _tmpDir;

    public TextFileEncodingDetectorTests()
    {
        _tmpDir = Path.Combine(Path.GetTempPath(), "TextFileEncodingDetectorTests_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tmpDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tmpDir))
            Directory.Delete(_tmpDir, true);
    }

    [Fact]
    public void DetectTextFileEncoding_ShouldFindUtf8Bom_WhenStreamReturnsOneBytePerRead()
    {
        // Arrange: um FileStream que devolve no maximo 1 byte por Read, como o contrato de Stream permite
        string path = Path.Combine(_tmpDir, "bom.txt");
        File.WriteAllBytes(path, [0xEF, 0xBB, 0xBF, (byte)'a', (byte)'b', (byte)'c']);
        using var stream = new OneBytePerReadFileStream(path);

        // Act
        Encoding? encoding = TextFileEncodingDetector.DetectTextFileEncoding(stream, 0x10000, out bool hasBom);

        // Assert
        hasBom.Should().BeTrue();
        encoding.Should().Be(Encoding.UTF8);
        stream.Position.Should().Be(0);
    }

    // FileStream cujo Read devolve no maximo 1 byte por chamada, nas duas sobrecargas
    private sealed class OneBytePerReadFileStream(string path) : FileStream(path, FileMode.Open, FileAccess.Read)
    {
        public override int Read(byte[] buffer, int offset, int count) => base.Read(buffer, offset, System.Math.Min(count, 1));

        public override int Read(Span<byte> buffer) => base.Read(buffer[..System.Math.Min(buffer.Length, 1)]);
    }
}
