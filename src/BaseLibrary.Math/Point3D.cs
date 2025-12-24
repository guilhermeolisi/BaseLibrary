using static BaseLibrary.Math.MathMethods;
using static System.Math;

namespace BaseLibrary.Math;

public readonly record struct Point3D(double X, double Y, double Z)
{
    // o record struct implements IEquatable<Point3D> by default and toString, Equals, GetHashCode methods: https://chatgpt.com/c/69431683-053c-8332-8ef7-f15799bee693
    public Point3D(double[] arr) : this(arr[0], arr[1], arr[2])
    {
        if (arr.Length != 3)
            throw new ArgumentException("Array must have exactly 3 elements.");
    }
    public PointSpherical ToSpherical()
    {
        double radius = RadiusFromPointCartesian(X, Y, Z); // Sqrt(X * X + Y * Y + Z * Z);
        double theta = PolarFromPointCartesian(Z, radius); //Acos(Z / radius); // polar angle
        double phi = AzimuthFromPointCartesian(X, Y); //Atan2(Y, X); // azimuthal angle
        return new PointSpherical(radius, theta, phi);
    }
    public double[] PointToArray() => [X, Y, Z];

    public static Point3D operator +(Point3D a, Point3D b) => new Point3D(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
    public static Point3D operator -(Point3D a, Point3D b) => new Point3D(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
    public static Point3D operator *(Point3D a, double scalar) => new Point3D(a.X * scalar, a.Y * scalar, a.Z * scalar);
    public static Point3D operator /(Point3D a, double scalar) => new Point3D(a.X / scalar, a.Y / scalar, a.Z / scalar);
    public static Point3D operator *(double scalar, Point3D a) => new Point3D(a.X * scalar, a.Y * scalar, a.Z * scalar);
    public static Point3D operator /(double scalar, Point3D a) => new Point3D(a.X / scalar, a.Y / scalar, a.Z / scalar);
    public static double Dot(Point3D a, Point3D b)
    {
        return a.X * b.X + a.Y * b.Y + a.Z * b.Z;
    }
    public static Point3D Cross(Point3D a, Point3D b)
    {
        return new Point3D(
            Round(a.Y * b.Z - a.Z * b.Y, 14),
            Round(a.Z * b.X - a.X * b.Z, 14),
            Round(a.X * b.Y - a.Y * b.X, 14)
        );
    }
    public static double Length(Point3D v)
    {
        return Sqrt(v.X * v.X + v.Y * v.Y + v.Z * v.Z);
    }
    public static Point3D Normalize(Point3D v)
    {
        double n = Length(v);
        if (n < 1e-14)
            throw new InvalidOperationException("Vetor of length zero.");

        return new Point3D(v.X / n, v.Y / n, v.Z / n);
    }
    public static Point3D NormalFromAPlane3Points(Point3D a, Point3D b, Point3D c)
    {
        Point3D u = b - a;
        Point3D v = c - a;

        Point3D n = Cross(u, v); // não normalizado

        double len = Length(n);
        if (len < 1e-14)
            throw new ArgumentException("Os 3 pontos são colineares ou quase colineares; plano indefinido.");

        // Retorna unitário (recomendado)
        return new Point3D(n.X / len, n.Y / len, n.Z / len);
    }
    public static double DistanceOfAPointToAPlane(Point3D point, Point3D planePoint, Point3D planeNormal)
    {
        // planeNormal deve ser unitário
        Point3D diff = point - planePoint;
        return Dot(planeNormal, diff);
    }
    //public static Point3D ProjectPointOntoPlane(Point3D p, Point3D p0, Point3D nUnit)
    //{
    //    // d = n · (p - p0) (distância assinada)
    //    Point3D v = p - p0;
    //    double d = Dot(nUnit, v);

    //    // p_proj = p - n * d
    //    return p - nUnit * d;
    //}
    public static IEnumerable<Point2D> QuadratureForCurveLevel(Point3D A11, Point3D A12, Point3D A21, Point3D A22, double level)
    {
        // https://chatgpt.com/c/69420302-268c-8333-924d-9c65c31659f1

        double r11 = A11.Z;
        double r12 = A12.Z;
        double r21 = A21.Z;
        double r22 = A22.Z;

        //bool A11_above = r11 >= level - 1E-12;
        //bool A12_above = r12 >= level - 1E-12;
        //bool A21_above = r21 >= level - 1E-12;
        //bool A22_above = r22 >= level - 1E-12;

        ////TUDO abaixo ou TUDO acima
        //if ((A11_above && A22_above && A21_above && A12_above) || (!A11_above && !A22_above && !A21_above && !A12_above))
        //{
        //    return Array.Empty<Point2D>();
        //}
        //if ((A11_above == A12_above) && (A12_above == A22_above) && (A22_above == A21_above) && (A21_above == A11_above))
        //{
        //    // Nenhum ponto de interseção
        //    return Array.Empty<Point2D>();
        //}

        //List<Point2D> intersectionPoints = new List<Point2D>();

        //// Verifica cada aresta do quadrilátero
        //if (A11_above != A12_above)
        //{
        //    double t = (level - r11) / (r12 - r11);
        //    double x = A11.X + t * (A12.X - A11.X);
        //    double y = A11.Y + t * (A12.Y - A11.Y);
        //    intersectionPoints.Add(new Point2D(x, y));
        //}
        //if (A12_above != A22_above)
        //{
        //    double t = (level - r12) / (r22 - r12);
        //    double x = A12.X + t * (A22.X - A12.X);
        //    double y = A12.Y + t * (A22.Y - A12.Y);
        //    intersectionPoints.Add(new Point2D(x, y));
        //}
        //if (A21_above != A22_above)
        //{
        //    double t = (level - r21) / (r22 - r21);
        //    double x = A21.X + t * (A22.X - A21.X);
        //    double y = A21.Y + t * (A22.Y - A21.Y);
        //    intersectionPoints.Add(new Point2D(x, y));
        //}
        //if (A11_above != A21_above)
        //{
        //    double t = (level - r11) / (r21 - r11);
        //    double x = A11.X + t * (A21.X - A11.X);
        //    double y = A11.Y + t * (A21.Y - A11.Y);
        //    intersectionPoints.Add(new Point2D(x, y));
        //}

        double eps = 1E-10;

        double g11 = SnapToLevel(r11 - level, eps);
        double g12 = SnapToLevel(r12 - level, eps);
        double g22 = SnapToLevel(r22 - level, eps);
        double g21 = SnapToLevel(r21 - level, eps);

        bool hitA11A12 = (g11 >= 0 && g12 < 0) || (g11 < 0 && g12 >= 0) || g11 == 0 || g12 == 0;
        bool hitA12A22 = (g12 >= 0 && g22 < 0) || (g12 < 0 && g22 >= 0) || g12 == 0 || g22 == 0;
        bool hitA22A21 = (g22 >= 0 && g21 < 0) || (g22 < 0 && g21 >= 0) || g22 == 0 || g21 == 0;
        bool hitA21A11 = (g21 >= 0 && g11 < 0) || (g21 < 0 && g11 >= 0) || g21 == 0 || g11 == 0;

        List<Point2D> intersectionPoints = new List<Point2D>();

        //Avalia se tem arestas inteiras. Deve ser um casor raríssimo
        if (g11 == 0 && g12 == 0)
        {
            intersectionPoints.Add(new Point2D(A11.X, A11.Y));
            intersectionPoints.Add(new Point2D(A12.X, A12.Y));
        }
        if (g12 == 0 && g22 == 0)
        {
            intersectionPoints.Add(new Point2D(A12.X, A12.Y));
            intersectionPoints.Add(new Point2D(A22.X, A22.Y));
        }
        if (g22 == 0 && g21 == 0)
        {
            intersectionPoints.Add(new Point2D(A22.X, A22.Y));
            intersectionPoints.Add(new Point2D(A21.X, A21.Y));
        }
        if (g21 == 0 && g11 == 0)
        {
            intersectionPoints.Add(new Point2D(A21.X, A21.Y));
            intersectionPoints.Add(new Point2D(A11.X, A11.Y));
        }
        //TUDO abaixo ou TUDO acima
        if ((hitA11A12 == hitA12A22 == hitA22A21 == hitA21A11) != ((g11 >= 0 && g22 >= 0 && g21 >= 0 && g12 >= 0) || (g11 <= 0 && g22 <= 0 && g21 <= 0 && g12 <= 0)))
        {
            //Não deve usar a conferencia do hit, pq tem o caso em que o usou o segmento inteiro
        }
        if ((g11 >= 0 && g22 >= 0 && g21 >= 0 && g12 >= 0) || (g11 <= 0 && g22 <= 0 && g21 <= 0 && g12 <= 0))
        {
            return intersectionPoints.ToArray();
        }

        // 0101 ou 1010 - casos ambíguos
        bool isAmbiguous = (g11 < 0 && g12 >= 0 && g22 < 0 && g21 >= 0) || (g11 >= 0 && g12 < 0 && g22 >= 0 && g21 < 0);

        if (!isAmbiguous)
        {
            // Verifica cada aresta do quadrilátero
            //if (g11 > 0 && g12 < 0 || g11 < 0 && g12 > 0 || g11 == 0 && g12 != 0 || g11 != 0 && g12 == 0)
            if ((g11 > 0 && g12 < 0 || g11 < 0 && g12 > 0 || g11 == 0 && g12 != 0 || g11 != 0 && g12 == 0) != hitA11A12)
            {

            }
            if (hitA11A12)
            {
                double t = (level - r11) / (r12 - r11);
                double x = A11.X + t * (A12.X - A11.X);
                double y = A11.Y + t * (A12.Y - A11.Y);
                AddUnique(intersectionPoints, new Point2D(x, y), eps);
                //intersectionPoints.Add(new Point2D(x, y));
            }
            //if (g12 > 0 && g22 < 0 || g12 < 0 && g22 > 0 || g12 == 0 && g22 != 0 || g12 != 0 && g22 == 0)
            if ((g12 > 0 && g22 < 0 || g12 < 0 && g22 > 0 || g12 == 0 && g22 != 0 || g12 != 0 && g22 == 0) != hitA12A22)
            {

            }
            if (hitA12A22)
            {
                double t = (level - r12) / (r22 - r12);
                double x = A12.X + t * (A22.X - A12.X);
                double y = A12.Y + t * (A22.Y - A12.Y);
                AddUnique(intersectionPoints, new Point2D(x, y), eps);
                //intersectionPoints.Add(new Point2D(x, y));
            }
            //if (g22 > 0 && g21 < 0 || g22 < 0 && g21 > 0 || g22 == 0 && g21 != 0 || g22 != 0 && g21 == 0)
            if ((g22 > 0 && g21 < 0 || g22 < 0 && g21 > 0 || g22 == 0 && g21 != 0 || g22 != 0 && g21 == 0) != (hitA22A21))
            {

            }
            if (hitA22A21)
            {
                double t = (level - r21) / (r22 - r21);
                double x = A21.X + t * (A22.X - A21.X);
                double y = A21.Y + t * (A22.Y - A21.Y);
                AddUnique(intersectionPoints, new Point2D(x, y), eps);
                //intersectionPoints.Add(new Point2D(x, y));
            }
            //if (g21 > 0 && g11 < 0 || g21 < 0 && g11 > 0 || g21 == 0 && g11 != 0 || g21 != 0 && g11 == 0)
            if ((g21 > 0 && g11 < 0 || g21 < 0 && g11 > 0 || g21 == 0 && g11 != 0 || g21 != 0 && g11 == 0) != hitA21A11)
            {

            }
            if (hitA21A11)
            {
                double t = (level - r11) / (r21 - r11);
                double x = A11.X + t * (A21.X - A11.X);
                double y = A11.Y + t * (A21.Y - A11.Y);
                AddUnique(intersectionPoints, new Point2D(x, y), eps);
                //intersectionPoints.Add(new Point2D(x, y));
            }
        }
        else
        {
            // Ambíguo: há 4 interseções pontuais (em geral) e existem 2 emparelhamentos possíveis.
            // Nielson–Hamann (Asymptotic Decider):
            // Decide conectividade usando valor do campo no centro (bilinear aproximado).
            double fCenter = 0.25 * (A11.Z + A12.Z + A22.Z + A21.Z);

            // Coleta os pontos de interseção pontuais em cada aresta (devem existir no ambíguo)
            // (Se algum faltar por degenerado, caímos num pareamento por proximidade no final.)
            Point2D? eA11A12 = hitA11A12 && g11 != 0 && g12 != 0 ? Interpolation(A11, A12, level) : null;
            Point2D? eA21A22 = hitA12A22 && g12 != 0 && g22 != 0 ? Interpolation(A12, A22, level) : null;
            Point2D? eA22A21 = hitA22A21 && g22 != 0 && g21 != 0 ? Interpolation(A22, A21, level) : null;
            Point2D? eA21A11 = hitA21A11 && g21 != 0 && g11 != 0 ? Interpolation(A21, A11, level) : null;

            // Dois emparelhamentos possíveis (considerando arestas em ciclo):
            // Pairing 1: (AB—BC) e (CD—DA)
            // Pairing 2: (AB—DA) e (BC—CD)
            // O decider escolhe qual par representa o “lado” correto do bilinear.

            bool choosePairing1 = (fCenter >= level);

            // Para mask==0x5 e mask==0xA, a escolha pode ser invertida dependendo da convenção de bits;
            // este ajuste evita "flip" inesperado:
            // Uma forma prática: se diagonais (A,C) estão "acima" e (B,D) abaixo, use o critério direto.
            // Caso contrário, inverte.
            bool diagA11A22Above = g11 >= 0 && g12 < 0 && g22 >= 0 && g21 == 0;
            bool diagA12A21Above = g11 < 0 && g12 >= 0 && g22 < 0 && g21 == 0;

            if (diagA12A21Above) choosePairing1 = !choosePairing1;

            // Se faltou alguma interseção por degenerado, faça pareamento por proximidade como fallback
            if (eA11A12 is null || eA21A22 is null || eA22A21 is null || eA21A11 is null)
            {
                var pts = new List<Point2D>(4);
                if (eA11A12 is not null) AddUnique(pts, eA11A12.Value, eps);
                if (eA21A22 is not null) AddUnique(pts, eA21A22.Value, eps);
                if (eA22A21 is not null) AddUnique(pts, eA22A21.Value, eps);
                if (eA21A11 is not null) AddUnique(pts, eA21A11.Value, eps);

                // adiciona o que tiver por degenerate: emparelha por distância
                if (pts.Count >= 2)
                {
                    // Emparelhamento mínimo: pega o par mais próximo e depois o restante
                    var used = new bool[pts.Count];
                    int i0 = -1, i1 = -1;
                    double best = double.PositiveInfinity;

                    for (int i = 0; i < pts.Count; i++)
                        for (int j = i + 1; j < pts.Count; j++)
                        {
                            double dx = pts[i].X - pts[j].X;
                            double dy = pts[i].Y - pts[j].Y;
                            double d2 = dx * dx + dy * dy;
                            if (d2 < best) { best = d2; i0 = i; i1 = j; }
                        }

                    intersectionPoints.Add(pts[i0]);
                    intersectionPoints.Add(pts[i1]);
                    used[i0] = used[i1] = true;

                    // segundo par, se existir
                    int r0 = -1, r1 = -1;
                    for (int i = 0; i < pts.Count; i++) if (!used[i]) { if (r0 < 0) r0 = i; else { r1 = i; break; } }
                    if (r0 >= 0 && r1 >= 0)
                    {
                        intersectionPoints.Add(pts[r0]);
                        intersectionPoints.Add(pts[r1]);
                    }
                }
            }
        }
#if DEBUG
        if (intersectionPoints.Count > 2)
        {

        }
#endif
        return intersectionPoints;
    }
    private static Point2D Interpolation(Point3D p0, Point3D p1, double level)
    {
        double t = (level - p0.Z) / (p1.Z - p0.Z);
        double x = p0.X + t * (p1.X - p0.X);
        double y = p0.Y + t * (p1.Y - p0.Y);
        return new Point2D(x, y);

    }
    private static double SnapToLevel(double vMinusLevel, double eps)
        => Abs(vMinusLevel) <= eps ? 0.0 : vMinusLevel;
    private static void AddUnique(List<Point2D> pts, Point2D p, double eps)
    {
        for (int i = 0; i < pts.Count; i++)
            if (SamePoint(pts[i], p, eps))
                return;
        pts.Add(p);
    }
    // Comparação com tolerância (para pontos 2D)
    public static bool SamePoint(Point2D a, Point2D b, double eps)
    {
        double dx = a.X - b.X;
        double dy = a.Y - b.Y;
        return (dx * dx + dy * dy) <= (eps * eps);
    }

}
