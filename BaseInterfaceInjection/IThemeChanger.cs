using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseLibrary;

public interface IThemeChanger
{
    Task SetTheme(char theme);
    Task SetTransparency(bool isTransparent, bool isOnlyPanel, char? type);
}
