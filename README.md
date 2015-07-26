# roslyn-ef-linq-analyzer

A Roslyn Analyzer to check for common gotchas with LINQ expressions when used with Entity Framework

Requirements
------------

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
 * The LINQ queries use extension method syntax (haven't tried with query syntax)
 * The LINQ queries chain directly off of the DbSet property in the DbContext (it will most likely not pick up calls off of IQueryable<T> locals assigned from such a DbSet property)
 * The LINQ expressions are inline (it will most likely not pick up Expression<Func<T, bool>> locals passed in)

See the Test application code for the types of assumptions that the analyzer is operating under

TODO
----

Lots of things :) But a rough list:

 * Check for valid usages of collection navigation properties (#2)
 * Ensure that these gotchas are not falsely reported in valid contexts (eg. LINQ to Objects)
 * Support analysis of passed in Expression<Func<T, bool>> instances to the query call chain
 * Support analysis of IQueryable<T> call chains where we know that the root IQueryable<T> is a DbSet<T>