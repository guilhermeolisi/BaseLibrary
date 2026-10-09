using System.Globalization;
using BaseLibrary.Numbers;
using FluentAssertions;

namespace BaseLibrary.Tests;

/// <summary>
/// Texto de resultado com esd (E093): o esd corrigido entre colchetes (E116), na mesma unidade do parentese, e o zero
/// com esd. O texto usa a cultura corrente, entao cada caso roda sob a invariante.
/// </summary>
public class DoubleResultTextTests
{
    private readonly NumberServices numbers = new();

    private static string Invariant(Func<string> act)
    {
        CultureInfo previous = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
        try
        {
            return act();
        }
        finally
        {
            CultureInfo.CurrentCulture = previous;
        }
    }

    [Theory]
    [InlineData(18.5, 0.1, 0.3, "18.5(1)[3]")]
    [InlineData(18.5, 0.1, 1.2, "18.5(1)[12]")]
    [InlineData(1.23E-05, 4E-07, 8E-07, "1.23(4)[8]E-5")]
    public void DoubleResultText_ShouldAppendCorrectedEsdInTheSameUnit_WhenCorrectedEsdIsFinite(double value, double esd, double corrected, string expected)
    {
        string text = Invariant(() => numbers.DoubleResultText(value, esd, corrected));

        text.Should().Be(expected);
    }

    [Theory]
    [InlineData(18.5, 0.1)]
    [InlineData(1.23E-05, 4E-07)]
    [InlineData(1234.5678, 0.002)]
    public void DoubleResultText_ShouldMatchTheTextWithoutCorrection_WhenCorrectedEsdIsNaN(double value, double esd)
    {
        string withoutCorrection = Invariant(() => numbers.DoubleResultText(value, esd));

        string text = Invariant(() => numbers.DoubleResultText(value, esd, double.NaN));

        text.Should().Be(withoutCorrection);
    }

    [Fact]
    public void DoubleResultTextOfNumberEsd_ShouldWriteTheCorrectedEsd_LikeTheOverloadWithFourArguments()
    {
        NumberESD number = new NumberESD(18.5, 0.1).WithCorrectedEsd(0.3);

        string text = Invariant(() => numbers.DoubleResultText(number));

        text.Should().Be(Invariant(() => numbers.DoubleResultText(18.5, 0.1, 0.3)));
        text.Should().Be("18.5(1)[3]");
    }

    [Fact]
    public void DoubleResultTextOfNumberEsd_ShouldBeTheTextWithoutBrackets_WhenThereIsNoCorrectedEsd()
    {
        NumberESD number = new(18.5, 0.1);

        string text = Invariant(() => numbers.DoubleResultText(number));

        text.Should().Be(Invariant(() => numbers.DoubleResultText(18.5, 0.1)));
    }

    [Fact]
    public void DoubleResultText_ShouldNotWriteAnyParenthesis_WhenEsdIsNaNAndCorrectedIsFinite()
    {
        string text = Invariant(() => numbers.DoubleResultText(18.5, double.NaN, 0.3));

        text.Should().Be("18.5");
    }

    [Fact]
    public void DoubleResultText_ShouldWriteZeroWithTheDecimalsTheEsdAsks_WhenValueIsZero()
    {
        string text = Invariant(() => numbers.DoubleResultText(0, 7E-06));

        text.Should().Be("0.000000(7)");
    }

    [Fact]
    public void DoubleResultText_ShouldWriteZeroWithIntegerEsd_WhenValueIsZeroAndEsdIsAboveOne()
    {
        string text = Invariant(() => numbers.DoubleResultText(0, 30));

        text.Should().Be("0(30)");
    }

    [Theory]
    [InlineData(1.0489997671492222E-30)]
    [InlineData(double.PositiveInfinity)]
    public void DoubleResultText_ShouldWriteBareZero_WhenEsdOfZeroIsBelowDoublePrecisionOrInfinite(double esd)
    {
        string text = Invariant(() => numbers.DoubleResultText(0, esd));

        text.Should().Be("0");
    }

    [Fact]
    public void WithESDCorrected_ShouldMultiplyTheEsd_AndNumberTextShouldShowBothParentheses()
    {
        var number = new NumberESD(18.5, 0.1);

        NumberESD corrected = number.WithESDCorrected(3);

        corrected.ESDCorrected.Should().BeApproximately(0.3, 1E-15);
        Invariant(() => corrected.NumberText!).Should().Be("18.5(1)[3]");
        Invariant(() => number.NumberText!).Should().Be("18.5(1)", "a copia nao muda o original");
    }

    [Fact]
    public void WithESDCorrected_ShouldGiveNaN_WhenEsdIsNaN()
    {
        var number = new NumberESD(18.5, double.NaN);

        NumberESD corrected = number.WithESDCorrected(3);

        double.IsNaN(corrected.ESDCorrected).Should().BeTrue();
    }

    [Theory]
    [InlineData(0.3)]
    [InlineData(0.05)]
    public void WithCorrectedEsd_ShouldStoreTheGivenEsd_NotMultiplyIt(double correctedEsd)
    {
        var number = new NumberESD(18.5, 0.1);

        NumberESD corrected = number.WithCorrectedEsd(correctedEsd);

        corrected.ESDCorrected.Should().Be(correctedEsd);
        corrected.ESD.Should().Be(0.1, "o esd normal nao muda");
        double.IsNaN(number.ESDCorrected).Should().BeTrue("a copia nao muda o original");
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(-0.2)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    public void WithCorrectedEsd_ShouldGiveNaN_WhenTheEsdIsNotFiniteOrNotPositive(double correctedEsd)
    {
        NumberESD corrected = new NumberESD(18.5, 0.1).WithCorrectedEsd(correctedEsd);

        double.IsNaN(corrected.ESDCorrected).Should().BeTrue();
    }

    [Fact]
    public void WithCorrectedEsd_ShouldBeWrittenInBrackets_InTheUnitOfTheParenthesis()
    {
        NumberESD corrected = new NumberESD(18.5, 0.1).WithCorrectedEsd(0.07);

        Invariant(() => corrected.NumberText!).Should().Be("18.5(1)[1]", "0,07 em unidades de 0,1 arredonda para 1: o corrigido menor que o normal tambem aparece");
    }

    [Fact]
    public void NewNumberESD_ShouldHaveNaNCorrectedEsd()
    {
        double.IsNaN(new NumberESD(18.5, 0.1).ESDCorrected).Should().BeTrue();
        double.IsNaN(new NumberESD().ESDCorrected).Should().BeTrue();
        double.IsNaN(new NumberESD("x").ESDCorrected).Should().BeTrue();
    }
}
