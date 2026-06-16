namespace Sindarin.Math.Matrix;

/// <summary>
/// Estrutura conhecida de uma matriz. Usada para o despacho de operadores otimizados
/// (multiplicação, inversão, etc.) sem precisar inspecionar todos os elementos em tempo de execução.
/// </summary>
public enum MatrixStructure
{
    /// <summary>Sem estrutura especial conhecida (matriz densa genérica).</summary>
    General = 0,
    /// <summary>Apenas a diagonal principal é não-nula.</summary>
    Diagonal,
    /// <summary>Triangular superior (zeros abaixo da diagonal principal).</summary>
    UpperTriangular,
    /// <summary>Triangular inferior (zeros acima da diagonal principal).</summary>
    LowerTriangular,
    /// <summary>Esparsa: a maioria dos elementos é zero (armazenamento comprimido).</summary>
    Sparse,
    /// <summary>Identidade.</summary>
    Identity,
}
