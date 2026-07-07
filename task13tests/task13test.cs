using System;
using System.Collections.Generic;
using System.IO;
using Xunit;
using task13;

namespace task13tests;

public class StudentJsonTests
{
    private readonly StudentJsonService _service = new();

    private Student CreateSampleStudent()
    {
        return new Student
        {
            FirstName = "Олег",
            LastName = "Комаров",
            BirthDate = new DateTime(2000, 5, 15),
            Grades = new List<Subject>
            {
                new Subject { Name = "Математика", Grade = 5 },
                new Subject { Name = "Физика", Grade = 4 }
            }
        };
    }

    [Fact]
    public void Serialize_ShouldReturnValidJson()
    {
        var student = CreateSampleStudent();
        var json = _service.Serialize(student);

        Assert.NotNull(json);
        Assert.Contains("firstName", json);
        Assert.Contains("lastName", json);
        Assert.Contains("birthDate", json);
        Assert.Contains("grades", json);
        Assert.Contains("2000-05-15", json);
    }

    [Fact]
    public void Serialize_NullStudent_ShouldThrowArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => _service.Serialize(null));
    }

    [Fact]
    public void Deserialize_ShouldReturnValidStudent()
    {
        var student = CreateSampleStudent();
        var json = _service.Serialize(student);
        var result = _service.Deserialize(json);

        Assert.Equal("Олег", result.FirstName);
        Assert.Equal("Комаров", result.LastName);
        Assert.Equal(new DateTime(2000, 5, 15), result.BirthDate);
        Assert.Equal(2, result.Grades.Count);
        Assert.Equal("Математика", result.Grades[0].Name);
        Assert.Equal(5, result.Grades[0].Grade);
    }

    [Fact]
    public void Deserialize_EmptyJson_ShouldThrowArgumentException()
    {
        Assert.Throws<ArgumentException>(() => _service.Deserialize(""));
        Assert.Throws<ArgumentException>(() => _service.Deserialize(null));
    }

    [Fact]
    public void Deserialize_InvalidJson_ShouldThrowInvalidOperationException()
    {
        Assert.Throws<InvalidOperationException>(() => _service.Deserialize("{invalid json}"));
    }

    [Fact]
    public void Deserialize_InvalidGrade_ShouldThrowInvalidOperationException()
    {
        var json = @"{
            ""firstName"": ""Олег"",
            ""lastName"": ""Комаров"",
            ""birthDate"": ""2000-05-15"",
            ""grades"": [{""name"": ""Математика"", ""grade"": 10}]
        }";

        var ex = Assert.Throws<InvalidOperationException>(() => _service.Deserialize(json));
        Assert.Contains("вне допустимого диапазона", ex.Message);
    }

    [Fact]
    public void Deserialize_EmptyFirstName_ShouldThrowInvalidOperationException()
    {
        var json = @"{
            ""firstName"": """",
            ""lastName"": ""Комаров"",
            ""birthDate"": ""2000-05-15"",
            ""grades"": []
        }";

        var ex = Assert.Throws<InvalidOperationException>(() => _service.Deserialize(json));
        Assert.Contains("Имя студента не может быть пустым", ex.Message);
    }

    [Fact]
    public void Deserialize_FutureBirthDate_ShouldThrowInvalidOperationException()
    {
        var futureDate = DateTime.Now.AddYears(1).ToString("yyyy-MM-dd");
        var json = $@"{{
            ""firstName"": ""Олег"",
            ""lastName"": ""Комаров"",
            ""birthDate"": ""{futureDate}"",
            ""grades"": []
        }}";

        var ex = Assert.Throws<InvalidOperationException>(() => _service.Deserialize(json));
        Assert.Contains("в будущем", ex.Message);
    }

    [Fact]
    public void SaveToFile_ShouldCreateFile()
    {
        var filePath = Path.Combine(Path.GetTempPath(), $"student_{Guid.NewGuid()}.json");
        var student = CreateSampleStudent();

        try
        {
            _service.SaveToFile(student, filePath);
            Assert.True(File.Exists(filePath));
            var content = File.ReadAllText(filePath);
            Assert.Contains("Олег", content);
        }
        finally
        {
            if (File.Exists(filePath)) File.Delete(filePath);
        }
    }

    [Fact]
    public void LoadFromFile_ShouldReturnValidStudent()
    {
        var filePath = Path.Combine(Path.GetTempPath(), $"student_{Guid.NewGuid()}.json");
        var student = CreateSampleStudent();

        try
        {
            _service.SaveToFile(student, filePath);
            var loaded = _service.LoadFromFile(filePath);

            Assert.Equal(student.FirstName, loaded.FirstName);
            Assert.Equal(student.LastName, loaded.LastName);
            Assert.Equal(student.BirthDate, loaded.BirthDate);
            Assert.Equal(student.Grades.Count, loaded.Grades.Count);
        }
        finally
        {
            if (File.Exists(filePath)) File.Delete(filePath);
        }
    }

    [Fact]
    public void LoadFromFile_NonExistent_ShouldThrowFileNotFoundException()
    {
        Assert.Throws<FileNotFoundException>(() =>
            _service.LoadFromFile("/nonexistent/path.json"));
    }

    [Fact]
    public void SaveToFile_EmptyPath_ShouldThrowArgumentException()
    {
        var student = CreateSampleStudent();
        Assert.Throws<ArgumentException>(() => _service.SaveToFile(student, ""));
    }

    [Fact]
    public void Serialize_NullGrades_ShouldIgnoreNull()
    {
        var student = new Student
        {
            FirstName = "Олег",
            LastName = "Комаров",
            BirthDate = new DateTime(2000, 5, 15),
            Grades = null
        };

        var json = _service.Serialize(student);
        Assert.DoesNotContain("grades", json);
    }

    [Fact]
    public void RoundTrip_ShouldPreserveData()
    {
        var original = CreateSampleStudent();
        var json = _service.Serialize(original);
        var restored = _service.Deserialize(json);

        Assert.Equal(original.FirstName, restored.FirstName);
        Assert.Equal(original.LastName, restored.LastName);
        Assert.Equal(original.BirthDate, restored.BirthDate);
        Assert.Equal(original.Grades.Count, restored.Grades.Count);

        for (int i = 0; i < original.Grades.Count; i++)
        {
            Assert.Equal(original.Grades[i].Name, restored.Grades[i].Name);
            Assert.Equal(original.Grades[i].Grade, restored.Grades[i].Grade);
        }
    }
}
