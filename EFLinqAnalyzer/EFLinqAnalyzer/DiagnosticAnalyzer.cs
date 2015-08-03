using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace EFLinqAnalyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class EFLinqAnalyzerAnalyzer : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => DiagnosticCodes.SupportedDiagnostics;

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeEFCodeFirstModelClassReadOnlyProperty, SyntaxKind.PropertyDeclaration);
            context.RegisterSyntaxNodeAction(AnalyzeLinqExpression, SyntaxKind.SimpleLambdaExpression, SyntaxKind.ParenthesizedLambdaExpression, SyntaxKind.QueryExpression);
        }
        
        private static void AnalyzeEFCodeFirstModelClassReadOnlyProperty(SyntaxNodeAnalysisContext context)
        {
            var propNode = context.Node as PropertyDeclarationSyntax;
            if (propNode != null)
            {
                //Property only has a getter
                if (propNode.AccessorList != null)
                {
                    if (propNode.AccessorList.Accessors.Count == 1 && string.Equals(propNode.AccessorList.Accessors[0].Keyword.ValueText, "get", StringComparison.OrdinalIgnoreCase))
                    {
                        //Check whether its parent class is an EF code first model
                        var declCls = propNode.Parent as ClassDeclarationSyntax;
                        if (declCls != null && IsClassPartOfEFCodeFirstModel(declCls, context))
                        {
                            // For all such symbols, produce a diagnostic.
                            var diagnostic = Diagnostic.Create(DiagnosticCodes.EFLINQ001, propNode.GetLocation(), propNode.Identifier.ValueText, declCls.Identifier.ValueText);
                            context.ReportDiagnostic(diagnostic);
                        }
                    }
                }
                else
                {
                    //Or if we're using C# 6 expression-bodied members, does it have one?
                    if (propNode.ExpressionBody != null)
                    {
                        //Check whether its parent class is an EF code first model
                        var declCls = propNode.Parent as ClassDeclarationSyntax;
                        if (declCls != null && IsClassPartOfEFCodeFirstModel(declCls, context))
                        {
                            // For all such symbols, produce a diagnostic.
                            var diagnostic = Diagnostic.Create(DiagnosticCodes.EFLINQ001, propNode.GetLocation(), propNode.Identifier.ValueText, declCls.Identifier.ValueText);
                            context.ReportDiagnostic(diagnostic);
                        }
                    }
                }
            }
        }
        
        private static bool IsClassPartOfEFCodeFirstModel(ClassDeclarationSyntax classType, SyntaxNodeAnalysisContext context)
        {
            //Find classes that derive from DbContext
            var symbols = context.SemanticModel
                                 .LookupSymbols(classType.Identifier.Span.Start + 1)
                                 .OfType<ITypeSymbol>()
                                 .Where(t => EFUsageContext.TypeUltimatelyDerivesFromDbContext(t));
            
            foreach (var clsSym in symbols)
            {
                //Get all DbSet properties
                var dbSetProperties = clsSym.GetMembers()
                                            .OfType<IPropertySymbol>()
                                            .Where(p => p?.Type?.MetadataName == EFSpecialIdentifiers.DbSet);

                foreach (var propSym in dbSetProperties)
                {
                    var nts = propSym.Type as INamedTypeSymbol;
                    if (nts != null)
                    {
                        //Is DbSet<T> where T is the type of our class
                        if (nts.TypeArguments.Any(ta => ta.Name == classType.Identifier.ValueText))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        
        private static void AnalyzeLinqExpression(SyntaxNodeAnalysisContext context)
        {
            var efContext = new EFUsageContext(context);
            
            //Found nothing, don't bother continuing
            if (!efContext.Build())
                return;

            //Check if the lambda is part of an IQueryable call chain. If so, check that it only contains valid
            //EF LINQ constructs (initializers, entity members, entity navigation properties)
            var query = context.Node as QueryExpressionSyntax;
            var lambda = context.Node as LambdaExpressionSyntax;
            if (query != null)
            {
                AnalyzeQueryExpression(context, efContext, query);
            }
            else if (lambda != null)
            {
                AnalyzeLambdaInLinqExpression(context, efContext, lambda);
            }
        }

        private static void AnalyzeLambdaInLinqExpression(SyntaxNodeAnalysisContext context, EFUsageContext efContext, LambdaExpressionSyntax lambda)
        {
            var lambdaAssign = lambda.Parent as EqualsValueClauseSyntax;
            var arg = lambda.Parent as ArgumentSyntax;
            if (arg != null) //The lambda in question is being passed as an argument
            {
                var parent = arg.Parent;
                while (parent != null && !(parent is ArgumentListSyntax))
                {
                    parent = parent.Parent;
                }

                if (parent != null) //Which should be part of an ArgumentList
                {
                    var argList = parent;
                    var invoc = argList?.Parent as InvocationExpressionSyntax;
                    if (invoc != null) //Which should be part of an invocation
                    {
                        var memberExpr = invoc.Expression as MemberAccessExpressionSyntax;
                        if (memberExpr != null)
                        {
                            switch (memberExpr?.Name?.Identifier.ValueText)
                            {
                                case CanonicalMethodNames.LinqOperators.Select:
                                case CanonicalMethodNames.LinqOperators.Where:
                                    {
                                        var si = context.SemanticModel.GetSymbolInfo(memberExpr.Expression);

                                        var lts = si.Symbol as ILocalSymbol;
                                        var pts = si.Symbol as IPropertySymbol;
                                        //Is this method called on a property?
                                        if (pts != null)
                                        {
                                            var nts = pts.Type as INamedTypeSymbol;
                                            if (nts != null)
                                            {
                                                //Like a DbSet<T>?
                                                if (nts.MetadataName == EFSpecialIdentifiers.DbSet)
                                                {
                                                    //That is part of a class derived from DbContext?
                                                    if (pts?.ContainingType?.BaseType?.Name == EFSpecialIdentifiers.DbContext)
                                                    {
                                                        var typeArg = nts.TypeArguments[0];
                                                        //Let's give our method some assistance, by checking what T actually is
                                                        var clsInfo = efContext.GetClassInfo(typeArg);
                                                        if (clsInfo != null)
                                                        {
                                                            //Okay now let's see if this lambda is valid in the EF context
                                                            LinqExpressionValidator.ValidateLinqToEntitiesExpression(lambda, clsInfo, context, efContext);
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        else if (lts != null) //The linq method was called on a local variable
                                        {
                                            var nts = lts.Type as INamedTypeSymbol;
                                            if (nts != null && nts.TypeArguments.Length == 1)
                                            {
                                                //This is some generic type with one type argument
                                                var typeArg = nts.TypeArguments[0];
                                                if (nts.MetadataName == EFSpecialIdentifiers.DbSet)
                                                {
                                                    //TODO: Should still actually check that it is ultimately assigned
                                                    //from a DbSet<T> property of a DbContext derived class

                                                    var clsInfo = efContext.GetClassInfo(typeArg);
                                                    if (clsInfo != null)
                                                    {
                                                        LinqExpressionValidator.ValidateLinqToEntitiesExpression(lambda, clsInfo, context, efContext);
                                                    }
                                                }
                                                else if (nts.MetadataName == EFSpecialIdentifiers.IQueryable)
                                                {
                                                    var clsInfo = efContext.GetClassInfo(typeArg);
                                                    if (clsInfo != null)
                                                    {
                                                        LinqExpressionValidator.ValidateLinqToEntitiesExpression(lambda, clsInfo, context, efContext, treatAsWarning: true);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    break;
                            }
                        }
                    }
                }
            }
            else if (lambdaAssign != null) //The lambda in question is being assigned
            {
                var localLambdaDecl = lambdaAssign?.Parent?.Parent?.Parent as LocalDeclarationStatementSyntax;
                if (localLambdaDecl != null)
                {
                    var declType = localLambdaDecl?.Declaration?.Type as GenericNameSyntax;
                    if (declType != null)
                    {
                        //Is Expression<T>
                        if (declType.Identifier.ValueText == EFSpecialIdentifiers.Expression && declType.TypeArgumentList.Arguments.Count == 1)
                        {
                            //The T is Func<TInput, TOutput>
                            var exprTypeArg = declType.TypeArgumentList.Arguments[0] as GenericNameSyntax;
                            if (exprTypeArg != null &&
                                exprTypeArg.Identifier.ValueText == EFSpecialIdentifiers.Func &&
                                exprTypeArg.TypeArgumentList.Arguments.Count == 2)
                            {
                                var inputType = exprTypeArg.TypeArgumentList.Arguments[0] as IdentifierNameSyntax;
                                var outputType = exprTypeArg.TypeArgumentList.Arguments[1] as PredefinedTypeSyntax;
                                //The TOutput in Func<TInput, TOutput> is bool
                                if (inputType != null && outputType != null && outputType.Keyword.ValueText == EFSpecialIdentifiers.BooleanShort)
                                {
                                    var si = context.SemanticModel.GetSymbolInfo(inputType);
                                    var ts = efContext.EntityTypes.FirstOrDefault(t => t == si.Symbol);
                                    if (ts != null)
                                    {
                                        var clsInfo = efContext.GetClassInfo(ts);
                                        if (clsInfo != null)
                                        {
                                            LinqExpressionValidator.ValidateLinqToEntitiesExpression(lambda, clsInfo, context, efContext);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private static void AnalyzeQueryExpression(SyntaxNodeAnalysisContext context, EFUsageContext efContext, QueryExpressionSyntax query)
        {
            //I can't believe how much easier this is compared to Extension Method syntax! Then again
            //query syntax does mean its own dedicated set of C# keywords, which would means its dedicate
            //set of syntax node types

            //First item on checklist, find out our root queryable
            var memberExpr = query.FromClause.Expression as MemberAccessExpressionSyntax;
            if (memberExpr != null) // from $var in $ident.$prop
            {
                var ident = memberExpr.Expression as IdentifierNameSyntax;
                var prop = memberExpr.Name;
                if (ident != null && prop != null)
                {
                    var si = context.SemanticModel.GetSymbolInfo(ident);
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
                                    var cls = efContext.GetClassInfo(typeArg);
                                    if (cls != null)
                                    {
                                        bool treatAsWarning = false;

                                        var paramNodes = new Dictionary<string, ContextualLinqParameter>();
                                        var fromVar = new ContextualLinqParameter(query.FromClause.Identifier);
                                        paramNodes[fromVar.Name] = fromVar;
                                        var descendants = query.Body.DescendantNodes();
                                        var memberAccesses = descendants.OfType<MemberAccessExpressionSyntax>();
                                        foreach (var access in memberAccesses)
                                        {
                                            LinqExpressionValidator.ValidateMemberAccessInLinqExpression(access, cls, context, paramNodes, treatAsWarning);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
