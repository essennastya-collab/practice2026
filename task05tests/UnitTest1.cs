using Xunit;
using System;
using System.Linq;
using task05;
public class TestClass
{
    public int PublicField;
    private string _privateField;
    public int Property { get; set; }

    public void Method() { }
}

[Serializable]
public class AttributedClass { }

public class ClassAnalyzerTests
{
    [Fact]
    public void GetPublicMethods_ReturnsCorrectMethods()
    {
        var analyzer = new ClassAnalyzer(typeof(TestClass));
        var methods = analyzer.GetPublicMethods();

        Assert.Contains("Method", methods);
    }

    [Fact]
    public void GetAllFields_IncludesPrivateFields()
    {
        var analyzer = new ClassAnalyzer(typeof(TestClass));
        var fields = analyzer.GetAllFields();

        Assert.Contains("_privateField", fields);
    }
    [Fact]
    public void GetProperties_ReturnsPropertyNames()
    {
        var analyzer = new ClassAnalyzer(typeof(TestClass));
        var properties = analyzer.GetProperties();

        Assert.Contains("Property", properties);
    }
    [Fact]
    public void HasAttribute_ReturnsTrueWhenAttributeExists()
    {
        var analyzer = new ClassAnalyzer(typeof(AttributedClass));

        Assert.True(analyzer.HasAttribute<SerializableAttribute>());
    }
}