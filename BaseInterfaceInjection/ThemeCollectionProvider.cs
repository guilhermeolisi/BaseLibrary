using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BaseLibrary.IThemeChanger;

namespace BaseLibrary;

public interface IThemeCollectionProvider
{
    IEnumerable<(char type, IThemeBase theme)> GetAllThemes();
}
