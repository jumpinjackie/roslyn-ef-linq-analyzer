using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using TestHelper;
using EFLinqAnalyzer;

namespace EFLinqAnalyzer.Test
{
    [TestClass]
    public class UnitTest : CodeFixVerifier
    {
        //No diagnostics expected to show up
        [TestMethod]
        public void EmptyCode()
        {
            var test = @"";

            VerifyCSharpDiagnostic(test);
        }

        //Class with read-only property. No diagnostics as this is not a EF code first model entity class
        [TestMethod]
        public void NonEFClassWithReadOnlyProperty()
        {
            var test = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        class TypeName
        {
            public string Foo { get; set; }
            public string Bar { get; set; }

            public string FooBar
            {
                get { return this.Foo + "" "" + this.Bar; }
            }
        }
    }";
            VerifyCSharpDiagnostic(test);
        }

        //Class with read-only property (as an expression-bodied member). No diagnostics as this is not a EF code first model entity class
        [TestMethod]
        public void NonEFClassWithReadOnlyPropertyExprMember()
        {
            var test = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        class TypeName
        {
            public string Foo { get; set; }
            public string Bar { get; set; }

            public string FooBar => this.Foo + "" "" + this.Bar; 
        }
    }";
            VerifyCSharpDiagnostic(test);
            /*
            var expected = new DiagnosticResult
            {
                Id = "EFLINQ001",
                Message = String.Format("Property '{0}' in type '{1}' not translatable in LINQ to Entities", "FooBar", "TypeName"),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 16, 27)
                        }
            };

            VerifyCSharpDiagnostic(test, expected);
            */
        }

        [TestMethod]
        public void EFLINQ001_ClassWithReadOnlyProperty_ReferencedInUnqualifiedDbContext()
        {
            var test = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;
    using System.Data.Entity;

    namespace ConsoleApplication1
    {
        public class MyContext : DbContext
        {
            public DbSet<Thing> Things { get; set; }
        }

        public class Thing
        {
            public string Foo { get; set; }
            public string Bar { get; set; }

            public string FooBar
            {
                get { return this.Foo + "" "" + this.Bar; }
            }
        }
    }";
            var expected = new DiagnosticResult
            {
                Id = "EFLINQ001",
                Message = String.Format("Property '{0}' in type '{1}' not translatable in LINQ to Entities", "FooBar", "Thing"),
                Severity = DiagnosticSeverity.Info,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 22, 13)
                        }
            };

            VerifyCSharpDiagnostic(test, expected);
        }

        [TestMethod]
        public void EFLINQ001_ClassWithReadOnlyPropertyAndExprBodiedMember_ReferencedInUnqualifiedDbContext()
        {
            var test = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;
    using System.Data.Entity;

    namespace ConsoleApplication1
    {
        public class MyContext : DbContext
        {
            public DbSet<Thing> Things { get; set; }
        }

        public class Thing
        {
            public string Foo { get; set; }
            public string Bar { get; set; }

            public string FooBar
            {
                get { return this.Foo + "" "" + this.Bar; }
            }

            public string FooBarExpr => this.Foo + "" "" + this.Bar;
        }
    }";
            VerifyCSharpDiagnostic(test, 
                new DiagnosticResult
                {
                    Id = "EFLINQ001",
                    Message = String.Format("Property '{0}' in type '{1}' not translatable in LINQ to Entities", "FooBar", "Thing"),
                    Severity = DiagnosticSeverity.Info,
                    Locations =
                        new[] {
                                new DiagnosticResultLocation("Test0.cs", 22, 13)
                            }
                },
                new DiagnosticResult
                {
                    Id = "EFLINQ001",
                    Message = String.Format("Property '{0}' in type '{1}' not translatable in LINQ to Entities", "FooBarExpr", "Thing"),
                    Severity = DiagnosticSeverity.Info,
                    Locations =
                        new[] {
                                new DiagnosticResultLocation("Test0.cs", 27, 13)
                            }
                });
        }

        [TestMethod]
        public void EFLINQ001_ClassWithReadOnlyPropertyAndExprBodiedMember_ReferencedInFullyQualifiedDbContext()
        {
            var test = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        public class MyContext : System.Data.Entity.DbContext
        {
            public System.Data.Entity.DbSet<Thing> Things { get; set; }
        }

        public class Thing
        {
            public string Foo { get; set; }
            public string Bar { get; set; }

            public string FooBar
            {
                get { return this.Foo + "" "" + this.Bar; }
            }

            public string FooBarExpr => this.Foo + "" "" + this.Bar;
        }
    }";
            VerifyCSharpDiagnostic(test,
                new DiagnosticResult
                {
                    Id = "EFLINQ001",
                    Message = String.Format("Property '{0}' in type '{1}' not translatable in LINQ to Entities", "FooBar", "Thing"),
                    Severity = DiagnosticSeverity.Info,
                    Locations =
                        new[] {
                                new DiagnosticResultLocation("Test0.cs", 21, 13)
                            }
                },
                new DiagnosticResult
                {
                    Id = "EFLINQ001",
                    Message = String.Format("Property '{0}' in type '{1}' not translatable in LINQ to Entities", "FooBarExpr", "Thing"),
                    Severity = DiagnosticSeverity.Info,
                    Locations =
                        new[] {
                                new DiagnosticResultLocation("Test0.cs", 26, 13)
                            }
                });
        }

        [TestMethod]
        public void EFLINQ002_LinqWhere()
        {
            var test = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;
    using System.Data.Entity;

    namespace ConsoleApplication1
    {
        public class MyContext : DbContext
        {
            public DbSet<Thing> Things { get; set; }
        }

        public class Thing
        {
            public string Foo { get; set; }
            public string Bar { get; set; }

            public string FooBar
            {
                get { return this.Foo + "" "" + this.Bar; }
            }

            public string FooBarExpr => this.Foo + "" "" + this.Bar;
        }

        class Program
        {
            public static void Main(string [] args)
            {
                using (var context = new MyContext())
                {
                    var items = context.Things.Where(t => t.FooBar == ""Some Value"");
                    var items2 = context.Things.Where(t => t.FooBarExpr == ""Some other value"");
                }
            }
        }
    }";
            VerifyCSharpDiagnostic(test,
                new DiagnosticResult
                {
                    Id = "EFLINQ001",
                    Message = String.Format("Property '{0}' in type '{1}' not translatable in LINQ to Entities", "FooBar", "Thing"),
                    Severity = DiagnosticSeverity.Info,
                    Locations =
                        new[] {
                                new DiagnosticResultLocation("Test0.cs", 22, 13)
                            }
                },
                new DiagnosticResult
                {
                    Id = "EFLINQ001",
                    Message = String.Format("Property '{0}' in type '{1}' not translatable in LINQ to Entities", "FooBarExpr", "Thing"),
                    Severity = DiagnosticSeverity.Info,
                    Locations =
                        new[] {
                                new DiagnosticResultLocation("Test0.cs", 27, 13)
                            }
                },
                new DiagnosticResult
                {
                    Id = "EFLINQ002",
                    Message = String.Format("Read-Only property '{0}' of type '{1}' used in LINQ to Entities expression", "FooBar", "Thing"),
                    Severity = DiagnosticSeverity.Error,
                    Locations =
                        new[] {
                                new DiagnosticResultLocation("Test0.cs", 36, 59)
                            }
                },
                new DiagnosticResult
                {
                    Id = "EFLINQ002",
                    Message = String.Format("Read-Only property '{0}' of type '{1}' used in LINQ to Entities expression", "FooBarExpr", "Thing"),
                    Severity = DiagnosticSeverity.Error,
                    Locations =
                        new[] {
                                new DiagnosticResultLocation("Test0.cs", 37, 60)
                            }
                });
        }

        [TestMethod]
        public void EFLINQ002_LinqSelect()
        {
            var test = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;
    using System.Data.Entity;

    namespace ConsoleApplication1
    {
        public class MyContext : DbContext
        {
            public DbSet<Thing> Things { get; set; }
        }

        public class Thing
        {
            public string Foo { get; set; }
            public string Bar { get; set; }

            public string FooBar
            {
                get { return this.Foo + "" "" + this.Bar; }
            }

            public string FooBarExpr => this.Foo + "" "" + this.Bar;
        }

        class Program
        {
            public static void Main(string [] args)
            {
                using (var context = new MyContext())
                {
                    var items = context.Things.Select(t => new { t.Foo, t.Bar, t.FooBar, t.FooBarExpr });
                }
            }
        }
    }";
            VerifyCSharpDiagnostic(test,
                new DiagnosticResult
                {
                    Id = "EFLINQ001",
                    Message = String.Format("Property '{0}' in type '{1}' not translatable in LINQ to Entities", "FooBar", "Thing"),
                    Severity = DiagnosticSeverity.Info,
                    Locations =
                        new[] {
                                new DiagnosticResultLocation("Test0.cs", 22, 13)
                            }
                },
                new DiagnosticResult
                {
                    Id = "EFLINQ001",
                    Message = String.Format("Property '{0}' in type '{1}' not translatable in LINQ to Entities", "FooBarExpr", "Thing"),
                    Severity = DiagnosticSeverity.Info,
                    Locations =
                        new[] {
                                new DiagnosticResultLocation("Test0.cs", 27, 13)
                            }
                },
                new DiagnosticResult
                {
                    Id = "EFLINQ002",
                    Message = String.Format("Read-Only property '{0}' of type '{1}' used in LINQ to Entities expression", "FooBar", "Thing"),
                    Severity = DiagnosticSeverity.Error,
                    Locations =
                        new[] {
                                new DiagnosticResultLocation("Test0.cs", 36, 80)
                            }
                },
                new DiagnosticResult
                {
                    Id = "EFLINQ002",
                    Message = String.Format("Read-Only property '{0}' of type '{1}' used in LINQ to Entities expression", "FooBarExpr", "Thing"),
                    Severity = DiagnosticSeverity.Error,
                    Locations =
                        new[] {
                                new DiagnosticResultLocation("Test0.cs", 36, 90)
                            }
                });
        }

        [TestMethod]
        public void EFLINQ003_LinqWhere()
        {
            var test = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;
    using System.Data.Entity;

    namespace ConsoleApplication1
    {
        public class MyContext : DbContext
        {
            public DbSet<Thing> Things { get; set; }
        }

        public class Thing
        {
            public string Foo { get; set; }
            public string Bar { get; set; }
        }

        class Program
        {
            static bool FooIsBar(string foo, string bar)
            {
                return foo == bar;
            }

            public static void Main(string [] args)
            {
                using (var context = new MyContext())
                {
                    var items = context.Things.Where(t => FooIsBar(t.Foo, t.Bar));
                }
            }
        }
    }";
            VerifyCSharpDiagnostic(test,
                new DiagnosticResult
                {
                    Id = "EFLINQ003",
                    Message = String.Format("Static method '{0}' cannot be used within a LINQ to Entities expression", "FooIsBar"),
                    Severity = DiagnosticSeverity.Error,
                    Locations =
                        new[] {
                                new DiagnosticResultLocation("Test0.cs", 34, 59)
                            }
                });
        }

        [TestMethod]
        public void EFLINQ003_LinqSelect()
        {
            var test = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;
    using System.Data.Entity;

    namespace ConsoleApplication1
    {
        public class MyContext : DbContext
        {
            public DbSet<Thing> Things { get; set; }
        }

        public class Thing
        {
            public string Foo { get; set; }
            public string Bar { get; set; }
        }

        class Program
        {
            static bool FooIsBar(string foo, string bar)
            {
                return foo == bar;
            }

            public static void Main(string [] args)
            {
                using (var context = new MyContext())
                {
                    var items = context.Things.Select(t => new { t.Foo, t.Bar, IsMatch = FooIsBar(t.Foo, t.Bar) });
                }
            }
        }
    }";
            VerifyCSharpDiagnostic(test,
                new DiagnosticResult
                {
                    Id = "EFLINQ003",
                    Message = String.Format("Static method '{0}' cannot be used within a LINQ to Entities expression", "FooIsBar"),
                    Severity = DiagnosticSeverity.Error,
                    Locations =
                        new[] {
                                new DiagnosticResultLocation("Test0.cs", 34, 90)
                            }
                });
        }

        [TestMethod]
        public void EFLINQ003_LinqSelect_ValidMethods()
        {
            var test = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;
    using System.Data.Entity;

    namespace ConsoleApplication1
    {
        public class MyContext : DbContext
        {
            public DbSet<Thing> Things { get; set; }
        }

        public class Thing
        {
            public string Foo { get; set; }
            public DateTime At { get; set; }
            public double ADouble { get; set; }
            public DbGeometry Geom { get; set; }
            public DbGeography Geog { get; set; }
        }

        class Program
        {
            static bool FooIsBar(string foo, string bar)
            {
                return foo == bar;
            }

            public static void Main(string [] args)
            {
                using (var context = new MyContext())
                {
                    var items = context.Things.Select(t => new {
                        t.Foo,
                        t.At,
                        t.ADouble,
                        t.Geom,
                        t.Geog,
                        Math.Abs(t.ADouble),
                        Math.Ceiling(t.ADouble),
                        Math.Floor(t.ADouble),
                        Math.Power(t.ADouble, 2),
                        Math.Round(t.ADouble),
                        Math.Truncate(t.ADouble),
                        t.Foo.Concat(""bar""),
                        t.Foo.Contains(""bar""),
                        t.Foo.EndsWith(""bar""),
                        t.Foo.IndexOf(""bar""),
                        //TODO: valid Left() call
                        t.Foo.Length,
                        //TODO: valid LTrim() call
                        t.Foo.Replace(""Foo"", ""Bar""),
                        t.Foo.Reverse(),
                        //TODO: valid Right() call
                        //TODO: valid RTrim() call
                        t.Substring(0),
                        t.Substring(0, 2),
                        t.StartsWith(""Bar""),
                        t.ToLower(),
                        t.ToUpper()
                        t.Trim(),
                        //TODO: valid AddNanoseconds() call
                        //TODO: valid AddMicroseconds() call
                        t.At.AddMilliseconds(1),
                        t.At.AddSeconds(1),
                        t.At.AddMinutes(1),
                        t.At.AddHours(1),
                        t.At.AddDays(1),
                        t.At.AddMonths(1),
                        t.At.AddYears(1),
                        //TODO: valid CreateDateTime() call
                        //TODO: valid CreateDateTimeOffset() call
                        //TODO: valid CreateTime() call
                        //TODO: valid CurrentDateTime() call
                        //TODO: valid CurrentDateTimeOffset() call
                        //TODO: valid CurrentUtcDateTime() call
                        //TODO: valid Day() call
                        //TODO: valid DayOfYear() call
                        //TODO: valid DiffNanoseconds() call
                        //TODO: valid DiffMilliseconds() call
                        //TODO: valid DiffMicroseconds() call
                        //TODO: valid DiffSeconds() call
                        //TODO: valid DiffMinutes() call
                        //TODO: valid DiffHours() call
                        //TODO: valid DiffDays() call
                        //TODO: valid DiffMonths() call
                        //TODO: valid DiffYears() call
                        //TODO: valid GetTotalOffsetMinutes() call
                        //TODO: valid Hour() call
                        //TODO: valid Millisecond() call
                        //TODO: valid Minute() call
                        //TODO: valid Month() call
                        //TODO: valid Second() call
                        //TODO: valid TruncateTime() call
                        //TODO: valid Year() call
                        //TODO: valid BitWiseAnd() call
                        //TODO: valid BitWiseNot() call
                        //TODO: valid BitWiseOr() call
                        //TODO: valid BitWiseXor() call
                        //TODO: valid Area() call
                        //TODO: valid AsBinary() call
                        //TODO: valid AsGml() call
                        //TODO: valid AsText() call
                        //TODO: valid Centroid() call
                        //TODO: valid CoordinateSystemId()
                        //TODO: valid Distance() call
                        //TODO: valid Elevation() call
                        //TODO: valid EndPoint() call
                        //TODO: valid ExteriorRing() call
                        //TODO: valid GeographyCollectionFromBinary() call
                        //TODO: valid GeographyCollectionFromText() call
                        //TODO: valid GeographyFromBinary() call
                        //TODO: valid GeographyFromGml() call
                        //TODO: valid GeographyFromText() call
                        //TODO: valid GeographyLineFromBinary() call
                        //TODO: valid GeographyLineFromText() call
                        //TODO: valid GeographyMultiLineFromBinary() call
                        //TODO: valid GeographyMultiLineFromText() call
                        //TODO: valid GeographyMultiPointFromBinary() call
                        //TODO: valid GeographyMultiPointFromText() call
                        //TODO: valid GeographyMultiPolygonFromBinary() call
                        //TODO: valid GeographyMultiPolygonFromText() call
                        //TODO: valid GeographyPointFromBinary() call
                        //TODO: valid GeographyPointFromText() call
                        //TODO: valid GeographyPolygonFromBinary() call
                        //TODO: valid GeographyPolygonFromText() call
                        //TODO: valid GeometryCollectionFromBinary() call
                        //TODO: valid GeometryCollectionFromText() call
                        //TODO: valid GeometryFromBinary() call
                        //TODO: valid GeometryFromGml() call
                        //TODO: valid GeometryFromText() call
                        //TODO: valid GeometryLineFromBinary() call
                        //TODO: valid GeometryLineFromText() call
                        //TODO: valid GeometryMultiLineFromBinary() call
                        //TODO: valid GeometryMultiLineFromText() call
                        //TODO: valid GeometryMultiPointFromBinary() call
                        //TODO: valid GeometryMultiPointFromText() call
                        //TODO: valid GeometryMultiPolygonFromBinary() call
                        //TODO: valid GeometryMultiPolygonFromText() call
                        //TODO: valid GeometryPointFromBinary() call
                        //TODO: valid GeometryPointFromText() call
                        //TODO: valid GeometryPolygonFromBinary() call
                        //TODO: valid GeometryPolygonFromText() call
                        //TODO: valid InteriorRingAt() call
                        //TODO: valid InteriorRingCount() call
                        //TODO: valid IsClosedSpatial() call
                        //TODO: valid IsEmptySpatial() call
                        //TODO: valid IsRing() call
                        //TODO: valid IsSimpleGeometry() call
                        //TODO: valid IsValidGeometry() call
                        //TODO: valid Latitude() call
                        //TODO: valid Longitude() call
                        //TODO: valid Measure() call
                        //TODO: valid PointAt() call
                        //TODO: valid PointCount() call
                        //TODO: valid PointOnSurface() call
                        //TODO: valid SpatialBoundary() call
                        //TODO: valid SpatialBuffer() call
                        //TODO: valid SpatialContains() call
                        //TODO: valid SpatialConvexHull() call
                        //TODO: valid SpatialCrosses() call
                        //TODO: valid SpatialDifference() call
                        //TODO: valid SpatialDimension() call
                        //TODO: valid SpatialDisjoint() call
                        //TODO: valid SpatialElementAt() call
                        //TODO: valid SpatialElementCount() call
                        //TODO: valid SpatialEnvelope() call
                        //TODO: valid SpatialEquals() call
                        //TODO: valid SpatialIntersection() call
                        //TODO: valid SpatialIntersects() call
                        //TODO: valid SpatialLength() call
                        //TODO: valid SpatialOverlaps() call
                        //TODO: valid SpatialRelate() call
                        //TODO: valid SpatialSymmetricDifference() call
                        //TODO: valid SpatialTouches() call
                        //TODO: valid SpatialTypeName() call
                        //TODO: valid SpatialUnion() call
                        //TODO: valid SpatialWithin() call
                        //TODO: valid StartPoint() call
                        //TODO: valid XCoordinate() call
                        //TODO: valid YCooridnate() call
                        Guid.NewGuid()
                    });
                }
            }
        }
    }";
            VerifyCSharpDiagnostic(test);
        }

        [TestMethod]
        public void EFLINQ003_LinqSelect_IndirectDbSet()
        {
            var test = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;
    using System.Data.Entity;

    namespace ConsoleApplication1
    {
        public class MyContext : DbContext
        {
            public DbSet<Thing> Things { get; set; }
        }

        public class Thing
        {
            public string Foo { get; set; }
            public string Bar { get; set; }
        }

        class Program
        {
            static bool FooIsBar(string foo, string bar)
            {
                return foo == bar;
            }

            public static void Main(string [] args)
            {
                using (var context = new MyContext())
                {
                    var things = context.Things;
                    var items = things.Select(t => new { t.Foo, t.Bar, IsMatch = FooIsBar(t.Foo, t.Bar) });
                }
            }
        }
    }";

            VerifyCSharpDiagnostic(test,
                new DiagnosticResult
                {
                    Id = "EFLINQ003",
                    Message = String.Format("Static method '{0}' cannot be used within a LINQ to Entities expression", "FooIsBar"),
                    Severity = DiagnosticSeverity.Error,
                    Locations =
                        new[] {
                                new DiagnosticResultLocation("Test0.cs", 35, 82)
                            }
                });
        }

        [TestMethod]
        public void EFLINQ003_LinqSelect_IndirectDbSetFromMethod()
        {
            var test = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;
    using System.Data.Entity;

    namespace ConsoleApplication1
    {
        public class MyContext : DbContext
        {
            public DbSet<Thing> Things { get; set; }
        }

        public class Thing
        {
            public string Foo { get; set; }
            public string Bar { get; set; }
        }

        class Program
        {
            static bool FooIsBar(string foo, string bar)
            {
                return foo == bar;
            }

            static DbSet<Thing> GetThings(MyContext context)
            {
                return context.Things;
            }

            public static void Main(string [] args)
            {
                using (var context = new MyContext())
                {
                    var things = GetThings(context);
                    var items = things.Select(t => new { t.Foo, t.Bar, IsMatch = FooIsBar(t.Foo, t.Bar) });
                }
            }
        }
    }";

            VerifyCSharpDiagnostic(test,
                new DiagnosticResult
                {
                    Id = "EFLINQ003",
                    Message = String.Format("Static method '{0}' cannot be used within a LINQ to Entities expression", "FooIsBar"),
                    Severity = DiagnosticSeverity.Error,
                    Locations =
                        new[] {
                                new DiagnosticResultLocation("Test0.cs", 40, 82)
                            }
                });
        }
        
        [TestMethod]
        public void EFLINQ003_LinqSelect_NonDirectDbContextDescendant()
        {
            var test = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;
    using System.Data.Entity;

    namespace ConsoleApplication1
    {
        public class MyBaseContext : DbContext
        {
        }

        public class MyContext : MyBaseContext
        {
            public DbSet<Thing> Things { get; set; }
        }

        public class Thing
        {
            public string Foo { get; set; }
            public string Bar { get; set; }
        }

        class Program
        {
            static bool FooIsBar(string foo, string bar)
            {
                return foo == bar;
            }

            static DbSet<Thing> GetThings(MyContext context)
            {
                return context.Things;
            }

            public static void Main(string [] args)
            {
                using (var context = new MyContext())
                {
                    var things = GetThings(context);
                    var items = things.Select(t => new { t.Foo, t.Bar, IsMatch = FooIsBar(t.Foo, t.Bar) });
                }
            }
        }
    }";

            VerifyCSharpDiagnostic(test,
                new DiagnosticResult
                {
                    Id = "EFLINQ003",
                    Message = String.Format("Static method '{0}' cannot be used within a LINQ to Entities expression", "FooIsBar"),
                    Severity = DiagnosticSeverity.Error,
                    Locations =
                        new[] {
                                new DiagnosticResultLocation("Test0.cs", 44, 82)
                            }
                });
        }

        [TestMethod]
        public void EFLINQ003_LinqWhere_IndirectLambdaFromLocalVar()
        {
            var test = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;
    using System.Data.Entity;

    namespace ConsoleApplication1
    {
        public class MyContext : DbContext
        {
            public DbSet<Thing> Things { get; set; }
        }

        public class Thing
        {
            public string Foo { get; set; }
            public string Bar { get; set; }
        }

        class Program
        {
            static bool FooIsBar(string foo, string bar)
            {
                return foo == bar;
            }

            static DbSet<Thing> GetThings(MyContext context)
            {
                return context.Things;
            }

            public static void Main(string [] args)
            {
                using (var context = new MyContext())
                {
                    Expression<Func<Thing, bool>> predicate = t => FooIsBar(t.Foo, t.Bar);
                    var items = context.Things.Where(predicate);
                }
            }
        }
    }";

            VerifyCSharpDiagnostic(test,
                new DiagnosticResult
                {
                    Id = "EFLINQ003",
                    Message = String.Format("Static method '{0}' cannot be used within a LINQ to Entities expression", "FooIsBar"),
                    Severity = DiagnosticSeverity.Error,
                    Locations =
                        new[] {
                                new DiagnosticResultLocation("Test0.cs", 40, 68)
                            }
                });
        }

        [TestMethod]
        public void EFLINQ004_LinqWhere()
        {
            var test = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;
    using System.Data.Entity;

    namespace ConsoleApplication1
    {
        public class MyContext : DbContext
        {
            public DbSet<Thing> Things { get; set; }
        }

        public class Thing
        {
            public string Foo { get; set; }
            public string Bar { get; set; }
        }

        class Program
        {
            public static void Main(string [] args)
            {
                using (var context = new MyContext())
                {
                    var items = context.Things.Where(t => t.Foo.CompareTo(t.Bar) == 0);
                }
            }
        }
    }";
            VerifyCSharpDiagnostic(test,
                new DiagnosticResult
                {
                    Id = "EFLINQ004",
                    Message = String.Format("Method '{0}' cannot be used within a LINQ to Entities expression", "CompareTo"),
                    Severity = DiagnosticSeverity.Error,
                    Locations =
                        new[] {
                                new DiagnosticResultLocation("Test0.cs", 29, 59)
                            }
                });
        }

        [TestMethod]
        public void EFLINQ004_LinqSelect()
        {
            var test = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;
    using System.Data.Entity;

    namespace ConsoleApplication1
    {
        public class MyContext : DbContext
        {
            public DbSet<Thing> Things { get; set; }
        }

        public class Thing
        {
            public string Foo { get; set; }
            public string Bar { get; set; }
        }

        class Program
        {
            public static void Main(string [] args)
            {
                using (var context = new MyContext())
                {
                    var items = context.Things.Select(t => new { t.Foo, t.Bar, Clone = t.Foo.Clone() });
                }
            }
        }
    }";
            VerifyCSharpDiagnostic(test,
                new DiagnosticResult
                {
                    Id = "EFLINQ004",
                    Message = String.Format("Method '{0}' cannot be used within a LINQ to Entities expression", "Clone"),
                    Severity = DiagnosticSeverity.Error,
                    Locations =
                        new[] {
                                new DiagnosticResultLocation("Test0.cs", 29, 88)
                            }
                });
        }

        [TestMethod]
        public void EFLINQ005_LinqWhere()
        {
            var test = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;
    using System.Data.Entity;

    namespace ConsoleApplication1
    {
        public class MyContext : DbContext
        {
            public DbSet<Thing> Things { get; set; }
        }

        public class Thing
        {
            public string Foo { get; set; }
            public string Bar { get; set; }

            public string FooBar
            {
                get { return this.Foo + "" "" + this.Bar; }
            }

            public string FooBarExpr => this.Foo + "" "" + this.Bar;
        }

        class Program
        {
            static IQueryable<Thing> GetThings(MyContext context)
            {
                return context.Things;
            }

            public static void Main(string [] args)
            {
                using (var context = new MyContext())
                {
                    IQueryable<Thing> things = GetThings(context);
                    var items = things.Where(t => t.FooBar == ""Some Value"");
                    var items2 = things.Where(t => t.FooBarExpr == ""Some other value"");
                }
            }
        }
    }";
            VerifyCSharpDiagnostic(test,
                new DiagnosticResult
                {
                    Id = "EFLINQ001",
                    Message = String.Format("Property '{0}' in type '{1}' not translatable in LINQ to Entities", "FooBar", "Thing"),
                    Severity = DiagnosticSeverity.Info,
                    Locations =
                        new[] {
                                new DiagnosticResultLocation("Test0.cs", 22, 13)
                            }
                },
                new DiagnosticResult
                {
                    Id = "EFLINQ001",
                    Message = String.Format("Property '{0}' in type '{1}' not translatable in LINQ to Entities", "FooBarExpr", "Thing"),
                    Severity = DiagnosticSeverity.Info,
                    Locations =
                        new[] {
                                new DiagnosticResultLocation("Test0.cs", 27, 13)
                            }
                },
                new DiagnosticResult
                {
                    Id = "EFLINQ005",
                    Message = String.Format("Read-Only property '{0}' of type '{1}' potentially used in LINQ to Entities expression", "FooBar", "Thing"),
                    Severity = DiagnosticSeverity.Warning,
                    Locations =
                        new[] {
                                new DiagnosticResultLocation("Test0.cs", 42, 51)
                            }
                },
                new DiagnosticResult
                {
                    Id = "EFLINQ005",
                    Message = String.Format("Read-Only property '{0}' of type '{1}' potentially used in LINQ to Entities expression", "FooBarExpr", "Thing"),
                    Severity = DiagnosticSeverity.Warning,
                    Locations =
                        new[] {
                                new DiagnosticResultLocation("Test0.cs", 43, 52)
                            }
                });
        }

        [TestMethod]
        public void EFLINQ005_LinqSelect()
        {
            var test = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;
    using System.Data.Entity;

    namespace ConsoleApplication1
    {
        public class MyContext : DbContext
        {
            public DbSet<Thing> Things { get; set; }
        }

        public class Thing
        {
            public string Foo { get; set; }
            public string Bar { get; set; }

            public string FooBar
            {
                get { return this.Foo + "" "" + this.Bar; }
            }

            public string FooBarExpr => this.Foo + "" "" + this.Bar;
        }

        class Program
        {
            static IQueryable<Thing> GetThings(MyContext context)
            {
                return context.Things;
            }

            public static void Main(string [] args)
            {
                using (var context = new MyContext())
                {
                    IQueryable<Thing> things = GetThings(context);
                    var items = things.Select(t => new { t.Foo, t.Bar, t.FooBar, t.FooBarExpr });
                }
            }
        }
    }";
            VerifyCSharpDiagnostic(test,
                new DiagnosticResult
                {
                    Id = "EFLINQ001",
                    Message = String.Format("Property '{0}' in type '{1}' not translatable in LINQ to Entities", "FooBar", "Thing"),
                    Severity = DiagnosticSeverity.Info,
                    Locations =
                        new[] {
                                new DiagnosticResultLocation("Test0.cs", 22, 13)
                            }
                },
                new DiagnosticResult
                {
                    Id = "EFLINQ001",
                    Message = String.Format("Property '{0}' in type '{1}' not translatable in LINQ to Entities", "FooBarExpr", "Thing"),
                    Severity = DiagnosticSeverity.Info,
                    Locations =
                        new[] {
                                new DiagnosticResultLocation("Test0.cs", 27, 13)
                            }
                },
                new DiagnosticResult
                {
                    Id = "EFLINQ005",
                    Message = String.Format("Read-Only property '{0}' of type '{1}' potentially used in LINQ to Entities expression", "FooBar", "Thing"),
                    Severity = DiagnosticSeverity.Warning,
                    Locations =
                        new[] {
                                new DiagnosticResultLocation("Test0.cs", 42, 72)
                            }
                },
                new DiagnosticResult
                {
                    Id = "EFLINQ005",
                    Message = String.Format("Read-Only property '{0}' of type '{1}' potentially used in LINQ to Entities expression", "FooBarExpr", "Thing"),
                    Severity = DiagnosticSeverity.Warning,
                    Locations =
                        new[] {
                                new DiagnosticResultLocation("Test0.cs", 42, 82)
                            }
                });
        }

        [TestMethod]
        public void EFLINQ006_LinqSelect_IndirectDbSetFromMethodThruIQueryableFacade()
        {
            var test = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;
    using System.Data.Entity;

    namespace ConsoleApplication1
    {
        public class MyContext : DbContext
        {
            public DbSet<Thing> Things { get; set; }
        }

        public class Thing
        {
            public string Foo { get; set; }
            public string Bar { get; set; }
        }

        class Program
        {
            static bool FooIsBar(string foo, string bar)
            {
                return foo == bar;
            }

            static IQueryable<Thing> GetThings(MyContext context)
            {
                return context.Things;
            }

            public static void Main(string [] args)
            {
                using (var context = new MyContext())
                {
                    IQueryable<Thing> things = GetThings(context);
                    var items = things.Select(t => new { t.Foo, t.Bar, IsMatch = FooIsBar(t.Foo, t.Bar) });
                }
            }
        }
    }";

            VerifyCSharpDiagnostic(test,
                new DiagnosticResult
                {
                    Id = "EFLINQ006",
                    Message = String.Format("Unsupported static method '{0}' potentially being used within a LINQ to Entities expression", "FooIsBar"),
                    Severity = DiagnosticSeverity.Warning,
                    Locations =
                        new[] {
                                new DiagnosticResultLocation("Test0.cs", 40, 82)
                            }
                });
        }

        [TestMethod]
        public void EFLINQ008_LinqWhere()
        {
            var test = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;
    using System.Data.Entity;

    namespace ConsoleApplication1
    {
        public class MyContext : DbContext
        {
            public DbSet<Thing> Things { get; set; }

            public DbSet<Sprocket> Sprockets { get; set; }
        }

        public class Thing
        {
            public int Id { get; set; }
            public string Foo { get; set; }
            public string Bar { get; set; }
            public virtual ICollection<Sprocket> Sprockets { get; set; }
        }

        public class Sprocket
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public int? ThingId { get; set; }
            public virtual Thing Thing { get; set; }
        }

        class Program
        {
            static bool FooIsBar(string foo, string bar)
            {
                return foo == bar;
            }

            static IQueryable<Thing> GetThings(MyContext context)
            {
                return context.Things;
            }

            public static void Main(string [] args)
            {
                using (var context = new MyContext())
                {
                    var items = context.Things.Where(t => t.Sprockets.Where(s => s.Name == ""Ssdkfjd"");
                }
            }
        }
    }";

            VerifyCSharpDiagnostic(test,
                new DiagnosticResult
                {
                    Id = "EFLINQ008",
                    Message = String.Format("Unsupported static method '{0}' potentially being used within a LINQ to Entities expression", "FooIsBar"),
                    Severity = DiagnosticSeverity.Error,
                    Locations =
                        new[] {
                                new DiagnosticResultLocation("Test0.cs", 40, 82)
                            }
                });
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return null;
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new EFLinqAnalyzerAnalyzer();
        }
    }
}