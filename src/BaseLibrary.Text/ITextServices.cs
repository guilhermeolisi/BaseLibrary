﻿namespace BaseLibrary.Text;

public interface ITextServices
{
    string FirstLetterToUpper(string text);
    string CamelCaseToWithSpace(string text);
    string TextToNumber(string text);
    string TextToNumberBetweenSpaces(string text);
    string NumberToText(string text);
    bool IsGreek(char letter);
    //string TextToGreek(string text, bool isUpper = false);
    string RemoveSpaceBetweenNumberGreek(string text);
    string TextToGreekBetweenSpaces(string text, bool isUpper = false);
    string RemoveSpaces(string text);
    (string nameResult, string sufix) GetNameAndSufixAvailable(string name, char? mode = null);
    string CombineNameAndSufix(string name, string sufix, int index);
    string TextWithWhiteSpaceToCamelCaseStrict(string text);
    string TextWithWhiteSpaceToCamelCase(string text);
    string FirstLetterToUpperStric(string text);
    int Count(string text, char letter);
}