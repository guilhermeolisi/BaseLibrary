using System.Numerics;
using static System.Math;

namespace BaseLibrary.Math;

/// <summary>
/// Famílias de wavelets ortogonais suportadas pela Transformada Wavelet Discreta (DWT).
/// </summary>
public enum WaveletFamily
{
    /// <summary>Haar (equivalente a Daubechies de ordem 1).</summary>
    Haar,
    /// <summary>Daubechies (dbN, comprimento do filtro = 2N).</summary>
    Daubechies,
    /// <summary>Symlets (symN, comprimento do filtro = 2N), quase simétricas.</summary>
    Symlet,
    /// <summary>Coiflets (coifN, comprimento do filtro = 6N).</summary>
    Coiflet
}

/// <summary>
/// Modo de limiarização (thresholding) dos coeficientes de detalhe no denoising por wavelet.
/// </summary>
public enum ThresholdMode
{
    /// <summary>Limiar suave (Donoho): encolhe em direção a zero. Produz reconstruções mais suaves.</summary>
    Soft,
    /// <summary>Limiar rígido: mantém o coeficiente ou zera. Preserva melhor as amplitudes dos picos.</summary>
    Hard
}

public static class NumericMethods
{

    public static bool ToInitialize;
    private static readonly double DoubleZero = 1E-12;

    // Randon/RandonGaussian (e a instancia unica de Random) movidos para BaseLibrary.Math.RandomMethods
    // na Onda 2.5 (auto-contencao do Sindarin.Math.Nodes). Consumidores: nos Randon/RandonGauss e DataXYE.
    /// <summary>
    /// Performs linear interpolation between two points.
    /// </summary>
    /// <param name="x1">The x-coordinate of the first point.</param>
    /// <param name="y1">The y-coordinate of the first point.</param>
    /// <param name="x2">The x-coordinate of the second point.</param>
    /// <param name="y2">The y-coordinate of the second point.</param>
    /// <param name="x">The x-coordinate at which to interpolate.</param>
    /// <returns>The interpolated y-coordinate.</returns>
    public static double Interpolation(in double x1, in double y1, in double x2, in double y2, in double x) => y1 + (y2 - y1) * (x - x1) / (x2 - x1);
    /// <summary>
    /// Interpolação log-log (lei de potência) entre dois pontos. Apropriada para grandezas estritamente
    /// positivas que variam por ordens de grandeza entre os nós e se comportam como leis de potência —
    /// ex.: f'' anômalo de Henke e o coeficiente de atenuação mássico (μ/ρ) do NIST entre bordas de absorção.
    /// Faz fallback para <see cref="Interpolation(in double, in double, in double, in double, in double)"/>
    /// quando algum valor não for positivo/finito (onde o logaritmo não é definido) ou x1 == x2.
    /// </summary>
    /// <param name="x1">The x-coordinate of the first point.</param>
    /// <param name="y1">The y-coordinate of the first point.</param>
    /// <param name="x2">The x-coordinate of the second point.</param>
    /// <param name="y2">The y-coordinate of the second point.</param>
    /// <param name="x">The x-coordinate at which to interpolate.</param>
    /// <returns>The interpolated y-coordinate.</returns>
    public static double InterpolationLogLog(in double x1, in double y1, in double x2, in double y2, in double x)
    {
        if (x1 > 0 && x2 > 0 && x > 0 && y1 > 0 && y2 > 0)
        {
            double lx1 = System.Math.Log(x1);
            double lx2 = System.Math.Log(x2);
            if (lx1 != lx2)
                return System.Math.Exp(System.Math.Log(y1) + (System.Math.Log(y2) - System.Math.Log(y1)) * (System.Math.Log(x) - lx1) / (lx2 - lx1));
        }
        return Interpolation(x1, y1, x2, y2, x);
    }
    /// <summary>
    /// Performs interpolation on a complex number array.
    /// </summary>
    /// <param name="xs">The x-coordinates of the data points.</param>
    /// <param name="ys">The y-coordinates of the data points.</param>
    /// <param name="x">The x-coordinate at which to interpolate.</param>
    /// <returns>The interpolated y-coordinate.</returns>
    public static double Interpolation(in double[] xs, in Complex[] ys, in int index, in double x) => ys[index].Real + (ys[index + 1].Real - ys[index].Real) * (x - xs[index]) / (xs[index + 1] - xs[index]);

    /// <summary>
    /// Calculates the greatest common divisor of a set of integers.
    /// </summary>
    /// <param name="integers">The integers.</param>
    /// <returns>The greatest common divisor.</returns>
    private static uint GreatestCommomDivisor(params uint[] integers)
    {
        if (integers.Length == 0)
            return 0;
        if (integers.Length == 1)
            return integers[0];
        if (integers.Length == 2)
        {
            //https://stackoverflow.com/questions/18541832/c-sharp-find-the-greatest-common-divisor
            while (integers[0] != 0 && integers[1] != 0)
            {
                if (integers[0] > integers[1])
                    integers[0] %= integers[1];
                else
                    integers[1] %= integers[0];
            }

            return integers[0] | integers[1];
        }
        uint result = GreatestCommomDivisor(integers[0], integers[1]);

        for (int i = 2; i < integers.Length; i++)
        {
            result = GreatestCommomDivisor(result, integers[i]);
        }
        return result;
    }
    /// <summary>
    /// Copy the values of the array from the start to the end to another array
    /// </summary>
    /// <param name="initial"></param>
    /// <param name="final"></param>
    /// <param name="start"></param>
    /// <param name="length"></param>
    /// <exception cref="Exception"></exception>
    public static void CopyArray(double[] initial, double[] final, int start, int length)
    {
        // Guard com diagnóstico: o throw genérico anterior (new Exception()) chegava à
        // telemetria como "Exception of type 'System.Exception' was thrown", sem indicar
        // qual pré-condição falhou. A mensagem agora identifica a causa e os tamanhos.
        // Também cobrimos o lado da leitura (start + length > initial.Length), que antes
        // estourava IndexOutOfRange dentro do laço. O caminho válido é inalterado e a
        // construção da string só ocorre no erro (sem impacto de performance no sucesso).
        if (initial is null || final is null || start < 0 || length < 0
            || length > final.Length || start + length > initial.Length)
            throw new ArgumentException(
                $"CopyArray argumentos inválidos: initial={(initial is null ? "null" : initial.Length.ToString())}, " +
                $"final={(final is null ? "null" : final.Length.ToString())}, start={start}, length={length}.");
        for (int i = 0; i < length; i++)
            final[i] = initial[start + i];
    }
    public static double[] ToDouble(this int[] ints)
    {
        double[] result = new double[ints.Length];
        for (int i = 0; i < ints.Length; i++)
            result[i] = ints[i];
        return result;
    }
    /// <summary>
    /// Calculates the Legendre polynomial of degree n at x.
    /// </summary>
    /// <param name="n">The degree of the polynomial.</param>
    /// <param name="x">The value at which to evaluate the polynomial.</param>
    /// <returns>The value of the polynomial at x.</returns>
    public static double Legendre(byte n, double x)
    {
        //https://en.wikipedia.org/wiki/Legendre_polynomials
        //https://mathworld.wolfram.com/LegendrePolynomial.html

        switch (n)
        {
            case 0:
                return 1.0;
            case 1:
                return x;
            case 2:
                return (3 * Pow(x, 2.0) - 1) / 2.0;
            case 3:
                return (5 * Pow(x, 3.0) - 3 * x) / 2.0;
            case 4:
                return (35 * Pow(x, 4.0) - 30 * Pow(x, 2.0) + 3.0) / 8.0;
            case 5:
                return (63 * Pow(x, 5.0) - 70 * Pow(x, 3.0) + 15 * x) / 8.0;
            case 6:
                return (231 * Pow(x, 6.0) - 315 * Pow(x, 4.0) + 105 * Pow(x, 2.0) - 5) / 16.0;
            case 7:
                return (429 * Pow(x, 7.0) - 693 * Pow(x, 5.0) + 315 * Pow(x, 3.0) - 35 * x) / 16.0;
            case 8:
                return (6435 * Pow(x, 8.0) - 12012 * Pow(x, 6.0) + 6930 * Pow(x, 4.0) - 1260 * Pow(x, 2.0) + 35) / 128.0;
            case 9:
                return (12155 * Pow(x, 9.0) - 25740 * Pow(x, 7.0) + 18018 * Pow(x, 5.0) - 4620 * Pow(x, 3.0) + 315 * x) / 128.0;
            case 10:
                return (46189 * Pow(x, 10.0) - 109395 * Pow(x, 8.0) + 90090 * Pow(x, 6.0) - 30030 * Pow(x, 4.0) + 3465 * Pow(x, 2.0) - 63) / 256.0;
            default:
                //TODO aqui pode-se usar a equação genérica para calcular segundo as referencias. Mas acho que não vamos usar ordem superiores a 8
                return double.NaN;
        }
    }
    #region getIndexX
    /// <summary>
    /// returns the index in the series that corresponds to the exact value or the immediately lower value in the series
    /// </summary>
    /// <param name="xs">The array of x series</param>
    /// <param name="x">The value of x to be found</param>
    /// <param name="Steps">The steps of each x array, if is x is regular</param>
    /// <returns>The correspondent index</returns>
    public static int[] GetIndexX(in double[][] xs, in double x, double[] Steps)
    {
        int[] result = new int[2];
        //para achar em que arquivo está o x
        for (int i = 0; i < xs.Length; i++)
            if (xs[i][0] < x)
                result[0] = i;
        //Encontra o IndexXCenter[1]
        //Se o step for constante é bem rápido de encontrar o indice
        if (Steps is not null && !double.IsNaN(Steps[result[0]]) && Steps[result[0]] != 0)
        {
            int indexTemp = (int)((xs[result[0]][(int)(((x - xs[result[0]][0]) / Steps[result[0]]) < (xs[result[0]].Length - 1) ? ((x - xs[result[0]][0]) / Steps[result[0]]) > 0 ? ((x - xs[result[0]][0]) / Steps[result[0]]) : 0 : (xs[result[0]].Length - 1))] - xs[result[0]][0]) / Steps[result[0]]);
            if (xs[result[0]][indexTemp] < x)
                while (indexTemp + 1 < xs[result[0]].Length && xs[result[0]][indexTemp + 1] < x)
                    indexTemp++;
            else
                while (indexTemp - 1 >= 0 && xs[result[0]][indexTemp] > x)
                    indexTemp--;
            result[1] = indexTemp;
        }
        //Se o step não for constante tem que passar por cada etapa para achar o indice
        else
        {
            int indexX1 = 0;
            int indexX2 = xs[result[0]].Length - 1;
            int index1Temp = indexX1;
            int index2Temp = indexX2;
            do
            {
                if (xs[result[0]][(indexX1 + indexX2) / 2] < x)
                {
                    index1Temp = indexX1;
                    indexX1 = (indexX1 + indexX2) / 2;
                }
                else
                {
                    index2Temp = indexX2;
                    indexX2 = (indexX1 + indexX2) / 2;
                }
            } while (indexX1 != index1Temp || indexX2 != index2Temp && indexX2 - indexX1 > 1);
            for (int i = indexX1 + 1; i <= indexX2; i++)
                if (xs[result[0]][i - 1] < x && xs[result[0]][i] > x)
                    indexX1 = i - 1;
            result[1] = indexX1;
        }
        if (result[1] + 1 < xs[result[0]].Length)
            if (Abs(xs[result[0]][result[1] + 1] - x) < DoubleZero)
                result[1]++;
        return result;
    }
    /// <summary>
    /// returns the index in the series that corresponds to the exact value or the immediately lower value in the series
    /// </summary>
    /// <param name="xs">The array of x series</param>
    /// <param name="x">The value of x to be found</param>
    /// <param name="IndexXs">The index of xs that must be search</param>
    /// <param name="Steps">The steps of each x array, if is x is regular</param>
    /// <returns>The correspondent index</returns>
    public static int GetIndexX(double[][] xs, double x, int IndexXs, double[]? Steps)
    {
        int result;
        //Se o step for constante é bem ráido de encontrar o indice
        if (Steps is not null && !double.IsNaN(Steps[IndexXs]))
        {
            result = (int)((x - xs[IndexXs][0]) / Steps[IndexXs]);
            if (result < 0)
                result = 0;
            if (result >= xs[IndexXs].Length)
                result = xs[IndexXs].Length - 1;
            if (xs[IndexXs][result] < x)
                while (result + 1 < xs[IndexXs].Length && xs[IndexXs][result + 1] < x)
                    result++;
            else
                while (result - 1 >= 0 && xs[IndexXs][result] > x)
                    result--;
        }
        //Se o step não for constante tem que passar por cada etapa para achar o indice
        else
        {
            int indexX1 = 0;
            int indexX2 = xs[IndexXs].Length - 1;
            int index1Temp = indexX1;
            int index2Temp = indexX2;
            do
            {
                if (xs[IndexXs][(indexX1 + indexX2) / 2] < x)
                {
                    index1Temp = indexX1;
                    indexX1 = (indexX1 + indexX2) / 2;
                }
                else
                {
                    index2Temp = indexX2;
                    indexX2 = (indexX1 + indexX2) / 2;
                }
            } while (indexX1 != index1Temp || indexX2 != index2Temp && indexX2 - indexX1 > 1);
            for (int i = indexX1 + 1; i <= indexX2; i++)
                if (xs[IndexXs][i - 1] < x && xs[IndexXs][i] > x)
                    indexX1 = i - 1;
            result = indexX1;
        }
        if (result + 1 < xs[IndexXs].Length)
            if (Abs(xs[IndexXs][result + 1] - x) < DoubleZero)
                result++;
        return result;
    }
    /// <summary>
    /// returns the index in the series that corresponds to the exact value or the immediately lower value in the series
    /// </summary>
    /// <param name="xs">The array of x series</param>
    /// <param name="x">The value of x to be found</param>
    /// <returns>The correspondent index</returns>
    public static int GetIndexX(in double[] xs, in double x)
    {
        int result;
        int indexX1 = 0;
        int indexX2 = xs.Length - 1;
        int index1Temp = indexX1;
        int index2Temp = indexX2;
        do
        {
            if (xs[(indexX1 + indexX2) / 2] < x)
            {
                index1Temp = indexX1;
                indexX1 = (indexX1 + indexX2) / 2;
            }
            else
            {
                index2Temp = indexX2;
                indexX2 = (indexX1 + indexX2) / 2;
            }
        } while (indexX1 != index1Temp || indexX2 != index2Temp && indexX2 - indexX1 > 1);
        for (int i = indexX1 + 1; i <= indexX2; i++)
            if (xs[i - 1] < x && xs[i] > x)
                indexX1 = i - 1;
        result = indexX1;
        if (result + 1 < xs.Length)
            if (Abs(xs[result + 1] - x) < DoubleZero)
                result++;
        return result;
    }
    /// <summary>
    /// returns the index in the series that corresponds to the exact value or the immediately lower value in the series, considering the index closeTo
    /// </summary>
    /// <param name="xs">The array of x series</param>
    /// <param name="x">The value of x to be found</param>
    /// <returns></returns>
    public static void GetIndexX(in double[] xs, in double x, ref int closeTo)
    {
        if (xs[closeTo] > x)
        {
            while (xs[closeTo] > x && closeTo > 0)
                closeTo--;
        }
        else if (xs[closeTo] < x)
        {
            while (closeTo < xs.Length - 1 && xs[closeTo] < x)
                closeTo++;
            if (xs[closeTo] > x)
                closeTo--;
        }
        //if (closeTo + 1 < xs.Length)
        //    if (Abs(xs[closeTo + 1] - x) < Constants.DoubleZero)
        //        closeTo++;
    }

    #endregion
    /// <summary>
    /// Calculates the wavelet coefficient of a signal.
    /// </summary>
    /// <param name="y">The signal values.</param>
    /// <param name="wavelet">The wavelet values.</param>
    /// <param name="centralIndex">The central index of the wavelet.</param>
    /// <param name="step">The step size for the wavelet calculation.</param>
    /// <returns>The wavelet coefficient of the signal.</returns>
    public static double WaveletCoefficient(double[] y, double[] wavelet, int centralIndex, double step)
    {
        double result = 0;
        //Correção: para alinhar o centro da wavelet (índice wavelet.Length/2) com centralIndex,
        //o primeiro ponto é centralIndex - wavelet.Length/2. O "(wavelet.Length + 1) / 2" anterior
        //introduzia um deslocamento de uma amostra.
        int firstY = centralIndex - wavelet.Length / 2;
        for (int i = 0; i < wavelet.Length - 1; i++)
        {
            //Correção: yInd precisa deslizar junto com a wavelet (yInd = firstY + i).
            //Antes, yInd ficava fixo em firstY, integrando sempre o mesmo par de amostras de y.
            int yInd = firstY + i;
            //O produto interno ⟨y, ψ⟩ é aproximado integrando o produto de y por ψ em cada subintervalo.
            //Nas bordas, repete-se a primeira/última amostra de y (extensão constante).
            result += NumericIntegrationForWavelet(
                yInd < 0 ? y[0] : yInd >= y.Length ? y[y.Length - 1] : y[yInd],
                yInd + 1 < 0 ? y[0] : yInd + 1 >= y.Length ? y[y.Length - 1] : y[yInd + 1],
                wavelet[i], wavelet[i + 1], step, 3);
        }

        return result;
    }
    /// <summary>
    /// Generates a Ricker (or Mexican hat) wavelet.
    /// </summary>
    /// <param name="a">The parameter 'a' controls the width of the wavelet.</param>
    /// <param name="b">The parameter 'b' controls the position of the wavelet.</param>
    /// <param name="step">The step size for the wavelet calculation.</param>
    /// <returns>An array representing the Ricker wavelet.</returns>

    public static double[] RickerWavelet(double a, double b, double step)
    {
        ///Mexican Hat
        /// (1 − x^2) exp[−x^2/2]
        ///GregoireVanDover 2011
        /// g(a) a^-1/2 (1 − ((x - b) / a)^2) exp[−((x - b) / a)^2 / 2]
        /// sendo g(a) = a^-1
        /// a largura total no y = 0 no meio é 2a

        //fazer [-5a , 5a]

        double max = 5 * a;
        double count = (2 * max / step);
        int length = (int)count;
        if (count > length)
            length++;

        //se for par acrescentar 
        if (length % 2.0 == 0)
            length++;

        double[] result = new double[length];

        //Correção: o centro de um vetor simétrico de tamanho ímpar é (length-1)/2.
        //O "+1" anterior deslocava o pico e tornava a wavelet assimétrica (mais pontos à esquerda que à direita).
        int middle = (length - 1) / 2;

        for (int i = 0; i < length; i++)
        {
            result[i] = CalculeRickerWaveletValue(a, b, b + (i - middle) * step);
        }

        return result;
    }
    /// <summary>
    /// Calculates the value of a Ricker (or Mexican hat) wavelet at a specific point.
    /// </summary>
    /// <param name="a">The parameter 'a' controls the width of the wavelet.</param>
    /// <param name="b">The parameter 'b' controls the position of the wavelet.</param>
    /// <param name="x">The point at which to evaluate the wavelet.</param>
    /// <returns>The value of the Ricker wavelet at the point x.</ret
    public static double CalculeRickerWaveletValue(double a, double b, double x)
    {
        /// g(a) a^-1/2 (1 − ((x - b) / a)^2) exp[−((x - b) / a)^2 / 2]
        //Correção: -3 / 2 em C# é divisão inteira (= -1), o que dava a normalização errada a^-1.
        //O expoente correto, conforme a documentação acima (g(a)=a^-1 vezes a^-1/2), é a^-3/2.
        return Pow(a, -1.5) * (1 - Pow((x - b) / a, 2)) * Exp(-Pow((x - b) / a, 2) / 2.0);
    }
    /// <summary>
    /// Performs numeric integration for wavelet using the trapezoidal rule.
    /// </summary>
    /// <param name="y1a">The start value of the first interval.</param>
    /// <param name="y1b">The end value of the first interval.</param>
    /// <param name="y2a">The start value of the second interval.</param>
    /// <param name="y2b">The end value of the second interval.</param>
    /// <param name="step">The step size for the numeric integration.</param>
    /// <param name="n">The number of steps for the numeric integration.</param>
    /// <returns>The result of the numeric integration.</returns>
    private static double NumericIntegrationForWavelet(double y1a, double y1b, double y2a, double y2b, double step, int n)
    {
        double result = 0;

        double stepMin = step / (n + 1);

        //https://en.wikipedia.org/wiki/Trapezoidal_rule
        for (int i = 0; i <= n; i++)
        {
            result += stepMin * (Interpolation(0, y1a, step, y1b, i * stepMin) * Interpolation(0, y2a, step, y2b, i * stepMin) + Interpolation(0, y1a, step, y1b, (i + 1) * stepMin) * Interpolation(0, y2a, step, y2b, (i + 1) * stepMin)) / 2;
        }

        return result;
    }

    #region Transformada wavelet contínua LoG (Ricker) e detecção de picos (Gregoire, Dale & van Dover, 2011)

    // (3/4)·√π — fator da energia da wavelet-mãe LoG contínua (ver RickerWaveletContinuousEnergy).
    private const double LoGEnergyConstant = 0.75 * 1.7724538509055159;

    /// <summary>
    /// Energia da wavelet-mãe LoG (Ricker / Mexican hat) contínua na escala <paramref name="a"/>, com o peso
    /// g(a) = 1/a empregado por Gregoire, Dale &amp; van Dover (Rev. Sci. Instrum. 82, 015105, 2011, Eq. 4):
    /// ∫ wₐ,b(x)² dx = (3√π/4)·g(a)² = (3√π/4)/a². Usada na correção da wavelet discreta (Apêndice A2).
    /// </summary>
    public static double RickerWaveletContinuousEnergy(double a) => LoGEnergyConstant / (a * a);

    /// <summary>
    /// Gera um conjunto de escalas log-espaçado e estritamente decrescente, de <paramref name="aMax"/> até
    /// <paramref name="aMin"/>, com razão <paramref name="ratio"/> entre escalas consecutivas (aᵢ = aᵢ₋₁/ratio).
    /// É a amostragem recomendada por Gregoire et al. (2011, §III A) — eles usam log₁.₁₈ (~4,2 ciclos por
    /// oitava) — que evita o sobreamostramento das feições de grande escala do espaçamento linear.
    /// </summary>
    /// <param name="aMax">Maior escala (primeira do conjunto).</param>
    /// <param name="aMin">Menor escala (limite inferior; a última escala é ≥ aMin).</param>
    /// <param name="ratio">Razão entre escalas consecutivas (&gt; 1).</param>
    /// <returns>Escalas em ordem decrescente, conforme exigido pelo algoritmo de cumeeiras.</returns>
    public static double[] LogSpacedDescending(double aMax, double aMin, double ratio)
    {
        if (ratio <= 1.0)
            throw new ArgumentOutOfRangeException(nameof(ratio), "A razão entre escalas deve ser > 1.");
        if (aMin <= 0.0)
            throw new ArgumentOutOfRangeException(nameof(aMin), "As escalas devem ser positivas.");
        if (aMax < aMin)
            (aMax, aMin) = (aMin, aMax);

        var scales = new List<double>();
        for (double a = aMax; a >= aMin * (1.0 - 1e-9); a /= ratio)
            scales.Add(a);
        if (scales.Count == 0)
            scales.Add(aMax);
        return scales.ToArray();
    }

    /// <summary>
    /// Índices dos máximos locais estritos de <paramref name="values"/> cujo valor excede
    /// <paramref name="threshold"/>. Entradas NaN (p.ex. regiões de borda inválidas da transformada wavelet)
    /// são ignoradas e tratadas como −∞, de modo que nunca formam nem interrompem indevidamente um máximo.
    /// </summary>
    public static int[] FindLocalMaxima(double[] values, double threshold)
    {
        var maxima = new List<int>();
        int n = values.Length;
        for (int i = 0; i < n; i++)
        {
            double v = values[i];
            if (double.IsNaN(v) || v <= threshold)
                continue;
            double left = i > 0 ? values[i - 1] : double.NegativeInfinity;
            double right = i < n - 1 ? values[i + 1] : double.NegativeInfinity;
            if (double.IsNaN(left))
                left = double.NegativeInfinity;
            if (double.IsNaN(right))
                right = double.NegativeInfinity;
            if (v > left && v > right)
                maxima.Add(i);
        }
        return maxima.ToArray();
    }

    /// <summary>
    /// Transformada wavelet LoG (Ricker / Mexican hat) de um sinal 1D amostrado em passo regular, calculada
    /// sobre um conjunto de escalas, conforme Gregoire, Dale &amp; van Dover (Rev. Sci. Instrum. 82, 015105,
    /// 2011). A transformada T(a, b) = ∫ wₐ,b(x)·f(x) dx é a convolução discreta do sinal com a wavelet-mãe da
    /// Eq. (4) (peso g(a)=1/a), reaproveitando <see cref="CalculeRickerWaveletValue"/> para os valores da
    /// wavelet.
    /// </summary>
    /// <remarks>
    /// Aplica a correção da wavelet discreta do Apêndice (Eqs. A1-A2): em cada posição a wavelet amostrada é
    /// estirada de forma assimétrica por fatores α (lóbulo positivo) e β (lóbulos negativos), escolhidos para
    /// que ela tenha momento de ordem zero nulo (Σ w·δx = 0 — garante a subtração de fundo constante/linear) e
    /// energia igual à da wavelet contínua (Σ w²·δx = ∫ w²dx). Quando a energia bruta amostrada do suporte
    /// desvia mais que <paramref name="energyTolerance"/> da energia contínua — o que ocorre quando a escala é
    /// pequena demais frente ao passo (a ≲ 10·δx) ou quando o suporte é truncado pela borda dos dados
    /// (b a menos de ~5a do extremo) — a grade é considerada insuficiente e T recebe NaN naquele ponto.
    /// </remarks>
    /// <param name="y">Sinal (intensidades), amostrado em passo regular <paramref name="step"/>.</param>
    /// <param name="scales">Conjunto de escalas a (tipicamente decrescente; ver <see cref="LogSpacedDescending"/>).</param>
    /// <param name="step">Espaçamento δx entre as amostras do sinal (&gt; 0).</param>
    /// <param name="supportFactor">Meio-suporte da wavelet em unidades de a (nominalmente 5 ⇒ [−5a, 5a]).</param>
    /// <param name="energyTolerance">Desvio relativo de energia tolerado antes de invalidar o ponto (0,12 = 12%).</param>
    /// <returns>Matriz T[escala][posição]; entradas inválidas (borda/subamostragem) são NaN.</returns>
    public static double[][] RickerWaveletTransform(double[] y, double[] scales, double step,
        double supportFactor = 5.0, double energyTolerance = 0.12)
    {
        ArgumentNullException.ThrowIfNull(y);
        ArgumentNullException.ThrowIfNull(scales);
        if (step <= 0.0)
            throw new ArgumentOutOfRangeException(nameof(step), "O passo δx deve ser positivo.");

        int n = y.Length;
        int ns = scales.Length;
        double[][] transform = new double[ns][];

        for (int s = 0; s < ns; s++)
        {
            double a = scales[s];
            double[] row = new double[n];
            transform[s] = row;

            int half = (int)Ceiling(supportFactor * a / step);
            if (half < 1)
                half = 1;
            int len = 2 * half + 1;

            // Valores brutos da wavelet-mãe centrada em 0 (Eq. 4), reaproveitando CalculeRickerWaveletValue.
            double[] w = new double[len];
            for (int k = -half; k <= half; k++)
                w[k + half] = CalculeRickerWaveletValue(a, 0.0, k * step);

            double eCont = RickerWaveletContinuousEnergy(a);

            // Correção para o interior (suporte completo): α/β e validade pela energia (Apêndice A1/A2).
            ComputeRickerAlphaBeta(w, 0, len - 1, step, eCont, out double alphaIn, out double betaIn,
                out double rawEnergyIn);
            bool interiorValid = Abs(rawEnergyIn - eCont) <= energyTolerance * eCont;
            double[] modIn = new double[len];
            for (int k = 0; k < len; k++)
                modIn[k] = (w[k] >= 0.0 ? alphaIn : betaIn) * w[k];

            for (int b = 0; b < n; b++)
            {
                int kLo = b - half < 0 ? -b : -half;
                int kHi = b + half > n - 1 ? n - 1 - b : half;

                if (kLo == -half && kHi == half)
                {
                    // Posição interior: usa o núcleo modificado pré-calculado (multiplicação e soma de arrays).
                    if (!interiorValid)
                    {
                        row[b] = double.NaN;
                        continue;
                    }
                    double sum = 0.0;
                    for (int k = 0; k < len; k++)
                        sum += modIn[k] * y[b + k - half];
                    row[b] = sum * step;
                }
                else
                {
                    // Borda: suporte truncado → recalcula α/β para o trecho válido e revalida a energia.
                    ComputeRickerAlphaBeta(w, kLo + half, kHi + half, step, eCont, out double alpha,
                        out double beta, out double rawEnergy);
                    if (Abs(rawEnergy - eCont) > energyTolerance * eCont)
                    {
                        row[b] = double.NaN;
                        continue;
                    }
                    double sum = 0.0;
                    for (int k = kLo; k <= kHi; k++)
                    {
                        double wk = w[k + half];
                        sum += (wk >= 0.0 ? alpha : beta) * wk * y[b + k];
                    }
                    row[b] = sum * step;
                }
            }
        }
        return transform;
    }

    /// <summary>
    /// Resolve os fatores de estiramento assimétrico da wavelet LoG amostrada (Gregoire et al., 2011,
    /// Eqs. A1-A2): α (lóbulo positivo) e β (lóbulos negativos) tais que Σ w·δx = 0 (momento de ordem zero
    /// nulo) e Σ w²·δx = <paramref name="eCont"/> (energia igual à da wavelet contínua). Também devolve a
    /// energia bruta (sem correção) do suporte, para o teste de validade da grade.
    /// </summary>
    private static void ComputeRickerAlphaBeta(double[] w, int lo, int hi, double step, double eCont,
        out double alpha, out double beta, out double rawEnergy)
    {
        double sumPos = 0.0, sumNeg = 0.0, energyPos = 0.0, energyNeg = 0.0;
        for (int k = lo; k <= hi; k++)
        {
            double wk = w[k];
            if (wk >= 0.0)
            {
                sumPos += wk;
                energyPos += wk * wk;
            }
            else
            {
                sumNeg += wk;
                energyNeg += wk * wk;
            }
        }
        energyPos *= step;
        energyNeg *= step;
        rawEnergy = energyPos + energyNeg;

        // Suporte degenerado (sem lóbulo negativo amostrado): não reescalona.
        if (sumNeg == 0.0 || (energyPos == 0.0 && energyNeg == 0.0))
        {
            alpha = 1.0;
            beta = 1.0;
            return;
        }

        // Momento nulo ⇒ β = −α·sumPos/sumNeg; energia ⇒ α²·Eₚ + β²·Eₙ = eCont.
        double ratio = sumPos / sumNeg; // < 0, pois sumNeg < 0
        double denom = energyPos + ratio * ratio * energyNeg;
        alpha = denom > 0.0 ? Sqrt(eCont / denom) : 1.0;
        beta = -alpha * ratio; // > 0
    }

    #endregion

    #region Discrete Wavelet Transform (DWT) e denoising

    // Constante da estimativa MAD de ruído: 0.6745 ≈ Φ^-1(0.75), relaciona a MAD ao desvio-padrão de uma gaussiana.
    private const double MadToSigma = 0.6745;

    // ---------------------------------------------------------------------------------------------
    // Filtros de decomposição passa-baixa (scaling / dec_lo) das wavelets ortonormais.
    // Coeficientes conforme Daubechies / PyWavelets. O filtro passa-alta é obtido por QMF
    // (ver WaveletDetailFilter). Para wavelets ortonormais, vale: sum(h) = sqrt(2) e sum(h^2) = 1.
    // ---------------------------------------------------------------------------------------------
    private static readonly double[] _haar = { 0.70710678118654757, 0.70710678118654757 };
    private static readonly double[] _db2 = { -0.12940952255126037, 0.22414386804201339, 0.83651630373780794, 0.48296291314453416 };
    private static readonly double[] _db3 = { 0.035226291885709533, -0.085441273882026658, -0.13501102001025458, 0.45987750211849154, 0.80689150931109255, 0.33267055295008263 };
    private static readonly double[] _db4 = { -0.010597401785069032, 0.032883011666885197, 0.030841381835560764, -0.18703481171909309, -0.027983769416859854, 0.63088076792985892, 0.71484657055291567, 0.23037781330889651 };
    private static readonly double[] _db5 = { 0.0033357252854737712, -0.012580751999081999, -0.0062414902127982744, 0.077571493840045719, -0.032244869584638375, -0.24229488706638203, 0.13842814590132074, 0.72430852843777294, 0.60382926979718965, 0.16010239797419293 };
    private static readonly double[] _db6 = { -0.0010773010853084796, 0.0047772575109455108, 0.00055384220116149613, -0.03158203931748603, 0.027522865530305727, 0.097501605587323043, -0.12976686756726194, -0.22626469396543983, 0.31525035170919763, 0.75113390802109536, 0.49462389039845306, 0.11154074335010947 };
    private static readonly double[] _db7 = { 0.00035371379997452024, -0.0018016407040474908, 0.00042957797292136651, 0.01255099855609984, -0.016574541630666881, -0.038029936935014413, 0.080612609151083078, 0.071309219266830259, -0.22403618499387498, -0.14390600392856498, 0.46978228740519312, 0.72913209084623509, 0.39653931948191729, 0.077852054085009184 };
    private static readonly double[] _db8 = { -0.00011747678412476953, 0.00067544940645056933, -0.00039174037337694705, -0.0048703529934515741, 0.0087460940474057766, 0.013981027917398282, -0.044088253930794755, -0.017369301001807547, 0.12874742662047847, 0.00047248457391328279, -0.28401554296154691, -0.015829105256349306, 0.58535468365420673, 0.67563073629728976, 0.31287159091429995, 0.054415842243104008 };
    private static readonly double[] _db10 = { -1.3264202894521244e-05, 9.3588670320069592e-05, -0.00011646685512928545, -0.00068585669495971162, 0.0019924052951850561, 0.0013953517470529011, -0.010733175483330575, 0.0036065535669561697, 0.033212674059341002, -0.029457536821875813, -0.071394147166397082, 0.093057364603572348, 0.12736934033579325, -0.19594627437737705, -0.24984642432731538, 0.28117234366057747, 0.68845903945360354, 0.52720118893172563, 0.1881768000776915, 0.026670057900555554 };
    private static readonly double[] _sym4 = { -0.075765714789273325, -0.02963552764599851, 0.49761866763201545, 0.80373875180591614, 0.29785779560527736, -0.099219543576847216, -0.012603967262037833, 0.032223100604042702 };
    private static readonly double[] _sym5 = { 0.027333068345077982, 0.029519490925774643, -0.039134249302383094, 0.1993975339773936, 0.72340769040242059, 0.63397896345821192, 0.016602105764522319, -0.17532808990845047, -0.021101834024758855, 0.019538882735286728 };
    private static readonly double[] _sym6 = { 0.015404109327027373, 0.0034907120842174702, -0.11799011114819057, -0.048311742585632998, 0.49105594192674662, 0.787641141030194, 0.3379294217276218, -0.072637522786462516, -0.021060292512300564, 0.044724901770665779, 0.0017677118642428036, -0.007800708325034148 };
    private static readonly double[] _sym8 = { -0.0033824159510061256, -0.00054213233179114812, 0.031695087811492981, 0.0076074873249176054, -0.14329423835080971, -0.061273359067658524, 0.48135965125837221, 0.77718575170052351, 0.3644418948353314, -0.051945838107709037, -0.027219029917056003, 0.049137179673607506, 0.0038087520138906151, -0.014952258337048231, -0.0003029205147213668, 0.0018899503327594609 };
    private static readonly double[] _coif1 = { -0.015655728135791993, -0.07273261951252645, 0.38486484686485778, 0.85257202021160039, 0.33789766245748182, -0.07273261951252645 };
    private static readonly double[] _coif2 = { -0.00072054944552034698, -0.0018232088709110323, 0.0056114348193688343, 0.02368017194684777, -0.059434418646431092, -0.076488599078280761, 0.41700518442323908, 0.81272363544941351, 0.38611006682276289, -0.067372554723725595, -0.041464936786871777, 0.016387336463203641 };
    private static readonly double[] _coif3 = { -3.4599773197272781e-05, -7.0983302506379004e-05, 0.00046621695982040288, 0.0011175187708306303, -0.0025745176881367972, -0.0090079761367306242, 0.015880544863669452, 0.034555027573297738, -0.082301927106299827, -0.071799821619154838, 0.42848347637737, 0.79377722262608719, 0.40517690240911824, -0.061123390002972552, -0.065771911281469364, 0.023452696142077168, 0.0077825964256727463, -0.0037935128643808019 };
    private static readonly double[] _coif4 = { -1.7849909144933469e-06, -3.259647940030751e-06, 3.1229861599195265e-05, 6.2338854312787192e-05, -0.00025997433712225682, -0.00058902022463321654, 0.0012665610789256603, 0.0037514346971460866, -0.0056582838001308835, -0.015211728187697211, 0.025082253337949612, 0.039334422605589149, -0.096220424535952642, -0.066627472366817167, 0.43438603311435653, 0.78223893442428261, 0.41530842700068227, -0.056077319603569258, -0.081266710249193727, 0.02668230466960483, 0.016068947131575029, -0.0073461679362680507, -0.001629492425226786, 0.00089231390253700297 };
    private static readonly double[] _coif5 = { -9.6040101127678941e-08, -1.6237995172048338e-07, 2.0612203985788783e-06, 3.7007277113394796e-06, -2.1270221672515614e-05, -4.1219861924265501e-05, 0.00014035632812373243, 0.00030185794166824478, -0.00063755892612588115, -0.0016616273039298788, 0.0024315754425382886, 0.0067615202206204169, -0.0091595073386761625, -0.019758391600965465, 0.032674799467057355, 0.041287530472117834, -0.10556315130733723, -0.06203775157498196, 0.43798230665916338, 0.77429362286032744, 0.42157126673075435, -0.052046670253554764, -0.091921588060086087, 0.028169744270532353, 0.023408322118927783, -0.010131584846900276, -0.0041593126275786402, 0.0021782943778456947, 0.00035857774116175768, -0.000212081862067494 };

    /// <summary>
    /// Retorna o filtro passa-baixa de decomposição (scaling / dec_lo) da wavelet ortonormal indicada.
    /// </summary>
    /// <param name="family">Família da wavelet.</param>
    /// <param name="order">Ordem (N) da wavelet (ignorada para Haar).</param>
    /// <returns>Coeficientes do filtro de decomposição passa-baixa.</returns>
    /// <exception cref="ArgumentException">Lançada quando a combinação família/ordem não é suportada.</exception>
    public static double[] GetWaveletScalingFilter(WaveletFamily family, int order)
    {
        switch (family)
        {
            case WaveletFamily.Haar:
                return _haar;
            case WaveletFamily.Daubechies:
                return order switch
                {
                    1 => _haar,
                    2 => _db2,
                    3 => _db3,
                    4 => _db4,
                    5 => _db5,
                    6 => _db6,
                    7 => _db7,
                    8 => _db8,
                    10 => _db10,
                    _ => throw new ArgumentException($"Daubechies de ordem {order} não suportada (disponíveis: 1-8, 10).", nameof(order))
                };
            case WaveletFamily.Symlet:
                return order switch
                {
                    4 => _sym4,
                    5 => _sym5,
                    6 => _sym6,
                    8 => _sym8,
                    _ => throw new ArgumentException($"Symlet de ordem {order} não suportada (disponíveis: 4, 5, 6, 8).", nameof(order))
                };
            case WaveletFamily.Coiflet:
                return order switch
                {
                    1 => _coif1,
                    2 => _coif2,
                    3 => _coif3,
                    4 => _coif4,
                    5 => _coif5,
                    _ => throw new ArgumentException($"Coiflet de ordem {order} não suportada (disponíveis: 1-5).", nameof(order))
                };
            default:
                throw new ArgumentException($"Família de wavelet desconhecida: {family}.", nameof(family));
        }
    }

    /// <summary>
    /// Constrói o filtro passa-alta de decomposição (wavelet / dec_hi) a partir do passa-baixa,
    /// pela relação de espelhamento em quadratura (QMF): g[k] = (-1)^k · h[L-1-k].
    /// </summary>
    /// <param name="scalingFilter">Filtro passa-baixa (dec_lo).</param>
    /// <returns>Filtro passa-alta (dec_hi) de mesmo comprimento.</returns>
    public static double[] WaveletDetailFilter(double[] scalingFilter)
    {
        int length = scalingFilter.Length;
        double[] highPass = new double[length];
        for (int k = 0; k < length; k++)
            highPass[k] = ((k & 1) == 0 ? 1.0 : -1.0) * scalingFilter[length - 1 - k];
        return highPass;
    }

    /// <summary>
    /// Executa um nível da DWT periódica (algoritmo de Mallat) sobre um sinal de comprimento par.
    /// Para wavelets ortonormais, a transformada é ortogonal (preserva energia) e perfeitamente invertível.
    /// </summary>
    /// <param name="signal">Sinal de entrada (comprimento par).</param>
    /// <param name="lowPass">Filtro passa-baixa de decomposição.</param>
    /// <param name="highPass">Filtro passa-alta de decomposição.</param>
    /// <param name="approximation">Saída: coeficientes de aproximação (comprimento signal.Length/2).</param>
    /// <param name="detail">Saída: coeficientes de detalhe (comprimento signal.Length/2).</param>
    public static void DiscreteWaveletStepForward(double[] signal, double[] lowPass, double[] highPass, double[] approximation, double[] detail)
    {
        int n = signal.Length;
        int filterLength = lowPass.Length;
        int half = n / 2;
        for (int i = 0; i < half; i++)
        {
            double a = 0.0, d = 0.0;
            int baseIndex = 2 * i;
            for (int k = 0; k < filterLength; k++)
            {
                int index = baseIndex + k;
                if (index >= n)
                    index %= n; // extensão periódica
                double s = signal[index];
                a += lowPass[k] * s;
                d += highPass[k] * s;
            }
            approximation[i] = a;
            detail[i] = d;
        }
    }

    /// <summary>
    /// Executa um nível da IDWT periódica, reconstruindo o sinal a partir dos coeficientes de
    /// aproximação e de detalhe. É a transposta (inversa, por ortogonalidade) de <see cref="DiscreteWaveletStepForward"/>.
    /// </summary>
    /// <param name="approximation">Coeficientes de aproximação.</param>
    /// <param name="detail">Coeficientes de detalhe (mesmo comprimento da aproximação).</param>
    /// <param name="lowPass">Filtro passa-baixa de decomposição (reutilizado na síntese pela ortogonalidade).</param>
    /// <param name="highPass">Filtro passa-alta de decomposição.</param>
    /// <param name="output">Saída: sinal reconstruído (comprimento 2·approximation.Length).</param>
    public static void DiscreteWaveletStepInverse(double[] approximation, double[] detail, double[] lowPass, double[] highPass, double[] output)
    {
        int half = approximation.Length;
        int n = 2 * half;
        int filterLength = lowPass.Length;
        Array.Clear(output, 0, n);
        for (int i = 0; i < half; i++)
        {
            double a = approximation[i];
            double d = detail[i];
            int baseIndex = 2 * i;
            for (int k = 0; k < filterLength; k++)
            {
                int index = baseIndex + k;
                if (index >= n)
                    index %= n; // extensão periódica
                output[index] += lowPass[k] * a + highPass[k] * d;
            }
        }
    }

    /// <summary>
    /// Decomposição multinível pela DWT periódica. O comprimento do sinal deve ser múltiplo de 2^levels.
    /// </summary>
    /// <param name="signal">Sinal de entrada.</param>
    /// <param name="lowPass">Filtro passa-baixa de decomposição.</param>
    /// <param name="highPass">Filtro passa-alta de decomposição.</param>
    /// <param name="levels">Número de níveis de decomposição.</param>
    /// <returns>
    /// Vetor jagged [cA_n, cD_n, cD_{n-1}, ..., cD_1]: índice 0 é a aproximação mais grosseira,
    /// índice 1 o detalhe do nível mais grosseiro, e o último índice o detalhe mais fino (cD_1).
    /// </returns>
    public static double[][] WaveletDecompose(double[] signal, double[] lowPass, double[] highPass, int levels)
    {
        double[][] coefficients = new double[levels + 1][];
        double[] current = signal;
        for (int level = 0; level < levels; level++)
        {
            int half = current.Length / 2;
            double[] approximation = new double[half];
            double[] detail = new double[half];
            DiscreteWaveletStepForward(current, lowPass, highPass, approximation, detail);
            // cD_1 vai no último índice; cD_n no índice 1.
            coefficients[levels - level] = detail;
            current = approximation;
        }
        coefficients[0] = current; // aproximação mais grosseira (cA_n)
        return coefficients;
    }

    /// <summary>
    /// Reconstrói o sinal a partir dos coeficientes produzidos por <see cref="WaveletDecompose"/>.
    /// </summary>
    /// <param name="coefficients">Coeficientes [cA_n, cD_n, ..., cD_1].</param>
    /// <param name="lowPass">Filtro passa-baixa de decomposição.</param>
    /// <param name="highPass">Filtro passa-alta de decomposição.</param>
    /// <returns>Sinal reconstruído.</returns>
    public static double[] WaveletReconstruct(double[][] coefficients, double[] lowPass, double[] highPass)
    {
        double[] approximation = coefficients[0];
        for (int level = 1; level < coefficients.Length; level++)
        {
            double[] detail = coefficients[level];
            double[] output = new double[2 * approximation.Length];
            DiscreteWaveletStepInverse(approximation, detail, lowPass, highPass, output);
            approximation = output;
        }
        return approximation;
    }

    /// <summary>
    /// Limiar suave (soft threshold) de Donoho: encolhe o valor em direção a zero em <paramref name="threshold"/>.
    /// </summary>
    public static double SoftThreshold(double value, double threshold)
    {
        double magnitude = Abs(value) - threshold;
        if (magnitude <= 0)
            return 0.0;
        return value < 0 ? -magnitude : magnitude;
    }

    /// <summary>
    /// Limiar rígido (hard threshold): mantém o valor se |value| ≥ threshold, caso contrário zera.
    /// </summary>
    public static double HardThreshold(double value, double threshold) => Abs(value) < threshold ? 0.0 : value;

    /// <summary>
    /// Estima o desvio-padrão do ruído pela mediana dos desvios absolutos (MAD) dos coeficientes de
    /// detalhe: σ = mediana(|d|) / 0.6745. Robusto a outliers (picos) — base do estimador de Donoho-Johnstone.
    /// </summary>
    /// <param name="detailCoefficients">Coeficientes de detalhe (tipicamente os do nível mais fino).</param>
    /// <returns>Estimativa do desvio-padrão do ruído.</returns>
    public static double EstimateNoiseStandardDeviation(double[] detailCoefficients)
    {
        if (detailCoefficients.Length == 0)
            return 0.0;
        double[] magnitudes = new double[detailCoefficients.Length];
        for (int i = 0; i < detailCoefficients.Length; i++)
            magnitudes[i] = Abs(detailCoefficients[i]);
        return Median(magnitudes) / MadToSigma;
    }

    /// <summary>
    /// Limiar universal de VisuShrink (Donoho-Johnstone): λ = σ · √(2 ln N).
    /// </summary>
    /// <param name="sigma">Desvio-padrão estimado do ruído.</param>
    /// <param name="length">Número de amostras do sinal (N).</param>
    public static double UniversalThreshold(double sigma, int length) => length < 2 ? 0.0 : sigma * Sqrt(2.0 * Log(length));

    /// <summary>
    /// Número máximo de níveis de decomposição viáveis para um sinal de tamanho <paramref name="length"/>
    /// (⌊log2(length)⌋), de modo que 2^levels ≤ length.
    /// </summary>
    public static int MaxWaveletLevels(int length)
    {
        int levels = 0;
        while ((1 << (levels + 1)) <= length)
            levels++;
        return levels;
    }

    /// <summary>
    /// Mediana de um vetor (modifica a ordem do vetor de entrada). Para tamanho par, retorna a média
    /// dos dois elementos centrais.
    /// </summary>
    public static double Median(double[] values)
    {
        Array.Sort(values);
        int n = values.Length;
        int mid = n / 2;
        return (n & 1) == 1 ? values[mid] : (values[mid - 1] + values[mid]) / 2.0;
    }

    #endregion

    #region Sistemas banda-simétricos, suavização de Whittaker, B-splines e seleção de penalidade

    // Métodos matemáticos reaproveitáveis para a suavização por verossimilhança penalizada e a eliminação
    // de Kα2 pelo modelo de ligação composta penalizado (de Rooi, van der Pers, Hendrikx, Delhez, Böttger
    // & Eilers, J. Appl. Cryst. 2014, 47, 852-860).

    /// <summary>
    /// Resolve o sistema linear A·x = b para A simétrica positiva-definida em banda, por fatoração de
    /// Cholesky em banda (A = Rᵀ·R). Complexidade O(n·p²), com p = meia-largura de banda. É a forma de
    /// alta performance para os suavizadores de Whittaker/P-spline, cuja matriz normal é estreita em banda.
    /// </summary>
    /// <param name="diagonal">Diagonal principal de A (comprimento n).</param>
    /// <param name="superDiagonals">
    /// Superdiagonais de A: <c>superDiagonals[d-1][i] = A[i, i+d]</c> para d = 1..p (p = meia-largura).
    /// Por simetria, as subdiagonais são idênticas. <c>superDiagonals[d-1]</c> tem comprimento n-d.
    /// </param>
    /// <param name="rhs">Vetor do lado direito b (comprimento n).</param>
    /// <returns>Solução x (novo vetor). Os argumentos não são modificados.</returns>
    /// <exception cref="InvalidOperationException">Se A não for positiva-definida.</exception>
    public static double[] SolveSymmetricBanded(double[] diagonal, double[][] superDiagonals, double[] rhs)
    {
        int n = diagonal.Length;
        int p = superDiagonals.Length;
        double[] rDiag = (double[])diagonal.Clone();
        double[][] rSup = new double[p][];
        for (int d = 0; d < p; d++)
            rSup[d] = (double[])superDiagonals[d].Clone();

        // Fatoração de Cholesky em banda: R triangular superior, R[i][i+off] = rSup[off-1][i] e R[i][i] = rDiag[i].
        for (int i = 0; i < n; i++)
        {
            double sum = rDiag[i];
            for (int d = 1; d <= p; d++)
            {
                int k = i - d;
                if (k < 0)
                    break;
                double rki = rSup[d - 1][k]; // R[k][i], pois i = k + d
                sum -= rki * rki;
            }
            if (sum <= 0.0)
                throw new InvalidOperationException("Matriz não é positiva-definida (Cholesky em banda).");
            double rii = Sqrt(sum);
            rDiag[i] = rii;
            int maxOff = Min(p, n - 1 - i);
            for (int off = 1; off <= maxOff; off++)
            {
                int j = i + off;
                double s = rSup[off - 1][i]; // A[i][j]
                for (int d = 1; d <= p; d++)
                {
                    int k = i - d;
                    if (k < 0)
                        break;
                    int off2 = j - k; // = d + off
                    if (off2 <= p)
                        s -= rSup[d - 1][k] * rSup[off2 - 1][k]; // R[k][i]·R[k][j]
                }
                rSup[off - 1][i] = s / rii;
            }
        }

        // Substituição direta (Rᵀ z = b) e reversa (R x = z).
        double[] x = (double[])rhs.Clone();
        for (int i = 0; i < n; i++)
        {
            double s = x[i];
            for (int d = 1; d <= p; d++)
            {
                int k = i - d;
                if (k < 0)
                    break;
                s -= rSup[d - 1][k] * x[k];
            }
            x[i] = s / rDiag[i];
        }
        for (int i = n - 1; i >= 0; i--)
        {
            double s = x[i];
            int maxOff = Min(p, n - 1 - i);
            for (int off = 1; off <= maxOff; off++)
                s -= rSup[off - 1][i] * x[i + off];
            x[i] = s / rDiag[i];
        }
        return x;
    }

    /// <summary>
    /// Monta a representação em banda (diagonal + superdiagonais) da matriz de penalidade Dᵀ·D, onde D é
    /// o operador de diferenças finitas de ordem <paramref name="order"/>. Meia-largura de banda = order.
    /// </summary>
    /// <param name="length">Número de pontos do sinal (n).</param>
    /// <param name="order">Ordem da diferença (2 = segunda diferença, padrão do suavizador de Whittaker).</param>
    /// <returns>Diagonal e superdiagonais de Dᵀ·D (não escalada pela penalidade λ).</returns>
    public static (double[] diagonal, double[][] superDiagonals) DifferencePenaltyBand(int length, int order = 2)
    {
        double[] diagonal = new double[length];
        double[][] superDiagonals = new double[order][];
        for (int d = 1; d <= order; d++)
            superDiagonals[d - 1] = new double[length - d > 0 ? length - d : 0];

        int lastRow = length - order - 1; // índice máximo de linha de D (linhas r = 0..lastRow)
        if (lastRow < 0)
            return (diagonal, superDiagonals); // sinal curto demais: penalidade nula

        // Coeficientes do operador de diferença de ordem 'order': coef[a] = (-1)^a · C(order, a).
        double[] coef = new double[order + 1];
        long binom = 1;
        for (int a = 0; a <= order; a++)
        {
            coef[a] = ((a & 1) == 0 ? 1.0 : -1.0) * binom;
            binom = binom * (order - a) / (a + 1);
        }

        // (DᵀD)[i, i+off] = Σ_a coef[a]·coef[a+off], com a limitado pelas linhas válidas de D.
        for (int i = 0; i < length; i++)
        {
            for (int off = 0; off <= order; off++)
            {
                if (i + off >= length)
                    break;
                int lo = Max(0, i - lastRow);
                int hi = Min(order - off, i);
                double s = 0.0;
                for (int a = lo; a <= hi; a++)
                    s += coef[a] * coef[a + off];
                if (off == 0)
                    diagonal[i] = s;
                else
                    superDiagonals[off - 1][i] = s;
            }
        }
        return (diagonal, superDiagonals);
    }

    /// <summary>
    /// Suavizador de Whittaker (mínimos quadrados penalizados, ponderado): minimiza
    /// Σ wᵢ(yᵢ − zᵢ)² + λ·‖D·z‖², resolvendo (W + λ·DᵀD)·z = W·y em banda (Whittaker, 1923; Eilers, 2003).
    /// </summary>
    /// <param name="y">Sinal observado.</param>
    /// <param name="weights">Pesos wᵢ (nulo = pesos unitários).</param>
    /// <param name="lambda">Peso da penalidade de rugosidade (λ ≥ 0).</param>
    /// <param name="order">Ordem da diferença (padrão 2).</param>
    /// <returns>Sinal suavizado z.</returns>
    public static double[] WhittakerSmooth(double[] y, double[]? weights, double lambda, int order = 2)
    {
        int n = y.Length;
        var (pDiag, pSup) = DifferencePenaltyBand(n, order);
        int p = pSup.Length;
        double[] diag = new double[n];
        double[] rhs = new double[n];
        for (int i = 0; i < n; i++)
        {
            double w = weights is null ? 1.0 : weights[i];
            diag[i] = w + lambda * pDiag[i];
            rhs[i] = w * y[i];
        }
        double[][] sup = new double[p][];
        for (int d = 0; d < p; d++)
        {
            double[] src = pSup[d];
            double[] dst = new double[src.Length];
            for (int i = 0; i < src.Length; i++)
                dst[i] = lambda * src[i];
            sup[d] = dst;
        }
        return SolveSymmetricBanded(diag, sup, rhs);
    }

    /// <summary>
    /// Deviance de Poisson G = 2·Σ(μᵢ − yᵢ·ln μᵢ) (de Rooi &amp; Eilers, 2014, eq. 10), a medida de
    /// (in)fidelidade da verossimilhança penalizada e dos critérios de seleção de penalidade.
    /// </summary>
    public static double PoissonDeviance(double[] y, double[] mu)
    {
        double g = 0.0;
        for (int i = 0; i < y.Length; i++)
            g += mu[i] - (y[i] > 0.0 ? y[i] * Log(mu[i]) : 0.0);
        return 2.0 * g;
    }

    /// <summary>
    /// Forma quadrática vᵀ·A·v para A simétrica em banda (mesma representação de <see cref="SolveSymmetricBanded"/>).
    /// Usada para avaliar o termo de penalidade ‖D·v‖² = vᵀ(DᵀD)v na seleção automática da penalidade.
    /// </summary>
    public static double BandQuadraticForm(double[] diagonal, double[][] superDiagonals, double[] v)
    {
        double result = 0.0;
        for (int i = 0; i < diagonal.Length; i++)
            result += diagonal[i] * v[i] * v[i];
        for (int d = 0; d < superDiagonals.Length; d++)
        {
            double[] sup = superDiagonals[d];
            int off = d + 1;
            for (int i = 0; i < sup.Length; i++)
                result += 2.0 * sup[i] * v[i] * v[i + off];
        }
        return result;
    }

    /// <summary>
    /// Número de funções-base B-spline de uma grade uniforme: <c>segments + degree</c>.
    /// </summary>
    public static int BSplineBasisCount(int segments, int degree) => segments + degree;

    /// <summary>
    /// Avalia as (degree+1) funções-base B-spline não nulas no ponto <paramref name="u"/>, sobre uma grade
    /// de nós uniforme (recorrência triangular de Cox-de Boor; cf. NURBS Book, alg. A2.2). Para B-splines de
    /// grau q apenas q+1 bases são não nulas em qualquer ponto, o que mantém as matrizes em banda estreita.
    /// </summary>
    /// <param name="u">Ponto de avaliação (pode ser irregular; só a grade de nós é uniforme).</param>
    /// <param name="xMin">Início do domínio (primeiro nó interno).</param>
    /// <param name="knotSpacing">Espaçamento dx entre nós internos.</param>
    /// <param name="segments">Número de segmentos (intervalos) da grade interna.</param>
    /// <param name="degree">Grau q das B-splines.</param>
    /// <param name="basis">Buffer de saída de tamanho ≥ degree+1: recebe as q+1 bases não nulas.</param>
    /// <param name="firstIndex">Saída: índice (0-based) da primeira base não nula.</param>
    public static void EvaluateBSplineNonZero(double u, double xMin, double knotSpacing, int segments, int degree,
        double[] basis, out int firstIndex)
    {
        int segment = (int)Floor((u - xMin) / knotSpacing);
        if (segment < 0)
            segment = 0;
        else if (segment > segments - 1)
            segment = segments - 1;
        int span = segment + degree; // índice do nó tal que knot[span] ≤ u < knot[span+1]
        firstIndex = segment;

        // knot(idx) = xMin + (idx − degree)·dx. O denominador (right+left) reduz a j·dx (uniforme), sempre > 0.
        basis[0] = 1.0;
        for (int j = 1; j <= degree; j++)
        {
            double saved = 0.0;
            for (int r = 0; r < j; r++)
            {
                double rightVal = (xMin + (span + r + 1 - degree) * knotSpacing) - u; // right[r+1]
                double leftVal = u - (xMin + (span - (j - r) + 1 - degree) * knotSpacing); // left[j-r]
                double temp = basis[r] / (rightVal + leftVal);
                basis[r] = saved + rightVal * temp;
                saved = leftVal * temp;
            }
            basis[j] = saved;
        }
    }

    /// <summary>
    /// Localiza o "canto" da curva-L para seleção automática da penalidade (Frasso &amp; Eilers, 2012):
    /// dado, para cada λ da grade, φ = ln(resíduo) e ψ = ln(penalidade), retorna a média dos dois λ
    /// associados ao menor passo (Δφ)² + (Δψ)².
    /// </summary>
    public static double LCurveCorner(double[] lambdas, double[] phi, double[] psi)
    {
        int best = 0;
        double bestStep = double.MaxValue;
        for (int k = 0; k < lambdas.Length - 1; k++)
        {
            double dPhi = phi[k + 1] - phi[k];
            double dPsi = psi[k + 1] - psi[k];
            double step = dPhi * dPhi + dPsi * dPsi;
            if (step < bestStep)
            {
                bestStep = step;
                best = k;
            }
        }
        return 0.5 * (lambdas[best] + lambdas[best + 1]);
    }

    /// <summary>
    /// Inversa seletiva em banda: dado A simétrica positiva-definida em banda (mesma representação de
    /// <see cref="SolveSymmetricBanded"/>), devolve as entradas de A⁻¹ dentro da mesma banda (diagonal e
    /// p superdiagonais), por fatoração LDLᵀ seguida das recursões de Takahashi (Erisman &amp; Tinney, 1975).
    /// Complexidade O(n·p²) — muito mais barato que a inversa completa. A diagonal de A⁻¹ é suficiente
    /// para a dimensão efetiva ED = tr[(X̂ᵀŴX̂ + P)⁻¹·…] usada no critério AIC (de Rooi &amp; Eilers, 2014, eq. 26).
    /// </summary>
    /// <param name="diagonal">Diagonal de A (comprimento n).</param>
    /// <param name="superDiagonals">Superdiagonais de A: <c>superDiagonals[d-1][i] = A[i, i+d]</c>, d = 1..p.</param>
    /// <returns>Diagonal e superdiagonais de A⁻¹ dentro da banda de meia-largura p.</returns>
    public static (double[] diagonal, double[][] superDiagonals) BandedSelectedInverse(double[] diagonal, double[][] superDiagonals)
    {
        int n = diagonal.Length;
        int p = superDiagonals.Length;

        // Fatoração LDLᵀ em banda: L unidiagonal inferior (lSub[d-1][i] = L[i+d, i]), D diagonal.
        double[] d = new double[n];
        double[][] lSub = new double[p][];
        for (int t = 0; t < p; t++)
            lSub[t] = new double[superDiagonals[t].Length];

        for (int j = 0; j < n; j++)
        {
            double dj = diagonal[j];
            for (int kk = 1; kk <= p; kk++)
            {
                int k = j - kk;
                if (k < 0)
                    break;
                double ljk = lSub[kk - 1][k]; // L[j, k], j = k + kk
                dj -= ljk * ljk * d[k];
            }
            if (dj == 0.0)
                throw new InvalidOperationException("Matriz singular na fatoração LDLᵀ em banda.");
            d[j] = dj;

            int iMax = Min(j + p, n - 1);
            for (int i = j + 1; i <= iMax; i++)
            {
                double s = superDiagonals[i - j - 1][j]; // A[i, j] (simétrica)
                for (int kk = 1; kk <= p; kk++)
                {
                    int k = j - kk;
                    if (k < 0)
                        break;
                    int di = i - k;
                    if (di > p)
                        continue;
                    s -= lSub[di - 1][k] * lSub[kk - 1][k] * d[k]; // L[i,k]·L[j,k]·D[k]
                }
                lSub[i - j - 1][j] = s / dj;
            }
        }

        // Recursões de Takahashi para as entradas de A⁻¹ dentro da banda (de baixo para cima).
        double[] sDiag = new double[n];
        double[][] sSup = new double[p][];
        for (int t = 0; t < p; t++)
            sSup[t] = new double[superDiagonals[t].Length];

        for (int i = n - 1; i >= 0; i--)
        {
            int bandTop = Min(p, n - 1 - i);
            for (int jd = bandTop; jd >= 0; jd--)
            {
                int j = i + jd;
                double sum = jd == 0 ? 1.0 / d[i] : 0.0;
                for (int kd = 1; kd <= bandTop; kd++)
                {
                    int k = i + kd;
                    double lki = lSub[kd - 1][i]; // L[i+kd, i]
                    sum -= lki * SelectedInverseEntry(sDiag, sSup, k, j);
                }
                if (jd == 0)
                    sDiag[i] = sum;
                else
                    sSup[jd - 1][i] = sum;
            }
        }
        return (sDiag, sSup);
    }

    // Lê S[a, b] (simétrica) da banda já computada.
    private static double SelectedInverseEntry(double[] sDiag, double[][] sSup, int a, int b)
    {
        int lo = a < b ? a : b;
        int delta = a < b ? b - a : a - b;
        return delta == 0 ? sDiag[lo] : sSup[delta - 1][lo];
    }

    #endregion

    #region Suavização de Savitzky-Golay (mínimos quadrados por convolução, 1964)

    // Suavização e diferenciação de dados por mínimos quadrados simplificados (Savitzky & Golay, Anal. Chem.
    // 1964, 36, 1627-1639). Sobre um abscissa de passo uniforme, o ajuste por mínimos quadrados de um
    // polinômio de grau n a uma janela de 2m+1 pontos reduz-se a uma convolução: o valor (ou a s-ésima
    // derivada) no ponto central é uma soma ponderada dos pontos da janela por inteiros de convolução fixos.
    // O tratamento das bordas (avaliação do mesmo polinômio em pontos fora do centro) segue Gorry (Anal. Chem.
    // 1990, 62, 570-573), preservando o comprimento do sinal sem extrapolação artificial.

    /// <summary>
    /// Resolve o sistema linear denso A·x = b por eliminação de Gauss com pivoteamento parcial. Método de
    /// uso geral (não modifica os argumentos); adequado a sistemas pequenos e densos, como as equações
    /// normais do ajuste polinomial de Savitzky-Golay.
    /// </summary>
    /// <param name="a">Matriz quadrada A (n×n).</param>
    /// <param name="b">Vetor do lado direito b (comprimento n).</param>
    /// <returns>Solução x (novo vetor).</returns>
    /// <exception cref="InvalidOperationException">Se A for singular.</exception>
    public static double[] SolveDenseLinearSystem(double[,] a, double[] b)
    {
        int n = b.Length;
        double[,] m = (double[,])a.Clone();
        double[] x = (double[])b.Clone();

        for (int col = 0; col < n; col++)
        {
            // Pivoteamento parcial: maior magnitude na coluna para estabilidade numérica.
            int pivot = col;
            double max = Abs(m[col, col]);
            for (int r = col + 1; r < n; r++)
            {
                double v = Abs(m[r, col]);
                if (v > max)
                {
                    max = v;
                    pivot = r;
                }
            }
            if (max == 0.0)
                throw new InvalidOperationException("Sistema linear singular (Gauss denso).");
            if (pivot != col)
            {
                for (int k = col; k < n; k++)
                    (m[col, k], m[pivot, k]) = (m[pivot, k], m[col, k]);
                (x[col], x[pivot]) = (x[pivot], x[col]);
            }

            double diag = m[col, col];
            for (int r = col + 1; r < n; r++)
            {
                double factor = m[r, col] / diag;
                if (factor == 0.0)
                    continue;
                for (int k = col; k < n; k++)
                    m[r, k] -= factor * m[col, k];
                x[r] -= factor * x[col];
            }
        }

        // Substituição reversa.
        for (int row = n - 1; row >= 0; row--)
        {
            double sum = x[row];
            for (int k = row + 1; k < n; k++)
                sum -= m[row, k] * x[k];
            x[row] = sum / m[row, row];
        }
        return x;
    }

    /// <summary>
    /// Pesos de convolução de Savitzky-Golay (1964) para uma janela simétrica de 2·<paramref name="halfWindow"/>+1
    /// pontos, ajustada por mínimos quadrados a um polinômio de grau <paramref name="polynomialOrder"/>. Os pesos
    /// avaliam a <paramref name="derivativeOrder"/>-ésima derivada do polinômio ajustado no deslocamento
    /// <paramref name="offset"/> (em amostras a partir do centro): o resultado em um ponto é Σ pesos[i+m]·y[centro+i].
    /// </summary>
    /// <remarks>
    /// Apêndice I do artigo: minimizando Σ(fᵢ − yᵢ)² para fᵢ = Σ_k b_k iᵏ, as equações normais são
    /// Σ_k b_k S_{r+k} = F_r, com S_j = Σ_{i=−m}^{m} iʲ (momentos da janela) e F_r = Σ_i yᵢ iʳ. Como o valor
    /// da s-ésima derivada do polinômio em i = t é Σ_k [k!/(k−s)!] tᵏ⁻ˢ b_k, o peso de cada yᵢ é
    /// pesoᵢ = Σ_r uᵣ iʳ, onde u resolve M·u = c, M[r,k] = S_{r+k} e c_k = [k!/(k−s)!] tᵏ⁻ˢ. Para a suavização
    /// (s = 0, t = 0) reproduz exatamente os inteiros de convolução tabelados (ex.: 5 pontos, grau 2 →
    /// [−3, 12, 17, 12, −3]/35). Para derivadas, divide-se por <paramref name="step"/>ˢ (passo da abscissa).
    /// </remarks>
    /// <param name="halfWindow">Meia-janela m (≥ 1); a janela tem 2m+1 pontos.</param>
    /// <param name="polynomialOrder">Grau n do polinômio (0 ≤ n ≤ 2m).</param>
    /// <param name="derivativeOrder">Ordem s da derivada (0 = suavização; ≤ n).</param>
    /// <param name="offset">Deslocamento t (em amostras) do ponto de avaliação a partir do centro da janela.</param>
    /// <param name="step">Passo δx da abscissa (relevante apenas para derivadas; padrão 1).</param>
    /// <returns>Os 2m+1 pesos de convolução, na ordem i = −m … +m.</returns>
    public static double[] SavitzkyGolayWeights(int halfWindow, int polynomialOrder, int derivativeOrder = 0,
        double offset = 0.0, double step = 1.0)
    {
        if (halfWindow < 1)
            throw new ArgumentOutOfRangeException(nameof(halfWindow), "A meia-janela deve ser ≥ 1.");
        if (polynomialOrder < 0)
            throw new ArgumentOutOfRangeException(nameof(polynomialOrder), "O grau do polinômio deve ser ≥ 0.");
        if (polynomialOrder > 2 * halfWindow)
            throw new ArgumentException("O grau do polinômio deve ser ≤ 2·halfWindow (n < 2m+1).",
                nameof(polynomialOrder));
        if (derivativeOrder < 0 || derivativeOrder > polynomialOrder)
            throw new ArgumentOutOfRangeException(nameof(derivativeOrder), "A ordem da derivada deve estar em [0, grau].");
        if (step <= 0.0)
            throw new ArgumentOutOfRangeException(nameof(step), "O passo deve ser positivo.");

        int m = halfWindow;
        int n = polynomialOrder;
        int dim = n + 1;
        int s = derivativeOrder;

        // Momentos S_j = Σ_{i=−m}^{m} iʲ, j = 0..2n (nulos para j ímpar). Matriz normal M[r,k] = S_{r+k}.
        double[] moments = new double[2 * n + 1];
        for (int i = -m; i <= m; i++)
        {
            double p = 1.0;
            for (int j = 0; j <= 2 * n; j++)
            {
                moments[j] += p;
                p *= i;
            }
        }
        double[,] normal = new double[dim, dim];
        for (int r = 0; r < dim; r++)
            for (int k = 0; k < dim; k++)
                normal[r, k] = moments[r + k];

        // Vetor da avaliação da s-ésima derivada em t = offset: c_k = [k!/(k−s)!]·tᵏ⁻ˢ (0 para k < s).
        double[] c = new double[dim];
        for (int k = s; k < dim; k++)
        {
            double falling = 1.0; // k!/(k−s)!
            for (int q = 0; q < s; q++)
                falling *= k - q;
            c[k] = falling * Pow(offset, k - s);
        }

        // u = M⁻¹·c (M simétrica). pesoᵢ = Σ_r uᵣ·iʳ, dividido por stepˢ (derivada em relação à abscissa).
        double[] u = SolveDenseLinearSystem(normal, c);

        int len = 2 * m + 1;
        double[] weights = new double[len];
        double invStepS = s == 0 ? 1.0 : 1.0 / Pow(step, s);
        for (int i = -m; i <= m; i++)
        {
            double w = 0.0;
            double p = 1.0;
            for (int r = 0; r < dim; r++)
            {
                w += u[r] * p;
                p *= i;
            }
            weights[i + m] = w * invStepS;
        }
        return weights;
    }

    /// <summary>
    /// Suaviza (ou diferencia) um sinal 1D amostrado em passo uniforme pelo método de Savitzky-Golay (1964):
    /// convolução com os pesos de mínimos quadrados de uma janela deslizante de <paramref name="windowLength"/>
    /// pontos e polinômio de grau <paramref name="polynomialOrder"/>. O comprimento é preservado: as m bordas de
    /// cada extremidade usam a avaliação do polinômio da janela extrema em pontos fora do centro (Gorry, 1990).
    /// </summary>
    /// <param name="y">Sinal de entrada (intensidades), em passo uniforme. Não é modificado.</param>
    /// <param name="windowLength">Tamanho da janela (ímpar, ≥ 3 e &gt; grau).</param>
    /// <param name="polynomialOrder">Grau do polinômio de ajuste (&lt; janela e ≥ ordem da derivada).</param>
    /// <param name="derivativeOrder">Ordem da derivada (0 = suavização, padrão).</param>
    /// <param name="step">Passo δx da abscissa (relevante apenas para derivadas; padrão 1).</param>
    /// <returns>Novo vetor suavizado, do mesmo comprimento de <paramref name="y"/>.</returns>
    public static double[] SavitzkyGolaySmooth(double[] y, int windowLength, int polynomialOrder,
        int derivativeOrder = 0, double step = 1.0)
    {
        ArgumentNullException.ThrowIfNull(y);
        if (windowLength < 3)
            throw new ArgumentOutOfRangeException(nameof(windowLength), "A janela deve ter ≥ 3 pontos.");
        if ((windowLength & 1) == 0)
            throw new ArgumentException("A janela deve ter um número ímpar de pontos (2m+1).", nameof(windowLength));
        if (polynomialOrder >= windowLength)
            throw new ArgumentException("O grau do polinômio deve ser menor que a janela.", nameof(polynomialOrder));
        if (polynomialOrder < derivativeOrder)
            throw new ArgumentException("O grau do polinômio deve ser ≥ ordem da derivada.", nameof(polynomialOrder));

        int nData = y.Length;
        if (nData < windowLength)
            throw new ArgumentException("O sinal é mais curto que a janela.", nameof(y));

        int m = windowLength / 2;
        double[] result = new double[nData];

        // Interior (janela completa, t = 0): mesmos pesos para todos os pontos centrais.
        double[] interior = SavitzkyGolayWeights(m, polynomialOrder, derivativeOrder, 0.0, step);
        for (int p = m; p < nData - m; p++)
        {
            double acc = 0.0;
            for (int i = -m; i <= m; i++)
                acc += interior[i + m] * y[p + i];
            result[p] = acc;
        }

        // Bordas: a janela extrema (esquerda y[0..2m], direita y[N−2m−1..N−1]) é fixa; o ponto de saída desliza
        // dentro dela, avaliando o polinômio ajustado em deslocamento off-center (Gorry, 1990).
        for (int p = 0; p < m; p++)
        {
            double[] leftWeights = SavitzkyGolayWeights(m, polynomialOrder, derivativeOrder, p - m, step);
            double accLeft = 0.0;
            for (int j = 0; j < windowLength; j++)
                accLeft += leftWeights[j] * y[j];
            result[p] = accLeft;

            double[] rightWeights = SavitzkyGolayWeights(m, polynomialOrder, derivativeOrder, m - p, step);
            double accRight = 0.0;
            int rightBase = nData - windowLength;
            for (int j = 0; j < windowLength; j++)
                accRight += rightWeights[j] * y[rightBase + j];
            result[nData - 1 - p] = accRight;
        }
        return result;
    }

    #endregion

    #region Detecção de extremos por derivadas: cruzamentos por zero e localização sub-amostral

    // Primitivas de processamento de sinal reutilizadas pela detecção de picos pelo método das derivadas
    // (Huang, Aust. J. Phys. 1988, 41, 201-212): no ápice de um pico a primeira derivada cruza zero de
    // positivo para negativo e a segunda derivada atinge um mínimo negativo. São o espelho/complemento de
    // FindLocalMaxima, igualmente úteis para qualquer análise de extremos de uma série amostrada.

    /// <summary>
    /// Índices dos mínimos locais estritos de <paramref name="values"/> cujo valor é menor que
    /// <paramref name="threshold"/> (espelho de <see cref="FindLocalMaxima"/>). Entradas NaN são ignoradas
    /// e tratadas como +∞, de modo que nunca formam nem interrompem indevidamente um mínimo. Reutilizado
    /// para localizar os mínimos negativos da segunda derivada (ápices/ombros de picos sobrepostos).
    /// </summary>
    public static int[] FindLocalMinima(double[] values, double threshold)
    {
        var minima = new List<int>();
        int n = values.Length;
        for (int i = 0; i < n; i++)
        {
            double v = values[i];
            if (double.IsNaN(v) || v >= threshold)
                continue;
            double left = i > 0 ? values[i - 1] : double.PositiveInfinity;
            double right = i < n - 1 ? values[i + 1] : double.PositiveInfinity;
            if (double.IsNaN(left))
                left = double.PositiveInfinity;
            if (double.IsNaN(right))
                right = double.PositiveInfinity;
            if (v < left && v < right)
                minima.Add(i);
        }
        return minima.ToArray();
    }

    /// <summary>
    /// Índices i dos cruzamentos por zero descendentes (de positivo para não-positivo) de uma série:
    /// posições onde <c>values[i] &gt; 0</c> e <c>values[i+1] ≤ 0</c>. O cruzamento exato fica em
    /// <c>[i, i+1]</c> e pode ser localizado sub-amostralmente por interpolação linear,
    /// <c>frac = values[i] / (values[i] − values[i+1])</c>. Reutilizado para localizar o ápice de um pico
    /// pela primeira derivada, que cruza zero de + para − no máximo. Pares com NaN são ignorados.
    /// </summary>
    public static int[] FindDownwardZeroCrossings(double[] values)
    {
        var crossings = new List<int>();
        for (int i = 0; i + 1 < values.Length; i++)
        {
            double a = values[i];
            double b = values[i + 1];
            if (double.IsNaN(a) || double.IsNaN(b))
                continue;
            if (a > 0.0 && b <= 0.0)
                crossings.Add(i);
        }
        return crossings.ToArray();
    }

    /// <summary>
    /// Deslocamento sub-amostral do vértice da parábola que passa por três pontos igualmente espaçados
    /// (<paramref name="yLeft"/> em −1, <paramref name="yCenter"/> em 0, <paramref name="yRight"/> em +1):
    /// δ = ½·(yLeft − yRight)/(yLeft − 2·yCenter + yRight), limitado a [−½, ½]. Para um extremo discreto
    /// (máximo ou mínimo no ponto central) refina a posição do extremo entre as amostras. Quando os três
    /// pontos são colineares (denominador nulo) devolve 0.
    /// </summary>
    public static double ParabolicVertexOffset(double yLeft, double yCenter, double yRight)
    {
        double denom = yLeft - 2.0 * yCenter + yRight;
        if (denom == 0.0)
            return 0.0;
        double offset = 0.5 * (yLeft - yRight) / denom;
        if (offset > 0.5)
            offset = 0.5;
        else if (offset < -0.5)
            offset = -0.5;
        return offset;
    }

    #endregion

    #region Amostragem uniforme e interpolação linear (suporte à filtragem por PDF)

    /// <summary>Gera <paramref name="count"/> pontos uniformemente espaçados em [start, end] (inclusivo).</summary>
    public static double[] Linspace(double start, double end, int count)
    {
        if (count < 2)
            throw new ArgumentOutOfRangeException(nameof(count), "São necessários ao menos 2 pontos.");

        var result = new double[count];
        double step = (end - start) / (count - 1);
        for (int i = 0; i < count; i++)
            result[i] = start + step * i;

        result[count - 1] = end; // garante a extremidade exata
        return result;
    }

    /// <summary>
    /// Interpolação linear de (xKnown, yKnown) avaliada em <paramref name="xQuery"/>.
    /// <paramref name="xKnown"/> deve ser estritamente crescente; pontos fora da faixa
    /// são fixados (clamp) ao valor da extremidade mais próxima.
    /// </summary>
    public static double[] LinearInterpolate(double[] xKnown, double[] yKnown, double[] xQuery)
    {
        ArgumentNullException.ThrowIfNull(xKnown);
        ArgumentNullException.ThrowIfNull(yKnown);
        ArgumentNullException.ThrowIfNull(xQuery);
        if (xKnown.Length != yKnown.Length)
            throw new ArgumentException("xKnown e yKnown devem ter o mesmo tamanho.");
        if (xKnown.Length < 2)
            throw new ArgumentException("São necessários ao menos 2 pontos conhecidos.", nameof(xKnown));

        var result = new double[xQuery.Length];
        int j = 0; // índice do segmento corrente

        for (int i = 0; i < xQuery.Length; i++)
        {
            double x = xQuery[i];

            if (x <= xKnown[0]) { result[i] = yKnown[0]; continue; }
            if (x >= xKnown[^1]) { result[i] = yKnown[^1]; continue; }

            if (x < xKnown[j]) j = 0;                               // permite xQuery não-ordenado
            while (j < xKnown.Length - 1 && xKnown[j + 1] < x) j++; // avança até o segmento certo

            double x0 = xKnown[j];
            double x1 = xKnown[j + 1];
            double t = (x - x0) / (x1 - x0);
            result[i] = yKnown[j] + t * (yKnown[j + 1] - yKnown[j]);
        }

        return result;
    }

    #endregion
}
