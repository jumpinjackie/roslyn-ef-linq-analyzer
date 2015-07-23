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

                Console.WriteLine("Press any key to continue");
                Console.Read();
            }
        }

        static void TestCase_ProjectionWithReadOnlyProperty(SchoolContext context)
        {
            Console.WriteLine("Test case: Projection with read-only property");
            foreach (var student in context.Students.Select(s => new { Name = s.DisplayName, s.EnrollmentDate }))
            {
                Console.WriteLine($"\t{student.Name} enrolled on {student.EnrollmentDate}");
            }
        }
    }
}
