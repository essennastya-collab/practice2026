using System.Linq;

namespace task02;

public class StudentService
{
    private readonly List<Student> _students;

    public StudentService(List<Student> students) => _students = students;

    public IEnumerable<Student> GetStudentsByFaculty(string faculty)
    {
        return _students.Where(delegate (Student s)
        {
            return s.Faculty == faculty;
        });
    }

    public IEnumerable<Student> GetStudentsWithMinAverageGrade(double minAverageGrade)
    {
        return _students.Where(delegate (Student s)
        {
            return s.Grades.Average() >= minAverageGrade;
        });
    }

    public IEnumerable<Student> GetStudentsOrderedByName()
    {
        return _students.OrderBy(delegate (Student s)
        {
            return s.Name;
        });
    }

    public ILookup<string, Student> GroupStudentsByFaculty()
    {
        return _students.ToLookup(delegate (Student s)
        {
            return s.Faculty;
        });
    }

    public string GetFacultyWithHighestAverageGrade()
    {
        if (_students == null || _students.Count == 0)
        {
            return null;
        }

        var grouped = _students.GroupBy(delegate (Student s)
        {
            return s.Faculty;
        });

        var order = grouped.OrderByDescending(delegate (IGrouping<string, Student> g)
        {
            return g.Average(delegate (Student s)
            {
                return s.Grades.Average();
            });
        });

        var highest = order.FirstOrDefault();
        return highest?.Key;
    }
}