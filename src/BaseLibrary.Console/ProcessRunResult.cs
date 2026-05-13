namespace BaseLibrary;

public sealed record ProcessRunResult(
    bool Started,
    bool Success,
    int? ExitCode,
    string StandardOutput,
    string StandardError,
    Exception? Exception);
