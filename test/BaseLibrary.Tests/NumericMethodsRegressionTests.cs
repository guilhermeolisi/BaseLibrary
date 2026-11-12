using FluentAssertions;
using BaseLibrary.Math;
using static BaseLibrary.Tests.RegressionHash;

namespace BaseLibrary.Tests;

/// <summary>
/// Regressao bit a bit de <see cref="NumericMethods"/> contra a saida da classe de origem no motor (antes da E269) e
/// casos limite com valor analitico. O corpo subiu verbatim do Sindarin.Math; estes testes provam que a mudanca de casa
/// nao mudou um bit.
/// Os valores de regressao vieram da captura da E269 (programa de captura sobre Sindarin.Math do Sindarin 5a0fd38e,
/// master de 12/11/2026, win-x64, .NET 10, Debug), com as entradas de <see cref="RegressionInputs"/>.
/// </summary>
public class NumericMethodsRegressionTests
{
    private static readonly double[] Y = RegressionInputs.Signal();
    private static readonly double[] X = RegressionInputs.Grid();

    [Fact]
    public void InterpolationAndIndex_ShouldMatchTheEngineOutputBitForBit()
    {
        double[][] xs = { RegressionInputs.Grid(40), Enumerable.Range(0, 60).Select(i => 30.0 + 0.5 * i).ToArray() };
        var copy = new double[10];
        NumericMethods.CopyArray(Y, copy, 5, 10);
        int close = 10;
        NumericMethods.GetIndexX(X, 33.3, ref close);

        // Oraculo: regressao, saida de Sindarin.Math.GeneralMethods em Sindarin 5a0fd38e
        Bits(NumericMethods.Interpolation(1.0, 2.0, 3.0, 5.0, 2.2)).Should().Be(0x400E666666666667UL);
        Bits(NumericMethods.InterpolationLogLog(1.0, 2.0, 10.0, 50.0, 3.3)).Should().Be(0x40253A658E73393DUL);
        Bits(NumericMethods.Interpolation(new[] { 1.0, 2.0, 3.0 }, RegressionInputs.ComplexSeries(), 1, 2.4)).Should().Be(0x3FF999999999999CUL);
        Of(copy).Should().Be(0xED41696A33CAF59CUL);
        Of(new[] { 1, -2, 3 }.ToDouble()).Should().Be(0x1C4B2CD0A97D27CFUL);
        Of(NumericMethods.GetIndexX(xs, 41.2, new[] { 0.5, 0.5 })).Should().Be(0xB5EF42E0D553C4D0UL);
        Of(NumericMethods.GetIndexX(xs, 41.2, new[] { double.NaN, double.NaN })).Should().Be(0xB5EF42E0D553C4D0UL);
        NumericMethods.GetIndexX(xs, 41.2, 1, new[] { 0.5, 0.5 }).Should().Be(22);
        NumericMethods.GetIndexX(xs, 41.2, 1, null).Should().Be(22);
        NumericMethods.GetIndexX(X, 33.3).Should().Be(46);
        close.Should().Be(46);
        Of(NumericMethods.Linspace(10.0, 20.0, 11)).Should().Be(0x9B666A7D6FDECB48UL);
        Of(NumericMethods.Linspace(0.0, 1.0, 7)).Should().Be(0xEFE04F0738089D63UL);
        Of(NumericMethods.LinearInterpolate(X, Y, Enumerable.Range(0, 81).Select(k => 10.25 + 0.75 * k).ToArray()))
            .Should().Be(0x56556C64A3402A97UL);
    }

    [Theory]
    [InlineData(0, 0x3FF0000000000000UL)]
    [InlineData(1, 0x3FD7AE147AE147AEUL)]
    [InlineData(2, 0xBFD2DB8BAC710CB3UL)]
    [InlineData(3, 0xBFDB6A5F84CAD57BUL)]
    [InlineData(4, 0xBFACDDE62F7461D8UL)]
    [InlineData(5, 0x3FD3877D48B71B38UL)]
    [InlineData(6, 0x3FD04119F9AE8454UL)]
    [InlineData(7, 0xBFB648059A092640UL)]
    [InlineData(8, 0xBFD216414CC97D3CUL)]
    public void Legendre_ShouldMatchTheEngineOutputBitForBit(byte n, ulong expected)
    {
        // Oraculo: regressao, saida de Sindarin.Math.GeneralMethods em Sindarin 5a0fd38e
        Bits(NumericMethods.Legendre(n, 0.37)).Should().Be(expected);
    }

    [Fact]
    public void RickerAndPeakSearch_ShouldMatchTheEngineOutputBitForBit()
    {
        double[] ricker = NumericMethods.RickerWavelet(4.0, 0.0, 0.5);
        double mean = Y.Average();

        // Oraculo: regressao, saida de Sindarin.Math.GeneralMethods em Sindarin 5a0fd38e
        Of(ricker).Should().Be(0x15CDA0DACBA96AD9UL);
        Bits(NumericMethods.WaveletCoefficient(Y, ricker, 40, 0.5)).Should().Be(0x404379A8A837D584UL);
        Bits(NumericMethods.CalculeRickerWaveletValue(4.0, 0.0, 1.3)).Should().Be(0x3FBB25D1599E80CBUL);
        Bits(NumericMethods.RickerWaveletContinuousEnergy(4.0)).Should().Be(0x3FB544FA6D47B390UL);
        Of(NumericMethods.LogSpacedDescending(16.0, 1.0, 2.0)).Should().Be(0xD4850402E9532F3DUL);
        Of(NumericMethods.FindLocalMaxima(Y, 20.0)).Should().Be(0x9EE52D240046A670UL);
        Of(NumericMethods.RickerWaveletTransform(Y, new[] { 2.0, 4.0, 8.0 }, 0.5)).Should().Be(0xEFCB0992E4D67BEAUL);
        Of(NumericMethods.FindLocalMinima(Y, 20.0)).Should().Be(0x3065892AD2DCB137UL);
        Of(NumericMethods.FindDownwardZeroCrossings(Y.Select(v => v - mean).ToArray())).Should().Be(0xCF19D5AD498D1396UL);
        Bits(NumericMethods.ParabolicVertexOffset(3.0, 5.0, 4.0)).Should().Be(0x3FC5555555555555UL);
    }

    [Theory]
    [InlineData(WaveletFamily.Haar, 1, 0x092357EEC68A631FUL)]
    [InlineData(WaveletFamily.Daubechies, 1, 0x092357EEC68A631FUL)]
    [InlineData(WaveletFamily.Daubechies, 2, 0x97BFDC89A906AF83UL)]
    [InlineData(WaveletFamily.Daubechies, 3, 0xA03933FF86C41ACEUL)]
    [InlineData(WaveletFamily.Daubechies, 4, 0xB5F5449D83D08A44UL)]
    [InlineData(WaveletFamily.Daubechies, 5, 0x67343298F3545E02UL)]
    [InlineData(WaveletFamily.Daubechies, 6, 0xE923A0B4D3727237UL)]
    [InlineData(WaveletFamily.Daubechies, 7, 0xC333057E1C3420E3UL)]
    [InlineData(WaveletFamily.Daubechies, 8, 0x7CF2E1E8534C6A01UL)]
    [InlineData(WaveletFamily.Daubechies, 10, 0x06A9B9DCE436B32DUL)]
    [InlineData(WaveletFamily.Symlet, 4, 0xEAA3237EF7E90C8AUL)]
    [InlineData(WaveletFamily.Symlet, 5, 0xA0DF356E0635C38CUL)]
    [InlineData(WaveletFamily.Symlet, 6, 0xE43EA8E4C76599A6UL)]
    [InlineData(WaveletFamily.Symlet, 8, 0x3736C8E5AAEB4867UL)]
    [InlineData(WaveletFamily.Coiflet, 1, 0x988D18DA595CA683UL)]
    [InlineData(WaveletFamily.Coiflet, 2, 0x92EA4F2F9FF206DFUL)]
    [InlineData(WaveletFamily.Coiflet, 3, 0xFC583BC28A21FB6CUL)]
    [InlineData(WaveletFamily.Coiflet, 4, 0x7E284B7DF6EFF950UL)]
    [InlineData(WaveletFamily.Coiflet, 5, 0x26C5A5F3A08D56CFUL)]
    public void GetWaveletScalingFilter_ShouldMatchTheEngineOutputBitForBit(WaveletFamily family, int order, ulong expected)
    {
        // Oraculo: regressao, saida de Sindarin.Math.GeneralMethods em Sindarin 5a0fd38e
        Of(NumericMethods.GetWaveletScalingFilter(family, order)).Should().Be(expected);
    }

    [Fact]
    public void DiscreteWavelet_ShouldMatchTheEngineOutputBitForBit()
    {
        double[] low = NumericMethods.GetWaveletScalingFilter(WaveletFamily.Daubechies, 4);
        double[] high = NumericMethods.WaveletDetailFilter(low);
        double[] sig = Y.Take(16).ToArray();
        var a = new double[8];
        var d = new double[8];
        NumericMethods.DiscreteWaveletStepForward(sig, low, high, a, d);
        var inverse = new double[16];
        NumericMethods.DiscreteWaveletStepInverse(a, d, low, high, inverse);
        double[][] coeffs = NumericMethods.WaveletDecompose(Y, low, high, 3);

        // Oraculo: regressao, saida de Sindarin.Math.GeneralMethods em Sindarin 5a0fd38e
        Of(high).Should().Be(0xB40B80B5DD7594BCUL);
        Of(a).Should().Be(0x56B7FB94B5DBFFD5UL);
        Of(d).Should().Be(0x7D303A28913EC0AEUL);
        Of(inverse).Should().Be(0x57BE38722D18F240UL);
        Of(coeffs).Should().Be(0x88EF111B037E662FUL);
        Of(NumericMethods.WaveletReconstruct(coeffs, low, high)).Should().Be(0x1E526B01D2E06460UL);
        Bits(NumericMethods.SoftThreshold(1.7, 1.2)).Should().Be(0x3FE0000000000000UL);
        Bits(NumericMethods.SoftThreshold(-0.5, 1.2)).Should().Be(0x0000000000000000UL);
        Bits(NumericMethods.HardThreshold(1.7, 1.2)).Should().Be(0x3FFB333333333333UL);
        Bits(NumericMethods.HardThreshold(-0.5, 1.2)).Should().Be(0x0000000000000000UL);
        Bits(NumericMethods.EstimateNoiseStandardDeviation((double[])coeffs[3].Clone())).Should().Be(0x3FF8A4EACFE70E3BUL);
        Bits(NumericMethods.UniversalThreshold(2.5, 128)).Should().Be(0x401F26BE4A726ECFUL);
        NumericMethods.MaxWaveletLevels(128).Should().Be(7);
        NumericMethods.MaxWaveletLevels(100).Should().Be(6);
        Bits(NumericMethods.Median(new[] { 5.5, -1.0, 3.25, 9.0, 0.5 })).Should().Be(0x400A000000000000UL);
        Bits(NumericMethods.Median(new[] { 5.5, -1.0, 3.25, 9.0, 0.5, 2.0 })).Should().Be(0x4005000000000000UL);
    }

    [Fact]
    public void BandWhittakerAndBSpline_ShouldMatchTheEngineOutputBitForBit()
    {
        var (bd, bs) = RegressionInputs.Band();
        var (pd2, ps2) = NumericMethods.DifferencePenaltyBand(12, 2);
        var (pd3, ps3) = NumericMethods.DifferencePenaltyBand(12, 3);
        double[] whit = NumericMethods.WhittakerSmooth(Y, RegressionInputs.Weights(), 100.0, 2);
        var basis = new double[4];
        NumericMethods.EvaluateBSplineNonZero(2.37, 0.0, 0.5, 10, 3, basis, out int first);
        var (sd, ss) = NumericMethods.BandedSelectedInverse(bd, bs);

        // Oraculo: regressao, saida de Sindarin.Math.GeneralMethods em Sindarin 5a0fd38e
        Of(NumericMethods.SolveSymmetricBanded(bd, bs, RegressionInputs.BandRhs())).Should().Be(0x612462385197381FUL);
        Combine(Of(pd2), Of(ps2)).Should().Be(0x6B67B285E4D84B0CUL);
        Combine(Of(pd3), Of(ps3)).Should().Be(0xDDDEAFCE1C76B0B9UL);
        Of(whit).Should().Be(0xB7566C361963D3A3UL);
        Of(NumericMethods.WhittakerSmooth(Y, null, 100.0, 2)).Should().Be(0xEA3F48C5D17DAC2FUL);
        Bits(NumericMethods.PoissonDeviance(Y, whit)).Should().Be(0xC0D368471D31D06FUL);
        Bits(NumericMethods.BandQuadraticForm(bd, bs, RegressionInputs.BandRhs())).Should().Be(0x4062500000000000UL);
        NumericMethods.BSplineBasisCount(10, 3).Should().Be(13);
        Of(basis).Should().Be(0x3324B480130151AAUL);
        first.Should().Be(4);
        Bits(NumericMethods.LCurveCorner(new[] { 0.1, 1.0, 10.0, 100.0, 1000.0, 10000.0 },
            new[] { 1.0, 1.1, 1.5, 3.0, 9.0, 30.0 }, new[] { 50.0, 20.0, 8.0, 4.0, 3.0, 2.8 })).Should().Be(0x404B800000000000UL);
        Combine(Of(sd), Of(ss)).Should().Be(0x642DB7BD18AFA7CFUL);
    }

    [Fact]
    public void DenseSystemAndSavitzkyGolay_ShouldMatchTheEngineOutputBitForBit()
    {
        // Oraculo: regressao, saida de Sindarin.Math.GeneralMethods em Sindarin 5a0fd38e
        Of(NumericMethods.SolveDenseLinearSystem(RegressionInputs.DenseSpd(), RegressionInputs.DenseRhs())).Should().Be(0x280921B722E3ABCEUL);
        Of(NumericMethods.SavitzkyGolayWeights(3, 2)).Should().Be(0x49F19AE07BC079D8UL);
        Of(NumericMethods.SavitzkyGolayWeights(3, 2, 1, 0.0, 0.5)).Should().Be(0x06FB50DD7AA374AEUL);
        Of(NumericMethods.SavitzkyGolayWeights(4, 3, 0, 0.25, 1.0)).Should().Be(0x67D826967D7B785AUL);
        Of(NumericMethods.SavitzkyGolaySmooth(Y, 7, 2)).Should().Be(0x80EE851312FDEE45UL);
        Of(NumericMethods.SavitzkyGolaySmooth(Y, 9, 3, 1, 0.5)).Should().Be(0x5FEF493FC99C4517UL);
    }

    // ---- Casos limite, com o valor esperado escrito a mao ----

    [Fact]
    public void Interpolation_ShouldReturnY1_WhenXIsX1()
    {
        // Oraculo: caso limite, y1 + (y2 - y1) * 0 / (x2 - x1) = y1 exato
        NumericMethods.Interpolation(1.0, 2.0, 3.0, 5.0, 1.0).Should().Be(2.0);
    }

    [Fact]
    public void Linspace_ShouldReturnBothEnds_WhenCountIsTwo_AndRefuseFewerThanTwoPoints()
    {
        // Oraculo: caso limite, dois pontos em [start, end] inclusivo sao as extremidades
        NumericMethods.Linspace(-3.5, 7.25, 2).Should().Equal(-3.5, 7.25);
        Action one = () => NumericMethods.Linspace(0.0, 1.0, 1);
        one.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData(1, 0)]
    [InlineData(2, 1)]
    [InlineData(3, 1)]
    [InlineData(4, 2)]
    public void MaxWaveletLevels_ShouldBeTheFloorOfLog2(int length, int expected)
    {
        // Oraculo: a mao, floor(log2(length)), o maior L com 2^L <= length
        NumericMethods.MaxWaveletLevels(length).Should().Be(expected);
    }

    [Fact]
    public void Thresholds_ShouldTreatAValueEqualToTheThresholdAsTheirContractSays()
    {
        // Oraculo: caso limite, |v| - t = 0 encolhe a zero no suave; o rigido mantem |v| >= t
        NumericMethods.SoftThreshold(1.2, 1.2).Should().Be(0.0);
        NumericMethods.SoftThreshold(-1.2, 1.2).Should().Be(0.0);
        NumericMethods.HardThreshold(1.2, 1.2).Should().Be(1.2);
        NumericMethods.HardThreshold(-1.2, 1.2).Should().Be(-1.2);
    }

    [Fact]
    public void SoftThreshold_ShouldKeepTheSignOfTheValue_WhenItShrinksAboveTheThreshold()
    {
        // Oraculo: a mao, sign(v)(|v| - t): -3 com t = 1 vai a -2 e 3 vai a 2, exatos em ponto flutuante
        NumericMethods.SoftThreshold(-3.0, 1.0).Should().Be(-2.0);
        NumericMethods.SoftThreshold(3.0, 1.0).Should().Be(2.0);
    }

    [Fact]
    public void FindLocalMaxima_ShouldFindNothing_WhenTheVectorIsConstant()
    {
        // Oraculo: caso limite, sem maximo estrito num vetor constante acima do limiar
        NumericMethods.FindLocalMaxima(Enumerable.Repeat(5.0, 9).ToArray(), 1.0).Should().BeEmpty();
    }

    [Fact]
    public void SolveDenseLinearSystem_ShouldReturnTheRightHandSide_WhenTheMatrixIsTheIdentity()
    {
        var identity = new double[,] { { 1.0, 0.0, 0.0 }, { 0.0, 1.0, 0.0 }, { 0.0, 0.0, 1.0 } };

        // Oraculo: caso limite, I x = b da x = b exato (pivo 1, nada a eliminar)
        NumericMethods.SolveDenseLinearSystem(identity, new[] { 1.5, -2.0, 0.25 }).Should().Equal(1.5, -2.0, 0.25);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(6)]
    [InlineData(7)]
    [InlineData(8)]
    [InlineData(9)]
    [InlineData(10)]
    public void Legendre_ShouldBeOne_AtXEqualToOne(byte n)
    {
        // Oraculo: caso limite, P_n(1) = 1 para todo n (Abramowitz e Stegun 22.4.6); exato com coeficientes inteiros
        NumericMethods.Legendre(n, 1.0).Should().Be(1.0);
    }

    [Fact]
    public void Legendre_ShouldBeNaN_AboveTheLastImplementedDegree()
    {
        // Oraculo: caso limite, o contrato do metodo devolve NaN acima do grau 10
        double.IsNaN(NumericMethods.Legendre(11, 0.5)).Should().BeTrue();
    }

    [Fact]
    public void Median_ShouldTakeTheMiddle_ForOddLength_AndSortTheInput()
    {
        double[] values = { 9.0, -1.0, 4.0, 2.5, 7.0 };

        // Oraculo: a mao, ordenado (-1, 2.5, 4, 7, 9), o do meio e 4; o <summary> diz que a entrada e reordenada
        NumericMethods.Median(values).Should().Be(4.0);
        values.Should().Equal(-1.0, 2.5, 4.0, 7.0, 9.0);
    }

    [Fact]
    public void Median_ShouldAverageTheTwoMiddleValues_ForEvenLength()
    {
        // Oraculo: a mao, ordenado (1, 3, 6, 10), media dos centrais (3 + 6) / 2 = 4.5
        NumericMethods.Median(new[] { 10.0, 1.0, 6.0, 3.0 }).Should().Be(4.5);
    }
}
