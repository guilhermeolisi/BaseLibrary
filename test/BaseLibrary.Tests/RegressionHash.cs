namespace BaseLibrary.Tests;

/// <summary>
/// FNV-1a de 64 bits sobre os BITS de cada valor (comprimento primeiro), para a regressao bit a bit da E269: um
/// vetor inteiro cabe numa constante, e qualquer ulp diferente muda o hash. E a mesma conta do programa que capturou
/// a saida no master, entao o hash do teste e o da captura sao comparaveis.
/// </summary>
internal static class RegressionHash
{
    private const ulong Offset = 14695981039346656037UL;
    private const ulong Prime = 1099511628211UL;

    private static ulong Mix(ulong h, ulong v)
    {
        for (int b = 0; b < 8; b++)
        {
            h ^= (v >> (8 * b)) & 0xFF;
            h *= Prime;
        }
        return h;
    }

    public static ulong Of(double[] v)
    {
        ulong h = Mix(Offset, (ulong)v.Length);
        foreach (double d in v)
            h = Mix(h, (ulong)BitConverter.DoubleToInt64Bits(d));
        return h;
    }

    public static ulong Of(int[] v)
    {
        ulong h = Mix(Offset, (ulong)v.Length);
        foreach (int i in v)
            h = Mix(h, (ulong)(long)i);
        return h;
    }

    public static ulong Of(double[][] v)
    {
        ulong h = Mix(Offset, (ulong)v.Length);
        foreach (double[] r in v)
            h = Mix(h, Of(r));
        return h;
    }

    public static ulong Combine(ulong a, ulong b) => Mix(Mix(Offset, a), b);

    /// <summary>Os bits do double, para comparar com a captura sem tolerancia.</summary>
    public static ulong Bits(double v) => (ulong)BitConverter.DoubleToInt64Bits(v);
}
