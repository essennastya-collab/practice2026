using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Metadata;

public class Program
{
    public static void Main(string[] args)
    {
        if (args.Length == 0)
        {
            Console.WriteLine("Ошибка: путь к DLL не указан.");
            Console.WriteLine("Использование: Metadata <путь-к-dll>");
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

        try
        {
            foreach (var type in assembly.GetTypes().Where(t => !t.IsInterface && !t.IsAbstract))
            {
                Console.WriteLine($"\nClass: {type.Name}");

                var attributes = type.GetCustomAttributes(false);
                if (attributes.Length > 0)
                {
                    Console.WriteLine("  Attributes:");
                    foreach (var attr in attributes)
                    {
                        Console.WriteLine($"    - {attr.GetType().Name}");
                    }
                }

                foreach (var ctor in type.GetConstructors())
                {
                    var parameters = ctor.GetParameters();
                    var paramInfo = string.Join(", ", parameters.Select(p => $"{p.ParameterType.Name} {p.Name}"));
                    Console.WriteLine($"  Constructor: {type.Name}({paramInfo})");
                }

                foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
                {
                    var parameters = method.GetParameters();
                    var paramInfo = string.Join(", ", parameters.Select(p => $"{p.ParameterType.Name} {p.Name}"));
                    Console.WriteLine($"  Method: {method.ReturnType.Name} {method.Name}({paramInfo})");

                    var methodAttrs = method.GetCustomAttributes(false);
                    if (methodAttrs.Length > 0)
                    {
                        Console.WriteLine("    Attributes:");
                        foreach (var attr in methodAttrs)
                        {
                            Console.WriteLine($"      - {attr.GetType().Name}");
                        }
                    }
                }
            }
        }
        catch (ReflectionTypeLoadException ex)
        {
            Console.WriteLine($"Ошибка: не удалось загрузить типы из сборки: {ex.Message}");
            foreach (var loaderEx in ex.LoaderExceptions)
            {
                Console.WriteLine($"  - {loaderEx.Message}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при анализе метаданных: {ex.Message}");
        }
    }
}