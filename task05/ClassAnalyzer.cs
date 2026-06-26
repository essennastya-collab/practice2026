using System;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
namespace task05;
public class ClassAnalyzer
{
    private Type _type;

    public ClassAnalyzer(Type type)
    {
        _type = type;
    }
    public IEnumerable<string> GetPublicMethods()
    {
     return _type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
                .Select(m => m.Name);
    }
    public IEnumerable<string> GetMethodParams(string methodName)
    {
        var method = _type.GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
        if (method == null)
        {
            return Enumerable.Empty<string>();
        }
        var param = method.GetParameters().Select(p => p.Name);
        var type = new[] { method.ReturnType.Name };
        return param.Concat(type);
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
        return _type.GetCustomAttributes(typeof(T), inherit: true).Any();
    }
}
