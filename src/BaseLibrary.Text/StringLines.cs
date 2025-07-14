using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace BaseLibrary.Text;

public class StringLines
{
    int totalLines;
    Dictionary<uint, string> lines = new();
    uint indexLine = 0;
    public StringLines(int totalLines)
    {
        this.totalLines = totalLines;
    }
    public void ChangeTotalLines(int totalLines)
    {
        this.totalLines = totalLines;
    }
    public void Append(string text)
    {
        string[] strings = GetLinesFromString(text);
        AddLines(strings, false);
    }
    public void AppendLine(string text)
    {
        if (lines.Count != 0)
            indexLine++;
        lines.Add(indexLine, text);
    }
    public void AppendLines(string text)
    {
        string[] strings = GetLinesFromString(text);
        AddLines(strings, true);
    }
    private string[] GetLinesFromString(string text)
    {
        string[] strings = text.Split(new char[] { '\n', '\r' });
        return strings;
    }
    private void AddLines(string[] strings, bool isNewLine)
    {
        if (strings is null || strings.Length == 0)
            return;
        if (!isNewLine)
            lines[indexLine] += strings[0];
        for (int i = isNewLine ? 0 : 1; i < strings.Length; i++)
        {
            if (lines.Count != 0)
                indexLine++;
            lines.Add(indexLine, strings[i]);
        }
    }
    public void Clear()
    {
        lines.Clear();
    }
    public int Lenght()
    {
        return lines.Sum(x => x.Value.Length);
    }
    public int CountLines() => lines.Count() < totalLines ? lines.Count : totalLines;
    public int CountAllLines() => lines.Count();
    public string GetText()
    {
        return lines?.Count > 0 ? string.Join(Environment.NewLine, lines!.Where(x => x.Key >= lines!.Count - totalLines).Select(x => x.Value)) : string.Empty;
    }
    public string GetAllText()
    {
        return lines?.Count > 0 ? string.Empty : string.Join(Environment.NewLine, lines!.Select(x => x.Value));
    }

    //public void Append(string text)
    //{
    //    textlist.Add(index, text);
    //    index++;
    //}
    //public void AppendLine(string text)
    //{
    //    textlist.Add(index, text + Environment.NewLine);
    //    index++;
    //}
    //public void Clear()
    //{
    //    textlist.Clear();
    //}
    //public int Lenght()
    //{
    //    return textlist.Sum(x => x.Value.Length);
    //}
    //public int CountLines()
    //{
    //    int count = 0;

    //    foreach (var item in textlist)
    //    {
    //        for (int i = 0; i < item.Value.Length; i++)
    //        {
    //            if (item.Value[i] == '\r' || item.Value[i] == '\n')
    //            {
    //                count++;
    //                if (item.Value[i] == '\r' && i < item.Value.Length - 1 && item.Value[i + 1] == '\n')
    //                    i++;
    //            }
    //        }
    //    }

    //    return count;
    //}
    //public string GetText()
    //{
    //    int countLine = 0;
    //    return textlist?.Count > 0 ? string.Empty : string.Join(Environment.NewLine, textlist!.Where(x =>
    //    {

    //        return x.Key >= textlist!.Count - totalLines;
    //    }).Select(x => x.Value));
    //}
}
