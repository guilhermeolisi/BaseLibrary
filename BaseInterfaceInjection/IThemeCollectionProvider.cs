using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BaseLibrary.IThemeChanger;

namespace BaseLibrary;

public class ThemeCollectionProvider : IThemeCollectionProvider
{
    IEnumerable<(char type, IThemeBase theme)>? values;
    public ThemeCollectionProvider(IEnumerable<(char type, IThemeBase theme)>? value) => values = value;
    public IEnumerable<(char type, IThemeBase theme)>? GetAllThemes() => values;
}
