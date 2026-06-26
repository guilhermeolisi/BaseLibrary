using BaseLibrary;
using FluentAssertions;

namespace BaseLibrary.Tests;

public class TelemetryGateTests
{
    private readonly TelemetryGate sut = new();

    [Theory]
    [InlineData("1")]
    [InlineData("true")]
    [InlineData("TRUE")]
    [InlineData("yes")]
    [InlineData("on")]
    public void IsExportAllowed_ShouldReturnFalse_WhenSindarinOptOutEnvVarIsTruthy(string value)
    {
        RunWithEnv("SINDARIN_TELEMETRY_OPTOUT", value, () =>
            sut.IsExportAllowed().Should().BeFalse("the env-var opt-out must suppress telemetry regardless of region"));
    }

    [Fact]
    public void IsExportAllowed_ShouldReturnFalse_WhenNimlothOptOutEnvVarIsSet()
    {
        RunWithEnv("NIMLOTH_TELEMETRY_OPTOUT", "1", () =>
            sut.IsExportAllowed().Should().BeFalse());
    }

    [Theory]
    [InlineData("0")]
    [InlineData("false")]
    [InlineData("")]
    [InlineData("   ")]
    public void IsExportAllowed_ShouldNotSuppressViaEnv_WhenValueIsNotTruthy(string value)
    {
        // Valor não-truthy ⇒ o opt-out por env NÃO é acionado; o resultado passa a depender
        // apenas da região da máquina (não determinística no teste), então garantimos só que
        // a avaliação não lança.
        RunWithEnv("SINDARIN_TELEMETRY_OPTOUT", value, () =>
        {
            Action act = () => sut.IsExportAllowed();
            act.Should().NotThrow();
        });
    }

    private static void RunWithEnv(string name, string? value, Action assert)
    {
        string? sindarinPrev = Environment.GetEnvironmentVariable("SINDARIN_TELEMETRY_OPTOUT");
        string? nimlothPrev = Environment.GetEnvironmentVariable("NIMLOTH_TELEMETRY_OPTOUT");
        try
        {
            // Isola o caso sob teste limpando ambos os env vars antes de aplicar o valor.
            Environment.SetEnvironmentVariable("SINDARIN_TELEMETRY_OPTOUT", null);
            Environment.SetEnvironmentVariable("NIMLOTH_TELEMETRY_OPTOUT", null);
            Environment.SetEnvironmentVariable(name, value);
            assert();
        }
        finally
        {
            Environment.SetEnvironmentVariable("SINDARIN_TELEMETRY_OPTOUT", sindarinPrev);
            Environment.SetEnvironmentVariable("NIMLOTH_TELEMETRY_OPTOUT", nimlothPrev);
        }
    }
}
