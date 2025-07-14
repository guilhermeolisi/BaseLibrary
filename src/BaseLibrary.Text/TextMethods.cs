namespace BaseLibrary.Text;

public static class TextMethods
{
    static ITextServices services = new TextServices();

    public static string FirstLetterToUpper(this string text) => services.FirstLetterToUpper(text);
    public static string FirstLetterToUpperStric(this string text) => services.FirstLetterToUpperStric(text);
    public static string CamelCaseWithSpace(this string text) => services.CamelCaseToWithSpace(text);
    public static string TextWithWhiteSpaceToCamelCase(this string text) => services.TextWithWhiteSpaceToCamelCase(text);
    public static string TextWithWhiteSpaceToCamelCaseStrict(this string text) => services.TextWithWhiteSpaceToCamelCaseStrict(text);
    public static string TextToNumber(this string text) => services.TextToNumber(text);
    public static string TextToNumberBetweenSpaces(this string text) => services.TextToNumberBetweenSpaces(text);
    public static string NumberToText(this string text) => services.NumberToText(text);
    public static bool IsGreek(this char letter) => services.IsGreek(letter);
    //public static string TextToGreek(this string text, bool isUpper = false) => services.TextToGreek(text, isUpper);
    public static string RemoveSpaceBetweenNumberGreek(this string text) => services.RemoveSpaceBetweenNumberGreek(text);
    public static string TextToGreekBetweenSpaces(this string text, bool isUpper = false) => services.TextToGreekBetweenSpaces(text, isUpper);
    public static string RemoveSpaces(this string text) => services.RemoveSpaces(text);

    public static (string nameResult, string sufix) GetNameAndSufixAvailable(this string text, char? mode = null) => services.GetNameAndSufixAvailable(text, mode);
    public static string CombineNameAndSufix(this string name, string sufix, int index) => services.CombineNameAndSufix(name, sufix, index);
    public static bool IsNullOrWhiteSpace(this string? text) => string.IsNullOrWhiteSpace(text);
    public static int Count(this string text, char character) => services.Count(text, character);
  
}
