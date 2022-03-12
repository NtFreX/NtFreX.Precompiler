using Xunit;

namespace NtFreX.Precompiler.Tests;

public class PathNormalizationTests
{
    [Theory]
#if IS_WINDOWS
    [InlineData(@"C:\projects\", "./test.xml", @"C:\PROJECTS\TEST.XML")]
    [InlineData(@"C:\projects.x", "./test.xml", @"C:\TEST.XML")]
    [InlineData(@"C:\projects", "./test.xml", @"C:\TEST.XML")]
    [InlineData(@"C:\projects\", "../test.xml", @"C:\TEST.XML")]
    [InlineData(@"C:\projects\", "../test/test.xml", @"C:\TEST\TEST.XML")]
    [InlineData(@"C:\projects\path\", "../test/test.xml", @"C:\PROJECTS\TEST\TEST.XML")]
    [InlineData(@"C:\projects\path\", "../test/../test.xml", @"C:\PROJECTS\TEST.XML")]
#else
    [InlineData(@"/home/projects/", "./test.xml", @"/home/projects/test.xml")]
    [InlineData(@"/home/projects.x", "./test.xml", @"/home/test.xml")]
    [InlineData(@"/home/projects", "./test.xml", @"/home/test.xml")]
    [InlineData(@"/home/projects\", "../test.xml", @"/home/test.xml")]
    [InlineData(@"/home/projects\", "../test/test.xml", @"/home/test/test.xml")]
    [InlineData(@"/home/projects\path\", "../test/test.xml", @"/home/projects/test/test.xml")]
    [InlineData(@"/home/projects\path\", "../test/../test.xml", @"/home/projects/test.xml")]
#endif
    public void CanNormalizePath(string root, string input, string expected)
    {
        var normalized = PathExtensions.NormalizeRelativePath(input, root);
        Assert.Equal(expected, normalized);
    }
}
