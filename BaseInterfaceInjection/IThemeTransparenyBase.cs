﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseLibrary;

public interface IThemeTransparencyBase : IThemeBase
{
    bool Transparency { get; }
    bool isAllTransparent { get; }
}
