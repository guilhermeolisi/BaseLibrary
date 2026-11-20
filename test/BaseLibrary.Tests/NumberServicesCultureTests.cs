using BaseLibrary.Numbers;
using FluentAssertions;
using System.Globalization;

namespace BaseLibrary.Tests;

/// <summary>
/// O texto de valor com esd nao depende da cultura da thread (E318, "quero que sempre use a cultura invariante
/// internacional"): antes, sob pt-BR, o mesmo resultado saia "1,2346(12)", e o motor relia com virgula.
/// </summary>
public class NumberServicesCultureTests
{
    public static TheoryData<double, double, string?> Cases => new()
    {
        { 1.23456, 0.0012, null },          // esd < 1: F com as casas do esd
        { 1.23456, 0.0012, "F2" },
        { 12345.678, 3.2, null },           // esd >= 1
        { 0.000123456, 0.0000012, null },   // notacao cientifica
        { 98765432.1, 1234.0, null },       // esd grande
        { 1234.5678, double.NaN, "F1" },    // sem esd, com formato
        { 1234.5678, 0, null },             // sem esd, sem formato
        { -0.5, 0.25, null },
    };

    [Theory]
    [MemberData(nameof(Cases))]
    public void DoubleResultText_ShouldNotDependOnTheThreadCulture(double value, double esd, string? format)
    {
        // Oraculo: caso limite, a cultura da thread nao muda o texto (o mesmo da thread invariante).
        var services = new NumberServices();
        string invariant = RunUnder(CultureInfo.InvariantCulture, () => services.DoubleResultText(value, esd, format));

        string ptBr = RunUnder(CultureInfo.GetCultureInfo("pt-BR"), () => services.DoubleResultText(value, esd, format));
        string deDe = RunUnder(CultureInfo.GetCultureInfo("de-DE"), () => services.DoubleResultText(value, esd, format));

        ptBr.Should().Be(invariant);
        deDe.Should().Be(invariant);
        invariant.Should().NotContain(",", "o separador decimal e o ponto");
    }

    [Fact]
    public void DoubleResultText_ShouldUseThePointAsDecimalSeparator_UnderPtBr()
    {
        // Oraculo: a mao, o esd 0.0012 arredonda a um algarismo (0.001, RoundAlgharisms(esd, 1)), entao o valor sai com
        // tres casas (1.23456 -> 1.235) e o algarismo do esd entre parenteses.
        var services = new NumberServices();

        string text = RunUnder(CultureInfo.GetCultureInfo("pt-BR"), () => services.DoubleResultText(1.23456, 0.0012));

        text.Should().Be("1.235(1)");
    }

    private static string RunUnder(CultureInfo culture, Func<string> action)
    {
        string result = string.Empty;
        var thread = new Thread(() =>
        {
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;
            result = action();
        });
        thread.Start();
        thread.Join();
        return result;
    }
}
