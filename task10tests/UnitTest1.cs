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
    }
}