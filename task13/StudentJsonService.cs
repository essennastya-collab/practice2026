using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace task13;

public class StudentJsonService
{
    private readonly JsonSerializerOptions _options;

    public StudentJsonService()
    {
        _options = new JsonSerializerOptions
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };
        _options.Converters.Add(new DateTimeJsonConverter("yyyy-MM-dd"));
    }

    public string Serialize(Student student)
    {
        if (student == null)
            throw new ArgumentNullException(nameof(student));

        return JsonSerializer.Serialize(student, _options);
    }

    public Student Deserialize(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            throw new ArgumentException("JSON не может быть пустым", nameof(json));

        Student? student;
        try
        {
            student = JsonSerializer.Deserialize<Student>(json, _options);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"Ошибка парсинга JSON: {ex.Message}", ex);
        }

        if (student == null)
            throw new InvalidOperationException("Десериализованный объект равен null");

        Validate(student);
        return student;
    }

    public void SaveToFile(Student student, string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("Путь к файлу не может быть пустым", nameof(filePath));

        var json = Serialize(student);
        File.WriteAllText(filePath, json);
    }

    public Student LoadFromFile(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("Путь к файлу не может быть пустым", nameof(filePath));

        if (!File.Exists(filePath))
            throw new FileNotFoundException($"Файл не найден: {filePath}");

        var json = File.ReadAllText(filePath);
        return Deserialize(json);
    }

    private void Validate(Student student)
    {
        if (string.IsNullOrWhiteSpace(student.FirstName))
            throw new InvalidOperationException("Имя студента не может быть пустым");

        if (string.IsNullOrWhiteSpace(student.LastName))
            throw new InvalidOperationException("Фамилия студента не может быть пустой");

        if (student.BirthDate == default)
            throw new InvalidOperationException("Дата рождения не указана");

        if (student.BirthDate > DateTime.Now)
            throw new InvalidOperationException("Дата рождения не может быть в будущем");

        if (student.Grades != null)
        {
            foreach (var subject in student.Grades)
            {
                if (string.IsNullOrWhiteSpace(subject.Name))
                    throw new InvalidOperationException("Название предмета не может быть пустым");

                if (subject.Grade < 2 || subject.Grade > 5)
                    throw new InvalidOperationException($"Оценка '{subject.Grade}' по предмету '{subject.Name}' вне допустимого диапазона (2-5)");
            }
        }
    }
}

public class DateTimeJsonConverter : JsonConverter<DateTime>
{
    private readonly string _format;

    public DateTimeJsonConverter(string format)
    {
        _format = format;
    }

    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var dateString = reader.GetString();
        if (string.IsNullOrWhiteSpace(dateString))
            throw new JsonException("Дата не может быть пустой");

        if (!DateTime.TryParseExact(dateString, _format, null, System.Globalization.DateTimeStyles.None, out var date))
            throw new JsonException($"Неверный формат даты. Ожидался: {_format}");

        return date;
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString(_format));
    }
}