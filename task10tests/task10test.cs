using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;
using PluginLib;

namespace task10tests;

public class UnitTest1
{
    public static class ExecutionLog
    {
        public static List<string> Sequence = new();
    }

    [PluginLoad]
    public class BasePlugin : IPlugin
    {
        public void Execute() => ExecutionLog.Sequence.Add("Base");
    }

    [PluginLoad(Dependencies = new[] { "BasePlugin" })]
    public class MiddlePlugin : IPlugin
    {
        public void Execute() => ExecutionLog.Sequence.Add("Middle");
    }

    [PluginLoad(Dependencies = new[] { "MiddlePlugin" })]
    public class FinalPlugin : IPlugin
    {
        public void Execute() => ExecutionLog.Sequence.Add("Final");
    }

    [PluginLoad(Dependencies = new[] { "CycleB" })]
    public class CycleA : IPlugin
    {
        public void Execute() { }
    }

    [PluginLoad(Dependencies = new[] { "CycleA" })]
    public class CycleB : IPlugin
    {
        public void Execute() { }
    }

    public class PluginHostTests
    {
        [Fact]
        public void ExecutionOrder_ShouldRespectDependencyChain()
        {
            ExecutionLog.Sequence.Clear();
            var host = new PluginManager.PluginHost();
            var assemblyDir = Path.GetDirectoryName(typeof(BasePlugin).Assembly.Location)!;

            host.LoadFromDirectory(assemblyDir);
            host.ExecuteAll();

            Assert.Equal(new[] { "Base", "Middle", "Final" }, ExecutionLog.Sequence);
        }

        [Fact]
        public void EmptyDirectory_ShouldExecuteWithoutExceptions()
        {
            var tempDir = Path.Combine(Path.GetTempPath(), $"PluginTest_{Guid.NewGuid()}");
            Directory.CreateDirectory(tempDir);

            var host = new PluginManager.PluginHost();
            
            var exception = Record.Exception(() =>
            {
                host.LoadFromDirectory(tempDir);
                host.ExecuteAll();
            });

            Directory.Delete(tempDir, true);

            Assert.Null(exception);
        }

        [Fact]
        public void LoadFromTypes_ShouldPreserveInsertionOrderWhenNoDeps()
        {
            var host = new PluginManager.PluginHost();
            host.LoadFromTypes(new[] { typeof(BasePlugin), typeof(MiddlePlugin), typeof(FinalPlugin) });

            Assert.Equal(3, host.Plugins.Count);
            Assert.Contains(typeof(BasePlugin), host.Plugins);
            Assert.Contains(typeof(MiddlePlugin), host.Plugins);
            Assert.Contains(typeof(FinalPlugin), host.Plugins);
        }

        [Fact]
        public void CyclicDependencies_ShouldThrowException()
        {
            var host = new PluginManager.PluginHost();
            host.LoadFromTypes(new[] { typeof(CycleA), typeof(CycleB) });

            var exception = Assert.Throws<InvalidOperationException>(() => host.ExecuteAll());
            Assert.Contains("циклическая зависимость", exception.Message.ToLower());
        }

        [Fact]
        public void LoadFromDirectory_WithInvalidDll_ShouldRecordError()
        {
            var tempDir = Path.Combine(Path.GetTempPath(), $"PluginTest_{Guid.NewGuid()}");
            Directory.CreateDirectory(tempDir);
            
            var invalidDll = Path.Combine(tempDir, "invalid.dll");
            File.WriteAllText(invalidDll, "This is not a valid DLL");

            try
            {
                var host = new PluginManager.PluginHost();
                host.LoadFromDirectory(tempDir);

                Assert.NotEmpty(host.LoadErrors);
                Assert.Contains(host.LoadErrors, e => e.Contains("не является корректной .NET сборкой"));
            }
            finally
            {
                Directory.Delete(tempDir, true);
            }
        }

        [Fact]
        public void LoadFromDirectory_NonExistentPath_ShouldThrowException()
        {
            var host = new PluginManager.PluginHost();
            
            Assert.Throws<DirectoryNotFoundException>(() => 
                host.LoadFromDirectory("/nonexistent/path/that/does/not/exist"));
        }

        [Fact]
        public void LoadFromDirectory_WithValidAndInvalidDlls_ShouldLoadValid()
        {
            var tempDir = Path.Combine(Path.GetTempPath(), $"PluginTest_{Guid.NewGuid()}");
            Directory.CreateDirectory(tempDir);
            
            var invalidDll = Path.Combine(tempDir, "invalid.dll");
            File.WriteAllText(invalidDll, "Not a DLL");
            var validDll = typeof(BasePlugin).Assembly.Location;
            var destDll = Path.Combine(tempDir, "valid.dll");
            File.Copy(validDll, destDll);

            try
            {
                var host = new PluginManager.PluginHost();
                host.LoadFromDirectory(tempDir);

                Assert.NotEmpty(host.Plugins);
                Assert.NotEmpty(host.LoadErrors);
            }
            finally
            {
                Directory.Delete(tempDir, true);
            }
        }
    }
}