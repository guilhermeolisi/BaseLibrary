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
#pragma warning disable CS8766 // Nullability of reference types in return type doesn't match implicitly implemented member (possibly because of nullability attributes).
    public IEnumerable<(char type, IThemeBase theme)>? GetAllThemes() => values;
#pragma warning restore CS8766 // Nullability of reference types in return type doesn't match implicitly implemented member (possibly because of nullability attributes).
}
