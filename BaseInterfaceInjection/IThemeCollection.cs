using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BaseLibrary.IThemeChanger;

namespace BaseLibrary;

public interface IThemeCollection
{
    List<(char type, object theme)> Themes { get; }
    int Length => Themes.Count;
    public (char type, object theme) this[int index]
    {
        get => Themes[index];
        set => Themes[index] = value;
    }
}
