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
                Console.WriteLine("Press any key to continue");
                Console.Read();
            }
        }
    }
}
