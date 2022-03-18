using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace BaseLibrary
{
    public static class SystemUtility
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns>int 0 Window, 1 Linux, 2 OSX</returns>
        public static (PlatformID, int, bool) OSCheck()
        {
            PlatformID platform = Environment.OSVersion.Platform;
            bool is64x = System.Environment.Is64BitOperatingSystem;
            int os;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                os = 0;
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                os = 1;
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                os = 2;
            }
            else
            {
                os = -1;
            }
            return (platform, os, is64x);
        }
    }
}
