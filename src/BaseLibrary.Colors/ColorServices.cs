using BaseLibrary.Numbers;
using static System.Math;

namespace BaseLibrary;

//Código gerado pelo ChatGPT
public class ColorServices : IColorServices
{
    INumberServices numberServices;
    public ColorServices()
    {
        numberServices = new NumberServices();
    }
    public ColorServices(INumberServices numberServices)
    {
        this.numberServices = numberServices;
    }
    public (byte r, byte g, byte b) HexToRGB(string hex)
    {
        if (string.IsNullOrWhiteSpace(hex) || hex.Length != 7 || hex[0] != '#')
            throw new ArgumentException("Hexadecimal color must be in the format #RRGGBB");
        byte r = Convert.ToByte(hex.Substring(1, 2), 16);
        byte g = Convert.ToByte(hex.Substring(3, 2), 16);
        byte b = Convert.ToByte(hex.Substring(5, 2), 16);
        return (r, g, b);
    }
    public string RGBToHex(byte r, byte g, byte b)
    {
        return $"#{r:X2}{g:X2}{b:X2}";
    }
    public (double, double, double) RGBToHSV(byte r, byte g, byte b)
    {
        double rd = r / 255.0;
        double gd = g / 255.0;
        double bd = b / 255.0;
        double max = Math.Max(rd, Math.Max(gd, bd));
        double min = Math.Min(rd, Math.Min(gd, bd));
        double delta = max - min;
        // Valor
        double v = max;
        // Saturação
        double s = (max == 0) ? 0 : delta / max;
        // Matiz
        double h = 0;
        if (delta != 0)
        {
            if (max == rd)
                h = (gd - bd) / delta + (gd < bd ? 6 : 0);
            else if (max == gd)
                h = (bd - rd) / delta + 2;
            else // max == bd
                h = (rd - gd) / delta + 4;
            h /= 6; // normaliza para [0,1]
        }
        return (h * 360.0, s, v); // converte h para graus
    }
    public (byte, byte, byte) HSVToRGB(double h, double s, double v)
    {
        double rd, gd, bd;
        if (s == 0)
        {
            // tons de cinza
            rd = gd = bd = v;
        }
        else
        {
            double hue = h / 360.0; // normaliza para [0,1]
            int i = (int)(hue * 6);
            double f = hue * 6 - i;
            double p = v * (1 - s);
            double q = v * (1 - f * s);
            double t = v * (1 - (1 - f) * s);
            i %= 6;
            switch (i)
            {
                case 0: rd = v; gd = t; bd = p; break;
                case 1: rd = q; gd = v; bd = p; break;
                case 2: rd = p; gd = v; bd = t; break;
                case 3: rd = p; gd = q; bd = v; break;
                case 4: rd = t; gd = p; bd = v; break;
                case 5: rd = v; gd = p; bd = q; break;
                default: throw new ArgumentOutOfRangeException();
            }
        }
        // Arredonda ao invés de truncar
        int R = (int)Math.Round(rd * 255);
        int G = (int)Math.Round(gd * 255);
        int B = (int)Math.Round(bd * 255);
        return ((byte)Clamp(R, 0, 255), (byte)Clamp(G, 0, 255), (byte)Clamp(B, 0, 255));
    }
    /// <summary>
    /// Converte uma cor RGB para HSL
    /// </summary>
    /// <param name="r"></param>
    /// <param name="g"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    public (double, double, double) RGBToHSL(byte r, byte g, byte b)
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
    public (byte, byte, byte) HSLToRGB(double h, double s, double l)
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

            rd = HueToRGB(p, q, hue + 1.0 / 3.0);
            gd = HueToRGB(p, q, hue);
            bd = HueToRGB(p, q, hue - 1.0 / 3.0);
        }

        // Arredonda ao invés de truncar
        int R = (int)Math.Round(rd * 255);
        int G = (int)Math.Round(gd * 255);
        int B = (int)Math.Round(bd * 255);

        return ((byte)Clamp(R, 0, 255), (byte)Clamp(G, 0, 255), (byte)Clamp(B, 0, 255));
    }
    // Função auxiliar correta, com limiares 1/6, 1/2 e 2/3
    private double HueToRGB(double p, double q, double t)
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
    public (byte, byte, byte) RGBDarkenColor(byte r, byte g, byte b, double factorChange)
    {
        var (h, s, l) = RGBToHSL(r, g, b);
        l *= factorChange; // reduzir a luminosidade
        return HSLToRGB(h, s, l);
    }

    /// <summary>
    /// Método para clarear uma cor
    /// </summary>
    /// <param name="r"></param>
    /// <param name="g"></param>
    /// <param name="b"></param>
    /// <param name="factorChange"></param>
    /// <returns></returns>
    public (byte, byte, byte) RGBLightenColor(byte r, byte g, byte b, double factorChange)
    {
        var (h, s, l) = RGBToHSL(r, g, b);
        l += (1 - l) * factorChange; // aumentar a luminosidade
        return HSLToRGB(h, s, l);
    }
    public (byte, byte, byte) RGBLightnessChange(byte r, byte g, byte b, double factorChange)
    {
        var (h, s, l) = RGBToHSL(r, g, b);
#if DEBUG
        var (r2, g2, b2) = HSLToRGB(h, s, l);

        if (Abs(r - r2) > 1 || Abs(g - g2) > 1 || Abs(b - b2) > 1)
        {

        }
#endif


        if (factorChange < 0)
            l *= (1 - Abs(factorChange)); // reduzir a luminosidade
        else
            l += (1 - l) * factorChange; // aumentar a luminosidade
        return HSLToRGB(h, s, l);
    }
    public (byte, byte, byte) RGBLightnessChange(byte r, byte g, byte b, int cycle, bool isDark)
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
    public (byte r, byte g, byte b) GenerateNewColor(byte[][] existingColorsRGB, int countMore)
    {
        if (countMore < 1)
            throw new ArgumentException("Count must be greater than 0");
        byte r = 0;
        byte g = 0;
        byte b = 0;
        for (int i = 0; i < countMore; i++)
        {
            (r, g, b) = GenerateNewColor(existingColorsRGB);
            existingColorsRGB = existingColorsRGB.Append([r, g, b]).ToArray();
        }
        return (r, g, b);
    }
    public (byte r, byte g, byte b) GenerateNewColor(byte[][] existingColorsRGB)
    {
        (double h, double s, double l)[] hslColors = existingColorsRGB.Select(x => RGBToHSL(x[0], x[1], x[2])).ToArray();

        double avgS = hslColors.Average(c => c.s);
        double avgL = hslColors.Average(c => c.l);

        int toMuchCount = 6; // número de cores que consideramos "muitas"

        // Ajuste adaptativo: se já temos muitas cores, variar saturação e luminosidade
        int count = existingColorsRGB.Length;
        double satVar = Max(Min(0.3, (count - toMuchCount) * 0.03), 0); // até ±30%, só altera se houver mais de toMuchCount cores
        double lightVar = Max(Min(0.25, (count - toMuchCount) * 0.025), 0); // até ±25%, só altera se houver mais de toMuchCount cores

        double bestScore = double.MinValue;
        double[] moreDistanceHue = new double[2];
        // Pegar o maior intervalo entre hues consecutivos
        var hslColorsSorted = hslColors.OrderBy(c => c.h).ToArray();
        for (int i = 0; i < hslColorsSorted.Length; i++)
        {
            // Calcula a distância entre as cores
            if (i != hslColorsSorted.Length - 1)
            {
                double dist = HueDistance(hslColorsSorted[i].h, hslColorsSorted[i + 1].h);
                if (dist > bestScore)
                {
                    bestScore = dist;
                    moreDistanceHue[0] = hslColorsSorted[i].h;
                    moreDistanceHue[1] = hslColorsSorted[i + 1].h;
                }
            }
            else
            {
                double dist = HueDistance(hslColorsSorted[i].h, hslColorsSorted[0].h);
                if (dist > bestScore)
                {
                    bestScore = dist;
                    moreDistanceHue[0] = hslColorsSorted[i].h;
                    moreDistanceHue[1] = hslColorsSorted[0].h;
                }
            }

        }
        double bestHue = Min(moreDistanceHue[0], moreDistanceHue[1]) + HueDistance(moreDistanceHue[0], moreDistanceHue[1]) / 2;

        // Garantir que hava mudança mínima de hue antes de mudar saturação e luminosidade
        if (bestScore > (360 / toMuchCount) * 1.2)
        {
            satVar = 0;
            lightVar = 0;
        }


        // Ajusta saturação e luminosidade para aumentar contraste em paletas grandes
        // Para garantir que tenha mudança de S e L esperada
        int mult = avgS + satVar < 1 || avgS - satVar > 0.4 ? numberServices.RandomOneOrMinusOne() :
            avgS + satVar < 1 ? +1 : -1;
        double s = Clamp(avgS + mult * satVar, 0.4, 1);
        mult = mult = avgL + lightVar < 1 || avgL - lightVar > 0.4 ? numberServices.RandomOneOrMinusOne() :
            avgL + lightVar < 1 ? +1 : -1;
        double l = Clamp(avgL + mult * lightVar, 0.3, 0.7);

        return HSLToRGB(bestHue, s, l);
    }
    public double HueDistance(double h1, double h2)
    {
        double diff = Abs(h1 - h2);
        if (diff > 180)
        {
            diff = 360 - diff; // distância mínima no círculo de cores
        }
        return diff;
    }
}
