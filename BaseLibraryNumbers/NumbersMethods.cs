using System;
using System.Globalization;
using System.Linq;
using static System.Math;

namespace BaseLibrary.Numbers;

public static class NumberMethods
{
    private static INumberServices numberServices = new NumberServices();

    /// <summary>
    /// retorna se o inteiro é impar
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static bool IsOdd(int value) => numberServices.IsOdd(value);
    /// <summary>
    /// Conta o número de casas decimais de um número, zeros a esquerda depois da vírgula não é considerado
    /// </summary>
    /// <param name="value"></param>
    /// <returns>valor negativo significa a quantidade de zero antes da virgula</returns>
    public static int CountDecimal(double value) => numberServices.CountDecimal(value);
    /// <summary>
    /// Conta o número de algarismos significativos, zeros a direita e a esquerda são desconsiderados
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static int CountAlgharisms(double value, bool removeZeroRigth) => numberServices.CountAlgharisms(value, removeZeroRigth);
    /// <summary>
    /// Arredonda de acordo com o número de casas decimais
    /// </summary>
    /// <param name="value"></param>
    /// <param name="decimals">Negativo indica a quantidade de casas antes da vígula que será zero</param>
    /// <returns></returns>
    public static double RoundDecimal(double value, int decimals) => numberServices.RoundDecimal(value, decimals);
    public static double RoundAlgharisms(double value, uint algharims) => numberServices.RoundAlgharisms(value, algharims);
    public static string DoubleResultText(double valueNull, double esdNull, string? arredonda = null) => numberServices.DoubleResultText(valueNull, esdNull, arredonda);
    public static int OrderNumber(double value) => numberServices.OrderNumber(value);
    //public static Random Rand = new();
    public static string GenerateCodeID(short legnth, bool isCaseSensitive) => numberServices.GenerateCodeID(legnth, isCaseSensitive);
    public static string OrdinalIntegerToString(int number) => numberServices.OrdinalIntegerToString(number);
    public static T Clamp<T>(T value, T min, T max) where T : IComparable<T> => numberServices.Clamp(value, min, max);
}
