using System;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;

namespace task05;

public class ClassAnalyzer
{
    private readonly Type _type;

    public ClassAnalyzer(Type type)
    {
        if (type == null)
        {
            throw new ArgumentNullException(nameof(type), "Тип не может быть null");
        }
        _type = type;
    }

    public IEnumerable<string> GetPublicMethods()
    {
        return _type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
                    .Select(m => m.Name);
    }

    public IEnumerable<string> GetMethodParams(string methodName)
    {
        if (methodName == null)
        {
            throw new ArgumentNullException(nameof(methodName), "Имя метода не может быть null");
        }

        var method = _type.GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
        if (method == null)
        {
            return Enumerable.Empty<string>();
        }

        var param = method.GetParameters().Select(p => p.Name);
        var returnType = new[] { method.ReturnType.Name };
        return param.Concat(returnType);
    }

    public IEnumerable<string> GetAllFields()
    {
        return _type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
                    .Select(f => f.Name);
    }

    public IEnumerable<string> GetProperties()
    {
        return _type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
                    .Select(p => p.Name);
    }

    public bool HasAttribute<T>() where T : Attribute
    {
        return _type.IsDefined(typeof(T), inherit: true);
    }
}