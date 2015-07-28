using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestApplication.Model;

namespace TestApplication
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var context = new SchoolContext())
            {
                Console.WriteLine($"Checking if we have students: {context.Students.Count()} students found");

                Console.WriteLine("Running test cases");

                TestCase_ProjectionWithReadOnlyProperty(context);
                TestCase_ProjectionWithUnsupportedMethod(context);
                TestCase_WhereClauseWithReadOnlyProperty(context);
                TestCase_WhereClauseWithUnsupportedMethod(context);

                Console.WriteLine("Press any key to continue");
                Console.Read();
            }
        }

        static void TestCase_ProjectionWithReadOnlyProperty(SchoolContext context)
        {
            Console.WriteLine("Test case: Projection with read-only property");
            foreach (var student in context.Students.Select(s => new { Name = s.DisplayName, s.DisplayName2, s.EnrollmentDate }))
            {
                Console.WriteLine($"\t{student.Name} enrolled on {student.EnrollmentDate}");
            }
        }

        static void TestCase_ProjectionWithUnsupportedMethod(SchoolContext context)
        {
            Console.WriteLine("Test case: Projection with unsupported method");
            foreach (var student in context.Students.Select(s => new { s.FirstMidName, s.LastName, EnrolDate = s.EnrollmentDate.GetHashCode() }))
            {
                Console.WriteLine($"\t{student.FirstMidName} {student.LastName} enrolled on {student.EnrolDate}");
            }
        }

        static void TestCase_WhereClauseWithReadOnlyProperty(SchoolContext context)
        {
            Console.WriteLine("Test case: Where clause with read-only property");
            foreach (var student in context.Students.Where(s => s.DisplayName.Contains("a") || s.DisplayName2.Contains("b")))
            {
                Console.WriteLine($"\t{student.FirstMidName} {student.LastName} enrolled on {student.EnrollmentDate}");
            }
        }

        static bool StringContains(string str, string value)
        {
            return str.Contains(value);
        }

        static void TestCase_WhereClauseWithUnsupportedMethod(SchoolContext context)
        {
            Console.WriteLine("Test case: Where clause with unsupported method");
            System.Linq.Expressions.Expression<Func<Student, bool>> predicate = s => StringContains(s.FirstMidName, "A");
            foreach (var student in context.Students.Where(predicate))
            {
                Console.WriteLine($"\t{student.FirstMidName} {student.LastName} enrolled on {student.EnrollmentDate}");
            }
        }
    }
}
