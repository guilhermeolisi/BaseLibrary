using BaseLibrary.Numbers;
using static System.Math;

namespace BaseLibrary;

//Código gerado pelo ChatGPT
public class ColorServices : IColorServices
{
    /// <summary>
    /// Converte uma cor RGB para HSL
    /// </summary>
    /// <param name="r"></param>
    /// <param name="g"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    public (double, double, double) RgbToHsl(int r, int g, int b)
    {

        double rd = r / 255.0;
        double gd = g / 255.0;
        double bd = b / 255.0;

        double max = Math.Max(rd, Math.Max(gd, bd));
        double min = Math.Min(rd, Math.Min(gd, bd));
        double delta = max - min;

        // Luminosidade
        double l = (max + min) / 2.0;

        double h = 0;
        double s = 0;

        if (delta != 0)
        {
            // Saturação
            s = l < 0.5
                ? delta / (max + min)
                : delta / (2.0 - max - min);

            // Matiz
            if (max == rd)
                h = ((gd - bd) / delta + (gd < bd ? 6 : 0));
            else if (max == gd)
                h = ((bd - rd) / delta + 2);
            else // max == bd
                h = ((rd - gd) / delta + 4);

            h *= 60.0; // converte de fração para graus
        }

        return (h, s, l);
    }

    /// <summary>
    /// Converte uma cor HSL para RGB
    /// </summary>
    /// <param name="h"></param>
    /// <param name="s"></param>
    /// <param name="l"></param>
    /// <returns></returns>
    public (int, int, int) HslToRgb(double h, double s, double l)
    {
        double rd, gd, bd;

        if (s == 0)
        {
            // tons de cinza
            rd = gd = bd = l;
        }
        else
        {
            double hue = h / 360.0; // normaliza para [0,1]

            double q = l < 0.5
                ? l * (1 + s)
                : l + s - l * s;
            double p = 2 * l - q;

            rd = HueToRgb(p, q, hue + 1.0 / 3.0);
            gd = HueToRgb(p, q, hue);
            bd = HueToRgb(p, q, hue - 1.0 / 3.0);
        }

        // Arredonda ao invés de truncar
        int R = (int)Math.Round(rd * 255);
        int G = (int)Math.Round(gd * 255);
        int B = (int)Math.Round(bd * 255);

        return (Clamp(R, 0, 255), Clamp(G, 0, 255), Clamp(B, 0, 255));
    }
    // Função auxiliar correta, com limiares 1/6, 1/2 e 2/3
    private double HueToRgb(double p, double q, double t)
    {
        if (t < 0) t += 1;
        if (t > 1) t -= 1;
        if (t < 1.0 / 6.0) return p + (q - p) * 6 * t;
        if (t < 1.0 / 2.0) return q;
        if (t < 2.0 / 3.0) return p + (q - p) * (2.0 / 3.0 - t) * 6;
        return p;
    }
    /// <summary>
    /// Método para escurecer uma cor
    /// </summary>
    /// <param name="r"></param>
    /// <param name="g"></param>
    /// <param name="b"></param>
    /// <param name="factorChange"></param>
    /// <returns></returns>
    public (int, int, int) RGBDarkenColor(int r, int g, int b, double factorChange)
    {
        var (h, s, l) = RgbToHsl(r, g, b);
        l *= factorChange; // reduzir a luminosidade
        return HslToRgb(h, s, l);
    }

    /// <summary>
    /// Método para clarear uma cor
    /// </summary>
    /// <param name="r"></param>
    /// <param name="g"></param>
    /// <param name="b"></param>
    /// <param name="factorChange"></param>
    /// <returns></returns>
    public (int, int, int) RGBLightenColor(int r, int g, int b, double factorChange)
    {
        var (h, s, l) = RgbToHsl(r, g, b);
        l += (1 - l) * factorChange; // aumentar a luminosidade
        return HslToRgb(h, s, l);
    }
    public (int, int, int) RGBLightnessChange(int r, int g, int b, double factorChange)
    {
        var (h, s, l) = RgbToHsl(r, g, b);
#if DEBUG
        var (r2, g2, b2) = HslToRgb(h, s, l);

        if (Abs(r - r2) > 1 || Abs(g - g2) > 1 || Abs(b - b2) > 1)
        {

        }
#endif


        if (factorChange < 0)
            l *= (1 - Abs(factorChange)); // reduzir a luminosidade
        else
            l += (1 - l) * factorChange; // aumentar a luminosidade
        return HslToRgb(h, s, l);
    }
    public (int, int, int) RGBLightnessChange(int r, int g, int b, int cycle, bool isDark)
    {
        double factor;
        if (cycle.IsOdd())
        {
            factor = cycle / 2 + 1;
        }
        else
        {
            factor = cycle / 2;
        }
        factor *= 0.2 * (isDark ? -1 : 1); // a mudança de luminosidade é proporcional ao ciclo com 20% de intensidade para cada ciclo

        if (cycle.IsEven())
        {
            factor *= -1;
        }

        return RGBLightnessChange(r, g, b, factor);
    }
}
