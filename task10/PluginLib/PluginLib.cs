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
        private readonly List<string> _loadErrors = new();

        public IReadOnlyList<Type> Plugins => _plugins.AsReadOnly();
        public IReadOnlyList<string> LoadErrors => _loadErrors.AsReadOnly();

        public void LoadFromDirectory(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
                throw new DirectoryNotFoundException($"Directory not found: {directoryPath}");

            var dllFiles = Directory.GetFiles(directoryPath, "*.dll");
            
            foreach (var dllFile in dllFiles)
            {
                try
                {
                    var assembly = Assembly.LoadFrom(dllFile);
                    var pluginTypes = from type in assembly.GetTypes()
                                      where type.GetCustomAttribute<PluginLoadAttribute>() != null
                                      where typeof(IPlugin).IsAssignableFrom(type)
                                      where !type.IsAbstract
                                      select type;

                    _plugins.AddRange(pluginTypes);
                }
                catch (BadImageFormatException)
                {
                    _loadErrors.Add($"Файл не является корректной .NET сборкой: {dllFile}");
                }
                catch (FileLoadException ex)
                {
                    _loadErrors.Add($"Не удалось загрузить сборку {dllFile}: {ex.Message}");
                }
                catch (ReflectionTypeLoadException ex)
                {
                    _loadErrors.Add($"Ошибка загрузки типов из {dllFile}: {ex.Message}");
                }
                catch (Exception ex)
                {
                    _loadErrors.Add($"Непредвиденная ошибка при загрузке {dllFile}: {ex.Message}");
                }
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
                try
                {
                    var instance = (IPlugin)Activator.CreateInstance(pluginType)!;
                    instance.Execute();
                }
                catch (MissingMethodException)
                {
                    Console.WriteLine($"Ошибка: у плагина {pluginType.Name} отсутствует конструктор без параметров");
                }
                catch (TargetInvocationException ex)
                {
                    Console.WriteLine($"Ошибка при создании экземпляра {pluginType.Name}: {ex.InnerException?.Message}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Непредвиденная ошибка при выполнении {pluginType.Name}: {ex.Message}");
                }
            }
        }

        private List<Type> GetTopologicalOrder()
        {
            var result = new List<Type>();
            var state = new Dictionary<Type, VisitState>();
            
            foreach (var plugin in _plugins)
            {
                state[plugin] = VisitState.NotVisited;
            }

            foreach (var plugin in _plugins)
            {
                if (state[plugin] == VisitState.NotVisited)
                {
                    ProcessDependencies(plugin, state, result);
                }
            }

            return result;
        }

        private void ProcessDependencies(Type pluginType, Dictionary<Type, VisitState> state, List<Type> result)
        {
            if (state[pluginType] == VisitState.Visiting)
            {
                throw new InvalidOperationException(
                    $"Обнаружена циклическая зависимость с участием плагина {pluginType.Name}");
            }

            if (state[pluginType] == VisitState.Visited)
                return;

            state[pluginType] = VisitState.Visiting;

            var attribute = pluginType.GetCustomAttribute<PluginLoadAttribute>();
            
            if (attribute?.Dependencies != null)
            {
                foreach (var dependencyName in attribute.Dependencies)
                {
                    var dependency = _plugins.FirstOrDefault(p => p.Name == dependencyName);
                    
                    if (dependency != null)
                    {
                        ProcessDependencies(dependency, state, result);
                    }
                }
            }

            state[pluginType] = VisitState.Visited;
            result.Add(pluginType);
        }

        private enum VisitState
        {
            NotVisited,
            Visiting,
            Visited
        }
    }
}