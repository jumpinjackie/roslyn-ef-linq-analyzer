using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestApplication.Model
{
    public class Student
    {
        public int ID { get; set; }
        public string LastName { get; set; }
        public string FirstMidName { get; set; }
        public DateTime EnrollmentDate { get; set; }

        public virtual ICollection<Enrollment> Enrollments { get; set; }

        /// <summary>
        /// GOTCHA: Read-only property (expression-bodied member form). EF generally complains when such a property is referenced in a LINQ expression
        /// </summary>
        public string DisplayName => this.FirstMidName + " " + this.LastName;

        /// <summary>
        /// GOTCHA: Read-only property (expression-bodied member form). EF generally complains when such a property is referenced in a LINQ expression
        /// </summary>
        public string DisplayName2
        {
            get { return this.FirstMidName + " " + this.LastName; }
        }
    }
}
