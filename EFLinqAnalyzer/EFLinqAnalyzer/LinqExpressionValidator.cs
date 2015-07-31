using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFLinqAnalyzer
{
    public class LinqExpressionValidator
    {
        /// <summary>
        /// Validates the give lambda to see if it is a valid LINQ to Entities expression
        /// </summary>
        /// <param name="lambda">The lambda syntax node</param>
        /// <param name="rootQueryableType">The type of the IQueryable instance where a known LINQ operator is invoked on with this lambda</param>
        /// <param name="context">The analysis context</param>
        /// <param name="efContext">The EF-specific view of the semantic model</param>
        /// <param name="treatAsWarning">If true, instructs any diagnostic reports to be flagged as warnings instead of errors. This is normally true when the analyzer cannot fully determine that the LINQ expression is made against an actual DbSet</param>
        public static void ValidateLinqToEntitiesExpression(LambdaExpressionSyntax lambda, EFCodeFirstClassInfo rootQueryableType, SyntaxNodeAnalysisContext context, EFUsageContext efContext, bool treatAsWarning = false)
        {
            var descendants = lambda.DescendantNodes();

            var accessNodes = descendants.OfType<MemberAccessExpressionSyntax>();
            var methodCallNodes = descendants.OfType<InvocationExpressionSyntax>();
            var parameterNodes = descendants.OfType<ParameterSyntax>()
                                            .ToDictionary(p => p.Identifier.ValueText, p => new ContextualLinqParameter(p));
            var stringNodes = descendants.OfType<InterpolatedStringExpressionSyntax>();

            //Easy one, all interpolated strings are invalid, it's just a case of whether to raise an
            //error or warning
            //
            //TODO: Code fix candidate. Offer to replace the interpolated string with a raw concatenated
            //equivalent
            foreach (var node in stringNodes)
            {
                var diagnostic = Diagnostic.Create(treatAsWarning ? DiagnosticCodes.EFLINQ012 : DiagnosticCodes.EFLINQ011, node.GetLocation());
                context.ReportDiagnostic(diagnostic);
            }

            //Check for property accesses on read-only properties, expression-bodied members
            //TODO: Also check for properties tagged with [NotMapped]
            foreach (var node in accessNodes)
            {
                ValidateMemberAccessInLinqExpression(node, rootQueryableType, context, parameterNodes, treatAsWarning);
            }

            foreach (var node in methodCallNodes)
            {
                ValidateMethodCallInLinqExpression(node, rootQueryableType, context, efContext, treatAsWarning);
            }
        }

        internal static void ValidateMethodCallInLinqExpression(InvocationExpressionSyntax node, EFCodeFirstClassInfo rootQueryableType, SyntaxNodeAnalysisContext context, EFUsageContext efContext, bool treatAsWarning)
        {
            string methodName = null;
            var memberExpr = node.Expression as MemberAccessExpressionSyntax;
            var identExpr = node.Expression as IdentifierNameSyntax;
            if (memberExpr != null)
            {
                methodName = memberExpr?.Name?.Identifier.ValueText;

                //This is a LINQ operator (Where, Select, etc)
                if (CanonicalMethodNames.IsLinqOperator(methodName))
                {
                    var expr = memberExpr.Expression as MemberAccessExpressionSyntax;
                    if (expr != null)
                    {
                        var member = expr.Name as IdentifierNameSyntax;
                        if (member != null)
                        {
                            string memberName = member.Identifier.ValueText;
                            var applicableClasses = efContext.GetClassForProperty(memberName)
                                                             .Where(c => c.HasProperty(memberName));

                            if (applicableClasses.Count() > 1)
                            {
                                //TODO: Code fix candidate
                                //
                                //In such a case, inject an .AsQueryable() before the LINQ operator call
                                //and add using System.Linq if required
                                var diagnostic = Diagnostic.Create(DiagnosticCodes.EFLINQ010, member.GetLocation(), memberName);
                                context.ReportDiagnostic(diagnostic);
                            }
                            else
                            {
                                var cls = applicableClasses.FirstOrDefault();
                                //There is only one class with this property and it is confirmed to be a collection
                                //navigation property
                                if (cls != null && cls.IsCollectionNavigationProperty(memberName))
                                {
                                    //TODO: Code fix candidate
                                    //
                                    //In such a case, inject an .AsQueryable() before the LINQ operator call
                                    //and add using System.Linq if required
                                    var diagnostic = Diagnostic.Create(treatAsWarning ? DiagnosticCodes.EFLINQ009 : DiagnosticCodes.EFLINQ008, member.GetLocation(), memberName, cls.Name);
                                    context.ReportDiagnostic(diagnostic);
                                }
                            }
                        }
                        //TODO: If not, check that the preceding member is IQueryable<T> and that T is a known
                        //entity type
                    }
                }
                else
                {
                    //TODO: AsQueryable() shouldn't be a blanket exception.
                    //We obviously should check what precedes it
                    if (methodName != EFSpecialIdentifiers.AsQueryable)
                    {
                        bool bValid = IsSupportedLinqToEntitiesMethod(node, memberExpr, rootQueryableType, efContext, context);
                        if (!bValid)
                        {
                            var diagnostic = Diagnostic.Create(treatAsWarning ? DiagnosticCodes.EFLINQ007 : DiagnosticCodes.EFLINQ004, node.GetLocation(), methodName);
                            context.ReportDiagnostic(diagnostic);
                        }
                    }
                }
            }
            else if (identExpr != null) //A non-instance (static) method call, most certainly illegal
            {
                if (!CanonicalMethodNames.IsKnownMethod(identExpr))
                {
                    methodName = identExpr.Identifier.ValueText;
                    var diagnostic = Diagnostic.Create(treatAsWarning ? DiagnosticCodes.EFLINQ006 : DiagnosticCodes.EFLINQ003, node.GetLocation(), methodName);
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }

        internal static void ValidateMemberAccessInLinqExpression(MemberAccessExpressionSyntax node, EFCodeFirstClassInfo rootQueryableType, SyntaxNodeAnalysisContext context, Dictionary<string, ContextualLinqParameter> parameterNodes, bool treatAsWarning)
        {
            bool bValid = true;

            var identifier = (node.Expression as IdentifierNameSyntax);
            var memberName = node?.Name;

            if (identifier != null && memberName != null)
            {
                var identText = identifier?.Identifier.ValueText ?? string.Empty;
                if (!string.IsNullOrEmpty(identText) && parameterNodes.ContainsKey(identText))
                {
                    bValid = !(rootQueryableType.IsReadOnly(memberName.Identifier.ValueText));
                }
            }

            if (!bValid)
            {
                var diagnostic = Diagnostic.Create(treatAsWarning ? DiagnosticCodes.EFLINQ005 : DiagnosticCodes.EFLINQ002, node.GetLocation(), memberName.Identifier.ValueText, rootQueryableType.Name);
                context.ReportDiagnostic(diagnostic);
            }
        }

        private static bool IsSupportedLinqToEntitiesMethod(InvocationExpressionSyntax node, MemberAccessExpressionSyntax memberExpr, EFCodeFirstClassInfo rootQueryableType, EFUsageContext efContext, SyntaxNodeAnalysisContext context)
        {
            return CanonicalMethodNames.IsKnownMethod(node, memberExpr, rootQueryableType, efContext, context);
        }
    }
}
