namespace BaseLibrary.Text;

public class TextServices : ITextServices
{
    public string FirstLetterToUpper(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;
        if (text.Length > 0 && /*char.IsAsciiLetterLower(text[0])*/ char.IsLower(text[0]))
        {
            text = char.ToUpper(text[0]) + text.Substring(1, text.Length - 1);
        }
        return text;
    }
    public string CamelCaseWithSpace(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;
        int ind = 0;
        while (ind < text.Length - 1)
        {
            //if (char.IsAsciiLetterUpper(text[ind]) && ind != 0 && !char.IsAsciiLetterUpper(text[ind + 1]))
            if (char.IsUpper(text[ind]) && ind != 0 && !char.IsUpper(text[ind + 1]))
            {
                text = text.Insert(ind, " ");
                ind++;
            }
            ind++;
        }
        return text;
    }
    static (string text, int num)[] numbers =
    {
        ("Zero", 0),
        ("One", 1),
        ("Two", 2),
        ("Three", 3),
        ("Four", 4),
        ("Five", 5),
        ("Six", 6),
        ("Seven", 7),
        ("Eight", 8),
        ("Nine", 9),

    };
    public string TextToNumber(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;
        for (int i = 0; i < numbers.Length; i++)
        {
            text = text.Replace(numbers[i].text, numbers[i].num.ToString(), StringComparison.CurrentCultureIgnoreCase);
        }
        return text;
    }
    public string TextToNumberBetweenSpaces(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;
        for (int i = 0; i < numbers.Length; i++)
        {
            int ind = 0;
            while (ind <= text.Length - numbers[i].text.Length)
            {
                if (ind != 0 && text[ind - 1] != ' ' && ind + numbers[i].text.Length - 1 < text.Length && text[ind + numbers[i].text.Length - 1] != ' ')
                {
                    ind++;
                    continue;
                }
                bool match = true;

                for (int j = 0; j < numbers[i].text.Length; j++)
                {
                    if (text[ind + j] != numbers[i].text[j])
                    {
                        match = false;
                        break;
                    }
                }
                if (match)
                {
#if DEBUG
                    string trash = text.Remove(ind, numbers[i].text.Length);
                    var trash2 = trash.Insert(ind, numbers[i].num.ToString());
#endif

                    text = text.Remove(ind, numbers[i].text.Length).Insert(ind, numbers[i].num.ToString());
                    ind++;
                }
                ind++;
            }
        }
        return text;
    }
    public string NumberToText(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;
        for (int i = 0; i < numbers.Length; i++)
        {
            //if (text.Contains(numbers[i].num.ToString(), StringComparison.InvariantCultureIgnoreCase))
            //{
            //    text.Replace(numbers[i].num.ToString(), numbers[i].text, StringComparison.CurrentCultureIgnoreCase);
            //}
            text = text.Replace(numbers[i].num.ToString(), numbers[i].text, StringComparison.CurrentCultureIgnoreCase);
        }
        return text;
    }
    //https://www.rapidtables.com/math/symbols/greek_alphabet.html
    static (char upper, char lower, string text)[] greeks =
    {
        ('Α', 'α', "Alpha"),
        ('Β', 'β', "Beta"),
        ('Γ', 'γ', "Gamma"),
        ('Δ', 'δ', "Delta"),
        //Ε   ε   Epsilon     e
        //Ζ   ζ   Zeta    z
        ('Η', 'η', "Eta"),
        ('Θ', 'θ', "Theta"),
        //Ι   ι   Iota    i
        //Κ   κ   Kappa   k
        ('Λ', 'λ', "Lambda"),
        ('Μ', 'μ', "Mu"),
        //Ν   ν   Nu  n
        //Ξ   ξ   Xi  x
        //Ο   ο   Omicron     o
        //Π   π   Pi  p
        //Ρ   ρ   Rho     r
        //Σ   σ,ς *   Sigma   s
        ('Τ', 'τ', "Tau"),
        //Υ   υ   Upsilon     u
        //Φ   φ   Phi     ph
        ('Χ', 'χ', "Chi"),
        ('Ψ', 'ψ', "Psi"),
        ('Ω', 'ω', "Omega")
    };
    //public string TextToGreek(string text, bool isUpper = false)
    //{
    //    for (int i = 0; i < greeks.Length; i++)
    //    {
    //        if (text.Contains(greeks[i].text))
    //        {
    //            int ind = 0;
    //            while (ind <= text.Length - greeks[i].text.Length)
    //            {

    //                string sub = text.Substring(ind, ind + greeks[i].text.Length);

    //                if (sub.Equals(greeks[i].text, StringComparison.CurrentCultureIgnoreCase))
    //                {
    //                    text = text.Remove(ind, greeks[i].text.Length).Insert(ind, (isUpper ? greeks[i].upper : greeks[i].lower).ToString());
    //                }
    //                ind++;
    //            }
    //            text = text.Replace(greeks[i].text, (isUpper ? greeks[i].upper : greeks[i].lower).ToString(), StringComparison.CurrentCultureIgnoreCase);
    //        }
    //    }
    //    return text;
    //}
    public string TextToGreekBetweenSpaces(string text, bool isUpper = false)
    {
        if (string.IsNullOrEmpty(text))
            return text;
        for (int i = 0; i < greeks.Length; i++)
        {
            if (text.Contains(greeks[i].text))
            {
                int ind = 0;
                while (ind <= text.Length - greeks[i].text.Length)
                {
                    if (ind != 0 && text[ind - 1] != ' ' && ind + greeks[i].text.Length - 1 < text.Length && text[ind + greeks[i].text.Length - 1] != ' ')
                    {
                        ind++;
                        continue;
                    }
                    bool match = true;

                    for (int j = 0; j < greeks[i].text.Length; j++)
                    {
                        if (text[ind + j] != greeks[i].text[j])
                        {
                            match = false;
                            break;
                        }
                    }
                    //string sub = text.Substring(ind, greeks[i].text.Length);

                    //if (sub.Equals(greeks[i].text, StringComparison.CurrentCultureIgnoreCase))
                    if (match)
                    {
#if DEBUG
                        string trash = text.Remove(ind, greeks[i].text.Length);
                        var trash2 = trash.Insert(ind, (isUpper ? greeks[i].upper : greeks[i].lower).ToString());
#endif

                        text = text.Remove(ind, greeks[i].text.Length).Insert(ind, (isUpper ? greeks[i].upper : greeks[i].lower).ToString());
                        ind++;
                    }
                    ind++;
                }
                //text = text.Replace(greek[i].text, (isUpper ? greek[i].greekU : greek[i].greekL).ToString(), StringComparison.CurrentCultureIgnoreCase);
            }
        }
        return text;
    }
    public string RemoveSpaceBetweenNumberGreek(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;
        int ind = 0;
        while (ind < text.Length - 1)
        {
            if ((char.IsDigit(text[ind]) || text[ind].IsGreek()) && text[ind + 1] == ' ')
            {
                int count = 1;
                while (ind < text.Length - count && text[ind + count] == ' ')
                {
                    count++;
                }
                count--;
                if (ind < text.Length - count - 1 && ((char.IsDigit(text[ind]) && text[ind + count + 1].IsGreek()) || (text[ind].IsGreek() && char.IsDigit(text[ind + count + 1]))))
                {
                    text = text.Remove(ind + 1, count);
                    ind--;
                }
            }
            ind++;
        }
        return text;
    }
    public bool IsGreek(char letter)
    {
        for (int i = 0; i < greeks.Length; i++)
        {
            if (letter == greeks[i].lower || letter == greeks[i].upper)
                return true;
        }
        return false;
    }
    public string RemoveSpaces(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;
        return text.Replace(" ", "");
    }
    public (string nameResult, string sufix) GetNameAndSufixAvailable(string name, char? mode = null)
    {
        name = name.Trim();
        string sufix = mode switch
        {
            'C' => "Copied",
            'E' => "Exported",
            'I' => "Imported",
            'M' => "Moved",
            _ => ""
        };

        int ind = name.Length - 1;
        if (name[ind] == ')')
        {
            bool foundIndex = false;
            while (ind >= 0 && (name[ind] == ')' || char.IsDigit(name[ind]) || name[ind] == '(' || name[ind] == ' '))
            {
                if (name[ind] == '(')
                {
                    foundIndex = true;
                }
                ind--;
            }
            ind++;
            if (foundIndex && ind > 0)
            {
                name = name[..ind];
            }
        }
        ind = name.Length - 1;
        if (!string.IsNullOrWhiteSpace(sufix) && name.Contains(sufix))
        {
            int indSurfix = sufix.Length - 1;
            while (ind >= 0 && ((indSurfix >= 0 && name[ind] == sufix[indSurfix]) || name[ind] == ' ' || name[ind] == '-'))
            {
                ind--;
                indSurfix--;
            }
            ind++;
            if (indSurfix < 0 && ind > 0)
            {
                name = name[..ind];
            }
        }
        return (name, sufix);
    }
    public string CombineNameAndSufix(string name, string sufix, int index)
    {
        if (string.IsNullOrWhiteSpace(name))
            return name;
        return string.Format("{0}{1}{2}", name, string.IsNullOrWhiteSpace(sufix) ? "" : " - " + sufix, index == 1 ? "" : " (" + index + ")");
    }
}