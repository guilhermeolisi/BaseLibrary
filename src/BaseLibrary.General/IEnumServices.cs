
namespace BaseLibrary
{
    public interface IEnumServices
    {
        string GetEnumName<T>(T value) where T : Enum;
        IEnumerable<string> GetEnumNames<T>() where T : Enum;
        /// <remarks>
        /// Compatibilizado com Native AOT: usa <c>Enum.GetValues&lt;T&gt;()</c>.
        /// Requer um tipo de enum concreto real.
        /// </remarks>
        IEnumerable<T> GetEnumValues<T>() where T : struct, Enum;
        T ParseEnum<T>(string name) where T : struct, Enum;
    }
}