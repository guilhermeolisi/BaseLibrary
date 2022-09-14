namespace BaseLibrary;

public interface ICopyTextClipboard : IInteractionInvokeServices
{
    Task<string?> CopyFromClipBoard();
    Task CopyToClipBoard(string message);
}
