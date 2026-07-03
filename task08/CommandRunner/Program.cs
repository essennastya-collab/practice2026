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
                    arguments[i] = "*.txt";
                }
                var instance = (ICommand)Activator.CreateInstance(type, arguments)!;
                instance.Execute();
            }
        }
    }
}