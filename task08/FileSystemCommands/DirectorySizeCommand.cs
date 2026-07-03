using System.IO;
using System.Linq;
using CommandLib;
namespace FileSystemCommands;
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