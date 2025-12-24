namespace BaseLibrary.Math;

public readonly struct Projection2DLine
{
    public Point2D[] Points { get; }
    public double Level { get; }

    public Projection2DLine(double level, Point2D[] points)
    {
        Points = points;
        Level = level;
    }
}
