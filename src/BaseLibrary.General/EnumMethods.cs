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
    /// <remarks>
    /// Compatibilizado com Native AOT: usa <c>Enum.GetValues&lt;T&gt;()</c> (genérico)
    /// em vez de <c>Enum.GetValues(Type)</c>. Requer um tipo de enum concreto real.
    /// </remarks>
    public static IEnumerable<T> GetEnumValues<T>() where T : struct, Enum
    {
        return enumServices.GetEnumValues<T>();
    }
    public static T ParseEnum<T>(this string name) where T : struct, Enum
    {
        return enumServices.ParseEnum<T>(name);
    }
}
