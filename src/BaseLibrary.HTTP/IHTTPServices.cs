namespace BaseLibrary;

public interface IHTTPServices
{
    bool IsConnectedToInternetPing(string? host = null);
    bool IsConnectedToInternet();
    bool IsValidURL(string url);
    bool IsValidEmail(string email);
    public bool OpenUrlDefaultBrowse(string url);
}
