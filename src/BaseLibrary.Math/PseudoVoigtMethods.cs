using static System.Math;

namespace BaseLibrary.Math;

/// <summary>
/// Relacoes basicas da pseudo-Voigt (aproximacao de Thompson-Cox-Hastings): conversoes entre os FWHM
/// das componentes Gaussiana/Lorentziana, o FWHM/eta da pseudo-Voigt e o alargamento integrado (beta).
/// Casa canonica UNICA (Onda 2.5): consumida pelos nos de expressao (Sindarin.Math.Nodes) e pelo engine
/// (via os aliases em Sindarin.Math.Constants.cH/cEta/cEtaG/cEtaL, que apontam para os arrays daqui).
/// </summary>
public static class PseudoVoigtMethods
{
    /// <summary>Coeficientes de conversao HG/HL/HGL -> H (TCH).</summary>
    public static readonly double[] cH = { 1, 2.69269, 2.42843, 4.47163, 0.07842, 1 };
    /// <summary>Coeficientes de eta a partir de HL/H (TCH).</summary>
    public static readonly double[] cEta = { 1.36603, -0.47719, 0.11116 };
    /// <summary>Coeficientes de HL a partir de H/eta.</summary>
    public static readonly double[] cEtaL = { 0.72928, 0.19289, 0.07783 };
    /// <summary>Coeficientes de HG a partir de H/eta.</summary>
    public static readonly double[] cEtaG = { 1, -0.74417, -0.24781, -0.00810 };

    public static (double fwhm, double eta) psVoigtFWHM(double fwhmGauss, double fwhmLorentz)
    {
        double fwhm = 0;
        double eta = 0;
        for (int i = 0; i < cH.Length; i++)
            fwhm += cH[i] * Pow(fwhmGauss, 5 - i) * Pow(fwhmLorentz, i);
        fwhm = Pow(fwhm, 1.0 / 5.0);
        for (int i = 0; i < cEta.Length; i++)
            eta += cEta[i] * Pow(fwhmLorentz / fwhm, i + 1);

        return (fwhm, eta);
    }
    public static double psVoigtBeta(double fwhmGauss, double fwhmLorentz)
    {
        double fwhm;
        double eta;
        (fwhm, eta) = psVoigtFWHM(fwhmGauss, fwhmLorentz);
        return PI * fwhm / (2 * eta + 2 * (1 - eta) * Sqrt(PI * Log(2.0)));
    }
    public static double psVoigtHG(double fwhm, double eta) => fwhm * Sqrt(cEtaG[0] + cEtaG[1] * eta + cEtaG[2] * Pow(eta, 2.0) + cEtaG[3] * Pow(eta, 3.0));
    public static double psVoigtHL(double fwhm, double eta) => fwhm * (cEtaL[0] * eta + cEtaL[1] * Pow(eta, 2.0) + cEtaL[2] * Pow(eta, 3.0));
    public static double psVoigtHBeta(double beta, double eta) => beta * (2 * eta + 2 * (1 - eta) * Sqrt(PI * Log(2.0))) / PI;
}
