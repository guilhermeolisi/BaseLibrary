namespace BaseLibrary
{
    public interface IColorServices
    {
        (byte, byte, byte) RGBDarkenColor(byte r, byte g, byte b, double factorChange);
        (byte, byte, byte) HSLToRGB(double h, double s, double l);
        (byte, byte, byte) RGBLightenColor(byte r, byte g, byte b, double factorChange);
        (double, double, double) RGBToHSL(byte r, byte g, byte b);
        (byte, byte, byte) RGBLightnessChange(byte r, byte g, byte b, double factorChange);
        (byte, byte, byte) RGBLightnessChange(byte r, byte g, byte b, int cycle, bool isDark);
        (double, double, double) RGBToHSV(byte r, byte g, byte b);
        (byte, byte, byte) HSVToRGB(double h, double s, double v);
        (byte r, byte g, byte b) HexToRGB(string hex);
        string RGBToHex(byte r, byte g, byte b);
        double HueDistance(double h1, double h2);
        (byte r, byte g, byte b) GenerateNewColor(byte[][] existingColorsRGB);
        (byte r, byte g, byte b) GenerateNewColor(byte[][] existingColorsRGB, int countMore);
    }
}