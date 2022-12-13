using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseLibrary.Numbers;

public struct NumberESD : IComparable
{
    public double Value = double.NaN;
    public double ESD = double.NaN;
    private readonly string? numberText = null;
    private readonly string? stringFormat = null;
    public string? NumberText { get => !HasValue ? null : !double.IsNaN(Value) ? NumberMethods.DoubleResultText(Value, ESD, stringFormat) : !string.IsNullOrWhiteSpace(numberText) ? numberText : "-"; }
    public bool HasValue;
    //public NumberESD()
    //{
    //    Value = double.NaN;
    //    ESD = double.NaN;
    //    this.stringFormat = null;
    //    this.numberText = null;
    //}
    public NumberESD(double value, double esd, string? stringFormat, string? numberText)
    {
        Value = value;
        ESD = esd;
        this.stringFormat = stringFormat;
        this.numberText = numberText;
        HasValue = true;
    }
    public NumberESD(double value, double esd)
    {
        Value = value;
        ESD = esd;
        HasValue = true;
    }
    public NumberESD(double value, string? stringFormat)
    {
        Value = value;
        this.stringFormat = stringFormat;
        ESD = double.NaN;
        HasValue = true;
    }
    public NumberESD(string numberText)
    {
        this.numberText = numberText;
        Value = double.NaN;
        ESD = double.NaN;
        HasValue = true;
    }

    public int CompareTo(object? obj)
    {
        //https://learn.microsoft.com/pt-br/troubleshoot/developer/visualstudio/csharp/language-compilers/use-icomparable-icomparer
        NumberESD? number = (NumberESD)obj!;
        if (number is not null)
            return Value.CompareTo(number.Value);

        double? doub = (double)obj;
        if (doub is not null)
            return Value.CompareTo(doub);

        throw new ArithmeticException();
    }
}
