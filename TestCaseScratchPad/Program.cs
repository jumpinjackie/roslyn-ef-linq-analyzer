/*
You can use this file as a "scratch area" for building sources for analyzer test cases.

Having it here ensures the source code for your analyzer test case is at least synatically sound by just
simply pasting the contents of the test case right here (and commenting out the default entry point
if required) and seeing if it compiles
 */
/*
namespace TestCaseScratchPad
{
   class Program
   {
       public static void Main(string[] args) { }
   }
}
*/

using System;
using System.Linq;
using System.Data.Entity;
using System.Data.Entity.Spatial;
using System.Data.Entity.SqlServer;

namespace UnitTest
{
    public class Thing
    {
        public int Id { get; set; }
        public double Value { get; set; }
        public string Name { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public DateTime? CreatedOn { get; set; }
        public DbGeometry Geom { get; set; }
        public DbGeography Geog { get; set; }
    }

    public class MyContext : DbContext
    {
        public DbSet<Thing> Things { get; set; }
    }

    class Program
    {
        public static void Main(string[] args)
        {
            using (var context = new MyContext())
            {
                var things = context.Things.Where(t => t.Id < 10).Select(t => t.Value);
                var items = context.Things
                                   .Select(t => new
                                   {
                                       t.Id,
                                       t.Name,
                                       t.CreatedOn,
                                       t.Geom,
                                       t.Geog,
                                       Equals1 = t.CreatedOn.Equals(t.UpdatedOn),
                                       Equals2 = DateTime.Equals(t.UpdatedOn, t.CreatedOn),
                                       Equals3 = t.Name.Equals("Foo"),
                                       Equals4 = string.Equals("Foo", t.Name),
                                       Abs1 = Math.Abs(-1.2),
                                       Abs2 = Math.Abs(-4),
                                       Abs3 = Math.Abs(-1.2f),
                                       Abs4 = Math.Abs(-3l),
                                       Ceiling = Math.Ceiling(t.Value),
                                       Floor = Math.Floor(t.Value),
                                       Round = Math.Round(t.Value, 3),
                                       Truncate = Math.Truncate(t.Value),
                                       Power = Math.Pow(t.Value, 3),
                                       Year = t.CreatedOn.Value.Year,
                                       Month = t.CreatedOn.Value.Month,
                                       Day = t.CreatedOn.Value.Day,
                                       Hour = t.CreatedOn.Value.Hour,
                                       Minute = t.CreatedOn.Value.Minute,
                                       Second = t.CreatedOn.Value.Second,
                                       Milliseconds = t.CreatedOn.Value.Millisecond,
                                       CurrentDateTimeOffset = DateTimeOffset.Now,
                                       CurrentDateTime = DateTime.Now,
                                       CurrentUtcDateTime = DateTime.UtcNow,
                                       LTrim = t.Name.TrimStart(),
                                       RTrim = t.Name.TrimEnd(),
                                       Trim = t.Name.Trim(),
                                       ToUpper = t.Name.ToUpper(),
                                       ToLower = t.Name.ToLower(),
                                       Substring1 = t.Name.Substring(0),
                                       Substring2 = t.Name.Substring(0, 3),
                                       Substring3 = t.Name.Remove(0),
                                       Replace = t.Name.Replace("Foo", "Bar"),
                                       Concat1 = t.Name.Remove(0, 3),
                                       Concat2 = t.Name.Insert(0, "sdf"),
                                       Concat3 = String.Concat(t.Name, "a"),
                                       Concat4 = String.Concat(t.Name, "a", "b"),
                                       Concat5 = String.Concat(t.Name, "a", "b", "c"),
                                       IndexOf = t.Name.IndexOf("sdf"),
                                       Length = t.Name.Length,
                                       Like1 = t.Name.StartsWith("Foo"),
                                       Like2 = t.Name.EndsWith("Foo"),
                                       Like3 = t.Name.Contains("Foo"),
                                       NewGuid = Guid.NewGuid(),
                                       IsNullOrEmpty = String.IsNullOrEmpty(t.Name),
                                       AddDays = DbFunctions.AddDays(t.CreatedOn, 1),
                                       AddHours = DbFunctions.AddHours(t.CreatedOn, 1),
                                       AddMicroseconds = DbFunctions.AddMicroseconds(t.CreatedOn, 1),
                                       AddMilliseconds = DbFunctions.AddMilliseconds(t.CreatedOn, 1),
                                       AddMinutes = DbFunctions.AddMinutes(t.CreatedOn, 1),
                                       AddMonths = DbFunctions.AddMonths(t.CreatedOn, 1),
                                       AddNanoseconds = DbFunctions.AddNanoseconds(t.CreatedOn, 1),
                                       AddSeconds = DbFunctions.AddSeconds(t.CreatedOn, 1),
                                       AddYears = DbFunctions.AddYears(t.CreatedOn, 1),
                                       AsNonUnicode = DbFunctions.AsNonUnicode(t.Name),
                                       AsUnicode = DbFunctions.AsUnicode(t.Name),
                                       CreateDateTime = DbFunctions.CreateDateTime(2015, 1, 1, 1, 1, 1),
                                       CreateDateTimeOffset = DbFunctions.CreateDateTimeOffset(2015, 1, 1, 1, 1, 1, 1),
                                       CreateTime = DbFunctions.CreateTime(1, 1, 1),
                                       DiffDays = DbFunctions.DiffDays(t.CreatedOn, t.UpdatedOn),
                                       DiffHours = DbFunctions.DiffHours(t.CreatedOn, t.UpdatedOn),
                                       DiffMicroseconds = DbFunctions.DiffMicroseconds(t.CreatedOn, t.UpdatedOn),
                                       DiffMilliseconds = DbFunctions.DiffMilliseconds(t.CreatedOn, t.UpdatedOn),
                                       DiffMinutes = DbFunctions.DiffMinutes(t.CreatedOn, t.UpdatedOn),
                                       DiffMonths = DbFunctions.DiffMonths(t.CreatedOn, t.UpdatedOn),
                                       DiffNanoseconds = DbFunctions.DiffNanoseconds(t.CreatedOn, t.UpdatedOn),
                                       DiffSeconds = DbFunctions.DiffSeconds(t.CreatedOn, t.UpdatedOn),
                                       DiffYears = DbFunctions.DiffYears(t.CreatedOn, t.UpdatedOn),
                                       GetTotalOffsetMinutes = DbFunctions.GetTotalOffsetMinutes(DbFunctions.CreateDateTimeOffset(2015, 1, 1, 1, 1, 1, 1)),
                                       Left = DbFunctions.Left(t.Name, 3),
                                       Reverse = DbFunctions.Reverse(t.Name),
                                       Right = DbFunctions.Right(t.Name, 3),
                                       StandardDeviation = DbFunctions.StandardDeviation(things),
                                       StandardDeviationP = DbFunctions.StandardDeviationP(things),
                                       Truncate2 = DbFunctions.Truncate(t.Value, 2),
                                       TruncateTime = DbFunctions.TruncateTime(t.CreatedOn),
                                       Var = DbFunctions.Var(things),
                                       VarP = DbFunctions.VarP(things),
                                       Area = t.Geom.Area,
                                       Binary = t.Geom.AsBinary(),
                                       Gml = t.Geom.AsGml(),
                                       Text = t.Geom.AsText(),
                                       BoundaryArea = t.Geom.Boundary.Area,
                                       Buffer = t.Geom.Buffer(2.0),
                                       Centroid = t.Geom.Centroid,
                                       GeomContains = t.Geom.Contains(DbGeometry.FromText("POINT (1 1)")),
                                       ConvexHull = t.Geom.ConvexHull,
                                       Srid = t.Geom.CoordinateSystemId,
                                       GeomCrosses = t.Geom.Crosses(DbGeometry.FromText("POINT (1 1)")),
                                       GeomDifference = t.Geom.Difference(DbGeometry.FromText("POINT (1 1)")),
                                       GeomDimension = t.Geom.Dimension,
                                       GeomDisjoint = t.Geom.Disjoint(DbGeometry.FromText("POINT (1 1)")),
                                       GeomDistance = t.Geom.Distance(DbGeometry.FromText("POINT (1 1)")),
                                       GeomElementAt = t.Geom.ElementAt(0),
                                       GeomElementCount = t.Geom.ElementCount,
                                       GeomElevation = t.Geom.Elevation,
                                       GeomEndPoint = t.Geom.EndPoint,
                                       GeomEnvelope = t.Geom.Envelope,
                                       GeomEquals = t.Geom.Equals(DbGeometry.FromText("POINT (1 1)")),
                                       GeomExteriorRing = t.Geom.ExteriorRing,
                                       GeomInteriorRing = t.Geom.InteriorRingAt(0),
                                       GeomInteriorRingCount = t.Geom.InteriorRingCount,
                                       GeomIntersection = t.Geom.Intersection(DbGeometry.FromText("POINT (1 1)")),
                                       GeomIntersects = t.Geom.Intersects(DbGeometry.FromText("POINT (1 1)")),
                                       GeomIsClosed = t.Geom.IsClosed,
                                       GeomIsEmpty = t.Geom.IsEmpty,
                                       GeomIsRing = t.Geom.IsRing,
                                       GeomIsSimple = t.Geom.IsSimple,
                                       GeomIsValid = t.Geom.IsValid,
                                       GeomLength = t.Geom.Length,
                                       GeomMeasure = t.Geom.Measure,
                                       GeomOverlaps = t.Geom.Overlaps(DbGeometry.FromText("POINT (1 1)")),
                                       GeomPointAt = t.Geom.PointAt(0),
                                       GeomPointCount = t.Geom.PointCount,
                                       GeomPointOnSurface = t.Geom.PointOnSurface,
                                       GeomRelate = t.Geom.Relate(DbGeometry.FromText("POINT (1 1)"), ""),
                                       GeomSpatialEquals = t.Geom.SpatialEquals(DbGeometry.FromText("POINT (1 1)")),
                                       GeomSpatialTypeName = t.Geom.SpatialTypeName,
                                       GeomStartPoint = t.Geom.StartPoint,
                                       GeomSymmetricDifference = t.Geom.SymmetricDifference(DbGeometry.FromText("POINT (1 1)")),
                                       GeomTouches = t.Geom.Touches(DbGeometry.FromText("POINT (1 1)")),
                                       GeomUnion = t.Geom.Union(DbGeometry.FromText("POINT (1 1)")),
                                       GeomWithin = t.Geom.Within(DbGeometry.FromText("POINT (1 1)")),
                                       GeomXCoordinate = t.Geom.XCoordinate,
                                       GeomYCoordinate = t.Geom.YCoordinate,
                                       Acos = SqlFunctions.Acos(t.Value)
                                   });
            }
        }
    }
}