using System.Reflection;
using task07;
using Xunit;           
using System;          
using System.IO;  
namespace task07tests;

public class AttributeReflectionTests
{
    [Fact]
    public void Class_HasDisplayNameAttribute()
    {
        var type = typeof(SampleClass);
        var attribute = type.GetCustomAttribute<DisplayNameAttribute>();
        Assert.NotNull(attribute);
        Assert.Equal("Пример класса", attribute.DisplayName);
    }

    [Fact]
    public void Method_HasDisplayNameAttribute()
    {
        var method = typeof(SampleClass).GetMethod("TestMethod");
        var attribute = method.GetCustomAttribute<DisplayNameAttribute>();
         Assert.NotNull(attribute);
        Assert.Equal("Тестовый метод", attribute.DisplayName);
    }

    [Fact]
    public void Property_HasDisplayNameAttribute()
    {
        var prop = typeof(SampleClass).GetProperty("Number");
        var attribute = prop.GetCustomAttribute<DisplayNameAttribute>();
        Assert.NotNull(attribute);
        Assert.Equal("Числовое свойство", attribute.DisplayName);
    }

    [Fact]
    public void Class_HasVersionAttribute()
    {
        var type = typeof(SampleClass);
        var attribute = type.GetCustomAttribute<VersionAttribute>();
        Assert.NotNull(attribute);
        Assert.Equal(1, attribute.Major);
        Assert.Equal(0, attribute.Minor);
    }

    [Fact]
    public void Class_PrintTypeInfo()
    {
        var output = new StringWriter();
        Console.SetOut(output);    
        ReflectionHelper.PrintTypeInfo(typeof(SampleClass));    
        var result = output.ToString();    
        Assert.Contains("SampleClass", result);
        Assert.Contains("Пример класса", result);
        Assert.Contains("Version: 1.0", result);
        Assert.Contains("TestMethod", result);
        Assert.Contains("Тестовый метод", result);
        Assert.Contains("Number", result);
        Assert.Contains("Числовое свойство", result);
    }
}
