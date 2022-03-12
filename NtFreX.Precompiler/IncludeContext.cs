namespace NtFreX.Precompiler;

internal class IncludeContext
{
    private HashSet<string> includes = new();

    public bool TryAddInclude(string path, string basePath, out string normalized)
    {
        normalized = PathExtensions.NormalizeRelativePath(path, basePath);

        if (includes.Contains(normalized))
            return false;

        includes.Add(normalized);
        return true;
    }
}