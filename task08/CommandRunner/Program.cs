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
            Console.WriteLine("Error");
            return;
        }

        Assembly assembly = Assembly.LoadFrom(args[0]);

        string dir = Path.Combine(Path.GetTempPath(), "Dir");
        Directory.CreateDirectory(dir);
        File.WriteAllText(Path.Combine(dir, "file1.txt"), "Hello World");
        File.WriteAllText(Path.Combine(dir, "file2.doc"), "Document");
        File.WriteAllText(Path.Combine(dir, "file3.txt"), "README");

        var commandTypes = assembly.GetTypes()
            .Where(t => typeof(ICommand).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

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

                var instance = (ICommand)Activator.CreateInstance(type, arguments)!;
                instance.Execute();

                Console.WriteLine($"\nExecuted: {type.Name}");
                foreach (var prop in type.GetProperties())
                {
                    var value = prop.GetValue(instance);
                    if (value != null)
                    {
                        Console.WriteLine($"  {prop.Name}: {value}");
                    }
                }
            }
        }
    }
}
