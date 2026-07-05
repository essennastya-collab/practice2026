using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Task11;

public static class DynamicCompiler
{
    private const string CalculatorCode = @"
using Task11;

public class DynamicCalculator : ICalculator
{
    public int Add(int a, int b) => a + b;
    public int Minus(int a, int b) => a - b;
    public int Mul(int a, int b) => a * b;
    public int Div(int a, int b) => a / b;
}";

    public static ICalculator Build()
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(CalculatorCode);

        var runtimePath = Path.GetDirectoryName(typeof(object).Assembly.Location)!;
        
        var metadataReferences = new List<MetadataReference>
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ICalculator).Assembly.Location),
            MetadataReference.CreateFromFile(Path.Combine(runtimePath, "System.Runtime.dll")),
            MetadataReference.CreateFromFile(Path.Combine(runtimePath, "System.Collections.dll"))
        };

        var compilation = CSharpCompilation.Create(
            assemblyName: "RuntimeCalculator",
            syntaxTrees: new[] { syntaxTree },
            references: metadataReferences,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
        );

        using var stream = new MemoryStream();
        var compilationResult = compilation.Emit(stream);

        if (!compilationResult.Success)
        {
            var errorMessages = compilationResult.Diagnostics
                .Where(d => d.Severity == DiagnosticSeverity.Error)
                .Select(d => d.GetMessage());
            
            throw new CompilationException($"Dynamic compilation failed: {string.Join("; ", errorMessages)}");
        }

        stream.Position = 0;
        var assembly = Assembly.Load(stream.ToArray());
        var calculatorType = assembly.GetTypes().First(t => t.Name == "DynamicCalculator");
        
        return (ICalculator)Activator.CreateInstance(calculatorType)!;
    }
}

public class CompilationException : Exception
{
    public CompilationException(string message) : base(message) { }
}