using BaseLibrary.Numbers;
using System;
using System.Collections.Generic;
using System.Text;
using static System.Math;

namespace BaseLibrary.Math;

public static class Point3DMaxDistance
{
    class Node
    {
        public int Start, End; // indices [Start, End) no array flattened
        public double MinX, MaxX, MinY, MaxY, MinZ, MaxZ;
        public double Cx, Cy, Cz; // centro da caixa (ou centro de massa)
        public double Radius; // max distance do centro a pontos deste nó
        public Node Left, Right;
        public int Id; // id único se precisar
        public bool IsLeaf => Left == null && Right == null;
        public int Count => End - Start;
    }

    static Node BuildBVH(Point3D[] pts, int start, int end, ref int nextId, int leafSize = 8)
    {
        Node n = new Node { Start = start, End = end, Id = nextId++ };
        double minX = double.PositiveInfinity, maxX = double.NegativeInfinity;
        double minY = double.PositiveInfinity, maxY = double.NegativeInfinity;
        double minZ = double.PositiveInfinity, maxZ = double.NegativeInfinity;
        double sx = 0, sy = 0, sz = 0;
        for (int i = start; i < end; i++)
        {
            var p = pts[i];
            if (p.X < minX) minX = p.X; if (p.X > maxX) maxX = p.X;
            if (p.Y < minY) minY = p.Y; if (p.Y > maxY) maxY = p.Y;
            if (p.Z < minZ) minZ = p.Z; if (p.Z > maxZ) maxZ = p.Z;
            sx += p.X; sy += p.Y; sz += p.Z;
        }
        n.MinX = minX; n.MaxX = maxX;
        n.MinY = minY; n.MaxY = maxY;
        n.MinZ = minZ; n.MaxZ = maxZ;

        int cnt = end - start;
        n.Cx = sx / cnt; n.Cy = sy / cnt; n.Cz = sz / cnt;

        double maxSq = 0;
        for (int i = start; i < end; i++)
        {
            double dx = pts[i].X - n.Cx;
            double dy = pts[i].Y - n.Cy;
            double dz = pts[i].Z - n.Cz;
            double d2 = dx * dx + dy * dy + dz * dz;
            if (d2 > maxSq) maxSq = d2;
        }
        n.Radius = Sqrt(maxSq);

        if (cnt <= leafSize) return n;

        // split along longest axis by sorting (simplicidade)
        double lenX = maxX - minX, lenY = maxY - minY, lenZ = maxZ - minZ;
        if (lenX >= lenY && lenX >= lenZ)
            Array.Sort(pts, start, cnt, Comparer<Point3D>.Create((a, b) => a.X.CompareTo(b.X)));
        else if (lenY >= lenX && lenY >= lenZ)
            Array.Sort(pts, start, cnt, Comparer<Point3D>.Create((a, b) => a.Y.CompareTo(b.Y)));
        else
            Array.Sort(pts, start, cnt, Comparer<Point3D>.Create((a, b) => a.Z.CompareTo(b.Z)));

        int mid = start + cnt / 2;
        n.Left = BuildBVH(pts, start, mid, ref nextId, leafSize);
        n.Right = BuildBVH(pts, mid, end, ref nextId, leafSize);
        return n;
    }

    // upper bound entre dois nós usando esferas (centros + raios)
    static double UpperBound(Node a, Node b)
    {
        double dx = a.Cx - b.Cx, dy = a.Cy - b.Cy, dz = a.Cz - b.Cz;
        double centerDist = Sqrt(dx * dx + dy * dy + dz * dz);
        return centerDist + a.Radius + b.Radius;
    }
    /// <summary>
    /// Calcula a distância máxima entre pontos em um conjunto de dados 3D usando uma estrutura de dados BVH (Bounding Volume Hierarchy).
    /// Este método garante encontrar o par de pontos exato com a maior distância através de uma busca branch-and-bound 
    /// com poda baseada em limites superiores (upper bounds) calculados a partir de esferas delimitadoras dos nós.
    /// </summary>
    /// <param name="jaggedData">
    /// Array jagged contendo os pontos 3D a serem analisados. Linhas nulas são ignoradas.
    /// Todos os pontos válidos serão achatados em um único array para processamento.
    /// </param>
    /// <param name="leafSize">
    /// Tamanho máximo de pontos por nó folha na árvore BVH. Valores menores criam árvores mais profundas 
    /// com melhor poda, mas maior overhead de construção. O padrão é 8 pontos por folha.
    /// </param>
    /// <returns>
    /// A distância euclidiana máxima exata entre quaisquer dois pontos no conjunto de dados.
    /// Retorna 0.0 se houver menos de 2 pontos válidos.
    /// </returns>
    /// <remarks>
    /// O algoritmo funciona em três fases principais:
    /// <list type="number">
    /// <item><description>Construção da BVH: Os pontos são organizados recursivamente em uma árvore binária, 
    /// onde cada nó contém uma caixa delimitadora (bounding box) e uma esfera delimitadora definida pelo 
    /// centro de massa e raio máximo dos pontos contidos.</description></item>
    /// <item><description>Busca branch-and-bound: Uma fila de prioridade (max-heap simulado) mantém pares de nós 
    /// ordenados por seu upper bound (distância máxima possível entre pontos dos dois nós). Pares com upper bound 
    /// menor ou igual à melhor distância encontrada são podados.</description></item>
    /// <item><description>Comparação de folhas: Quando ambos os nós de um par são folhas, todos os pontos entre 
    /// eles são comparados para atualizar a melhor distância.</description></item>
    /// </list>
    /// <para>
    /// Este método é exato e garante encontrar a distância máxima real, ao contrário de métodos heurísticos.
    /// A complexidade depende da distribuição dos dados, mas geralmente é mais eficiente que força bruta (O(n²)) 
    /// para conjuntos de dados grandes e bem distribuídos espacialmente.
    /// </para>
    /// <para>
    /// A BVH é construída dividindo os pontos ao longo do eixo mais longo em cada nível, usando ordenação 
    /// para particionar o conjunto. O upper bound entre dois nós é calculado como a distância entre seus 
    /// centros mais a soma de seus raios, fornecendo um limite superior conservador para todas as distâncias 
    /// possíveis entre pontos dos dois nós.
    /// </para>
    /// </remarks>
    public static double MaxDistanceByBVH(Point3D[][] jaggedData, int leafSize = 8)
    {
        // achatar os pontos
        List<Point3D> all = new List<Point3D>();
        foreach (var row in jaggedData) if (row != null) all.AddRange(row);
        int n = all.Count;
        if (n < 2) return 0.0;

        Point3D[] pts = all.ToArray();

        int nextId = 0;
        Node root = BuildBVH(pts, 0, n, ref nextId, leafSize);

        // priority queue de pares (max-heap por upper bound). Usamos PriorityQueue com prioridade negativa para simular max-heap.
        var pq = new PriorityQueue<(Node a, Node b), double>();
        // insere par inicial (root, root)
        double initialUB = UpperBound(root, root);
        pq.Enqueue((root, root), -initialUB);

        double best = 0.0; // melhor distância atual

        while (pq.Count > 0)
        {
            var pair = pq.Dequeue();
            Node A = pair.a, B = pair.b;
            // priority queue de pares (max-heap por upper bound). Usamos PriorityQueue com prioridade negativa para simular max-heap.
            //double ub = -pq.EnqueuePeekPriorityHack; // we can't read priority from API, so recompute:
            //ub = UpperBound(A, B); // recomputar UB é barato
            double ub = UpperBound(A, B); // recomputar UB é barato
            if (ub <= best) continue; // poda — nenhum par dentro desses nós pode superar best

            // se ambos são folhas -> comparar todos os pontos entre A e B
            if (A.IsLeaf && B.IsLeaf)
            {
                // se são o mesmo nó, evite duplicar pares
                if (A == B)
                {
                    for (int i = A.Start; i < A.End; i++)
                        for (int j = i + 1; j < A.End; j++)
                        {
                            double d2 = pts[i].SquaredDistance(pts[j]);
                            if (d2 > best * best) best = Sqrt(d2);
                        }
                }
                else
                {
                    for (int i = A.Start; i < A.End; i++)
                        for (int j = B.Start; j < B.End; j++)
                        {
                            double d2 = pts[i].SquaredDistance(pts[j]);
                            if (d2 > best * best) best = Sqrt(d2);
                        }
                }
                continue;
            }

            // se um dos nós é o mesmo (A==B) e não é leaf, subdivida
            if (A == B && !A.IsLeaf)
            {
                var L = A.Left; var R = A.Right;
                pq.Enqueue((L, L), -UpperBound(L, L));
                pq.Enqueue((L, R), -UpperBound(L, R));
                pq.Enqueue((R, R), -UpperBound(R, R));
                continue;
            }

            // caso geral: subdividir o nó com maior raio para criar novas combinações
            if (!A.IsLeaf && (B.IsLeaf || A.Radius >= B.Radius))
            {
                var L = A.Left; var R = A.Right;
                pq.Enqueue((L, B), -UpperBound(L, B));
                pq.Enqueue((R, B), -UpperBound(R, B));
            }
            else
            {
                var L = B.Left; var R = B.Right;
                pq.Enqueue((A, L), -UpperBound(A, L));
                pq.Enqueue((A, R), -UpperBound(A, R));
            }
        }

        return best;
    }

    /// <summary>
    /// Calcula a distância máxima entre pontos em um conjunto de dados 3D usando o método de projeção.
    /// Este método projeta os pontos em múltiplas direções (eixos coordenados e direções aleatórias) 
    /// e identifica pontos extremos como candidatos, reduzindo o número de comparações necessárias. Este é o método mais rápido, mas é uma heurística que pode não encontrar o par de pontos mais distante em casos raros, embora seja muito eficaz para conjuntos de dados grandes e distribuídos uniformemente.
    /// </summary>
    /// <param name="jaggedData">
    /// Array jagged contendo os pontos 3D a serem analisados. Linhas nulas são ignoradas.
    /// </param>
    /// <param name="numRandomDirections">
    /// Número de direções aleatórias na esfera unitária a serem usadas para projeção, além dos três eixos coordenados.
    /// O padrão é 50 direções aleatórias.
    /// </param>
    /// <returns>
    /// A distância euclidiana máxima encontrada entre quaisquer dois pontos no conjunto de dados.
    /// Retorna 0.0 se houver menos de 2 pontos válidos.
    /// </returns>
    /// <remarks>
    /// O algoritmo funciona em três etapas:
    /// <list type="number">
    /// <item><description>Projeta todos os pontos em direções específicas (3 eixos + direções aleatórias)</description></item>
    /// <item><description>Identifica pontos com projeções mínimas e máximas em cada direção como candidatos</description></item>
    /// <item><description>Calcula a distância máxima entre todos os candidatos usando processamento paralelo</description></item>
    /// </list>
    /// Este método é uma heurística que oferece boa precisão com desempenho melhorado em comparação com força bruta completa,
    /// especialmente para conjuntos de dados grandes. As direções aleatórias são geradas com uma semente fixa (12345) 
    /// para garantir resultados reproduzíveis.
    /// </remarks>
    public static double MaxDistanceByProjection(Point3D[][] jaggedData, int numRandomDirections = 50)
    {
        // Flatten
        List<Point3D> all = new List<Point3D>();
        foreach (var row in jaggedData) if (row != null) all.AddRange(row);
        if (all.Count < 2) return 0.0;

        int n = all.Count;

        // Build directions: coordinate axes + some random directions
        var directions = new List<(double X, double Y, double Z)>();
        directions.Add((1, 0, 0));
        directions.Add((0, 1, 0));
        directions.Add((0, 0, 1));
        var rnd = new Random(12345);
        for (int i = 0; i < numRandomDirections; i++)
        {
            // random direction on unit sphere
            double u = rnd.NextDouble();
            double v = rnd.NextDouble();
            double theta = 2.0 * PI * u;
            double phi = Acos(2.0 * v - 1.0);
            double sx = Sin(phi) * Cos(theta);
            double sy = Sin(phi) * Sin(theta);
            double sz = Cos(phi);
            directions.Add((sx, sy, sz));
        }

        // For each direction, find min and max projection and add corresponding points as candidates
        var candidateSet = new HashSet<Point3D>();
        foreach (var dir in directions)
        {
            double minProj = double.PositiveInfinity, maxProj = double.NegativeInfinity;
            Point3D minPt = new(), maxPt = new();
            for (int i = 0; i < n; i++)
            {
                var p = all[i];
                double proj = p.X * dir.X + p.Y * dir.Y + p.Z * dir.Z;
                if (proj < minProj) { minProj = proj; minPt = p; }
                if (proj > maxProj) { maxProj = proj; maxPt = p; }
            }
            if (minPt.X.IsNotNaN())
                candidateSet.Add(minPt);
            if (maxPt.X.IsNotNaN())
                candidateSet.Add(maxPt);
        }

        var candidates = candidateSet.ToList();
        int m = candidates.Count;
        if (m < 2) return 0.0;

        // compute maximum squared distance among candidates (parallelized)
        double maxSq = 0.0;
        object sync = new object();
        Parallel.For(0, m, i =>
        {
            double localMax = 0.0;
            var pi = candidates[i];
            for (int j = i + 1; j < m; j++)
            {
                double d2 = pi.SquaredDistance(candidates[j]);
                if (d2 > localMax) localMax = d2;
            }
            if (localMax > 0)
            {
                lock (sync)
                {
                    if (localMax > maxSq) maxSq = localMax;
                }
            }
        });

        return Sqrt(maxSq);
    }
    // jaggedSph: PointSpherical[][]
    // nTheta, nPhi: resolução da grade (aumente para maior precisão)
    public static double MaxDistanceUsingAngularBinning(PointSpherical[][] jaggedSph, int nTheta = 90, int nPhi = 180)
    {
        // dicionário (iTheta, iPhi) -> índice do melhor ponto (maior raio)
        var bestInCell = new Dictionary<(int, int), PointSpherical>();

        // preenche células
        foreach (var row in jaggedSph)
        {
            if (row == null) continue;
            foreach (var p in row)
            {
                // normaliza ângulos por segurança
                double th = p.Theta;
                double ph = p.Phi;
                // clamp/normalize
                if (th < 0) th = 0; if (th >PI) th =PI;
                ph = ph % (2 *PI); if (ph < 0) ph += 2 *PI;

                int i = (int)Min(nTheta - 1,Floor(th /PI * nTheta));
                int j = (int)Min(nPhi - 1,Floor(ph / (2 *PI) * nPhi));

                var key = (i, j);
                if (!bestInCell.TryGetValue(key, out var current) || p.Radius > current.Radius)
                    bestInCell[key] = p;
            }
        }

        // Converta representantes para cartesiano
        var reps = new List<Point3D>(bestInCell.Count);
        foreach (var kv in bestInCell.Values)
        {
            var (x, y, z) = kv.ToCartesian();
            reps.Add(new Point3D(x, y, z));
        }

        int m = reps.Count;
        if (m < 2) return 0.0;

        // Força-bruta paralelo entre representantes (exato)
        double maxSq = 0.0;
        object sync = new object();

        Parallel.For(0, m, () => 0.0, (i, loopState, localMax) =>
        {
            var pi = reps[i];
            for (int j = i + 1; j < m; j++)
            {
                double d2 = pi.SquaredDistance(reps[j]);
                if (d2 > localMax) localMax = d2;
            }
            return localMax;
        },
        localMax =>
        {
            lock (sync) { if (localMax > maxSq) maxSq = localMax; }
        });

        return Sqrt(maxSq);
    }
}
