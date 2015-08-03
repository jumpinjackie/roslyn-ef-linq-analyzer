# roslyn-ef-linq-analyzer

A Roslyn Analyzer to check for common gotchas with LINQ expressions when used with Entity Framework

Introduction
------------

Entity Framework, while being a solid data access framework is laden with many gotchas that make your application prone to:

 * Poor query performance due to improperly constructed queries
 * Hitting into runtime exceptions because your LINQ queries while syntactically valid C# code, cannot be translated into valid SQL by Entity Framework at runtime.

While profiling tools can help with the former, nothing much can help with the latter besides knowledge, experience and discipline ... until now.

This Roslyn analyzer will hopefully assist with the latter, detecting and flagging errors and warnings on such invalid usages before you
ever start the application.

Build Requirements
------------------

 * Visual Studio 2015 RTM (Community or higher edition)
 * Roslyn compiler SDK

Analyzer Checks
---------------

 * A method call involving a LINQ operator on an expression known to be an IQueryable does not include any LINQ expressions containing:
   * Usages of read-only properties (classic and expression-bodied forms) on EF model model-first entity objects
   * Usages of properties on EF model-first entity objects marked with NotMapped attribute
   * Invalid LINQ sub-query expression on a collection navigation property on EF model model-first entity objects
   * Use of C# 6 interpolated strings
   * Calls to methods known to not be supported in LINQ to Entities
      * Method calls to any non-canonical function (see: https://msdn.microsoft.com/en-us/library/vstudio/Bb738626(v=vs.110).aspx)
         * Currently only a method name check (see TODO)
      * Method calls to any method not marked with a DbFunction attribute
   
Analyzer Assumptions
--------------------

 * Your code is in C#. VB.net is not supported by this analyzer
 * The code under analysis is targeting the current version of Entity Framework (6.1.x)
 * You are using a Code First entity model
 * The LINQ queries chain directly off of the DbSet property in the DbContext (for any calls against IQueryable<T> that cannot be determined to originate from a DbSet<T> property of a DbContext any errors are downgraded to warnings)
 
See the (modified) Contoso University sample application code for the types of assumptions that the analyzer is operating under.

In particular, check out the "Minefield" controller for examples of syntatically valid EF code that will throw NotSupportedExceptions at runtime when invoked.

TODO
----

Lots of things :) But a rough list:

 * Ensure that these gotchas are not falsely reported in valid contexts (eg. LINQ to Objects)
 * Support analysis of IQueryable<T> call chains where we know that the root IQueryable<T> is a DbSet<T>
 * Refine the LINQ method whitelist to check for actual supported overloads instead of just by method name