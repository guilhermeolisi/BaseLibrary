namespace BaseLibrary.Numbers;

public static class NumbersMethods
{
    private static INumberServices numberServices = new NumberServices();

    /// <summary>
    /// retorna se o inteiro é impar
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static bool IsOdd(this int value) => numberServices.IsOdd(value);
    public static bool IsEven(this int value) => numberServices.IsEven(value);
    public static bool IsOdd(this byte value) => numberServices.IsOdd(value);
    public static bool IsEven(this byte value) => numberServices.IsEven(value);
    /// <summary>
    /// Conta o número de casas decimais de um número, zeros a esquerda depois da vírgula não é considerado
    /// </summary>
    /// <param name="value"></param>
    /// <returns>valor negativo significa a quantidade de zero antes da virgula</returns>
    public static int CountDecimal(this double value) => numberServices.CountDecimal(value);
    /// <summary>
    /// Conta o número de algarismos significativos, zeros a direita e a esquerda são desconsiderados
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static int CountAlgharisms(this double value, bool removeZeroRigth) => numberServices.CountAlgharisms(value, removeZeroRigth);
    /// <summary>
    /// Arredonda de acordo com o número de casas decimais
    /// </summary>
    /// <param name="value"></param>
    /// <param name="decimals">Negativo indica a quantidade de casas antes da vígula que será zero</param>
    /// <returns></returns>
    public static double RoundDecimal(this double value, int decimals) => numberServices.RoundDecimal(value, decimals);
    public static double RoundAlgharisms(this double value, uint algharims) => numberServices.RoundAlgharisms(value, algharims);
    public static string DoubleResultText(this double valueNull, double esdNull, string? arredonda = null) => numberServices.DoubleResultText(valueNull, esdNull, arredonda);
    public static int ScaleOrderNumber(this double value) => numberServices.ScaleOrderNumber(value);
    //public static Random Rand = new();
    public static string GenerateCodeID(short legnth, bool isCaseSensitive) => numberServices.GenerateCodeID(legnth, isCaseSensitive);
    public static string OrdinalIntegerToString(int number) => numberServices.OrdinalIntegerToString(number);
    public static T Clamp<T>(T value, T min, T max) where T : IComparable<T> => numberServices.Clamp(value, min, max);
    public static T Max<T>(this T value, T max) where T : IComparable<T> => numberServices.Max(value, max);
    public static T Min<T>(this T value, T min) where T : IComparable<T> => numberServices.Min(value, min);
    public static T Bigger<T>(this T value1, T value2) where T : IComparable<T> => numberServices.Bigger(value1, value2);
    public static T Smaller<T>(this T value1, T value2) where T : IComparable<T> => numberServices.Smaller(value1, value2);

    #region double Methods Extentions
    public static bool IsNaN(this double value) => double.IsNaN(value);
    public static bool IsNotNaN(this double value) => !double.IsNaN(value);
    public static bool IsInfinity(this double value) => double.IsInfinity(value);
    public static bool IsPositiveInfinity(this double value) => double.IsPositiveInfinity(value);
    public static bool IsNegativeInfinity(this double value) => double.IsNegativeInfinity(value);
    //public static double Max(this double value1, double value2) => Math.Max(value1, value2);
    //public static double Min(this double value1, double value2) => Math.Min(value1, value2);
    //public static int Max(this int value1, int value2) => Math.Max(value1, value2);
    //public static int Min(this int value1, int value2) => Math.Min(value1, value2);
    public static double Abs(this double value) => Math.Abs(value);
    public static int Abs(this int value) => Math.Abs(value);
    #endregion

}
