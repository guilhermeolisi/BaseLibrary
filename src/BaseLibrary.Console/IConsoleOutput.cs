namespace BaseLibrary;

public interface IConsoleOutput
{
    bool IsInputRedirected { get; }
    void Clear();
    ConsoleKeyInfo ReadKey(bool intercept = false);
    void ResetColor();
    void SetForegroundColor(ConsoleColor color);
    void Write(string value);
    void WriteLine(string? value = null);
}
