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
    public string? Text { get; private set; } = null;
    public string? StringFormat { get; private set; } = null;
    public string? NumberText { get => !HasValue ? null : !double.IsNaN(Value) ? NumberMethods.DoubleResultText(Value, ESD, StringFormat) : !string.IsNullOrWhiteSpace(Text) ? Text : "-"; }
    public bool HasValue { get; set; }
    public NumberESD()
    {
        Value = double.NaN;
        ESD = double.NaN;
        this.StringFormat = null;
        this.Text = null;
        HasValue = false;
    }
    public NumberESD(double value, double esd, string? stringFormat, string? text)
    {
        Value = value;
        ESD = esd;
        this.StringFormat = stringFormat;
        this.Text = text;
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
        this.StringFormat = stringFormat;
        ESD = double.NaN;
        HasValue = true;
    }
    public NumberESD(string text)
    {
        this.Text = text;
        Value = double.NaN;
        ESD = double.NaN;
        HasValue = true;
    }
    
    public int CompareTo(object? obj)
    {
        //https://learn.microsoft.com/pt-br/troubleshoot/developer/visualstudio/csharp/language-compilers/use-icomparable-icomparer
        NumberESD? number = (NumberESD)obj!;
        if (number is not null)
            return Value.CompareTo(((NumberESD)number).Value);

        double? doub = (double)obj;
        if (doub is not null)
            return Value.CompareTo((double)doub);

        throw new ArithmeticException();
    }
    public override string ToString()
    {
        return NumberText;
    }
}
