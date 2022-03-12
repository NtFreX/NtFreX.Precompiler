using System.Collections.Generic;
using Xunit;

namespace NtFreX.Precompiler.Tests;

public class PrecompilerTests
{
    [Theory]
    [InlineData("#{variable}", new[] { "variable" }, new[] { "hello" }, "hello")]
    [InlineData("#{variable}", new[] { "variable" }, new[] { "hello world" }, "hello world")]
    [InlineData("#{VARIABLE_}", new[] { "VARIABLE_" }, new[] { "hello world" }, "hello world")]
    public void CanReplaceExistingVariables(string input, string[] variableKeys, string[] variableValues, string expected)
    {
        var values = new Dictionary<string, string>();
        for (var i = 0; i < variableKeys.Length; i++)
        {
            values.Add(variableKeys[i], variableValues[i]);
        }

        var precompiled = Precompiler.PrecompileText(new Dictionary<string, bool>(), values, input);
        Assert.Equal(expected, precompiled);
    }
}