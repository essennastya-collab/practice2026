using System;
using System.IO;
using System.Linq;
using System.Reflection;
using CommandLib;

namespace CommandRunner;

public class Program
{
    public static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Ошибка: путь к DLL не указан.");
            Console.WriteLine("Использование: CommandRunner <путь-к-dll>");
            return;
        }

        string dllPath = args[0];

        if (!File.Exists(dllPath))
        {
            Console.WriteLine($"Ошибка: файл не найден: {dllPath}");
            return;
        }

        Assembly assembly;
        try
        {
            assembly = Assembly.LoadFrom(dllPath);
        }
        catch (BadImageFormatException)
        {
            Console.WriteLine($"Ошибка: файл не является корректной .NET сборкой: {dllPath}");
            return;
        }
        catch (FileLoadException ex)
        {
            Console.WriteLine($"Ошибка: не удалось загрузить сборку: {ex.Message}");
            return;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка: непредвиденная ошибка при загрузке сборки: {ex.Message}");
            return;
        }

        string dir = Path.Combine(Path.GetTempPath(), "Dir");
        Directory.CreateDirectory(dir);
        File.WriteAllText(Path.Combine(dir, "file1.txt"), "Hello World");
        File.WriteAllText(Path.Combine(dir, "file2.doc"), "Document");
        File.WriteAllText(Path.Combine(dir, "file3.txt"), "README");

        var commandTypes = assembly.GetTypes()
            .Where(t => typeof(ICommand).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
            .ToList();

        if (commandTypes.Count == 0)
        {
            Console.WriteLine("Предупреждение: в сборке не найдено команд.");
            return;
        }

        foreach (var type in commandTypes)
        {
            var constructors = type.GetConstructors();
            if (constructors.Length > 0)
            {
                var parameters = constructors[0].GetParameters();
                var arguments = new object[parameters.Length];
                for (int i = 0; i < parameters.Length; i++)
                {
                    if (parameters[i].ParameterType == typeof(string))
                        arguments[i] = dir;
                    else
                        arguments[i] = "*.txt";
                }

                try
                {
                    var instance = (ICommand)Activator.CreateInstance(type, arguments)!;
                    instance.Execute();

                    Console.WriteLine($"\nВыполнена команда: {type.Name}");
                    foreach (var prop in type.GetProperties())
                    {
                        var value = prop.GetValue(instance);
                        if (value != null)
                        {
                            Console.WriteLine($"  {prop.Name}: {value}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при выполнении команды {type.Name}: {ex.Message}");
                }
            }
        }
    }
}