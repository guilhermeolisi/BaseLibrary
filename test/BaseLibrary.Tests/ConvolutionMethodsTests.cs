using FluentAssertions;
using BaseLibrary.Math;

namespace BaseLibrary.Tests;

/// <summary>
/// Valida a convolução discreta normalizada <see cref="ConvolutionMethods.NormalizedConvolution"/>
/// (extraída dos perfis FPA TubeTails/TubeTailsExp/LPSDContinuousScan/LPSDDefocusing/AxialDivergenceFull).
/// Trava a convenção de centro (ponto médio da grade), o recorte de borda (sem wrap), a normalização pela
/// soma do kernel (área preservada no interior) e o caminho de kernel degenerado (identidade).
/// </summary>
public class ConvolutionMethodsTests
{
    [Fact]
    public void NormalizedConvolution_ShouldReturnIdentity_WhenKernelIsCenteredDelta_OddLength()
    {
        // len=5 (ímpar) ⇒ centro = (5-1)/2 = 2. Delta unitário no centro ⇒ saída = sinal.
        double[] signal = [10, 20, 30, 40, 50];
        double[] kernel = [0, 0, 1, 0, 0];
        double[] output = new double[signal.Length];

        ConvolutionMethods.NormalizedConvolution(kernel, signal, output);

        output.Should().Equal(signal);
    }

    [Fact]
    public void NormalizedConvolution_ShouldReturnIdentity_WhenKernelIsCenteredDelta_EvenLength()
    {
        // len=4 (par) ⇒ centro = 4/2-1 = 1. Delta unitário no centro ⇒ saída = sinal.
        double[] signal = [10, 20, 30, 40];
        double[] kernel = [0, 1, 0, 0];
        double[] output = new double[signal.Length];

        ConvolutionMethods.NormalizedConvolution(kernel, signal, output);

        output.Should().Equal(signal);
    }

    [Fact]
    public void NormalizedConvolution_ShouldMatchHandComputedMovingAverage_WithBoundaryClipping()
    {
        // len=5 ⇒ centro=2; kernel de 3 uns em k=1..3 (soma=3). Para cada i, taps em src = i+1, i, i-1.
        // Bordas: taps fora de [0,5) são recortados, mas a normalização usa a soma CHEIA do kernel (3),
        // logo as bordas são atenuadas (área não conservada nas extremidades) — comportamento esperado.
        double[] signal = [10, 20, 30, 40, 50];
        double[] kernel = [0, 1, 1, 1, 0];
        double[] output = new double[signal.Length];

        ConvolutionMethods.NormalizedConvolution(kernel, signal, output);

        // i=0: (20+10)/3=10; i=1: (30+20+10)/3=20; i=2: (40+30+20)/3=30; i=3: (50+40+30)/3=40; i=4: (50+40)/3=30.
        output.Should().Equal(10, 20, 30, 40, 30);
    }

    [Fact]
    public void NormalizedConvolution_ShouldPreserveConstantSignal_InTheInterior()
    {
        // Sinal constante: no interior (todos os taps dentro da grade) a média ponderada normalizada devolve
        // a mesma constante; só as bordas são atenuadas pelo recorte.
        double[] signal = [5, 5, 5, 5, 5, 5, 5];
        double[] kernel = [0, 0, 2, 3, 2, 0, 0]; // centro=3 (len=7 ímpar), pesos assimétricos, soma=7
        double[] output = new double[signal.Length];

        ConvolutionMethods.NormalizedConvolution(kernel, signal, output);

        for (int i = 2; i <= 4; i++) // interior (taps ±1 dentro da grade)
            output[i].Should().BeApproximately(5.0, 1e-12);
    }

    [Fact]
    public void NormalizedConvolution_ShouldCopySignalUnchanged_WhenKernelIsAllZeros()
    {
        double[] signal = [1, 2, 3, 4];
        double[] kernel = [0, 0, 0, 0];
        double[] output = new double[signal.Length];

        ConvolutionMethods.NormalizedConvolution(kernel, signal, output);

        output.Should().Equal(signal); // suporte vazio ⇒ identidade
    }

    [Fact]
    public void NormalizedConvolution_ShouldCopySignalUnchanged_WhenKernelSumIsNonPositive()
    {
        double[] signal = [1, 2, 3, 4];
        double[] kernel = [0, -1, 0, 0]; // suporte não-vazio, mas soma = -1 ≤ 0 ⇒ identidade
        double[] output = new double[signal.Length];

        ConvolutionMethods.NormalizedConvolution(kernel, signal, output);

        output.Should().Equal(signal);
    }
}
