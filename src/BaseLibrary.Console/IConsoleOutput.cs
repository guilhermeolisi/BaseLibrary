namespace BaseLibrary;

public interface IConsoleOutput
{
    bool IsInputRedirected { get; }
    bool IsOutputRedirected { get; }
    /// <summary>
    /// Largura da janela do terminal em colunas. Retorna um valor padrão seguro
    /// (ex.: 120) quando o output está redirecionado ou a janela não está disponível.
    /// </summary>
    int GetWindowWidth();
    void Clear();
    ConsoleKeyInfo ReadKey(bool intercept = false);
    void ResetColor();
    void SetForegroundColor(ConsoleColor color);
    void Write(string value);
    void WriteLine(string? value = null);
}
