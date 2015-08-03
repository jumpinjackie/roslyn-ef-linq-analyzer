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
            var test = SourceFiles.NonEFClassWithReadOnlyProperty;
            VerifyCSharpDiagnostic(test);
        }

        //Class with read-only property (as an expression-bodied member). No diagnostics as this is not a EF code first model entity class
        [TestMethod]
        public void NonEFClassWithReadOnlyPropertyExprMember()
        {
            var test = SourceFiles.NonEFClassWithReadOnlyPropertyExprMember;
            VerifyCSharpDiagnostic(test);
        }

        [TestMethod]
        public void EFLINQ001_ClassWithReadOnlyProperty_ReferencedInUnqualifiedDbContext()
        {
            var test = SourceFiles.EFLINQ001_ClassWithReadOnlyProperty_ReferencedInUnqualifiedDbContext;
            var expected = new DiagnosticResult
            {
                Id = "EFLINQ001",
                Message = String.Format("Property '{0}' in type '{1}' not translatable in LINQ to Entities", "FooBar", "Thing"),
                Severity = DiagnosticSeverity.Info,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 21, 9)
                        }
            };

            VerifyCSharpDiagnostic(test, expected);
        }

        [TestMethod]
        public void EFLINQ001_ClassWithReadOnlyPropertyAndExprBodiedMember_ReferencedInUnqualifiedDbContext()
        {
            var test = SourceFiles.EFLINQ001_ClassWithReadOnlyPropertyAndExprBodiedMember_ReferencedInUnqualifiedDbContext;
            VerifyCSharpDiagnostic(test, 
                new DiagnosticResult
                {
                    Id = "EFLINQ001",
                    Message = String.Format("Property '{0}' in type '{1}' not translatable in LINQ to Entities", "FooBar", "Thing"),
                    Severity = DiagnosticSeverity.Info,
                    Locations =
                        new[] {
                                new DiagnosticResultLocation("Test0.cs", 21, 9)
                            }
                },
                new DiagnosticResult
                {
                    Id = "EFLINQ001",
                    Message = String.Format("Property '{0}' in type '{1}' not translatable in LINQ to Entities", "FooBarExpr", "Thing"),
                    Severity = DiagnosticSeverity.Info,
                    Locations =
                        new[] {
                                new DiagnosticResultLocation("Test0.cs", 26, 9)
                            }
                });
        }

        [TestMethod]
        public void EFLINQ001_ClassWithReadOnlyPropertyAndExprBodiedMember_ReferencedInFullyQualifiedDbContext()
        {
            var test = SourceFiles.EFLINQ001_ClassWithReadOnlyPropertyAndExprBodiedMember_ReferencedInFullyQualifiedDbContext;
            VerifyCSharpDiagnostic(test,
                new DiagnosticResult
                {
                    Id = "EFLINQ001",
                    Message = String.Format("Property '{0}' in type '{1}' not translatable in LINQ to Entities", "FooBar", "Thing"),
                    Severity = DiagnosticSeverity.Info,
                    Locations =
                        new[] {
                                new DiagnosticResultLocation("Test0.cs", 20, 9)
                            }
                },
                new DiagnosticResult
                {
                    Id = "EFLINQ001",
                    Message = String.Format("Property '{0}' in type '{1}' not translatable in LINQ to Entities", "FooBarExpr", "Thing"),
                    Severity = DiagnosticSeverity.Info,
                    Locations =
                        new[] {
                                new DiagnosticResultLocation("Test0.cs", 25, 9)
                            }
                });
        }

        [TestMethod]
        public void EFLINQ002_LinqWhere()
        {
            var test = SourceFiles.EFLINQ002_LinqWhere;
            VerifyCSharpDiagnostic(test,
                new DiagnosticResult
                {
                    Id = "EFLINQ001",
                    Message = String.Format("Property '{0}' in type '{1}' not translatable in LINQ to Entities", "FooBar", "Thing"),
                    Severity = DiagnosticSeverity.Info,
                    Locations =
                        new[] {
                                new DiagnosticResultLocation("Test0.cs", 21, 9)
                            }
                },
                new DiagnosticResult
                {
                    Id = "EFLINQ001",
                    Message = String.Format("Property '{0}' in type '{1}' not translatable in LINQ to Entities", "FooBarExpr", "Thing"),
                    Severity = DiagnosticSeverity.Info,
                    Locations =
                        new[] {
                                new DiagnosticResultLocation("Test0.cs", 26, 9)
                            }
                },
                new DiagnosticResult
                {
                    Id = "EFLINQ002",
                    Message = String.Format("Read-Only property '{0}' of type '{1}' used in LINQ to Entities expression", "FooBar", "Thing"),
                    Severity = DiagnosticSeverity.Error,
                    Locations =
                        new[] {
                                new DiagnosticResultLocation("Test0.cs", 35, 55)
                            }
                },
                new DiagnosticResult
                {
                    Id = "EFLINQ002",
                    Message = String.Format("Read-Only property '{0}' of type '{1}' used in LINQ to Entities expression", "FooBarExpr", "Thing"),
                    Severity = DiagnosticSeverity.Error,
                    Locations =
                        new[] {
                                new DiagnosticResultLocation("Test0.cs", 36, 56)
                            }
                });
        }

        [TestMethod]
        public void EFLINQ002_LinqWhere_QuerySyntax()
        {
            var test = SourceFiles.EFLINQ002_LinqWhere_QuerySyntax;
            VerifyCSharpDiagnostic(test,
                new DiagnosticResult
                {
                    Id = "EFLINQ001",
                    Message = String.Format("Property '{0}' in type '{1}' not translatable in LINQ to Entities", "Foo", "Thing"),
                    Severity = DiagnosticSeverity.Info,
                    Locations =
                        new[] {
                                new DiagnosticResultLocation("Test0.cs", 10, 9)
                            }
                },
                new DiagnosticResult
                {
                    Id = "EFLINQ001",
                    Message = String.Format("Property '{0}' in type '{1}' not translatable in LINQ to Entities", "Bar", "Thing"),
                    Severity = DiagnosticSeverity.Info,
                    Locations =
                        new[] {
                                new DiagnosticResultLocation("Test0.cs", 14, 9)
                            }
                },
                new DiagnosticResult
                {
                    Id = "EFLINQ002",
                    Message = String.Format("Read-Only property '{0}' of type '{1}' used in LINQ to Entities expression", "Bar", "Thing"),
                    Severity = DiagnosticSeverity.Error,
                    Locations =
                        new[] {
                                new DiagnosticResultLocation("Test0.cs", 29, 35)
                            }
                },
                new DiagnosticResult
                {
                    Id = "EFLINQ002",
                    Message = String.Format("Read-Only property '{0}' of type '{1}' used in LINQ to Entities expression", "Foo", "Thing"),
                    Severity = DiagnosticSeverity.Error,
                    Locations =
                        new[] {
                                new DiagnosticResultLocation("Test0.cs", 30, 35)
                            }
                });
        }

        [TestMethod]
        public void EFLINQ002_LinqSelect_QuerySyntax()
        {
            var test = SourceFiles.EFLINQ002_LinqSelect_QuerySyntax;
            VerifyCSharpDiagnostic(test,
                new DiagnosticResult
                {
                    Id = "EFLINQ001",
                    Message = String.Format("Property '{0}' in type '{1}' not translatable in LINQ to Entities", "Foo", "Thing"),
                    Severity = DiagnosticSeverity.Info,
                    Locations =
                        new[] {
                                new DiagnosticResultLocation("Test0.cs", 10, 9)
                            }
                },
                new DiagnosticResult
                {
                    Id = "EFLINQ001",
                    Message = String.Format("Property '{0}' in type '{1}' not translatable in LINQ to Entities", "Bar", "Thing"),
                    Severity = DiagnosticSeverity.Info,
                    Locations =
                        new[] {
                                new DiagnosticResultLocation("Test0.cs", 14, 9)
                            }
                },
                new DiagnosticResult
                {
                    Id = "EFLINQ002",
                    Message = String.Format("Read-Only property '{0}' of type '{1}' used in LINQ to Entities expression", "Foo", "Thing"),
                    Severity = DiagnosticSeverity.Error,
                    Locations =
                        new[] {
                                new DiagnosticResultLocation("Test0.cs", 30, 56)
                            }
                },
                new DiagnosticResult
                {
                    Id = "EFLINQ002",
                    Message = String.Format("Read-Only property '{0}' of type '{1}' used in LINQ to Entities expression", "Bar", "Thing"),
                    Severity = DiagnosticSeverity.Error,
                    Locations =
                        new[] {
                                new DiagnosticResultLocation("Test0.cs", 30, 63)
                            }
                });
        }

        [TestMethod]
        public void EFLINQ002_LinqSelect()
        {
            var test = SourceFiles.EFLINQ002_LinqSelect;
            VerifyCSharpDiagnostic(test,
                new DiagnosticResult
                {
                    Id = "EFLINQ001",
                    Message = String.Format("Property '{0}' in type '{1}' not translatable in LINQ to Entities", "FooBar", "Thing"),
                    Severity = DiagnosticSeverity.Info,
                    Locations =
                        new[] {
                                new DiagnosticResultLocation("Test0.cs", 21, 9)
                            }
                },
                new DiagnosticResult
                {
                    Id = "EFLINQ001",
                    Message = String.Format("Property '{0}' in type '{1}' not translatable in LINQ to Entities", "FooBarExpr", "Thing"),
                    Severity = DiagnosticSeverity.Info,
                    Locations =
                        new[] {
                                new DiagnosticResultLocation("Test0.cs", 26, 9)
                            }
                },
                new DiagnosticResult
                {
                    Id = "EFLINQ002",
                    Message = String.Format("Read-Only property '{0}' of type '{1}' used in LINQ to Entities expression", "FooBar", "Thing"),
                    Severity = DiagnosticSeverity.Error,
                    Locations =
                        new[] {
                                new DiagnosticResultLocation("Test0.cs", 35, 76)
                            }
                },
                new DiagnosticResult
                {
                    Id = "EFLINQ002",
                    Message = String.Format("Read-Only property '{0}' of type '{1}' used in LINQ to Entities expression", "FooBarExpr", "Thing"),
                    Severity = DiagnosticSeverity.Error,
                    Locations =
                        new[] {
                                new DiagnosticResultLocation("Test0.cs", 35, 86)
                            }
                });
        }

        [TestMethod]
        public void EFLINQ003_LinqWhere()
        {
            var test = SourceFiles.EFLINQ003_LinqWhere;
            VerifyCSharpDiagnostic(test,
                new DiagnosticResult
                {
                    Id = "EFLINQ003",
                    Message = String.Format("Static method '{0}' cannot be used within a LINQ to Entities expression", "FooIsBar"),
                    Severity = DiagnosticSeverity.Error,
                    Locations =
                        new[] {
                                new DiagnosticResultLocation("Test0.cs", 33, 55)
                            }
                });
        }

        [TestMethod]
        public void EFLINQ003_LinqSelect()
        {
            var test = SourceFiles.EFLINQ003_LinqSelect;
            VerifyCSharpDiagnostic(test,
                new DiagnosticResult
                {
                    Id = "EFLINQ003",
                    Message = String.Format("Static method '{0}' cannot be used within a LINQ to Entities expression", "FooIsBar"),
                    Severity = DiagnosticSeverity.Error,
                    Locations =
                        new[] {
                                new DiagnosticResultLocation("Test0.cs", 33, 86)
                            }
                });
        }

        [TestMethod]
        public void EFLINQ003_LinqSelect_ValidMethods()
        {
            var test = SourceFiles.EFLINQ003_LinqSelect_ValidMethods;
            VerifyCSharpDiagnostic(test);
        }

        [TestMethod]
        public void EFLINQ003_LinqSelect_IndirectDbSet()
        {
            var test = SourceFiles.EFLINQ003_LinqSelect_IndirectDbSet;
            VerifyCSharpDiagnostic(test,
                new DiagnosticResult
                {
                    Id = "EFLINQ003",
                    Message = String.Format("Static method '{0}' cannot be used within a LINQ to Entities expression", "FooIsBar"),
                    Severity = DiagnosticSeverity.Error,
                    Locations =
                        new[] {
                                new DiagnosticResultLocation("Test0.cs", 34, 78)
                            }
                });
        }

        [TestMethod]
        public void EFLINQ003_LinqSelect_IndirectDbSetFromMethod()
        {
            var test = SourceFiles.EFLINQ003_LinqSelect_IndirectDbSetFromMethod;
            VerifyCSharpDiagnostic(test,
                new DiagnosticResult
                {
                    Id = "EFLINQ003",
                    Message = String.Format("Static method '{0}' cannot be used within a LINQ to Entities expression", "FooIsBar"),
                    Severity = DiagnosticSeverity.Error,
                    Locations =
                        new[] {
                                new DiagnosticResultLocation("Test0.cs", 39, 78)
                            }
                });
        }
        
        [TestMethod]
        public void EFLINQ003_LinqSelect_NonDirectDbContextDescendant()
        {
            var test = SourceFiles.EFLINQ003_LinqSelect_NonDirectDbContextDescendant;
            VerifyCSharpDiagnostic(test,
                new DiagnosticResult
                {
                    Id = "EFLINQ003",
                    Message = String.Format("Static method '{0}' cannot be used within a LINQ to Entities expression", "FooIsBar"),
                    Severity = DiagnosticSeverity.Error,
                    Locations =
                        new[] {
                                new DiagnosticResultLocation("Test0.cs", 43, 78)
                            }
                });
        }

        [TestMethod]
        public void EFLINQ003_LinqWhere_IndirectLambdaFromLocalVar()
        {
            var test = SourceFiles.EFLINQ003_LinqWhere_IndirectLambdaFromLocalVar;
            VerifyCSharpDiagnostic(test,
                new DiagnosticResult
                {
                    Id = "EFLINQ003",
                    Message = String.Format("Static method '{0}' cannot be used within a LINQ to Entities expression", "FooIsBar"),
                    Severity = DiagnosticSeverity.Error,
                    Locations =
                        new[] {
                                new DiagnosticResultLocation("Test0.cs", 39, 64)
                            }
                });
        }

        [TestMethod]
        public void EFLINQ004_LinqWhere()
        {
            var test = SourceFiles.EFLINQ004_LinqWhere;
            VerifyCSharpDiagnostic(test,
                new DiagnosticResult
                {
                    Id = "EFLINQ004",
                    Message = String.Format("Method '{0}' cannot be used within a LINQ to Entities expression", "CompareTo"),
                    Severity = DiagnosticSeverity.Error,
                    Locations =
                        new[] {
                                new DiagnosticResultLocation("Test0.cs", 28, 55)
                            }
                });
        }

        [TestMethod]
        public void EFLINQ004_LinqSelect()
        {
            var test = SourceFiles.EFLINQ004_LinqSelect;
            VerifyCSharpDiagnostic(test,
                new DiagnosticResult
                {
                    Id = "EFLINQ004",
                    Message = String.Format("Method '{0}' cannot be used within a LINQ to Entities expression", "Clone"),
                    Severity = DiagnosticSeverity.Error,
                    Locations =
                        new[] {
                                new DiagnosticResultLocation("Test0.cs", 28, 84)
                            }
                });
        }

        [TestMethod]
        public void EFLINQ005_LinqWhere()
        {
            var test = SourceFiles.EFLINQ005_LinqWhere;
            VerifyCSharpDiagnostic(test,
                new DiagnosticResult
                {
                    Id = "EFLINQ001",
                    Message = String.Format("Property '{0}' in type '{1}' not translatable in LINQ to Entities", "FooBar", "Thing"),
                    Severity = DiagnosticSeverity.Info,
                    Locations =
                        new[] {
                                new DiagnosticResultLocation("Test0.cs", 21, 9)
                            }
                },
                new DiagnosticResult
                {
                    Id = "EFLINQ001",
                    Message = String.Format("Property '{0}' in type '{1}' not translatable in LINQ to Entities", "FooBarExpr", "Thing"),
                    Severity = DiagnosticSeverity.Info,
                    Locations =
                        new[] {
                                new DiagnosticResultLocation("Test0.cs", 26, 9)
                            }
                },
                new DiagnosticResult
                {
                    Id = "EFLINQ005",
                    Message = String.Format("Read-Only property '{0}' of type '{1}' potentially used in LINQ to Entities expression", "FooBar", "Thing"),
                    Severity = DiagnosticSeverity.Warning,
                    Locations =
                        new[] {
                                new DiagnosticResultLocation("Test0.cs", 41, 47)
                            }
                },
                new DiagnosticResult
                {
                    Id = "EFLINQ005",
                    Message = String.Format("Read-Only property '{0}' of type '{1}' potentially used in LINQ to Entities expression", "FooBarExpr", "Thing"),
                    Severity = DiagnosticSeverity.Warning,
                    Locations =
                        new[] {
                                new DiagnosticResultLocation("Test0.cs", 42, 48)
                            }
                });
        }

        [TestMethod]
        public void EFLINQ005_LinqSelect()
        {
            var test = SourceFiles.EFLINQ005_LinqSelect;
            VerifyCSharpDiagnostic(test,
                new DiagnosticResult
                {
                    Id = "EFLINQ001",
                    Message = String.Format("Property '{0}' in type '{1}' not translatable in LINQ to Entities", "FooBar", "Thing"),
                    Severity = DiagnosticSeverity.Info,
                    Locations =
                        new[] {
                                new DiagnosticResultLocation("Test0.cs", 21, 9)
                            }
                },
                new DiagnosticResult
                {
                    Id = "EFLINQ001",
                    Message = String.Format("Property '{0}' in type '{1}' not translatable in LINQ to Entities", "FooBarExpr", "Thing"),
                    Severity = DiagnosticSeverity.Info,
                    Locations =
                        new[] {
                                new DiagnosticResultLocation("Test0.cs", 26, 9)
                            }
                },
                new DiagnosticResult
                {
                    Id = "EFLINQ005",
                    Message = String.Format("Read-Only property '{0}' of type '{1}' potentially used in LINQ to Entities expression", "FooBar", "Thing"),
                    Severity = DiagnosticSeverity.Warning,
                    Locations =
                        new[] {
                                new DiagnosticResultLocation("Test0.cs", 41, 68)
                            }
                },
                new DiagnosticResult
                {
                    Id = "EFLINQ005",
                    Message = String.Format("Read-Only property '{0}' of type '{1}' potentially used in LINQ to Entities expression", "FooBarExpr", "Thing"),
                    Severity = DiagnosticSeverity.Warning,
                    Locations =
                        new[] {
                                new DiagnosticResultLocation("Test0.cs", 41, 78)
                            }
                });
        }

        [TestMethod]
        public void EFLINQ006_LinqSelect_IndirectDbSetFromMethodThruIQueryableFacade()
        {
            var test = SourceFiles.EFLINQ006_LinqSelect_IndirectDbSetFromMethodThruIQueryableFacade;
            VerifyCSharpDiagnostic(test,
                new DiagnosticResult
                {
                    Id = "EFLINQ006",
                    Message = String.Format("Unsupported static method '{0}' potentially being used within a LINQ to Entities expression", "FooIsBar"),
                    Severity = DiagnosticSeverity.Warning,
                    Locations =
                        new[] {
                                new DiagnosticResultLocation("Test0.cs", 39, 78)
                            }
                });
        }

        [TestMethod]
        public void EFLINQ008_LinqWhere()
        {
            var test = SourceFiles.EFLINQ008_LinqWhere;
            VerifyCSharpDiagnostic(test,
                new DiagnosticResult
                {
                    Id = "EFLINQ008",
                    Message = String.Format("Navigation property '{0}' of type '{1}' within LINQ to Entities expression is not queryable", "Sprockets", "Thing"),
                    Severity = DiagnosticSeverity.Error,
                    Locations =
                        new[] {
                                new DiagnosticResultLocation("Test0.cs", 52, 71)
                            }
                });
        }

        [TestMethod]
        public void EFLINQ008_LinqWhere_LegitimateUse()
        {
            var test = SourceFiles.EFLINQ008_LinqWhere_LegitimateUse;
            VerifyCSharpDiagnostic(test);
        }

        [TestMethod]
        public void EFLINQ009_LinqWhere()
        {
            var test = SourceFiles.EFLINQ009_LinqWhere;
            VerifyCSharpDiagnostic(test,
                new DiagnosticResult
                {
                    Id = "EFLINQ009",
                    Message = String.Format("Navigation property '{0}' of type '{1}' within a potential LINQ to Entities expression is not queryable", "Sprockets", "Thing"),
                    Severity = DiagnosticSeverity.Warning,
                    Locations =
                        new[] {
                                new DiagnosticResultLocation("Test0.cs", 60, 63)
                            }
                });
        }

        [TestMethod]
        public void EFLINQ011_LinqSelect()
        {
            var test = SourceFiles.EFLINQ011_LinqSelect;
            VerifyCSharpDiagnostic(test,
                new DiagnosticResult
                {
                    Id = "EFLINQ011",
                    Message = String.Format("Interoplated string cannot be used in a LINQ to Entities expression"),
                    Severity = DiagnosticSeverity.Error,
                    Locations =
                        new[] {
                                new DiagnosticResultLocation("Test0.cs", 24, 77)
                            }
                });
        }

        [TestMethod]
        public void EFLINQ012_LinqSelect()
        {
            var test = SourceFiles.EFLINQ012_LinqSelect;
            VerifyCSharpDiagnostic(test,
                new DiagnosticResult
                {
                    Id = "EFLINQ012",
                    Message = String.Format("Interoplated string potentially used in a LINQ to Entities expression"),
                    Severity = DiagnosticSeverity.Warning,
                    Locations =
                        new[] {
                                new DiagnosticResultLocation("Test0.cs", 30, 69)
                            }
                });
        }

        [TestMethod]
        public void EFLINQ013_LinqSelect()
        {
            var test = SourceFiles.EFLINQ013_LinqSelect;
            VerifyCSharpDiagnostic(test,
                new DiagnosticResult
                {
                    Id = "EFLINQ013",
                    Message = String.Format("Property '{0}' of type '{1}' marked with [NotMapped] accessed within a LINQ to Entities expression", "Unmapped", "Thing"),
                    Severity = DiagnosticSeverity.Error,
                    Locations =
                        new[] {
                                new DiagnosticResultLocation("Test0.cs", 32, 76)
                            }
                },
                new DiagnosticResult
                {
                    Id = "EFLINQ013",
                    Message = String.Format("Property '{0}' of type '{1}' marked with [NotMapped] accessed within a LINQ to Entities expression", "Unmapped", "Thing"),
                    Severity = DiagnosticSeverity.Error,
                    Locations =
                        new[] {
                                new DiagnosticResultLocation("Test0.cs", 34, 57)
                            }
                });
        }

        [TestMethod]
        public void EFLINQ013_LinqWhere()
        {
            var test = SourceFiles.EFLINQ013_LinqWhere;
            VerifyCSharpDiagnostic(test,
                new DiagnosticResult
                {
                    Id = "EFLINQ013",
                    Message = String.Format("Property '{0}' of type '{1}' marked with [NotMapped] accessed within a LINQ to Entities expression", "Unmapped", "Thing"),
                    Severity = DiagnosticSeverity.Error,
                    Locations =
                        new[] {
                                new DiagnosticResultLocation("Test0.cs", 32, 55)
                            }
                },
                new DiagnosticResult
                {
                    Id = "EFLINQ013",
                    Message = String.Format("Property '{0}' of type '{1}' marked with [NotMapped] accessed within a LINQ to Entities expression", "Unmapped", "Thing"),
                    Severity = DiagnosticSeverity.Error,
                    Locations =
                        new[] {
                                new DiagnosticResultLocation("Test0.cs", 34, 36)
                            }
                });
        }

        [TestMethod]
        public void UseOfMethodInLinqExpressionTaggedWithDbFunction()
        {
            var test = SourceFiles.UseOfMethodInLinqExpressionTaggedWithDbFunction;
            VerifyCSharpDiagnostic(test);
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider() => null;

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() => new EFLinqAnalyzerAnalyzer();
    }
}