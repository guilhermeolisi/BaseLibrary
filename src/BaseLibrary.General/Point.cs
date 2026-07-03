// Onda 2.5 STEP 2(d): realocado de Sindarin/src/Basic/Point.cs para ca. Mantem o namespace Sindarin
// (o tipo continua Sindarin.Point) para nao afetar nenhum consumidor. Como o Sindarin.Basic ja
// referencia BaseLibrary.General, o Point segue transitivamente disponivel a toda a solucao Sindarin,
// e o projeto Nodes (que referencia BaseLibrary.General direto) tambem o alcanca sem passar pelo
// Sindarin.Basic. POCO puro, sem dependencias.
namespace Sindarin;

public class Point
{
    public double X { get; set; }
    public double Y { get; set; }
}
