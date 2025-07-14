using System.Runtime.InteropServices;

namespace BaseLibrary;

public static class SystemUtility
{
    /// <summary>
    /// 
    /// </summary>
    /// <returns>int 0 Window, 1 Linux, 2 OSX</returns>
    public static (PlatformID, sbyte, bool) OSCheck()
    {
        PlatformID platform = Environment.OSVersion.Platform;
        bool is64x = System.Environment.Is64BitOperatingSystem;
        sbyte os;
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
    public static WindowsVersion WindowsVersionCheck()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return WindowsVersion.Unknown;
        }
        //Get Operating system information.
        OperatingSystem os = Environment.OSVersion;
        //Get version information about the os.
        Version vs = os.Version;

        //Variable to hold our return value
        WindowsVersion operatingSystem = WindowsVersion.Unknown;

        if (os.Platform == PlatformID.Win32Windows)
        {
            //This is a pre-NT version of Windows
            switch (vs.Minor)
            {
                case 0:
                    operatingSystem = WindowsVersion.Windows95; //"95";
                    break;
                case 10:
                    if (vs.Revision.ToString() == "2222A")
                        operatingSystem = WindowsVersion.Windows98SE; //"98SE";
                    else
                        operatingSystem = WindowsVersion.Windows98; //"98";
                    break;
                case 90:
                    operatingSystem = WindowsVersion.WindowsMe; //"Me";
                    break;
                default:
                    break;
            }
        }
        else if (os.Platform == PlatformID.Win32NT)
        {
            switch (vs.Major)
            {
                case 3:
                    operatingSystem = WindowsVersion.WindowsNT351; // "NT 3.51";
                    break;
                case 4:
                    operatingSystem = WindowsVersion.WindowsNT40; //"NT 4.0";
                    break;
                case 5:
                    if (vs.Minor == 0)
                        operatingSystem = WindowsVersion.Windows2000; // "2000";
                    else
                        operatingSystem = WindowsVersion.WindowsXP; // "XP";
                    break;
                case 6:
                    if (vs.Minor == 0)
                        operatingSystem = WindowsVersion.WindowsVista;// "Vista";
                    else if (vs.Minor == 1)
                        operatingSystem = WindowsVersion.Windows7;// "7";
                    else if (vs.Minor == 2)
                        operatingSystem = WindowsVersion.Windows8;// "8";
                    else
                        operatingSystem = WindowsVersion.Windows81;// "8.1";
                    break;
                case 10:
                    if (vs.Build < 22000)
                    {
                        operatingSystem = WindowsVersion.Windows10;// "10";
                    }
                    else
                    {
                        operatingSystem = WindowsVersion.Windows11;// "11";
                    }
                    break;
                //case 11:
                //    operatingSystem = WindowsVersion.Windows11;// "11";
                //    break;
                default:
                    break;
            }
        }
        return operatingSystem;
    }
    public static string SystemVersionToString(sbyte OS, WindowsVersion version)
    {
        string result = (OS == 0 ? "Windows" : OS == 1 ? "Linux" : OS == 2 ? "macOS" : null);
        if (string.IsNullOrWhiteSpace(result))
            return result;
        switch (OS)
        {
            case 0:
                result += " " + version switch
                {
                    WindowsVersion.Windows95 => "95",
                    WindowsVersion.Windows98SE => "98SE",
                    WindowsVersion.Windows98 => "98",
                    WindowsVersion.WindowsMe => "Me",
                    WindowsVersion.WindowsNT351 => "NT 3.51",
                    WindowsVersion.WindowsNT40 => "NT 4.0",
                    WindowsVersion.Windows2000 => "2000",
                    WindowsVersion.WindowsXP => "XP",
                    WindowsVersion.WindowsVista => "Vista",
                    WindowsVersion.Windows7 => "7",
                    WindowsVersion.Windows8 => "8",
                    WindowsVersion.Windows81 => "8.1",
                    WindowsVersion.Windows10 => "10",
                    WindowsVersion.Windows11 => "11",
                    WindowsVersion.Unknown => "Unknown",
                    _ => ""
                };
                break;
        }
        return result.Trim();
    }
    public static bool IsRunningInWSL()
    {
        try
        {
            // Verifica se o arquivo /proc/version existe
            if (File.Exists("/proc/version"))
            {
                string versionInfo = File.ReadAllText("/proc/version");

                // Verifica se o arquivo contém a string "Microsoft"
                if (versionInfo.Contains("Microsoft"))
                {
                    return true;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao verificar o ambiente: {ex.Message}");
        }

        return false;
    }
    /// <summary>
    /// https://stackoverflow.com/questions/2819934/detect-windows-version-in-net
    /// </summary>
    /// <returns>0: 95; 1: 98; 2: 98SE; 3: Me; 4: NT 3.51; 5: NT 4.0; 6: 2000; 7: XP; 8: Vsta; 9: 7; 10: 8; 11: 8.1; 12: 10; 13: 11</returns>
    //public static sbyte WindowsVersionCheck()
    //{
    //    if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
    //    {
    //        return -1;
    //    }
    //    //Get Operating system information.
    //    OperatingSystem os = Environment.OSVersion;
    //    //Get version information about the os.
    //    Version vs = os.Version;

    //    //Variable to hold our return value
    //    sbyte operatingSystem = -1;

    //    if (os.Platform == PlatformID.Win32Windows)
    //    {
    //        //This is a pre-NT version of Windows
    //        switch (vs.Minor)
    //        {
    //            case 0:
    //                operatingSystem = 0; //"95";
    //                break;
    //            case 10:
    //                if (vs.Revision.ToString() == "2222A")
    //                    operatingSystem = 2; //"98SE";
    //                else
    //                    operatingSystem = 1; //"98";
    //                break;
    //            case 90:
    //                operatingSystem = 3; //"Me";
    //                break;
    //            default:
    //                break;
    //        }
    //    }
    //    else if (os.Platform == PlatformID.Win32NT)
    //    {
    //        switch (vs.Major)
    //        {
    //            case 3:
    //                operatingSystem = 4; // "NT 3.51";
    //                break;
    //            case 4:
    //                operatingSystem = 5; //"NT 4.0";
    //                break;
    //            case 5:
    //                if (vs.Minor == 0)
    //                    operatingSystem = 6; // "2000";
    //                else
    //                    operatingSystem = 7; // "XP";
    //                break;
    //            case 6:
    //                if (vs.Minor == 0)
    //                    operatingSystem = 8;// "Vista";
    //                else if (vs.Minor == 1)
    //                    operatingSystem = 9;// "7";
    //                else if (vs.Minor == 2)
    //                    operatingSystem = 10;// "8";
    //                else
    //                    operatingSystem = 11;// "8.1";
    //                break;
    //            case 10:
    //                operatingSystem = 12;// "10";
    //                break;
    //            case 11:
    //                operatingSystem = 13;// "11";
    //                break;
    //            default:
    //                break;
    //        }
    //    }
    //    return operatingSystem;
    //}
    //public static string SystemVersionToString(sbyte OS, sbyte version)
    //{
    //    string result = (OS == 0 ? "Windows" : OS == 1 ? "Linux" : OS == 2 ? "macOS" : null);
    //    if (string.IsNullOrWhiteSpace(result))
    //        return result;
    //    switch (OS)
    //    {
    //        case 0:
    //            result += " " + version switch
    //            {
    //                0 => "95",
    //                2 => "98SE",
    //                1 => "98",
    //                3 => "Me",
    //                4 => "NT 3.51",
    //                5 => "NT 4.0",
    //                6 => "2000",
    //                7 => "XP",
    //                8 => "Vista",
    //                9 => "7",
    //                10 => "8",
    //                11 => "8.1",
    //                12 => "10",
    //                13 => "11",
    //                _ => ""
    //            };
    //            break;
    //    }
    //    return result.Trim();
    //}
    public enum WindowsVersion : sbyte
    {
        Unknown = -1,
        Windows95,
        Windows98,
        Windows98SE,
        WindowsMe,
        WindowsNT351,
        WindowsNT40,
        Windows2000,
        WindowsXP,
        WindowsVista,
        Windows7,
        Windows8,
        Windows81,
        Windows10,
        Windows11
    }
}

