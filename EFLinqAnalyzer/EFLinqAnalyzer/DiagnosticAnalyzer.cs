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
        private static DiagnosticDescriptor Info_CodeFirstClassReadOnlyRule = new DiagnosticDescriptor(
            id: "EFLINQ001",
            title: new LocalizableResourceString(nameof(Resources.EFLINQ001_TITLE), Resources.ResourceManager, typeof(Resources)),
            messageFormat: new LocalizableResourceString(nameof(Resources.EFLINQ001_MSGFORMAT), Resources.ResourceManager, typeof(Resources)),
            category: "Entity Framework Gotchas",
            defaultSeverity: DiagnosticSeverity.Info,
            isEnabledByDefault: true,
            description: new LocalizableResourceString(nameof(Resources.EFLINQ001_DESC), Resources.ResourceManager, typeof(Resources)));
        
        private static DiagnosticDescriptor Error_CodeFirstClassReadOnlyPropertyUsageRule = new DiagnosticDescriptor(
            id: "EFLINQ002",
            title: new LocalizableResourceString(nameof(Resources.EFLINQ002_TITLE), Resources.ResourceManager, typeof(Resources)),
            messageFormat: new LocalizableResourceString(nameof(Resources.EFLINQ002_MSGFORMAT), Resources.ResourceManager, typeof(Resources)),
            category: "Entity Framework Gotchas",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: new LocalizableResourceString(nameof(Resources.EFLINQ002_DESC), Resources.ResourceManager, typeof(Resources)));

        private static DiagnosticDescriptor Error_CodeFirstUnsupportedStaticMethodInLinqExpressionRule = new DiagnosticDescriptor(
            id: "EFLINQ003",
            title: new LocalizableResourceString(nameof(Resources.EFLINQ003_TITLE), Resources.ResourceManager, typeof(Resources)),
            messageFormat: new LocalizableResourceString(nameof(Resources.EFLINQ003_MSGFORMAT), Resources.ResourceManager, typeof(Resources)),
            category: "Entity Framework Gotchas",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: new LocalizableResourceString(nameof(Resources.EFLINQ003_DESC), Resources.ResourceManager, typeof(Resources)));

        private static DiagnosticDescriptor Error_CodeFirstUnsupportedInstanceMethodInLinqExpressionRule = new DiagnosticDescriptor(
            id: "EFLINQ004",
            title: new LocalizableResourceString(nameof(Resources.EFLINQ004_TITLE), Resources.ResourceManager, typeof(Resources)),
            messageFormat: new LocalizableResourceString(nameof(Resources.EFLINQ004_MSGFORMAT), Resources.ResourceManager, typeof(Resources)),
            category: "Entity Framework Gotchas",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: new LocalizableResourceString(nameof(Resources.EFLINQ004_DESC), Resources.ResourceManager, typeof(Resources)));

        private static DiagnosticDescriptor Warning_CodeFirstClassReadOnlyPropertyUsageRule = new DiagnosticDescriptor(
            id: "EFLINQ005",
            title: new LocalizableResourceString(nameof(Resources.EFLINQ005_TITLE), Resources.ResourceManager, typeof(Resources)),
            messageFormat: new LocalizableResourceString(nameof(Resources.EFLINQ005_MSGFORMAT), Resources.ResourceManager, typeof(Resources)),
            category: "Entity Framework Gotchas",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: new LocalizableResourceString(nameof(Resources.EFLINQ005_DESC), Resources.ResourceManager, typeof(Resources)));

        private static DiagnosticDescriptor Warning_CodeFirstUnsupportedStaticMethodInLinqExpressionRule = new DiagnosticDescriptor(
            id: "EFLINQ006",
            title: new LocalizableResourceString(nameof(Resources.EFLINQ006_TITLE), Resources.ResourceManager, typeof(Resources)),
            messageFormat: new LocalizableResourceString(nameof(Resources.EFLINQ006_MSGFORMAT), Resources.ResourceManager, typeof(Resources)),
            category: "Entity Framework Gotchas",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: new LocalizableResourceString(nameof(Resources.EFLINQ006_DESC), Resources.ResourceManager, typeof(Resources)));

        private static DiagnosticDescriptor Warning_CodeFirstUnsupportedInstanceMethodInLinqExpressionRule = new DiagnosticDescriptor(
            id: "EFLINQ007",
            title: new LocalizableResourceString(nameof(Resources.EFLINQ007_TITLE), Resources.ResourceManager, typeof(Resources)),
            messageFormat: new LocalizableResourceString(nameof(Resources.EFLINQ007_MSGFORMAT), Resources.ResourceManager, typeof(Resources)),
            category: "Entity Framework Gotchas",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: new LocalizableResourceString(nameof(Resources.EFLINQ007_DESC), Resources.ResourceManager, typeof(Resources)));

        private static DiagnosticDescriptor Error_CodeFirstCollectionNavigationPropertyInLinqExpressionRule = new DiagnosticDescriptor(
            id: "EFLINQ008",
            title: new LocalizableResourceString(nameof(Resources.EFLINQ008_TITLE), Resources.ResourceManager, typeof(Resources)),
            messageFormat: new LocalizableResourceString(nameof(Resources.EFLINQ008_MSGFORMAT), Resources.ResourceManager, typeof(Resources)),
            category: "Entity Framework Gotchas",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: new LocalizableResourceString(nameof(Resources.EFLINQ008_DESC), Resources.ResourceManager, typeof(Resources)));

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(
                    Info_CodeFirstClassReadOnlyRule,
                    Error_CodeFirstClassReadOnlyPropertyUsageRule,
                    Error_CodeFirstUnsupportedStaticMethodInLinqExpressionRule,
                    Error_CodeFirstUnsupportedInstanceMethodInLinqExpressionRule,
                    Warning_CodeFirstClassReadOnlyPropertyUsageRule,
                    Warning_CodeFirstUnsupportedStaticMethodInLinqExpressionRule,
                    Warning_CodeFirstUnsupportedInstanceMethodInLinqExpressionRule,
                    Error_CodeFirstCollectionNavigationPropertyInLinqExpressionRule);
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeEFCodeFirstModelClassReadOnlyProperty, SyntaxKind.PropertyDeclaration);
            context.RegisterSyntaxNodeAction(AnalyzeLinqExpression, SyntaxKind.SimpleLambdaExpression, SyntaxKind.ParenthesizedLambdaExpression);
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
                            var diagnostic = Diagnostic.Create(Info_CodeFirstClassReadOnlyRule, propNode.GetLocation(), propNode.Identifier.ValueText, declCls.Identifier.ValueText);
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
                            var diagnostic = Diagnostic.Create(Info_CodeFirstClassReadOnlyRule, propNode.GetLocation(), propNode.Identifier.ValueText, declCls.Identifier.ValueText);
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
                                 .Where(t => t?.BaseType?.Name == "DbContext");
            
            foreach (var clsSym in symbols)
            {
                //Get all DbSet properties
                var dbSetProperties = clsSym.GetMembers()
                                            .OfType<IPropertySymbol>()
                                            .Where(p => p?.Type?.MetadataName == "DbSet`1");

                foreach (var propSym in dbSetProperties)
                {
                    INamedTypeSymbol nts = propSym.Type as INamedTypeSymbol;
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

        static void ValidateLinqToEntitiesExpression(LambdaExpressionSyntax lambda, EFCodeFirstClassInfo rootQueryableType, SyntaxNodeAnalysisContext context, bool treatAsWarning = false)
        {
            var accessNodes = lambda.DescendantNodes()
                                    .OfType<MemberAccessExpressionSyntax>();
            var methodCallNodes = lambda.DescendantNodes()
                                        .OfType<InvocationExpressionSyntax>();
            var parameterNodes = lambda.DescendantNodes()
                                       .OfType<ParameterSyntax>()
                                       .ToDictionary(p => p.Identifier.ValueText, p => p);

            foreach (var node in accessNodes)
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
                    var diagnostic = Diagnostic.Create(treatAsWarning ? Warning_CodeFirstClassReadOnlyPropertyUsageRule : Error_CodeFirstClassReadOnlyPropertyUsageRule, node.GetLocation(), memberName.Identifier.ValueText, rootQueryableType.Name);
                    context.ReportDiagnostic(diagnostic);
                }
            }

            foreach (var node in methodCallNodes)
            {
                string methodName = null;
                var memberExpr = node.Expression as MemberAccessExpressionSyntax;
                var identExpr = node.Expression as IdentifierNameSyntax;
                if (memberExpr != null)
                {
                    methodName = memberExpr?.Name?.Identifier.ValueText;
                    if (CanonicalMethodNames.IsLinqOperator(methodName))
                    {
                        //Check if the member being invoked on is a preceded by an AsQueryable() call

                        //If not, check that the preceding member is IQueryable<T> and that T is a known
                        //entity type
                        
                    }
                    else
                    {
                        bool bValid = IsSupportedLinqToEntitiesMethod(node, memberExpr, rootQueryableType);
                        if (!bValid)
                        {
                            var diagnostic = Diagnostic.Create(treatAsWarning ? Warning_CodeFirstUnsupportedInstanceMethodInLinqExpressionRule : Error_CodeFirstUnsupportedInstanceMethodInLinqExpressionRule, node.GetLocation(), methodName);
                            context.ReportDiagnostic(diagnostic);
                        }
                    }
                }
                else if (identExpr != null) //A non-instance (static) method call, most certainly illegal
                {
                    if (!CanonicalMethodNames.IsKnownMethod(identExpr))
                    {
                        methodName = identExpr.Identifier.ValueText;
                        var diagnostic = Diagnostic.Create(treatAsWarning ? Warning_CodeFirstUnsupportedStaticMethodInLinqExpressionRule : Error_CodeFirstUnsupportedStaticMethodInLinqExpressionRule, node.GetLocation(), methodName);
                        context.ReportDiagnostic(diagnostic);
                    }
                }
            }
        }

        private static bool IsSupportedLinqToEntitiesMethod(InvocationExpressionSyntax node, MemberAccessExpressionSyntax memberExpr, EFCodeFirstClassInfo rootQueryableType)
        {
            return CanonicalMethodNames.IsKnownMethod(node, memberExpr);
        }
        
        private static void AnalyzeLinqExpression(SyntaxNodeAnalysisContext context)
        {
            var efContext = new EFUsageContext(context);
            //If there's no known DbContext derived types in the semantic model, then there
            //is no point continuing
            if (efContext.DbContexts.Count == 0)
                return;

            //Check if the lambda is part of an IQueryable call chain. If so, check that it only contains valid
            //EF LINQ constructs (initializers, entity members, entity navigation properties)
            var lambda = context.Node as LambdaExpressionSyntax;
            if (lambda != null)
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
                                                    if (nts.MetadataName == "DbSet`1")
                                                    {
                                                        //That is part of a class derived from DbContext?
                                                        if (pts?.ContainingType?.BaseType?.Name == "DbContext")
                                                        {
                                                            var typeArg = nts.TypeArguments[0];
                                                            //Let's give our method some assistance, by checking what T actually is
                                                            var clsInfo = efContext.BuildEFClassInfo(typeArg);
                                                            //Okay now let's see if this lambda is valid in the EF context
                                                            ValidateLinqToEntitiesExpression(lambda, clsInfo, context);
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
                                                    if (nts.MetadataName == "DbSet`1")
                                                    {
                                                        //TODO: Should still actually check that it is ultimately assigned
                                                        //from a DbSet<T> property of a DbContext derived class

                                                        var clsInfo = efContext.BuildEFClassInfo(typeArg);
                                                        ValidateLinqToEntitiesExpression(lambda, clsInfo, context);
                                                    }
                                                    else if (nts.MetadataName == "IQueryable`1")
                                                    {
                                                        var clsInfo = efContext.BuildEFClassInfo(typeArg);
                                                        ValidateLinqToEntitiesExpression(lambda, clsInfo, context, treatAsWarning: true);
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
                            if (declType.Identifier.ValueText == "Expression" && declType.TypeArgumentList.Arguments.Count == 1)
                            {
                                //The T is Func<TInput, TOutput>
                                var exprTypeArg = declType.TypeArgumentList.Arguments[0] as GenericNameSyntax;
                                if (exprTypeArg != null &&
                                    exprTypeArg.Identifier.ValueText == "Func" &&
                                    exprTypeArg.TypeArgumentList.Arguments.Count == 2)
                                {
                                    var inputType = exprTypeArg.TypeArgumentList.Arguments[0] as IdentifierNameSyntax;
                                    var outputType = exprTypeArg.TypeArgumentList.Arguments[1] as PredefinedTypeSyntax;
                                    //The TOutput in Func<TInput, TOutput> is bool
                                    if (inputType != null && outputType != null && outputType.Keyword.ValueText == "bool")
                                    {
                                        var si = context.SemanticModel.GetSymbolInfo(inputType);
                                        var ts = efContext.EntityTypes.FirstOrDefault(t => t == si.Symbol);
                                        if (ts != null)
                                        {
                                            var clsInfo = efContext.BuildEFClassInfo(ts);
                                            ValidateLinqToEntitiesExpression(lambda, clsInfo, context);
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
