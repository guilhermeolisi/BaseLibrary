using System.ComponentModel;
using System.Data.Common;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text.Json;
using System.Xml;

namespace BaseLibrary;

/// <summary>
/// Enumeração AOT-safe das propriedades específicas dos tipos de exceção derivados
/// mais comuns. Substitui a reflexão (trim-unsafe, removida na compatibilização
/// Native AOT) por <c>switch</c> com pattern matching e acesso fortemente tipado:
/// nenhum membro é descoberto por reflexão, então o compilador AOT/trimmer não
/// precisa preservar metadados.
/// </summary>
/// <remarks>
/// A ordem dos casos vai do tipo mais derivado para o mais base (ex.:
/// <see cref="ArgumentNullException"/> antes de <see cref="ArgumentException"/>;
/// <see cref="SocketException"/> antes de <see cref="Win32Exception"/>, pois
/// <c>SocketException : Win32Exception</c>). Tipos não listados não acrescentam
/// propriedades — apenas os membros base de <see cref="System.Exception"/> são
/// emitidos por quem chama. <see cref="AggregateException"/> não é tratado aqui
/// porque já é expandido (InnerExceptions) pelo enumerador base.
/// </remarks>
internal static class KnownExceptionMembers
{
    /// <summary>
    /// Retorna pares (nome, valor) das propriedades declaradas pelo tipo concreto
    /// da exceção, além dos membros base. Vazio para tipos não mapeados.
    /// </summary>
    public static IEnumerable<(string Name, object? Value)> GetDerived(Exception exception)
    {
        switch (exception)
        {
            case ArgumentOutOfRangeException aoore:
                yield return ("ParamName", aoore.ParamName);
                yield return ("ActualValue", aoore.ActualValue);
                break;

            case ArgumentNullException ane:
                yield return ("ParamName", ane.ParamName);
                break;

            case ArgumentException ae:
                yield return ("ParamName", ae.ParamName);
                break;

            case ObjectDisposedException ode:
                yield return ("ObjectName", ode.ObjectName);
                break;

            case FileNotFoundException fnfe:
                yield return ("FileName", fnfe.FileName);
                yield return ("FusionLog", fnfe.FusionLog);
                break;

            case FileLoadException fle:
                yield return ("FileName", fle.FileName);
                yield return ("FusionLog", fle.FusionLog);
                break;

            // SocketException deriva de Win32Exception — tem que vir antes.
            case SocketException se:
                yield return ("SocketErrorCode", se.SocketErrorCode);
                yield return ("ErrorCode", se.ErrorCode);
                break;

            case Win32Exception w32:
                yield return ("NativeErrorCode", w32.NativeErrorCode);
                break;

            case HttpRequestException hre:
                yield return ("StatusCode", hre.StatusCode);
                yield return ("HttpRequestError", hre.HttpRequestError);
                break;

            case WebException we:
                yield return ("Status", we.Status);
                break;

            // Base comum de SqliteException, SqlException, etc. (sem dependência
            // do pacote do provedor — System.Data.Common é do shared framework).
            case DbException dbe:
                yield return ("ErrorCode", dbe.ErrorCode);
                yield return ("SqlState", dbe.SqlState);
                yield return ("IsTransient", dbe.IsTransient);
                break;

            case JsonException je:
                yield return ("Path", je.Path);
                yield return ("LineNumber", je.LineNumber);
                yield return ("BytePositionInLine", je.BytePositionInLine);
                break;

            case XmlException xe:
                yield return ("LineNumber", xe.LineNumber);
                yield return ("LinePosition", xe.LinePosition);
                yield return ("SourceUri", xe.SourceUri);
                break;
        }
    }
}
