namespace Sindarin.Math.Matrix;

/// <summary>
/// Vetor-coluna de dados experimentais armazenado diretamente como <c>double[][][]</c> nos três níveis
/// do Sindarin: <b>experimental → arquivo → ponto</b>. Evita a etapa de "transpor" os dados para uma
/// <c>Matrix&lt;double&gt;</c> ponto a ponto (o laço de <c>UpDateYToOptimize</c> em Calculation): o
/// otimizador pode operar sobre os mesmos arrays já produzidos pelas funções de cálculo.
///
/// <para>Logicamente é uma matriz <c>(total × 1)</c>, onde <c>total = Σ pontos</c> na ordem de varredura
/// experimental→arquivo→ponto (a mesma do <c>countData</c> de Calculation). Os caminhos rápidos são a
/// enumeração contígua (<see cref="CopyFlatTo"/>) e as operações de mínimos quadrados
/// (<see cref="WeightedNormalEquations"/>, <see cref="Multiply"/>, <see cref="SolveNormalEquations"/>),
/// não o indexador aleatório.</para>
/// </summary>
public sealed class MatrixJagged3D : iMatrix
{
    /// <summary>Dados nos níveis [experimental][arquivo][ponto].</summary>
    internal readonly double[][][] data;
    private readonly int total;
    /// <summary>Índice flat onde começa cada bloco (experimental,arquivo), achatado em ordem de varredura.</summary>
    private readonly int[] blockStart;
    private readonly int[] blockExp;
    private readonly int[] blockFile;

    public MatrixJagged3D(double[][][] data)
    {
        if (data is null || data.Length == 0)
            throw new Exception("The data must have at least one experimental level.");
        this.data = data;

        int blocks = 0;
        for (int e = 0; e < data.Length; e++) blocks += data[e].Length;

        blockStart = new int[blocks + 1];
        blockExp = new int[blocks];
        blockFile = new int[blocks];

        int b = 0, acc = 0;
        for (int e = 0; e < data.Length; e++)
            for (int f = 0; f < data[e].Length; f++)
            {
                blockStart[b] = acc;
                blockExp[b] = e;
                blockFile[b] = f;
                acc += data[e][f].Length;
                b++;
            }
        blockStart[blocks] = acc;
        total = acc;
    }

    /// <summary>Cria a estrutura jagged (não inicializada) com o mesmo formato de <paramref name="template"/>.</summary>
    public static MatrixJagged3D CreateLike(MatrixJagged3D template)
    {
        double[][][] src = template.data;
        double[][][] dst = new double[src.Length][][];
        for (int e = 0; e < src.Length; e++)
        {
            dst[e] = new double[src[e].Length][];
            for (int f = 0; f < src[e].Length; f++)
                dst[e][f] = new double[src[e][f].Length];
        }
        return new MatrixJagged3D(dst);
    }

    public int RowCount => total;
    public int ColumnCount => 1;

    /// <summary>Número total de pontos (= <see cref="RowCount"/>).</summary>
    public int Length => total;

    public double[][][] GetData() => data;

    public double this[int row, int column]
    {
        get
        {
            if (column != 0) throw new IndexOutOfRangeException("MatrixJagged3D has a single column.");
            Locate(row, out int e, out int f, out int p);
            return data[e][f][p];
        }
        set
        {
            if (column != 0) throw new IndexOutOfRangeException("MatrixJagged3D has a single column.");
            Locate(row, out int e, out int f, out int p);
            data[e][f][p] = value;
        }
    }

    /// <summary>Acesso flat (índice na ordem experimental→arquivo→ponto).</summary>
    public double this[int flatIndex]
    {
        get { Locate(flatIndex, out int e, out int f, out int p); return data[e][f][p]; }
        set { Locate(flatIndex, out int e, out int f, out int p); data[e][f][p] = value; }
    }

    public double GetValue(int row, int column) => this[row, 0];
    public double GetValueTransposed(int row, int column) => this[column, 0];

    public iVector GetRow(int index) => new Vector(new[] { this[index, 0] });
    public double[] GetRowArray(int index) => new[] { this[index, 0] };

    public void SetRow(int index, double[] value)
    {
        if (value.Length != 1) throw new Exception("A row of a single-column matrix has length 1.");
        this[index, 0] = value[0];
    }

    private void Locate(int flat, out int e, out int f, out int p)
    {
        if ((uint)flat >= (uint)total)
            throw new IndexOutOfRangeException("Index is outside the data boundaries.");
        // Busca binária do bloco que contém o índice flat.
        int lo = 0, hi = blockExp.Length - 1, block = 0;
        while (lo <= hi)
        {
            int mid = (lo + hi) >> 1;
            if (blockStart[mid] <= flat && flat < blockStart[mid + 1]) { block = mid; break; }
            if (blockStart[mid] > flat) hi = mid - 1; else lo = mid + 1;
        }
        e = blockExp[block];
        f = blockFile[block];
        p = flat - blockStart[block];
    }

    // ===================================================================================
    //  Achatamento contíguo (substitui o laço ponto-a-ponto de UpDateYToOptimize)
    // ===================================================================================

    /// <summary>Copia os dados para um array flat contíguo na ordem experimental→arquivo→ponto.</summary>
    public void CopyFlatTo(double[] destination)
    {
        if (destination.Length != total)
            throw new Exception("The destination length must match the total number of points.");
        int offset = 0;
        for (int e = 0; e < data.Length; e++)
        {
            double[][] de = data[e];
            for (int f = 0; f < de.Length; f++)
            {
                double[] seg = de[f];
                Array.Copy(seg, 0, destination, offset, seg.Length);
                offset += seg.Length;
            }
        }
    }

    /// <summary>Aloca e devolve um array flat contíguo com todos os pontos.</summary>
    public double[] ToFlat()
    {
        double[] flat = new double[total];
        CopyFlatTo(flat);
        return flat;
    }

    /// <summary>Preenche a estrutura jagged a partir de um array flat contíguo (ordem de varredura).</summary>
    public void FillFromFlat(double[] source)
    {
        if (source.Length != total)
            throw new Exception("The source length must match the total number of points.");
        int offset = 0;
        for (int e = 0; e < data.Length; e++)
        {
            double[][] de = data[e];
            for (int f = 0; f < de.Length; f++)
            {
                double[] seg = de[f];
                Array.Copy(source, offset, seg, 0, seg.Length);
                offset += seg.Length;
            }
        }
    }

    // ===================================================================================
    //  Operações elemento a elemento (sobre a mesma forma jagged, sem achatar)
    // ===================================================================================

    /// <summary>Subtração elemento a elemento (ex.: resíduo y − yc). Devolve uma nova estrutura jagged.</summary>
    public MatrixJagged3D Subtract(MatrixJagged3D other)
    {
        EnsureSameShape(other);
        MatrixJagged3D result = CreateLike(this);
        double[][][] a = data, b = other.data, r = result.data;
        for (int e = 0; e < a.Length; e++)
            for (int f = 0; f < a[e].Length; f++)
            {
                double[] ae = a[e][f], be = b[e][f], re = r[e][f];
                for (int p = 0; p < ae.Length; p++) re[p] = ae[p] - be[p];
            }
        return result;
    }

    /// <summary>Soma ponderada dos quadrados: Σ this·(value²). Usado para resíduos ponderados.</summary>
    public double WeightedSumOfSquares(MatrixJagged3D weights)
    {
        EnsureSameShape(weights);
        double sum = 0.0;
        double[][][] a = data, w = weights.data;
        for (int e = 0; e < a.Length; e++)
            for (int f = 0; f < a[e].Length; f++)
            {
                double[] ae = a[e][f], we = w[e][f];
                for (int p = 0; p < ae.Length; p++) sum += we[p] * ae[p] * ae[p];
            }
        return sum;
    }

    private void EnsureSameShape(MatrixJagged3D other)
    {
        if (other.total != total || other.data.Length != data.Length)
            throw new Exception("The matrices must have the same shape.");
    }

    // ===================================================================================
    //  Mínimos quadrados: multiplicação, Mᵀ·W·M e solução das equações normais
    // ===================================================================================

    /// <summary>
    /// Produto Mᵀ·v, onde v = este vetor de dados e M = Jᵀ (<paramref name="jacobianByParam"/>[k] é a
    /// derivada do parâmetro k sobre os pontos, na MESMA ordem de varredura). Resultado de tamanho p.
    /// </summary>
    public double[] Multiply(double[][] jacobianByParam)
    {
        double[] v = ToFlat();
        int p = jacobianByParam.Length;
        double[] r = new double[p];
        Parallel.For(0, p, k =>
        {
            double[] jk = jacobianByParam[k];
            double sum = 0.0;
            for (int n = 0; n < v.Length; n++) sum += jk[n] * v[n];
            r[k] = sum;
        });
        return r;
    }

    /// <summary>
    /// Equações normais ponderadas usando ESTE vetor como a diagonal de pesos W:
    /// <c>A = Mᵀ·W·M</c> com M = Jᵀ e A[k,j] = Σ_n w[n]·J[k][n]·J[j][n] (p×p simétrica).
    /// </summary>
    public MatrixArrays WeightedNormalEquations(double[][] jacobianByParam)
        => MatrixMethods.NormalEquations(jacobianByParam, ToFlat());

    /// <summary>Lado direito ponderado: <c>b = Mᵀ·W·residual</c> (W = este vetor de pesos).</summary>
    public double[] WeightedTransposeResidual(double[][] jacobianByParam, MatrixJagged3D residual)
        => MatrixMethods.TransposeWeightedResidual(jacobianByParam, ToFlat(), residual.ToFlat());

    /// <summary>
    /// Resolve as equações normais ponderadas <c>(Mᵀ·W·M)·Δ = Mᵀ·W·r</c> e devolve o passo Δ (tamanho p).
    /// W = este vetor de pesos; r = <paramref name="residual"/>. Inverte a pequena matriz p×p por LU.
    /// </summary>
    public double[] SolveNormalEquations(double[][] jacobianByParam, MatrixJagged3D residual)
    {
        double[] w = ToFlat();
        MatrixArrays a = MatrixMethods.NormalEquations(jacobianByParam, w);
        double[] b = MatrixMethods.TransposeWeightedResidual(jacobianByParam, w, residual.ToFlat());

        int p = jacobianByParam.Length;
        iMatrix inv = a.InverseLU();
        double[] delta = new double[p];
        for (int k = 0; k < p; k++)
        {
            double sum = 0.0;
            for (int j = 0; j < p; j++) sum += inv[k, j] * b[j];
            delta[k] = sum;
        }
        return delta;
    }
}
