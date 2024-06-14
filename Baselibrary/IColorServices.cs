namespace BaseLibrary
{
    public interface IColorServices
    {
        (int, int, int) DarkenColor(int r, int g, int b, double factorChange);
        (int, int, int) HslToRgb(double h, double s, double l);
        (int, int, int) LightenColor(int r, int g, int b, double factorChange);
        (double, double, double) RgbToHsl(int r, int g, int b);
    }
}