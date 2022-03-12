namespace NtFreX.Precompiler;

internal class SyntaxInclude : SyntaxTreeNode
{
    private readonly Precompiler currentCompiler;
    private readonly IncludeContext includeContext;
    private readonly string basePath;
    private readonly string path;

    public SyntaxInclude(Precompiler currentCompiler, IncludeContext includeContext, string basePath, string path)
    {
        this.currentCompiler = currentCompiler;
        this.includeContext = includeContext;
        this.basePath = basePath;
        this.path = path;
    }

    public override string Precompile()
    {
        if (includeContext.TryAddInclude(path, basePath, out var normalizedPath))
            return currentCompiler.Precompile(File.ReadAllText(normalizedPath), normalizedPath, includeContext);

        return string.Empty;
    }

    public override string ToString()
        => "{ Include: path=" + path + ", base=" + basePath + " }";
}
