using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFLinqAnalyzer
{
    public static class LinqExpressionValidator
    {
        /// <summary>
        /// Validates the given lambda to see if it is a valid LINQ to Entities expression
        /// </summary>
        /// <param name="lambda">The lambda syntax node</param>
        /// <param name="rootQueryableType">The type of the IQueryable instance where a known LINQ operator is invoked on with this lambda</param>
        /// <param name="context">The analysis context</param>
        /// <param name="efContext">The EF-specific view of the semantic model</param>
        /// <param name="treatAsWarning">If true, instructs any diagnostic reports to be flagged as warnings instead of errors. This is normally true when the analyzer cannot fully determine that the LINQ expression is made against an actual DbSet</param>
        public static void ValidateLinqToEntitiesExpression(LambdaExpressionSyntax lambda, EFCodeFirstClassInfo rootQueryableType, SyntaxNodeAnalysisContext context, EFUsageContext efContext, bool treatAsWarning = false)
        {
            var descendants = lambda.DescendantNodes();
            var parameterNodes = ContextualLinqParameter.BuildContext(descendants.OfType<ParameterSyntax>(), context, efContext);
            ValidateLinqToEntitiesUsageInSyntaxNodes(descendants, rootQueryableType, context, efContext, parameterNodes, treatAsWarning);
        }

        /// <summary>
        /// Validates the series of syntax nodes for valid usages of LINQ to Entities constructs
        /// </summary>
        /// <param name="descendants"></param>
        /// <param name="rootQueryableType"></param>
        /// <param name="context"></param>
        /// <param name="efContext"></param>
        /// <param name="treatAsWarning"></param>
        public static void ValidateLinqToEntitiesUsageInSyntaxNodes(IEnumerable<SyntaxNode> descendants, EFCodeFirstClassInfo rootQueryableType, SyntaxNodeAnalysisContext context, EFUsageContext efContext, Dictionary<string, ContextualLinqParameter> parameterNodes, bool treatAsWarning)
        {
            var accessNodes = descendants.OfType<MemberAccessExpressionSyntax>();
            var methodCallNodes = descendants.OfType<InvocationExpressionSyntax>();
            
            var stringNodes = descendants.OfType<InterpolatedStringExpressionSyntax>();
            var objCreationNodes = descendants.OfType<ObjectCreationExpressionSyntax>();

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

            //Another easy one, any object creation expressions inside a known LINQ expression cannot involve an entity type
            foreach (var node in objCreationNodes)
            {
                var objType = node.Type;
                var si = context.SemanticModel.GetSymbolInfo(objType);
                var ts = si.Symbol?.TryGetType();
                if (ts != null)
                {
                    var cls = efContext.GetClassInfo(ts);
                    if (cls != null)
                    {
                        var diagnostic = Diagnostic.Create(treatAsWarning ? DiagnosticCodes.EFLINQ016 : DiagnosticCodes.EFLINQ015, node.Type.GetLocation(), cls.Name);
                        context.ReportDiagnostic(diagnostic);
                    }
                }
            }

            //Check for property accesses on read-only properties, expression-bodied members
            foreach (var node in accessNodes)
            {
                ValidateMemberAccessInLinqExpression(node, rootQueryableType, context, parameterNodes, treatAsWarning);
            }

            foreach (var node in methodCallNodes)
            {
                ValidateMethodCallInLinqExpression(node, rootQueryableType, context, efContext, parameterNodes, treatAsWarning);
            }
        }

        internal static void ValidateMethodCallInLinqExpression(InvocationExpressionSyntax node, EFCodeFirstClassInfo rootQueryableType, SyntaxNodeAnalysisContext context, EFUsageContext efContext, Dictionary<string, ContextualLinqParameter> parameterNodes, bool treatAsWarning)
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
                    if (expr != null) //a.b.<linq operator>()
                    {
                        //What is b?
                        var member = expr.Name as IdentifierNameSyntax;
                        if (member != null)
                        {
                            string memberName = member.Identifier.ValueText;
                            var applicableClasses = efContext.GetClassForProperty(memberName)
                                                             .Where(c => c.HasProperty(memberName));
                            
                            if (applicableClasses.Count() > 1)
                            {
                                //See if semantic model can help us disambiguate
                                var si = context.SemanticModel.GetSymbolInfo(expr.Expression);
                                var type = si.Symbol?.TryGetType();
                                if (type != null)
                                {
                                    var cls = efContext.GetClassInfo(type);
                                    //There is only one class with this property and it is confirmed to be a collection
                                    //navigation property
                                    if (cls != null && cls.IsCollectionNavigationProperty(memberName))
                                    {
                                        ValidateNavigationPropertyAccess(node, context, efContext, treatAsWarning, memberName, cls);
                                    }
                                }
                                else
                                {
                                    //This potential navigation property resolves to multiple classes, see if we can resolve to a
                                    //single one via contextual variables
                                    var inst = expr.Expression as IdentifierNameSyntax;
                                    if (inst != null)
                                    {
                                        string name = inst.Identifier.ValueText;
                                        ContextualLinqParameter cparam;
                                        if (parameterNodes.TryGetValue(name, out cparam) && 
                                            cparam.ParameterType == ContextualLinqParameterType.Queryable &&
                                            applicableClasses.Any(c => c.ClassType == cparam.QueryableType.ClassType))
                                        {
                                            //TODO: Code fix candidate
                                            //
                                            //In such a case, inject an .AsQueryable() before the LINQ operator call
                                            //and add using System.Linq if required
                                            var diagnostic = Diagnostic.Create(treatAsWarning ? DiagnosticCodes.EFLINQ009 : DiagnosticCodes.EFLINQ008, member.GetLocation(), memberName, cparam.QueryableType.Name);
                                            context.ReportDiagnostic(diagnostic);
                                        }
                                        else
                                        {
                                            //TODO: Code fix candidate
                                            //
                                            //In such a case, inject an .AsQueryable() before the LINQ operator call
                                            //and add using System.Linq if required
                                            var diagnostic = Diagnostic.Create(DiagnosticCodes.EFLINQ010, member.GetLocation(), memberName);
                                            context.ReportDiagnostic(diagnostic);
                                        }
                                    }
                                    else
                                    {
                                        //TODO: Code fix candidate
                                        //
                                        //In such a case, inject an .AsQueryable() before the LINQ operator call
                                        //and add using System.Linq if required
                                        var diagnostic = Diagnostic.Create(DiagnosticCodes.EFLINQ010, member.GetLocation(), memberName);
                                        context.ReportDiagnostic(diagnostic);
                                    }
                                }
                            }
                            else
                            {
                                var cls = applicableClasses.FirstOrDefault();
                                //There is only one class with this property and it is confirmed to be a collection
                                //navigation property
                                if (cls != null && cls.IsCollectionNavigationProperty(memberName))
                                {
                                    ValidateNavigationPropertyAccess(node, context, efContext, treatAsWarning, memberName, cls);
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

        public static bool MemberAccessIsAccessingDbContext(MemberAccessExpressionSyntax memberExpr, SyntaxNodeAnalysisContext context, EFUsageContext efContext, out EFCodeFirstClassInfo clsInfo)
        {
            clsInfo = null;
            var compIdent = memberExpr.Expression as IdentifierNameSyntax;
            var prop = memberExpr.Name;
            if (compIdent != null && prop != null)
            {
                var si = context.SemanticModel.GetSymbolInfo(compIdent);
                var type = si.Symbol?.TryGetType();
                if (type != null)
                {
                    //$ident is a DbContext
                    if (efContext.DbContexts.Contains(type))
                    {
                        //We're expecting $prop to be a symbol
                        si = context.SemanticModel.GetSymbolInfo(prop);
                        var ps = si.Symbol as IPropertySymbol;
                        if (ps != null && ps.Type.MetadataName == EFSpecialIdentifiers.DbSet)
                        {
                            var nts = ps.Type as INamedTypeSymbol;
                            if (nts != null)
                            {
                                var typeArg = nts.TypeArguments[0];
                                clsInfo = efContext.GetClassInfo(typeArg);
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }

        private static void ValidateNavigationPropertyAccess(InvocationExpressionSyntax node, SyntaxNodeAnalysisContext context, EFUsageContext efContext, bool treatAsWarning, string memberName, EFCodeFirstClassInfo cls)
        {
            if (node.ArgumentList.Arguments.Count == 1 &&
                node.ArgumentList.Arguments[0].Expression.Kind() != SyntaxKind.SimpleLambdaExpression)
            {
                if (node.ArgumentList.Arguments[0].Expression.Kind() == SyntaxKind.IdentifierName)
                {
                    //Follow the identifier back to its assignment
                    var si = context.SemanticModel.GetSymbolInfo(node.ArgumentList.Arguments[0].Expression);
                    if (si.Symbol?.Kind == SymbolKind.Local)
                    {
                        var type = si.Symbol?.TryGetType() as INamedTypeSymbol;

                        //The variable inside our LINQ sub-operator is a Func<T, bool> where T
                        //is a known entity type
                        if (type != null &&
                            type.MetadataName == $"{EFSpecialIdentifiers.Func}`2" &&
                            efContext.GetClassInfo(type.TypeArguments[0]) != null &&
                            type.TypeArguments[1].MetadataName == EFSpecialIdentifiers.Boolean)
                        {
                            //TODO: Code fix candidate
                            //
                            //In such a case, inject an .AsQueryable() before the LINQ operator call
                            //and add using System.Linq if required and convert the variable from Func<T, bool> to Expression<Func<T, bool>>
                            var diagnostic = Diagnostic.Create(treatAsWarning ? DiagnosticCodes.EFLINQ009 : DiagnosticCodes.EFLINQ008, node.ArgumentList.Arguments[0].Expression.GetLocation(), memberName, cls.Name);
                            context.ReportDiagnostic(diagnostic);
                        }
                    }
                }
            }
        }

        internal static void ValidateMemberAccessInLinqExpression(MemberAccessExpressionSyntax node, EFCodeFirstClassInfo rootQueryableType, SyntaxNodeAnalysisContext context, Dictionary<string, ContextualLinqParameter> parameterNodes, bool treatAsWarning)
        {
            var identifier = (node.Expression as IdentifierNameSyntax);
            var memberName = node?.Name;

            if (identifier != null && memberName != null)
            {
                var identText = identifier?.Identifier.ValueText ?? string.Empty;
                if (!string.IsNullOrEmpty(identText) && parameterNodes.ContainsKey(identText))
                {
                    //TODO: Properties of explicit interface implementations. Does EF recognize these or not?
                    string propName = memberName.Identifier.ValueText;
                    if (rootQueryableType.IsReadOnly(propName))
                    {
                        var diagnostic = Diagnostic.Create(treatAsWarning ? DiagnosticCodes.EFLINQ005 : DiagnosticCodes.EFLINQ002, node.GetLocation(), memberName.Identifier.ValueText, rootQueryableType.Name);
                        context.ReportDiagnostic(diagnostic);
                    }
                    if (rootQueryableType.IsExplicitlyUnmapped(propName))
                    {
                        var diagnostic = Diagnostic.Create(treatAsWarning ? DiagnosticCodes.EFLINQ014 : DiagnosticCodes.EFLINQ013, node.GetLocation(), memberName.Identifier.ValueText, rootQueryableType.Name);
                        context.ReportDiagnostic(diagnostic);
                    }
                }
            }
        }

        internal static bool SymbolCanBeTracedBackToDbContext(ISymbol sym, SyntaxNodeAnalysisContext context, EFUsageContext efContext, EFCodeFirstClassInfo clsInfo)
        {
            bool bTraced = false;

            var assignments = sym.DeclaringSyntaxReferences
                                 .Where(decl => decl.GetSyntax()?.Kind() == SyntaxKind.EqualsValueClause)
                                 .Cast<EqualsValueClauseSyntax>()
                                 .Concat(sym.DeclaringSyntaxReferences.SelectMany(decl => decl.GetSyntax()?.DescendantNodes().OfType<EqualsValueClauseSyntax>()));
            
            //Find applicable assignments where:
            //var myVar = $SOME_EXPR;
            var applicableAssignments = assignments.Select(asn => asn.Parent)
                                                   .OfType<VariableDeclaratorSyntax>()
                                                   .Where(decl => decl.Identifier.ValueText == sym.Name);

            if (applicableAssignments.Any())
            {
                //Check what the RHS is
                foreach (var asn in applicableAssignments)
                {
                    var eq = asn.DescendantNodes()
                                .OfType<EqualsValueClauseSyntax>()
                                .FirstOrDefault();
                    if (eq != null)
                    {
                        switch (eq.Value.Kind())
                        {
                            //var myVar = $SOME_METHOD()
                            case SyntaxKind.InvocationExpression:
                                {
                                    var invoc = (InvocationExpressionSyntax)eq.Value;
                                    var method = invoc.Expression as IdentifierNameSyntax;
                                    if (method != null)
                                    {
                                        return DoesMethodReturnDbSet(method, context, efContext, clsInfo);
                                    }
                                }
                                break;
                            case SyntaxKind.SimpleMemberAccessExpression:
                                {
                                    var member = (MemberAccessExpressionSyntax)eq.Value;
                                    EFCodeFirstClassInfo cls;
                                    bool isDbContext = MemberAccessIsAccessingDbContext(member, context, efContext, out cls);
                                    return isDbContext && (cls == clsInfo);
                                }
                                break;
                        }
                    }
                }
            }

            return bTraced;
        }

        private static bool ReturnStatementTracesBackToDbSet(ReturnStatementSyntax ret, SyntaxNodeAnalysisContext context, EFCodeFirstClassInfo clsInfo)
        {
            var expr = ret.Expression;
            var kind = expr.Kind();
            //TODO: There are many more cases to handle, so just handle them as the cases arise
            switch (kind)
            {
                case SyntaxKind.SimpleMemberAccessExpression: //return $a.$b
                    {
                        var sma = (MemberAccessExpressionSyntax)expr;
                        //Is $a some DbContext?
                        if (sma.Expression.IsDbContextInstance(context) && 
                            sma.Name.Kind() == SyntaxKind.IdentifierName)
                        {
                            var si = context.SemanticModel.GetSymbolInfo(sma.Name);
                            if (si.Symbol != null)
                            {
                                var type = si.Symbol?.TryGetType() as INamedTypeSymbol;
                                //Should be DbSet<T>, but let's just verify
                                if (type?.MetadataName == EFSpecialIdentifiers.DbSet)
                                {
                                    //T is our type
                                    if (clsInfo?.ClassType == type.TypeArguments[0])
                                        return true;
                                }
                            }
                        }
                    }
                    break;
            }
            return false;
        }

        private static bool DoesMethodReturnDbSet(IdentifierNameSyntax methodIdent, SyntaxNodeAnalysisContext context, EFUsageContext efContext, EFCodeFirstClassInfo clsInfo)
        {
            var method = methodIdent.GetDeclaringMethod(context);
            var returnStatements = method.DescendantNodes().OfType<ReturnStatementSyntax>();
            //It has to be all so that we can be conclusive that all points of return a DbSet<T>
            bool returnsDbSet = returnStatements.All(ret => ReturnStatementTracesBackToDbSet(ret, context, clsInfo));
            return returnsDbSet;
        }

        private static bool IsSupportedLinqToEntitiesMethod(InvocationExpressionSyntax node, MemberAccessExpressionSyntax memberExpr, EFCodeFirstClassInfo rootQueryableType, EFUsageContext efContext, SyntaxNodeAnalysisContext context) => CanonicalMethodNames.IsKnownMethod(node, memberExpr, rootQueryableType, efContext, context);
    }
}
