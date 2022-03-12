namespace NtFreX.Precompiler;

public class CompilationException : Exception
{
    public string[] RawCode { get; set; } = Array.Empty<string>();
    public string[] PreCompiledCode { get; set; } = Array.Empty<string>();
    public string[] PreCompiledLines { get; set; } = Array.Empty<string>();

    public string? ErrorType { get; set; } = null;
    public uint? LineNumber { get; set; } = null;

    public CompilationException(string message, Exception innerException)
        : base(message, innerException) { }
}