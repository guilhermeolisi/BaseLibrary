namespace BaseLibrary.Math;

/// <summary>
/// Primitivas de convolução numérica genéricas (sem dependência de tipos de domínio), reutilizáveis por
/// qualquer projeto. Operam sobre spans de <c>double</c>.
/// </summary>
public static class ConvolutionMethods
{
    /// <summary>
    /// Convolução discreta NORMALIZADA (média móvel ponderada que PRESERVA A ÁREA) de <paramref name="signal"/>
    /// por <paramref name="kernel"/>. Para cada ponto i: <c>output[i] = (1/Σw)·Σ_k kernel[k]·signal[i-(k-centro)]</c>,
    /// onde o kernel é considerado centrado no ponto médio da grade (mesma convenção da forma amostrada do engine:
    /// <c>centro = (len%2!=0) ? (len-1)/2 : len/2-1</c>). O suporte não-nulo do kernel é detectado automaticamente e a
    /// soma dos pesos normaliza a saída (área conservada). As amostras fora da grade são RECORTADAS (sem wrap).
    /// Se o kernel for DEGENERADO (suporte vazio ou soma ≤ 0) a saída recebe o sinal inalterado (identidade).
    /// </summary>
    /// <param name="kernel">Pesos do kernel (só os índices em [0, min(kernel.Length, signal.Length)) são lidos).</param>
    /// <param name="signal">Sinal de entrada. NÃO pode compartilhar o buffer com <paramref name="output"/>.</param>
    /// <param name="output">Buffer de saída; DEVE ter o mesmo comprimento de <paramref name="signal"/> e ser
    /// distinto dele (a convolução lê o sinal enquanto escreve a saída).</param>
    public static void NormalizedConvolution(ReadOnlySpan<double> kernel, ReadOnlySpan<double> signal, Span<double> output)
    {
        int len = signal.Length;

        // Suporte (kMin..kMax) e soma do kernel (normalização p/ área preservada).
        int kMin = -1, kMax = -1; double kernelSum = 0;
        for (int k = 0; k < kernel.Length && k < len; k++)
        {
            double v = kernel[k];
            if (v != 0) { if (kMin < 0) kMin = k; kMax = k; kernelSum += v; }
        }
        if (kMin < 0 || kernelSum <= 0) // kernel degenerado → identidade
        {
            signal.CopyTo(output);
            return;
        }

        int medium = (len % 2 != 0) ? (len - 1) / 2 : len / 2 - 1; // mesmo centro da forma amostrada
        double inv = 1.0 / kernelSum;
        for (int i = 0; i < len; i++)
        {
            double s = 0;
            for (int k = kMin; k <= kMax; k++)
            {
                double w = kernel[k];
                if (w == 0) continue;
                int src = i - (k - medium);
                if ((uint)src < (uint)len) s += w * signal[src];
            }
            output[i] = s * inv;
        }
    }
}
