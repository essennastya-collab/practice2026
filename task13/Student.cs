using System;
using System.Collections.Generic;

namespace task13;

public class Student
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime BirthDate { get; set; }
    public List<Subject> Grades { get; set; } = new();
}
