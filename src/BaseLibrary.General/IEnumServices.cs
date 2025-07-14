
namespace BaseLibrary
{
    public interface IEnumServices
    {
        string GetEnumName<T>(T value) where T : Enum;
        IEnumerable<string> GetEnumNames<T>() where T : Enum;
        IEnumerable<T> GetEnumValues<T>() where T : Enum;
        T ParseEnum<T>(string name) where T : struct, Enum;
    }
}