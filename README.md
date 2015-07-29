# roslyn-ef-linq-analyzer

A Roslyn Analyzer to check for common gotchas with LINQ expressions when used with Entity Framework

Build Requirements
------------------

 * Visual Studio 2015 (Community or higher edition)
 * Roslyn compiler SDK

Analyzer Checks
---------------

 * A Where() or Select() call does not include:
   * Read-only properties of EF model first entity classes
   * Calls to methods known to not be supported in LINQ to Entities
   
Analyzer Assumptions
--------------------

 * The code under analysis is targeting the current version of Entity Framework (6.1.x)
 * You are using a Code First entity model
 * The LINQ queries use extension method syntax (haven't tried or tested with query syntax)
 * The LINQ queries chain directly off of the DbSet property in the DbContext (for any calls against IQueryable<T> that cannot be determined to originate from a DbSet<T> property of a DbContext any errors are downgraded to warnings)
 
See the Test application code for the types of assumptions that the analyzer is operating under

TODO
----

Lots of things :) But a rough list:

 * Check for valid usages of collection navigation properties (#2)
 * Ensure that these gotchas are not falsely reported in valid contexts (eg. LINQ to Objects)
 * Support analysis of IQueryable<T> call chains where we know that the root IQueryable<T> is a DbSet<T>
 * Refine the LINQ method whitelist to check for actual supported overloads instead of just by method name