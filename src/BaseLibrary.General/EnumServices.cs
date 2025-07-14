using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseLibrary;

public class EnumServices : IEnumServices
{
    public string GetEnumName<T>(T value) where T : Enum
    {
        return Enum.GetName(typeof(T), value) ?? throw new ArgumentException("Invalid enum value", nameof(value));
    }
    public IEnumerable<string> GetEnumNames<T>() where T : Enum
    {
        return Enum.GetNames(typeof(T));
    }
    public IEnumerable<T> GetEnumValues<T>() where T : Enum
    {
        return Enum.GetValues(typeof(T)).Cast<T>();
    }

    public T ParseEnum<T>(string name) where T : struct, Enum // Fix: Add 'struct' constraint to ensure T is a non-nullable value type
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be null or whitespace.", nameof(name));

        if (Enum.TryParse(name, out T result)) // Fix: Adjust Enum.TryParse usage to match the constraint
        {
            return result;
        }
        else
        {
            throw new ArgumentException($"'{name}' is not a valid value for enum '{typeof(T).Name}'.", nameof(name));
        }
    }
}
