﻿using Microsoft.CodeAnalysis;
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
        public void EFLINQ002_WhereClause()
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
                    Severity = DiagnosticSeverity.Warning,
                    Locations =
                        new[] {
                                new DiagnosticResultLocation("Test0.cs", 36, 59)
                            }
                },
                new DiagnosticResult
                {
                    Id = "EFLINQ002",
                    Message = String.Format("Read-Only property '{0}' of type '{1}' used in LINQ to Entities expression", "FooBarExpr", "Thing"),
                    Severity = DiagnosticSeverity.Warning,
                    Locations =
                        new[] {
                                new DiagnosticResultLocation("Test0.cs", 37, 60)
                            }
                });
        }

        [TestMethod]
        public void EFLINQ002_SelectProjection()
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
                    Severity = DiagnosticSeverity.Warning,
                    Locations =
                        new[] {
                                new DiagnosticResultLocation("Test0.cs", 36, 80)
                            }
                },
                new DiagnosticResult
                {
                    Id = "EFLINQ002",
                    Message = String.Format("Read-Only property '{0}' of type '{1}' used in LINQ to Entities expression", "FooBarExpr", "Thing"),
                    Severity = DiagnosticSeverity.Warning,
                    Locations =
                        new[] {
                                new DiagnosticResultLocation("Test0.cs", 36, 90)
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