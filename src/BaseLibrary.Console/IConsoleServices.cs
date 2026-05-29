using System.Text;

namespace BaseLibrary;

public interface IConsoleServices
{
    void WriteProgressBar(int percent, int progress = -1, bool update = false);
    void WriteProgress(int progress, bool update = false);
    GOSResult ExecCommandLine(string cmd, string? args, string? workFolder, bool isAsync, bool isShell, bool isQuite, bool isEscaped);
    bool DialogYesNo(string message);
    /// <summary>
    /// 0: White; 1: Green; 2: Yellow; 3: Red; 4: Gray
    /// </summary>
    /// <param name="str"></param>
    /// <param name="color"></param>
    void Write(string str, int color = 0, StringBuilder? sb = null);
    /// <summary>
    ///0: White; 1: Green; 2: Yellow; 3: Red; 4: Gray
    /// </summary>
    /// <param name="str"></param>
    /// <param name="color"></param>
    void WriteLine(string str = "", int color = 0, StringBuilder? sb = null);
    /// <summary>
    /// 0: White; 1: Green; 2: Yellow; 3: Red
    /// </summary>
    /// <param name="str"></param>
    /// <param name="color"></param>
    void EraseAndWrite(int eraseLength, string str, int color = 0);
    /// <summary>
    /// 0: White; 1: Green; 2: Yellow; 3: Red
    /// </summary>
    /// <param name="str"></param>
    /// <param name="color"></param>
    void EraseAndWrite(string oldStr, string newStr, int color = 0);
    bool ProcessGOSResult(GOSResult gosResult, StringBuilder logTemp);
    void OpenFile(string fileName, sbyte os);
    void InitizaliseInternalLog();
    string? GetInternalLog();
    void Clear();
    GOSResult RunProcess(string fileName, string arguments, string? workFolder, bool useShellWindow);

    // ------------------------------------------------------------------
    // Área de status fixa ("sticky") no rodapé do terminal
    // ------------------------------------------------------------------

    /// <summary>
    /// Escreve ou atualiza em-lugar as linhas de status fixas no rodapé do terminal.
    /// Em terminal interativo usa escape ANSI para reescrever sem scroll; quando a
    /// saída está redirecionada (arquivo/pipe) a chamada é ignorada para não poluir
    /// o log com códigos de escape.
    /// </summary>
    void UpdateStatusLines(params string[] lines);

    /// <summary>
    /// Escreve uma linha de log acima da área de status fixa, preservando o rodapé.
    /// Em terminal interativo sobe o cursor, imprime a linha e reimprime o status;
    /// quando redirecionado escreve normalmente com <see cref="WriteLine"/>.
    /// </summary>
    void WriteLineKeepingStatus(string line, int color = 0);

    /// <summary>
    /// Remove a área de status fixa do rodapé, apagando as linhas e posicionando
    /// o cursor onde o status começava, pronto para o próximo <see cref="WriteLine"/>.
    /// </summary>
    void ClearStatusLines();
}
