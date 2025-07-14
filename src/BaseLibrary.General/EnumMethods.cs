using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseLibrary;

public static class EnumMethods
{
    static IEnumServices enumServices = new EnumServices();
    public static string GetEnumName<T>(this T value) where T : Enum
    {
        return enumServices.GetEnumName(value);
    }
    public static IEnumerable<string> GetEnumNames<T>() where T : Enum
    {
        return enumServices.GetEnumNames<T>();
    }
    public static IEnumerable<T> GetEnumValues<T>() where T : Enum
    {
        return enumServices.GetEnumValues<T>();
    }
    public static T ParseEnum<T>(this string name) where T : struct, Enum
    {
        return enumServices.ParseEnum<T>(name);
    }
}
