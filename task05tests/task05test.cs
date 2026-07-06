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
    public int Calculate(int x, string y) => 0; 
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

    [Fact]
    public void Constructor_NullType_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new ClassAnalyzer(null));
    }

    [Fact]
    public void GetMethodParams_NullMethodName_ThrowsArgumentNullException()
    {
        var analyzer = new ClassAnalyzer(typeof(TestClass));
        Assert.Throws<ArgumentNullException>(() => analyzer.GetMethodParams(null).ToList());
    }

    [Fact]
    public void GetMethodParams_ReturnsParamsAndReturnType()
    {
        var analyzer = new ClassAnalyzer(typeof(TestClass));
        var result = analyzer.GetMethodParams("Calculate").ToList();

        Assert.Contains("x", result);
        Assert.Contains("y", result);
        Assert.Contains("Int32", result);
    }

    [Fact]
    public void GetMethodParams_NonExistentMethod_ReturnsEmpty()
    {
        var analyzer = new ClassAnalyzer(typeof(TestClass));
        var result = analyzer.GetMethodParams("NonExistentMethod").ToList();
        Assert.Empty(result);
    }

    [Fact]
    public void HasAttribute_ReturnsFalseWhenAttributeMissing()
    {
        var analyzer = new ClassAnalyzer(typeof(TestClass));
        Assert.False(analyzer.HasAttribute<SerializableAttribute>());
    }

    [Fact]
    public void GetPublicMethods_ExcludesInheritedMethods()
    {
        var analyzer = new ClassAnalyzer(typeof(TestClass));
        var methods = analyzer.GetPublicMethods().ToList();

        Assert.DoesNotContain("ToString", methods);
        Assert.DoesNotContain("Equals", methods);
        Assert.DoesNotContain("GetHashCode", methods);
        Assert.DoesNotContain("GetType", methods);
    }
}