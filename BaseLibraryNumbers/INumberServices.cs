﻿namespace BaseLibrary.Numbers;

public interface INumberServices
{
    /// <summary>
    /// retorna se o inteiro é impar
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    bool IsOdd(int value);
    /// <summary>
    /// retorna se o inteiro é par
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    bool IsEven(int value);
    /// <summary>
    /// Conta o número de casas decimais de um número, zeros a esquerda depois da vírgula não é considerado
    /// </summary>
    /// <param name="value"></param>
    /// <returns>valor negativo significa a quantidade de zero antes da virgula</returns>
    int CountDecimal(double value);
    /// <summary>
    /// Conta o número de algarismos significativos, zeros a direita e a esquerda são desconsiderados
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    int CountAlgharisms(double value, bool removeZeroRigth);
    /// <summary>
    /// Arredonda de acordo com o número de casas decimais
    /// </summary>
    /// <param name="value"></param>
    /// <param name="decimals">Negativo indica a quantidade de casas antes da vígula que será zero</param>
    /// <returns></returns>
    double RoundDecimal(double value, int decimals);
    double RoundAlgharisms(double value, uint algharims);
    string DoubleResultText(NumberESD value, string? arredonda = null);
    string DoubleResultText(double valueNull, double esdNull, string? arredonda = null);
    int ScaleOrderNumber(double value);
    string GenerateCodeID(short legnth, bool isCaseSensitive);
    string OrdinalIntegerToString(int number);
    T Clamp<T>(T value, T min, T max) where T : IComparable<T>;
    T Max<T>(T value, T max) where T : IComparable<T>;
    T Min<T>(T value, T min) where T : IComparable<T>;
    T Bigger<T>(T value1, T value2) where T : IComparable<T>;
    T Smaller<T>(T value1, T value2) where T : IComparable<T>;
}
