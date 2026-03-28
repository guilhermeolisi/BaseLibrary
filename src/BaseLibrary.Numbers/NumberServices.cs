using System.Globalization;
using System.Runtime.CompilerServices;
using static System.Math;

namespace BaseLibrary.Numbers;

/// <summary>
/// Fornece serviços utilitários para operações matemáticas e manipulação de números.
/// </summary>
public class NumberServices : INumberServices
{
    /// <summary>
    /// Verifica se um número inteiro é ímpar.
    /// </summary>
    /// <param name="value">O valor inteiro a ser verificado.</param>
    /// <returns>Retorna <c>true</c> se o valor for ímpar; caso contrário, <c>false</c>.</returns>
    public bool IsOdd(int value) => value % 2 != 0;
    
    /// <summary>
    /// Verifica se um byte é ímpar.
    /// </summary>
    /// <param name="value">O valor byte a ser verificado.</param>
    /// <returns>Retorna <c>true</c> se o valor for ímpar; caso contrário, <c>false</c>.</returns>
    public bool IsOdd(byte value) => value % 2 != 0;
    
    /// <summary>
    /// Verifica se um número inteiro é par.
    /// </summary>
    /// <param name="value">O valor inteiro a ser verificado.</param>
    /// <returns>Retorna <c>true</c> se o valor for par; caso contrário, <c>false</c>.</returns>
    public bool IsEven(int value) => value % 2 == 0;
    
    /// <summary>
    /// Verifica se um byte é par.
    /// </summary>
    /// <param name="value">O valor byte a ser verificado.</param>
    /// <returns>Retorna <c>true</c> se o valor for par; caso contrário, <c>false</c>.</returns>
    public bool IsEven(byte value) => value % 2 == 0;
    
    /// <summary>
    /// Conta o número de casas decimais de um número double.
    /// Zeros à esquerda após a vírgula não são considerados.
    /// </summary>
    /// <param name="value">O valor double para contar as casas decimais.</param>
    /// <returns>
    /// O número de casas decimais. Valor negativo significa a quantidade de zeros antes da vírgula.
    /// Retorna 0 se o valor for NaN ou inteiro. Máximo de 15 casas decimais devido à precisão do double.
    /// </returns>
    public int CountDecimal(double value)
    {
        //outra possibilidade para contar as casas decimais é
        //(3.1415.ToString(CultureInfo.InvariantCulture)).Split('.')[1].Length
        //https://pt.stackoverflow.com/questions/90782/obter-a-quantidade-de-casas-decimais-de-um-decimal

        if (double.IsNaN(value))
            return 0;
        if (value != (int)value)
        {
            if (Abs(value) >= 1)
            {
                string[] temp = value.ToString(CultureInfo.InvariantCulture).Split('.');
                return temp.Length < 2 ? 0 : temp[1].Length > 15 ? 15 : temp[1].Length;
            }
            else
            {
#if DEBUG
                var trash2 = (Abs(value) + 1).ToString(CultureInfo.InvariantCulture);
                if (Abs(Abs(value) + 1 - 1 - value) > 1E-12)
                {

                }

#endif
                string[] temp = value.ToString("F15", CultureInfo.InvariantCulture).Split('.'); // F15 é por que o padrão de precisão do double é 15 casas decimais, assim não perdemos precisão
                if (temp.Length < 2)
                    return 0;
                string temp2 = temp[1].TrimEnd('0');
                return temp2.Length;
            }
        }
        else
            return 0;
    }
    /// <summary>
    /// Conta o número de algarismos significativos de um número double.
    /// </summary>
    /// <param name="value">O valor double para contar os algarismos.</param>
    /// <param name="removeZeroRigth">Se <c>true</c>, remove zeros à direita; se <c>false</c>, mantém zeros à direita mas remove à esquerda.</param>
    /// <returns>
    /// O número de algarismos significativos. Retorna 0 se o valor for NaN, e 1 se o valor for 0.
    /// </returns>
    public int CountAlgharisms(double value, bool removeZeroRigth)
    {
        if (double.IsNaN(value))
            return 0;
        if (value == 0)
            return 1;
#if DEBUG
        string trash = value.ToString(CultureInfo.InvariantCulture);
        string trash2 = trash.Trim(new char[] { '-', '.', '0' });
        string[] parts = trash.Split('E');
        if (parts.Length == 1)
        { }
#endif
        string temp = value.ToString(CultureInfo.InvariantCulture).Split('E')[0];
        if (temp.Contains('.'))
        {
            temp = temp.Remove(temp.IndexOf('.'), 1);
        }

        if (removeZeroRigth)
        {
            return temp.Trim(new char[] { '-', '.', '0' }).Length;
        }
        else
        {
            return temp.ToString(CultureInfo.InvariantCulture).Trim(new char[] { '-', '.' }).TrimStart(new char[] { '0' }).Length;
        }
    }
    /// <summary>
    /// Arredonda um número double de acordo com o número de casas decimais especificado.
    /// </summary>
    /// <param name="value">O valor a ser arredondado.</param>
    /// <param name="decimals">
    /// O número de casas decimais. Valor positivo indica casas após a vírgula.
    /// Valor negativo indica a quantidade de casas antes da vírgula que será zero.
    /// </param>
    /// <returns>O valor arredondado. Se decimals &gt; 15, retorna o valor original sem arredondamento.</returns>
    public double RoundDecimal(double value, int decimals)
    {
        if (decimals >= 0)
        {
            if (decimals > 15) //Precisa dessa verificação para evitar erro de arredondamento no Round, digitos deve ser no máximo 15
                return value;
            else
                return Round(value, decimals);
        }
        else
        {
            double temp = value / Pow(10.0, -decimals);
#if DEBUG
            double trash = temp % 1;
            double trash2 = (temp - (temp % 1)) * Pow(10.0, -decimals);
#endif
            temp = Round(temp, 0);
            temp = temp - (temp % 1); // - (int)temp;
            return temp * Pow(10.0, -decimals);
        }
    }
    /// <summary>
    /// Arredonda um número double mantendo um número específico de algarismos significativos.
    /// </summary>
    /// <param name="value">O valor a ser arredondado.</param>
    /// <param name="algharims">O número de algarismos significativos a manter.</param>
    /// <returns>O valor arredondado com o número especificado de algarismos significativos. Retorna NaN se o valor for NaN, e 0 se o valor for 0.</returns>
    public double RoundAlgharisms(double value, uint algharims)
    {
        if (double.IsNaN(value))
            return double.NaN;
        if (value == 0)
            return 0;
        else if (Abs(value) >= 1)
        {
            double temp = Log10(Abs(value));
            //if (temp % (int)temp == 0)
            //    return RoundDecimal(value, (int)temp + 1);
            //else
            return RoundDecimal(value, -((int)temp + 1 - (int)algharims));
        }
        else
        {
#if DEBUG
            double trash = Log10(Abs(value));
#endif
            double temp = Abs(Log10(Abs(value)));
            int firstAlgharism;
            if (temp % (int)temp == 0)
                firstAlgharism = Abs((int)Log10(Abs(value)));
            else
                firstAlgharism = Abs((int)Log10(Abs(value))) + 1;
            return RoundDecimal(value, firstAlgharism + (int)algharims - 1);
        }
    }
    /// <summary>
    /// Converte um valor numérico com erro padrão (ESD) em texto formatado.
    /// </summary>
    /// <param name="value">O objeto NumberESD contendo o valor e o erro padrão.</param>
    /// <param name="arredonda">Formato opcional de arredondamento customizado.</param>
    /// <returns>Uma string representando o valor com o erro padrão entre parênteses quando aplicável.</returns>
    public string DoubleResultText(NumberESD value, string? arredonda = null) => DoubleResultText(value.Value, value.ESD, arredonda);
    
    /// <summary>
    /// Converte um valor numérico com erro padrão (ESD) em texto formatado com notação científica quando apropriado.
    /// </summary>
    /// <param name="valueNull">O valor numérico principal.</param>
    /// <param name="esdNull">O erro padrão do desvio (ESD - Estimated Standard Deviation).</param>
    /// <param name="arredonda">Formato opcional de arredondamento customizado.</param>
    /// <returns>
    /// Uma string representando o valor formatado com o ESD entre parênteses.
    /// Usa notação científica para valores muito grandes (&gt;10^4) ou muito pequenos (&lt;10^-3).
    /// </returns>
    public string DoubleResultText(double valueNull, double esdNull, string? arredonda = null)
    {
#if DEBUG
        //TRASH
        if (double.IsInfinity(valueNull))
        { }
        if (valueNull != 0)
        { }
        if (Abs(valueNull - 1.5965682E-05) < 1E-5)
        { }
        if (esdNull != 0 && !double.IsNaN(esdNull))
        { }
#endif
        double value = valueNull;
        if (double.IsNaN(value))
            return value.ToString();

        if (Abs(value) < 1E-14)
            value = 0;


        double esdTemp;
        string esdAlgarism = string.Empty;
        string result = string.Empty;
        int orderEsd = int.MinValue;
        int orderValue = int.MinValue;
        if (!double.IsNaN(esdNull) && esdNull != 0)
        {

            double esd = RoundAlgharisms(esdNull, 1);
            orderEsd = ScaleOrderNumber(esd);
            orderValue = ScaleOrderNumber(value);
            bool isGreatEsd = orderValue < orderEsd;
            //Processa ESD
            if (value == 0)
            {
                esdAlgarism = esd.ToString();
            }
            else if (isGreatEsd)
            {
                esdTemp = esd / Pow(10.0, orderEsd);

                esdTemp = esdTemp * Pow(10.0, orderEsd - orderValue);
                esdAlgarism = esdTemp.ToString();
                //if (orderEsd - orderValue <= 3)
                //{
                //    esdTemp = esdTemp * Pow(10.0, orderEsd - orderValue);
                //    esdAlgarism = esdTemp.ToString();
                //}
                //else
                //{
                //    esdAlgarism = esdTemp.ToString("F0") + "E+" + (orderEsd - orderValue);
                //}
                value = RoundAlgharisms(value, 1);
            }
            else if (Abs(esd) < 1)
            {
                value = RoundDecimal(value, Abs(orderEsd));
                esdTemp = esd / Pow(10.0, orderEsd);
                esdAlgarism = esdTemp.ToString("F0");
            }
            else
            {
                value = RoundDecimal(value, -orderEsd);
                if (orderValue > 4)
                {
                    esdTemp = esd / Pow(10.0, orderEsd);
                    esdAlgarism = esdTemp.ToString("F0");
                }
                else
                {
                    esdAlgarism = esd.ToString("F0");
                }
            }
            //Processa Value e result
            if (value != 0 && (orderValue > 4 || orderValue < -3))
            {
                string[] part;
                part = value.ToString(("E" + (CountAlgharisms(value, true) - 1)), CultureInfo.InvariantCulture).Split('E', StringSplitOptions.RemoveEmptyEntries);
                if (part.Length > 0)
                {
                    if (orderValue > 4)
                    {
                        int algharism = orderValue - orderEsd + 1;
                        if (algharism < 0)
                            algharism = 0;
                        else
                        {
                            int nozero = part[0].Length;
                            if (part[0].Contains('.'))
                                nozero--;
                            if (part[0].Contains('-') || part[0].Contains('+'))
                                nozero--;
                            if (algharism > nozero)
                                part[0] = part[0] + new string('0', algharism - nozero);
                        }
                    }
                    else
                    {

                        if (orderValue < orderEsd)
                        {
                            if (!isGreatEsd)
                            {
                                //Nunca deve entrar aqui
                                esdAlgarism = esdAlgarism + new string('0', orderEsd - orderValue);
                            }
                        }
                        else if (orderValue > orderEsd)
                        {
                            int nozero = part[0].Length;
                            if (part[0].Contains('.'))
                                nozero--;
                            if (part[0].Contains('-') || part[0].Contains('+'))
                                nozero--;
                            if (nozero < orderValue - orderEsd + 1)
                            {
                                if (!part[0].Contains('.'))
                                    part[0] += '.';
                                part[0] = part[0] + new string('0', (orderValue - orderEsd + 1) - nozero);
                            }

                        }
                    }
                }
                else
                {
                    //TODO verificar o que fazer quando entrar aqui
                }

                if (part.Length >= 2)
                {
                    int ind = 0;
                    if (part[1][ind] == '+' || part[1][ind] == '-')
                        ind++;
                    while (part[1][ind] == '0')
                        part[1] = part[1].Remove(ind, 1);

                    //string temp = "F"+ algharism.ToString();
                    esdAlgarism = string.IsNullOrWhiteSpace(esdAlgarism) ? "" : "(" + esdAlgarism + ")";
                    result = string.Format("{0}{1}E{2}", part[0], esdAlgarism, part[1]);
                }
                else
                {
                    //TODO verificar o que fazer quando entrar aqui
                }
            }
            else
            {
                string valueStr;
                if (isGreatEsd)
                {
                    if (orderValue > 0)
                    {
                        esdAlgarism += new string('0', orderValue);
                    }
#if DEBUG
                    _ = esdNull;
                    _ = valueNull;
                    double trash = RoundAlgharisms(value, 1);
#endif
                    valueStr = RoundAlgharisms(value, 1).ToString();
                }
                else if (!isGreatEsd && esd < 1)
                {
#if DEBUG
                    int decimals = CountDecimal(esd);
#endif
                    valueStr = value.ToString("F" + -orderEsd);

                }
                else
                {
                    valueStr = value.ToString();
                }
                esdAlgarism = string.IsNullOrWhiteSpace(esdAlgarism) ? "" : "(" + esdAlgarism + ")";
                result = string.Format("{0}{1}", valueStr, esdAlgarism);
            }
        }
        else if (!string.IsNullOrWhiteSpace(arredonda))
        {
            result = value.ToString(arredonda);
        }
        else
            result = value.ToString();
        //esdTemp = (esd / Pow(10.0, (int)Log10(esd)));
        //esdAlgarism = (esdTemp % (int)esdTemp).ToString();
#if DEBUG
        //TRASH
        if (result == "Infinity")
        { }
#endif

        return result;
    }
    /// <summary>
    /// Calcula a ordem de grandeza (potência de 10) de um número.
    /// </summary>
    /// <param name="value">O valor para calcular a ordem de grandeza.</param>
    /// <returns>O expoente inteiro da potência de 10 mais próxima do valor. Por exemplo, para 1234 retorna 3, para 0.0056 retorna -3.</returns>
    public int ScaleOrderNumber(double value)
    {
        double result = Log10(Abs(value));
        if (result < 0)
        {
            if (value == Pow(10, (int)Log10(Abs(value))))
                return (int)result;
            else
                return (int)result - 1;
        }
        else
        {
            return (int)result;
        }
    }
    /// <summary>
    /// Gerador de números aleatórios usado pelos métodos da classe.
    /// </summary>
    public Random Rand = new();
    
    /// <summary>
    /// Gera um código identificador alfanumérico aleatório.
    /// </summary>
    /// <param name="legnth">O comprimento do código a ser gerado.</param>
    /// <param name="isCaseSensitive">
    /// Se <c>true</c>, usa caracteres 0-9, A-Z e a-z (62 caracteres possíveis).
    /// Se <c>false</c>, usa apenas 0-9 e A-Z (36 caracteres possíveis).
    /// </param>
    /// <returns>Uma string aleatória com o comprimento especificado contendo caracteres alfanuméricos.</returns>
    public string GenerateCodeID(short legnth, bool isCaseSensitive)
    {
        char[] codeTemp = new char[legnth];

        if (isCaseSensitive)
        {
            //0-9A-Za-z: 10 + 26 + 26 = 62 caracteres
            //https://pt.wikipedia.org/wiki/ASCII
            for (int i = 0; i < legnth; i++)
            {
                int tempShort = (int)Rand.Next(0, 61);
                if (tempShort <= 9)
                {
                    codeTemp[i] = (char)(tempShort + 48);
                }
                else if (tempShort <= 35)
                {
                    codeTemp[i] = (char)(tempShort + 65 - 10);
                }
                else
                {
                    codeTemp[i] = (char)(tempShort + 97 - 36);
                }
            }
        }
        else
        {
            //0-9A-Z: 10 + 26 = 36 caracteres
            //https://pt.wikipedia.org/wiki/ASCII
            for (int i = 0; i < legnth; i++)
            {
                int tempShort = (int)Rand.Next(0, 35);
                if (tempShort <= 9)
                {
                    codeTemp[i] = (char)(tempShort + 48);
                }
                else if (tempShort <= 35)
                {
                    codeTemp[i] = (char)(tempShort + 65 - 10);
                }
            }
        }
#if DEBUG
        var trash = new string(codeTemp);
#endif

        return new string(codeTemp);
    }
    /// <summary>
    /// Converte um número inteiro em sua forma ordinal em inglês (1st, 2nd, 3rd, etc.).
    /// </summary>
    /// <param name="number">O número inteiro a ser convertido.</param>
    /// <returns>Uma string representando o número ordinal (ex: "1st", "2nd", "3rd", "4th").</returns>
    public string OrdinalIntegerToString(int number)
    {
        string str = number.ToString();
        if (str.Last() == '1' && (str.Length == 1 || str[1] != '1'))
        {
            str += "st";
        }
        else if (str.Last() == '2' && (str.Length == 1 || str[1] != '1'))
        {
            str += "nd";
        }
        else if (str.Last() == '3' && (str.Length == 1 || str[1] != '1'))
        {
            str += "rd";
        }
        else
        {
            str += "th";
        }
        return str;
    }
    /// <summary>
    /// Gera aleatoriamente o valor 1 ou -1.
    /// </summary>
    /// <returns>Retorna 1 ou -1 com probabilidade igual (50% cada).</returns>
    public int RandomOneOrMinusOne()
    {
#if DEBUG
        int sign = (Random.Shared.Next(2) << 1) - 1;
#endif
        //return Rand.Next(0, 2) == 0 ? 1 : -1;
        return (Random.Shared.Next(2) << 1) - 1;
    }
    /// <summary>
    /// Limita um valor entre um mínimo e um máximo especificados.
    /// </summary>
    /// <typeparam name="T">O tipo do valor, que deve implementar IComparable&lt;T&gt;.</typeparam>
    /// <param name="value">O valor a ser limitado.</param>
    /// <param name="min">O valor mínimo permitido.</param>
    /// <param name="max">O valor máximo permitido.</param>
    /// <returns>O valor limitado entre min e max. Se o valor for menor que min, retorna min. Se for maior que max, retorna max. Caso contrário, retorna o próprio valor.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T Clamp<T>(T value, T min, T max) where T : IComparable<T>
    {
        //https://stackoverflow.com/questions/3176602/how-to-force-a-number-to-be-in-a-range-in-c
        if (value.CompareTo(min) < 0)
            return min;
        if (value.CompareTo(max) > 0)
            return max;

        return value;
    }
    /// <summary>
    /// Retorna o menor valor entre o valor fornecido e um máximo.
    /// </summary>
    /// <typeparam name="T">O tipo do valor, que deve implementar IComparable&lt;T&gt;.</typeparam>
    /// <param name="value">O valor a ser comparado.</param>
    /// <param name="max">O valor máximo permitido.</param>
    /// <returns>O valor original se for menor ou igual ao máximo; caso contrário, retorna o máximo.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T Max<T>(T value, T max) where T : IComparable<T> => value.CompareTo(max) > 0 ? max : value;
    
    /// <summary>
    /// Retorna o maior valor entre o valor fornecido e um mínimo.
    /// </summary>
    /// <typeparam name="T">O tipo do valor, que deve implementar IComparable&lt;T&gt;.</typeparam>
    /// <param name="value">O valor a ser comparado.</param>
    /// <param name="min">O valor mínimo permitido.</param>
    /// <returns>O valor original se for maior ou igual ao mínimo; caso contrário, retorna o mínimo.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T Min<T>(T value, T min) where T : IComparable<T> => value.CompareTo(min) < 0 ? min : value;
    
    /// <summary>
    /// Retorna o maior valor entre dois valores fornecidos.
    /// </summary>
    /// <typeparam name="T">O tipo dos valores, que deve implementar IComparable&lt;T&gt;.</typeparam>
    /// <param name="value1">O primeiro valor a comparar.</param>
    /// <param name="value2">O segundo valor a comparar.</param>
    /// <returns>O maior valor entre value1 e value2.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T Bigger<T>(T value1, T value2) where T : IComparable<T> => value1.CompareTo(value2) > 0 ? value1 : value2;
    
    /// <summary>
    /// Retorna o menor valor entre dois valores fornecidos.
    /// </summary>
    /// <typeparam name="T">O tipo dos valores, que deve implementar IComparable&lt;T&gt;.</typeparam>
    /// <param name="value1">O primeiro valor a comparar.</param>
    /// <param name="value2">O segundo valor a comparar.</param>
    /// <returns>O menor valor entre value1 e value2.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T Smaller<T>(T value1, T value2) where T : IComparable<T> => value1.CompareTo(value2) < 0 ? value1 : value2;
}
