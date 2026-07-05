using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace PluginLib;

public interface IPlugin
{
    void Execute();
}

[AttributeUsage(AttributeTargets.Class)]
public class PluginLoadAttribute : Attribute
{
    public string[] Dependencies { get; set; } = Array.Empty<string>();
}

public class PluginManager
{
    public class PluginHost
    {
        private readonly List<Type> _plugins = new();

        public IReadOnlyList<Type> Plugins => _plugins.AsReadOnly();

        public void LoadFromDirectory(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
                throw new DirectoryNotFoundException($"Directory not found: {directoryPath}");

            var dllFiles = Directory.GetFiles(directoryPath, "*.dll");
            
            foreach (var dllFile in dllFiles)
            {
                var assembly = Assembly.LoadFrom(dllFile);
                var pluginTypes = from type in assembly.GetTypes()
                                  where type.GetCustomAttribute<PluginLoadAttribute>() != null
                                  where typeof(IPlugin).IsAssignableFrom(type)
                                  where !type.IsAbstract
                                  select type;

                _plugins.AddRange(pluginTypes);
            }
        }

        public void LoadFromTypes(IEnumerable<Type> types)
        {
            var validPlugins = from type in types
                               where type.GetCustomAttribute<PluginLoadAttribute>() != null
                               where typeof(IPlugin).IsAssignableFrom(type)
                               where !type.IsAbstract
                               select type;

            _plugins.AddRange(validPlugins);
        }

        public void ExecuteAll()
        {
            var sortedPlugins = GetTopologicalOrder();
            
            foreach (var pluginType in sortedPlugins)
            {
                var instance = (IPlugin)Activator.CreateInstance(pluginType)!;
                instance.Execute();
            }
        }

        private List<Type> GetTopologicalOrder()
        {
            var result = new List<Type>();
            var processed = new HashSet<Type>();

            foreach (var plugin in _plugins)
            {
                ProcessDependencies(plugin, processed, result);
            }

            return result;
        }

        private void ProcessDependencies(Type pluginType, HashSet<Type> processed, List<Type> result)
        {
            if (!processed.Add(pluginType))
                return;

            var attribute = pluginType.GetCustomAttribute<PluginLoadAttribute>();
            
            if (attribute?.Dependencies != null)
            {
                foreach (var dependencyName in attribute.Dependencies)
                {
                    var dependency = _plugins.FirstOrDefault(p => p.Name == dependencyName);
                    
                    if (dependency != null)
                    {
                        ProcessDependencies(dependency, processed, result);
                    }
                }
            }

            result.Add(pluginType);
        }
    }
}