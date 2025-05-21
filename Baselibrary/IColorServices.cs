namespace BaseLibrary
{
    public interface IColorServices
    {
        (int, int, int) RGBDarkenColor(int r, int g, int b, double factorChange);
        (int, int, int) HslToRgb(double h, double s, double l);
        (int, int, int) RGBLightenColor(int r, int g, int b, double factorChange);
        (double, double, double) RgbToHsl(int r, int g, int b);
        (int, int, int) RGBLightnessChange(int r, int g, int b, double factorChange);
        (int, int, int) RGBLightnessChange(int r, int g, int b, int cycle, bool isDark);
    }
}