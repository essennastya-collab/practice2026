using Xunit;
using System;
using System.IO;
using System.Reflection;
using Metadata;

namespace task09tests;

public class MetadataTests
{
    [Fact]
    public void Main_ShouldDisplayClassMetadata()
    {
        var output = new StringWriter();
        Console.SetOut(output);

        var dllPath = Path.GetFullPath(Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "..", "..", "..", "..", "task09", "FileSystemCommands",
            "bin", "Debug", "net9.0", "FileSystemCommands.dll"));

        Program.Main(new string[] { dllPath });

        var result = output.ToString();
        Assert.Contains("Class:", result);
        Assert.Contains("Constructor:", result);
        Assert.Contains("Method:", result);
    }
}
