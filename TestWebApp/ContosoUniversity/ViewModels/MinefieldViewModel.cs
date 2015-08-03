using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ContosoUniversity.ViewModels
{
    public class Link
    {
        public Link(string controllerName, string actionName, string label)
        {
            this.ControllerName = controllerName;
            this.ActionName = actionName;
            this.Label = label;
        }

        public string ControllerName { get; }

        public string ActionName { get; }

        public string Label { get; }
    }

    public class MinefieldViewModel
    {
        public IEnumerable<Link> Links { get; set; }
    }

    public class QueryResultViewModel<T>
    {
        public Exception Exception { get; set; }

        public T[] Results { get; set; }
    }

    public class StudentDisplay
    {
        public int ID { get; set; }
        
        public string LastName { get; set; }

        public string FirstMidName { get; set; }
        
        public string FullName { get; set; }

        public DateTime EnrollmentDate { get; set; }
    }
}