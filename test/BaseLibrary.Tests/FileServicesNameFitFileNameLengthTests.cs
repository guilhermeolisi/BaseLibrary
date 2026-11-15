using FluentAssertions;
using System.Text;

namespace BaseLibrary.Tests;

/// <summary>
/// Excecao de producao (08 e 09/11/2026, Nimloth 26.11.1 no Linux, sete relatos): a exportacao de uma imagem do Results
/// montava o nome do arquivo com o titulo inteiro do grafico ("&lt;work&gt; - MicrostrainPAH. Root mean square strain ...
/// Projection (positive) on lattice plane (1 0 1).png", ~290 caracteres), e o File.OpenWrite recusava ("is too long, or a
/// component of the specified path is too long"). O <see cref="FileServicesName.FitFileNameLength"/> e o teto do nome.
/// </summary>
public class FileServicesNameFitFileNameLengthTests : IDisposable
{
    private readonly string _tmpDir = Path.Combine(Path.GetTempPath(), "FitFileNameLength_" + Guid.NewGuid().ToString("N"));
    private readonly FileServicesName _sut = new();

    // O nome do relato, com o titulo do work trocado por um generico.
    private const string ReportedName = "2,5% Ag NaNbO3 - MicrostrainPAH. Root mean square strain  eps_hkl^2(L) ^1 2 by direction, " +
        "at Fourier length L. Adler-Houska model with Popa anisotropy, alpha 0.0003972, beta 2.405E-06. Fourier length L (A) = 100. " +
        "Projection (positive) on lattice plane (1 0 1)";

    public FileServicesNameFitFileNameLengthTests() => Directory.CreateDirectory(_tmpDir);

    public void Dispose()
    {
        try { Directory.Delete(_tmpDir, recursive: true); } catch (IOException) { }
    }

    [Fact]
    public void FitFileNameLength_ShouldLetTheReportedImageBeWritten_KeepingFolderAndExtension()
    {
        string original = Path.Combine(_tmpDir, ReportedName + ".png");

        string fitted = _sut.FitFileNameLength(original);

        Path.GetDirectoryName(fitted).Should().Be(_tmpDir);
        Path.GetExtension(fitted).Should().Be(".png");
        Encoding.UTF8.GetByteCount(Path.GetFileName(fitted)).Should().BeLessThanOrEqualTo(FileServicesName.DefaultMaxFileNameBytes);
        Path.GetFileNameWithoutExtension(fitted).Should().StartWith("2,5% Ag NaNbO3 - MicrostrainPAH. Root mean square strain");
        ReportedName.Should().StartWith(Path.GetFileNameWithoutExtension(fitted));
        // O mesmo caminho do relato, gravado de verdade (no Windows o componente acima de 255 tambem falha).
        File.WriteAllBytes(fitted, [1, 2, 3]);
        File.ReadAllBytes(fitted).Should().Equal(1, 2, 3);
    }

    // Controle: o caminho do relato, sem o teto, e recusado tambem aqui (componente acima de 255 no Windows e no Linux).
    [Fact]
    public void TheReportedName_WithoutTheLimit_IsRefusedByTheFileSystem()
    {
        string original = Path.Combine(_tmpDir, ReportedName + ".png");

        Action write = () => File.WriteAllBytes(original, [1]);

        write.Should().Throw<IOException>();
    }

    [Fact]
    public void FitFileNameLength_ShouldReturnTheSamePath_WhenTheNameFits()
    {
        string path = Path.Combine(_tmpDir, "work - CaF2 Dislocation RMSS.png");

        _sut.FitFileNameLength(path).Should().Be(path);
    }

    [Fact]
    public void FitFileNameLength_ShouldNotSplitAMultibyteCharacter_NorEndInSpaceOrDot()
    {
        // 'ε' ocupa 2 bytes e '𝛼' (fora do plano basico) 4: o corte cai entre elementos, nunca no meio.
        string name = string.Concat(Enumerable.Repeat("ε𝛼 .", 60));
        string path = Path.Combine(_tmpDir, name + ".svg");

        string fitted = _sut.FitFileNameLength(path, 40);

        string fittedName = Path.GetFileNameWithoutExtension(fitted);
        Encoding.UTF8.GetByteCount(fittedName + ".svg").Should().BeLessThanOrEqualTo(40);
        name.Should().StartWith(fittedName);
        fittedName.Should().NotEndWith(" ").And.NotEndWith(".");
        fittedName.Should().NotContain("�");
        char.IsHighSurrogate(fittedName[^1]).Should().BeFalse();
    }

    [Fact]
    public void FitFileNameLength_ByTheExtensionMethod_ShouldMatchTheService()
    {
        string original = Path.Combine(_tmpDir, ReportedName + ".txt");

        original.FitFileNameLength().Should().Be(_sut.FitFileNameLength(original));
    }
}
