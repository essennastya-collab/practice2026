 using System.IO;
using System.Collections.Generic;
using CommandLib;
using FileSystemCommands.Attributes; 
namespace FileSystemCommands;
[DisplayName("Нахождение файлов по маске")]
[Version(1,0)]

public class FindFilesCommand : ICommand
{
    private readonly string _path;
    private readonly string _pattern;
    public List<string> FoundFiles { get; private set; } = new();

    public FindFilesCommand(string path, string pattern)
    {
        _path = path;
        _pattern = pattern;
    }

    public void Execute()
    {
        FoundFiles = Directory.EnumerateFiles(_path, _pattern, SearchOption.AllDirectories).ToList();
    }
}
