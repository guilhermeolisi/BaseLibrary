using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseLibrary.Text;

public static class TextMethods
{
    static ITextServices services = new TextServices();

    public static string FirstLetterToUpper(this string text) => services.FirstLetterToUpper(text);
    public static string CamelCaseWithSpace(this string text) => services.CamelCaseWithSpace(text);
    public static string TextToNumber(this string text) => services.TextToNumber(text);
    public static string TextToNumberBetweenSpaces(this string text) => services.TextToNumberBetweenSpaces(text);
    public static string NumberToText(this string text) => services.NumberToText(text);
    public static bool IsGreek(this char letter) => services.IsGreek(letter);
    //public static string TextToGreek(this string text, bool isUpper = false) => services.TextToGreek(text, isUpper);
    public static string RemoveSpaceBetweenNumberGreek(this string text) => services.RemoveSpaceBetweenNumberGreek(text);
    public static string TextToGreekBetweenSpaces(this string text, bool isUpper = false) => services.TextToGreekBetweenSpaces(text, isUpper);
    public static string RemoveSpaces(this string text) => services.RemoveSpaces(text);
}
