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

        double h = 0;
        double s = 0;
        double l = (max + min) / 2.0;

        if (max != min)
        {
            double d = max - min;
            s = l > 0.5 ? d / (2.0 - max - min) : d / (max + min);

            if (max == rd)
            {
                h = (gd - bd) / d + (gd < bd ? 6 : 0);
            }
            else if (max == gd)
            {
                h = (bd - rd) / d + 2;
            }
            else if (max == bd)
            {
                h = (rd - gd) / d + 4;
            }

            h /= 6.0;
        }

        return (h * 360, s, l);
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
        double r, g, b;

        if (s == 0)
        {
            r = g = b = l; // cor acromática (cinza)
        }
        else
        {
            Func<double, double, double, double> hueToRgb = (p, q, t) =>
            {
                if (t < 0) t += 1;
                if (t > 1) t -= 1;
                if (t < 1 / 6.0) return p + (q - p) * 6 * t;
                if (t < 1 / 3.0) return q;
                if (t < 1 / 2.0) return p + (q - p) * (2 / 3.0 - t) * 6;
                return p;
            };

            double q = l < 0.5 ? l * (1 + s) : l + s - l * s;
            double p = 2 * l - q;
            r = hueToRgb(p, q, h + 1 / 3.0);
            g = hueToRgb(p, q, h);
            b = hueToRgb(p, q, h - 1 / 3.0);
        }

        return ((int)(r * 255), (int)(g * 255), (int)(b * 255));
    }

    /// <summary>
    /// Método para escurecer uma cor
    /// </summary>
    /// <param name="r"></param>
    /// <param name="g"></param>
    /// <param name="b"></param>
    /// <param name="factorChange"></param>
    /// <returns></returns>
    public (int, int, int) DarkenColor(int r, int g, int b, double factorChange)
    {
        var (h, s, l) = RgbToHsl(r, g, b);
        l *= (1 + factorChange); // reduzir a luminosidade
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
    public (int, int, int) LightenColor(int r, int g, int b, double factorChange)
    {
        var (h, s, l) = RgbToHsl(r, g, b);
        l += (1 - l) * (1 - factorChange); // aumentar a luminosidade
        return HslToRgb(h, s, l);
    }
}
