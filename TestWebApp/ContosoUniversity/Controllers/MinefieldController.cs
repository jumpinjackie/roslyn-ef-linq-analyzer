using ContosoUniversity.DAL;
using ContosoUniversity.Models;
using ContosoUniversity.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.Mvc;

namespace ContosoUniversity.Controllers
{
    public class MinefieldController : Controller
    {
        private SchoolContext db = new SchoolContext();

        public ActionResult Index()
        {
            var vm = new MinefieldViewModel();
            vm.Links = GetLinks();
            return View(vm);
        }

        public ActionResult SelectWithReadOnlyProperty_EMS()
        {
            var vm = new QueryResultViewModel<StudentDisplay>();
            try
            {
                var results = db.Students.Select(s => new StudentDisplay()
                {
                    ID = s.ID,
                    FirstMidName = s.FirstMidName,
                    LastName = s.LastName,
                    FullName = s.FullName,
                    EnrollmentDate = s.EnrollmentDate
                });
                vm.Results = results.ToArray();
                Response.StatusCode = 200;
            }
            catch (Exception ex)
            {
                vm.Results = null;
                vm.Exception = ex;
                Response.StatusCode = 500;
            }
            return View("QueryResult", vm);
        }

        public ActionResult SelectWithReadOnlyProperty_QS()
        {
            var vm = new QueryResultViewModel<StudentDisplay>();
            try
            {
                var results = from s in db.Students
                              select new StudentDisplay()
                              {
                                  ID = s.ID,
                                  FirstMidName = s.FirstMidName,
                                  LastName = s.LastName,
                                  FullName = s.FullName,
                                  EnrollmentDate = s.EnrollmentDate
                              };
                vm.Results = results.ToArray();
                Response.StatusCode = 200;
            }
            catch (Exception ex)
            {
                vm.Results = null;
                vm.Exception = ex;
                Response.StatusCode = 500;
            }
            return View("QueryResult", vm);
        }

        public ActionResult WhereWithReadOnlyProperty_EMS()
        {
            var vm = new QueryResultViewModel<StudentDisplay>();
            try
            {
                var results = db.Students
                                .Where(s => s.FullName.Contains(" A"))
                                .Select(s => new StudentDisplay()
                                {
                                    ID = s.ID,
                                    FirstMidName = s.FirstMidName,
                                    LastName = s.LastName,
                                    FullName = s.LastName + ", " + s.FirstMidName,
                                    EnrollmentDate = s.EnrollmentDate
                                });
                vm.Results = results.ToArray();
                Response.StatusCode = 200;
            }
            catch (Exception ex)
            {
                vm.Results = null;
                vm.Exception = ex;
                Response.StatusCode = 500;
            }
            return View("QueryResult", vm);
        }

        public ActionResult WhereWithReadOnlyProperty_QS()
        {
            var vm = new QueryResultViewModel<StudentDisplay>();
            try
            {
                var results = from s in db.Students
                              where s.FullName.Contains(" A")
                              select new StudentDisplay()
                              {
                                  ID = s.ID,
                                  FirstMidName = s.FirstMidName,
                                  LastName = s.LastName,
                                  FullName = s.LastName + ", " + s.FirstMidName,
                                  EnrollmentDate = s.EnrollmentDate
                              };
                vm.Results = results.ToArray();
                Response.StatusCode = 200;
            }
            catch (Exception ex)
            {
                vm.Results = null;
                vm.Exception = ex;
                Response.StatusCode = 500;
            }
            return View("QueryResult", vm);
        }

        public ActionResult BadEnrollmentFilter_EMS()
        {
            var vm = new QueryResultViewModel<StudentDisplay>();
            try
            {
                Func<Enrollment, bool> predicate = e => e.Grade == Grade.A;
                var results = db.Students
                                .Where(s => s.Enrollments.Any(predicate))
                                .Select(s => new StudentDisplay()
                                {
                                    ID = s.ID,
                                    FirstMidName = s.FirstMidName,
                                    LastName = s.LastName,
                                    FullName = s.LastName + ", " + s.FirstMidName,
                                    EnrollmentDate = s.EnrollmentDate
                                });
                vm.Results = results.ToArray();
                Response.StatusCode = 200;
            }
            catch (Exception ex)
            {
                vm.Results = null;
                vm.Exception = ex;
                Response.StatusCode = 500;
            }
            return View("QueryResult", vm);
        }

        public ActionResult BadEnrollmentFilter_QS()
        {
            var vm = new QueryResultViewModel<StudentDisplay>();
            try
            {
                Func<Enrollment, bool> predicate = e => e.Grade == Grade.A;
                var results = from s in db.Students
                              where s.Enrollments.Any(predicate)
                              select new StudentDisplay()
                              {
                                  ID = s.ID,
                                  FirstMidName = s.FirstMidName,
                                  LastName = s.LastName,
                                  FullName = s.LastName + ", " + s.FirstMidName,
                                  EnrollmentDate = s.EnrollmentDate
                              };
                vm.Results = results.ToArray();
                Response.StatusCode = 200;
            }
            catch (Exception ex)
            {
                vm.Results = null;
                vm.Exception = ex;
                Response.StatusCode = 500;
            }
            return View("QueryResult", vm);
        }

        public ActionResult SelectUseOfUnmappedProperty_EMS()
        {
            var vm = new QueryResultViewModel<StudentDisplay>();
            try
            {
                var results = db.Students
                                .Select(s => new StudentDisplay()
                                {
                                    ID = s.ID,
                                    FirstMidName = s.FirstMidName,
                                    LastName = s.LastName,
                                    FullName = s.LastName + ", " + s.FirstMidName + ", " + s.Unmapped,
                                    EnrollmentDate = s.EnrollmentDate
                                });
                vm.Results = results.ToArray();
                Response.StatusCode = 200;
            }
            catch (Exception ex)
            {
                vm.Results = null;
                vm.Exception = ex;
                Response.StatusCode = 500;
            }
            return View("QueryResult", vm);
        }

        public ActionResult SelectUseOfUnmappedProperty_QS()
        {
            var vm = new QueryResultViewModel<StudentDisplay>();
            try
            {
                var results = from s in db.Students
                              select new StudentDisplay()
                              {
                                  ID = s.ID,
                                  FirstMidName = s.FirstMidName,
                                  LastName = s.LastName,
                                  FullName = s.LastName + ", " + s.FirstMidName + ", " + s.Unmapped,
                                  EnrollmentDate = s.EnrollmentDate
                              };
                vm.Results = results.ToArray();
                Response.StatusCode = 200;
            }
            catch (Exception ex)
            {
                vm.Results = null;
                vm.Exception = ex;
                Response.StatusCode = 500;
            }
            return View("QueryResult", vm);
        }

        public ActionResult SelectWithReadOnlyPropertyExpr_EMS()
        {
            var vm = new QueryResultViewModel<StudentDisplay>();
            try
            {
                var results = db.Students.Select(s => new StudentDisplay()
                {
                    ID = s.ID,
                    FirstMidName = s.FirstMidName,
                    LastName = s.LastName,
                    FullName = s.FullNameExpr,
                    EnrollmentDate = s.EnrollmentDate
                });
                vm.Results = results.ToArray();
                Response.StatusCode = 200;
            }
            catch (Exception ex)
            {
                vm.Results = null;
                vm.Exception = ex;
                Response.StatusCode = 500;
            }
            return View("QueryResult", vm);
        }

        public ActionResult SelectWithReadOnlyPropertyExpr_QS()
        {
            var vm = new QueryResultViewModel<StudentDisplay>();
            try
            {
                var results = from s in db.Students
                              select new StudentDisplay()
                              {
                                  ID = s.ID,
                                  FirstMidName = s.FirstMidName,
                                  LastName = s.LastName,
                                  FullName = s.FullNameExpr,
                                  EnrollmentDate = s.EnrollmentDate
                              };
                vm.Results = results.ToArray();
                Response.StatusCode = 200;
            }
            catch (Exception ex)
            {
                vm.Results = null;
                vm.Exception = ex;
                Response.StatusCode = 500;
            }
            return View("QueryResult", vm);
        }

        public ActionResult WhereWithInterpolatedString_EMS()
        {
            var vm = new QueryResultViewModel<StudentDisplay>();
            try
            {
                string startLetter = "A";
                var results = db.Students
                                .Where(s => s.LastName.StartsWith($" {startLetter}"))
                                .Select(s => new StudentDisplay()
                                {
                                    ID = s.ID,
                                    FirstMidName = s.FirstMidName,
                                    LastName = s.LastName,
                                    FullName = s.LastName + ", " + s.FirstMidName,
                                    EnrollmentDate = s.EnrollmentDate
                                });
                vm.Results = results.ToArray();
                Response.StatusCode = 200;
            }
            catch (Exception ex)
            {
                vm.Results = null;
                vm.Exception = ex;
                Response.StatusCode = 500;
            }
            return View("QueryResult", vm);
        }

        public ActionResult WhereWithInterpolatedString_QS()
        {
            var vm = new QueryResultViewModel<StudentDisplay>();
            try
            {
                string startLetter = "A";
                var results = from s in db.Students
                              where s.LastName.StartsWith($" {startLetter}")
                              select new StudentDisplay()
                              {
                                  ID = s.ID,
                                  FirstMidName = s.FirstMidName,
                                  LastName = s.LastName,
                                  FullName = s.LastName + ", " + s.FirstMidName,
                                  EnrollmentDate = s.EnrollmentDate
                              };
                vm.Results = results.ToArray();
                Response.StatusCode = 200;
            }
            catch (Exception ex)
            {
                vm.Results = null;
                vm.Exception = ex;
                Response.StatusCode = 500;
            }
            return View("QueryResult", vm);
        }

        private IEnumerable<Link> GetLinks()
        {
            var controllerName = "Minefield";
            yield return new Link(controllerName, nameof(SelectWithReadOnlyProperty_EMS), "Test: LINQ query with projection containing read-only property (Extension Method syntax)");
            yield return new Link(controllerName, nameof(SelectWithReadOnlyProperty_QS), "Test: LINQ query with projection containing read-only property (Query syntax)");
            yield return new Link(controllerName, nameof(SelectWithReadOnlyPropertyExpr_EMS), "Test: LINQ query with projection containing expression-bodied member (Extension Method syntax)");
            yield return new Link(controllerName, nameof(SelectWithReadOnlyPropertyExpr_QS), "Test: LINQ query with projection containing expression-bodied member (Query syntax)");
            yield return new Link(controllerName, nameof(WhereWithReadOnlyProperty_EMS), "Test: LINQ query with where clause containing read-only property (Extension Method syntax)");
            yield return new Link(controllerName, nameof(WhereWithReadOnlyProperty_QS), "Test: LINQ query with where clause containing read-only property (Query syntax)");
            yield return new Link(controllerName, nameof(WhereWithInterpolatedString_EMS), "Test: LINQ query with where clause containing interpolated string (Extension Method syntax)");
            yield return new Link(controllerName, nameof(WhereWithInterpolatedString_QS), "Test: LINQ query with where clause containing interpolated string (Query syntax)");
            yield return new Link(controllerName, nameof(BadEnrollmentFilter_EMS), "Test: LINQ query with bad related enrollments filter (Extension Method syntax)");
            yield return new Link(controllerName, nameof(BadEnrollmentFilter_QS), "Test: LINQ query with bad related enrollments filter (Query syntax)");
            yield return new Link(controllerName, nameof(SelectUseOfUnmappedProperty_EMS), "Test: LINQ query with projection containing un-mapped property (Extension Method syntax)");
            yield return new Link(controllerName, nameof(SelectUseOfUnmappedProperty_QS), "Test: LINQ query with projection containing un-mapped property (Query syntax)");
        }
    }
}