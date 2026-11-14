using BaseLibrary.Math;
using FluentAssertions;

namespace BaseLibrary.Tests;

/// <summary>
/// E271: <see cref="GeometryMethods.Determinant3x3"/>, que substitui o <c>ResultsStructureAdapter.Determinant</c> do Nimloth.
/// Casos exatos pela regra de Sarrus e 1000 matrizes sorteadas bit a bit contra a copia antiga.
/// </summary>
public class GeometryMethodsDeterminant3x3Tests
{
    // Oraculo: regressao, copia de Nimloth/src/View Models/Nimloth.ViewModels.Results/Structure/ResultsStructureAdapter.cs:523
    // no commit 8f8efd30
    private static double AdapterDeterminant(double[] r)
        => r[0] * (r[4] * r[8] - r[5] * r[7]) - r[1] * (r[3] * r[8] - r[5] * r[6]) + r[2] * (r[3] * r[7] - r[4] * r[6]);

    // Oraculo: a mao, regra de Sarrus (seis produtos triplos), independente da expansao em cofatores
    private static double Sarrus(double[] r)
        => r[0] * r[4] * r[8] + r[1] * r[5] * r[6] + r[2] * r[3] * r[7] - r[2] * r[4] * r[6] - r[0] * r[5] * r[7] - r[1] * r[3] * r[8];

    private static long Bits(double x) => BitConverter.DoubleToInt64Bits(x);

    [Fact]
    public void Determinant3x3_ShouldGiveTheExactValue_ForIdentityMirrorAndRotation()
    {
        // Oraculo: caso limite, det I = 1, det de um espelho = -1, det de uma rotacao propria = 1 (valores exatos na
        // identidade e no espelho; a rotacao de 120 graus em torno de [111] permuta os eixos e tambem e exata)
        GeometryMethods.Determinant3x3([1, 0, 0, 0, 1, 0, 0, 0, 1]).Should().Be(1);
        GeometryMethods.Determinant3x3([-1, 0, 0, 0, 1, 0, 0, 0, 1]).Should().Be(-1);
        GeometryMethods.Determinant3x3([0, 0, 1, 1, 0, 0, 0, 1, 0]).Should().Be(1);
        GeometryMethods.Determinant3x3([0, 1, 0, 1, 0, 0, 0, 0, 1]).Should().Be(-1);
    }

    [Fact]
    public void Determinant3x3_ShouldMatchSarrus_ForAKnownIntegerMatrix()
    {
        // Oraculo: a mao, Sarrus em [[2, -3, 1], [2, 0, -1], [1, 4, 5]]: 0 + 3 + 8 - 0 - (-8) - (-30) = 49
        double[] m = [2, -3, 1, 2, 0, -1, 1, 4, 5];
        GeometryMethods.Determinant3x3(m).Should().Be(49);
        Sarrus(m).Should().Be(49);
    }

    [Fact]
    public void Determinant3x3_ShouldMatchTheAdapterCopyBitForBit_AndSarrusWithinRounding()
    {
        var random = new Random(271);
        int compared = 0;
        for (int i = 0; i < 1000; i++)
        {
            double[] m = new double[9];
            for (int k = 0; k < 9; k++)
                m[k] = random.NextDouble() * 4 - 2;
            double house = GeometryMethods.Determinant3x3(m);
            Bits(house).Should().Be(Bits(AdapterDeterminant(m)));
            // Folga 1e-12: entradas em [-2, 2], cada termo ate 8 e seis termos; o arredondamento das duas ordens fica em
            // poucos ulp de 8 (~1e-15), e 1e-12 deixa margem de tres ordens.
            house.Should().BeApproximately(Sarrus(m), 1e-12);
            compared++;
        }
        compared.Should().Be(1000);
    }

    [Fact]
    public void Determinant3x3_ShouldReadTheFirstNineEntriesOfALongerSpan()
    {
        // Oraculo: caso limite, a decima entrada nao entra na conta
        double[] m = [1, 0, 0, 0, 2, 0, 0, 0, 3, 99];
        GeometryMethods.Determinant3x3(m).Should().Be(6);
    }
}
