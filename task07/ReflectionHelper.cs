using System;
using System.Linq;
using System.Reflection;

namespace task07;

public static class ReflectionHelper
{
    public static void PrintTypeInfo(Type type)
    {
        var Name = type.GetCustomAttribute<DisplayNameAttribute>();
        if (Name != null)
        {
            Console.WriteLine($"Class DisplayName: {Name.DisplayName}");
        }

        var Version = type.GetCustomAttribute<VersionAttribute>();
        if (Version != null)
        {
            Console.WriteLine($"Class Version: {Version.Major}.{Version.Minor}");
        }

        var methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly).Where(a => a.GetCustomAttribute<DisplayNameAttribute>() != null).ToList();

        foreach (var method in methods)
        {
            var attr = method.GetCustomAttribute<DisplayNameAttribute>();
            Console.WriteLine($"Method: {attr!.DisplayName} ({method.Name})");
        }

        var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly).Where(a => a.GetCustomAttribute<DisplayNameAttribute>() != null).ToList();

        foreach (var prop in properties)
        {
            var attr = prop.GetCustomAttribute<DisplayNameAttribute>();
            Console.WriteLine($"Property: {attr!.DisplayName} ({prop.Name})");
        }
    }
}