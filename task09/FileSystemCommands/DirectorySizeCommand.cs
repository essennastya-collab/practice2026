using System.IO;
using System.Linq;
using CommandLib;
using FileSystemCommands.Attributes; 
namespace FileSystemCommands;
[DisplayName("Определение размера каталога")]
[Version(1,0)]
public class DirectorySizeCommand : ICommand
{
    private readonly string _path;
    public long Size { get; private set; }

    public DirectorySizeCommand(string path)
    {
        _path = path;
    }

    public void Execute()
    {
        Size = Directory.GetFiles(_path, "*", SearchOption.AllDirectories)
                        .Sum(f => new FileInfo(f).Length);
    }
} 
