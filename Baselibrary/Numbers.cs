using System;
using System.Globalization;
using System.Linq;
using static System.Math;

namespace BaseLibrary;

public static class Numbers
{
    /// <summary>
    /// retorna se o inteiro é impar
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static bool IsOdd(int value) => value % 2 != 0;
    /// <summary>
    /// Conta o número de casas decimais de um número, zeros a esquerda depois da vírgula não é considerado
    /// </summary>
    /// <param name="value"></param>
    /// <returns>valor negativo significa a quantidade de zero antes da virgula</returns>
    public static int CountDecimal(double value)
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
                //if ()
                //{
                return value.ToString(CultureInfo.InvariantCulture).Split('.')[1].Length;
                //}
            }
            else
                return (value + 1).ToString(CultureInfo.InvariantCulture).Split('.')[1].Length;// preciso acrescentar +1 para evitar que numeros menores que 0 sejam 
        }
        else
            return 0;
        //int res = 0;
        //while (d != (long)d) {
        //    res++;
        //    d = d * 10;// o problema desse método é que ele faz operações matemáticas e isso pode alterar o número também e dar resultados errados.
        //}
        //return res;
    }
    /// <summary>
    /// Conta o número de algarismos significativos, zeros a direita e a esquerda são desconsiderados
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static int CountAlgharisms(double value, bool removeZeroRigth)
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
    /// Arredonda de acordo com o número de casas decimais
    /// </summary>
    /// <param name="value"></param>
    /// <param name="decimals">Negativo indica a quantidade de casas antes da vígula que será zero</param>
    /// <returns></returns>
    public static double RoundDecimal(double value, int decimals)
    {
        if (decimals >= 0)
            return Round(value, decimals);
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
    public static double RoundAlgharisms(double value, uint algharims)
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
    public static string DoubleResultText(double valueNull, double esdNull, string arredonda = null)
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
        string result;
        int orderEsd = int.MinValue;
        int orderValue = int.MinValue;
        if (!double.IsNaN(esdNull) && esdNull != 0)
        {

            double esd = RoundAlgharisms(esdNull, 1);
            orderEsd = OrderNumber(esd);
            orderValue = OrderNumber(value);
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
    public static int OrderNumber(double value)
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
    public static Random Rand = new();
    public static string GenerateCodeID(short legnth, bool isCaseSensitive)
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
    public static string OrdinalIntegerToString(int number)
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
    public static T Clamp<T>(T value, T min, T max) where T : IComparable<T>
    {
        //https://stackoverflow.com/questions/3176602/how-to-force-a-number-to-be-in-a-range-in-c
        if (value.CompareTo(min) < 0)
            return min;
        if (value.CompareTo(max) > 0)
            return max;

        return value;
    }
}
