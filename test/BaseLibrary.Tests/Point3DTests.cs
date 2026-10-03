using FluentAssertions;
using BaseLibrary.Math;

namespace BaseLibrary.Tests;

/// <summary>
/// Valida <see cref="Point3D.QuadratureForCurveLevel"/>, o corte de uma celula da grade por um nivel.
/// </summary>
public class Point3DTests
{
    [Fact]
    public void QuadratureForCurveLevel_ShouldKeepPointsInsideTheCell_WhenOnlyOneEndOfAnEdgeIsSnappedToTheLevel()
    {
        // Arrange: a celula medida na E086 (A020), esfera de raio 100 no plano (0 1 0), nivel zero. O canto A12 tem
        // Z = -7,1e-11 e cai no snap de 1e-10 (vira o nivel); o A22, logo abaixo, tem Z = -1,07e-10 e nao cai. Na
        // aresta A12-A22 o t = (nivel - Z12) / (Z22 - Z12) dava -2, e o ponto saia extrapolado em (0, 100,37).
        Point3D a11 = new(-6.971397998532793, 99.75640502598084, 0.24344658247154327);
        Point3D a12 = new(-6.975647374435157, 99.75640502598084, -7.119920458011398E-11);
        Point3D a21 = new(-10.446478735247034, 99.45218953682378, 0.36479907580843557);
        Point3D a22 = new(-10.452846326799182, 99.45218953682378, -1.0669036207216986E-10);

        // Act
        Point2D[] points = Point3D.QuadratureForCurveLevel(a11, a12, a21, a22, 0).ToArray();

        // Assert: todo ponto fica dentro da caixa da celula (X entre -10,453 e -6,971; Y entre 99,452 e 99,756).
        points.Should().NotBeEmpty();
        foreach (Point2D p in points)
        {
            p.X.Should().BeInRange(a22.X, a11.X, "o corte de uma aresta fica entre os cantos dela");
            p.Y.Should().BeInRange(a21.Y, a11.Y, "o corte de uma aresta fica entre os cantos dela");
        }
    }

    [Fact]
    public void QuadratureForCurveLevel_ShouldInterpolateLinearly_WhenTheEdgeCrossesTheLevel()
    {
        // Arrange: controle sem snap, cantos de 1 a -1 em X e em Y; o nivel zero corta as arestas no meio.
        Point3D a11 = new(0, 0, 1);
        Point3D a12 = new(1, 0, -1);
        Point3D a21 = new(0, 1, 1);
        Point3D a22 = new(1, 1, -1);

        // Act
        Point2D[] points = Point3D.QuadratureForCurveLevel(a11, a12, a21, a22, 0).ToArray();

        // Assert
        points.Should().BeEquivalentTo([new Point2D(0.5, 0), new Point2D(0.5, 1)]);
    }
}
