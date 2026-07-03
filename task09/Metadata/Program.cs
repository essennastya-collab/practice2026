using System;
using System.IO;
using System.Linq;
using System.Reflection;
namespace Metadata;

public class Program
{
    public static void Main(string[] args)
    {
        Assembly assembly = Assembly.LoadFrom(args[0]);

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
}
