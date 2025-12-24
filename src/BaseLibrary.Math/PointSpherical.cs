using static BaseLibrary.Math.MathMethods;

namespace BaseLibrary.Math;

public readonly record struct PointSpherical(double Radius, double Theta, double Phi)
{
    // o record struct implements IEquatable<Point3D> by default and toString, Equals, GetHashCode methods: https://chatgpt.com/c/69431683-053c-8332-8ef7-f15799bee693
    public Point3D ToCartesian()
    {
        double x = XFromPointSpherical(Radius, Theta, Phi);
        double y = YFromPointSpherical(Radius, Theta, Phi);
        double z = ZFromPointSpherical(Radius, Theta);
        return new Point3D(x, y, z);
    }
}
