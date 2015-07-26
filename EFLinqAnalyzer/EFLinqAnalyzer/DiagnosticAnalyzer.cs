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
        private static DiagnosticDescriptor CodeFirstClassReadOnlyRule = new DiagnosticDescriptor(
            id: "EFLINQ001",
            title: new LocalizableResourceString(nameof(Resources.EFLINQ001_TITLE), Resources.ResourceManager, typeof(Resources)),
            messageFormat: new LocalizableResourceString(nameof(Resources.EFLINQ001_MSGFORMAT), Resources.ResourceManager, typeof(Resources)),
            category: "Entity Framework Gotchas",
            defaultSeverity: DiagnosticSeverity.Info,
            isEnabledByDefault: true,
            description: new LocalizableResourceString(nameof(Resources.EFLINQ001_DESC), Resources.ResourceManager, typeof(Resources)));

        private static DiagnosticDescriptor CodeFirstClassReadOnlyPropertyUsage = new DiagnosticDescriptor(
            id: "EFLINQ002",
            title: new LocalizableResourceString(nameof(Resources.EFLINQ002_TITLE), Resources.ResourceManager, typeof(Resources)),
            messageFormat: new LocalizableResourceString(nameof(Resources.EFLINQ002_MSGFORMAT), Resources.ResourceManager, typeof(Resources)),
            category: "Entity Framework Gotchas",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: new LocalizableResourceString(nameof(Resources.EFLINQ002_DESC), Resources.ResourceManager, typeof(Resources)));

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(CodeFirstClassReadOnlyRule, CodeFirstClassReadOnlyPropertyUsage);
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeEFCodeFirstModelClassReadOnlyProperty, SyntaxKind.PropertyDeclaration);
            //context.RegisterSyntaxNodeAction(AnalyzeEFCodeFirstModelClassReadOnlyProperty, SyntaxKind.ClassDeclaration);
            context.RegisterSyntaxNodeAction(AnalyzeLinqExpression, SyntaxKind.SimpleLambdaExpression, SyntaxKind.ParenthesizedLambdaExpression);
            //context.RegisterCompilationAction(OnCompilation);
        }

        private static void OnCompilation(CompilationAnalysisContext context)
        {
            /*
            var compilation = context.Compilation;
            var assemblies = compilation.ReferencedAssemblyNames;
            //Entity Framework is referenced
            if (assemblies.Any(asm => asm.Name == "EntityFramework"))
            {
                
            }
            */
        }

        private static void AnalyzeEFCodeFirstModelClassReadOnlyProperty_(SyntaxNodeAnalysisContext context)
        {
            var clsNode = context.Node as ClassDeclarationSyntax;
            if (clsNode != null)
            {
                if (clsNode.BaseList != null)
                {
                    //This is a class that inherits from DbContext
                    if (clsNode?.BaseList?.Types.OfType<BaseTypeSyntax>().Where(p => p.Type is IdentifierNameSyntax).Select(p => (IdentifierNameSyntax)p.Type).Any(id => id.Identifier.ValueText == "DbContext") == true)
                    {
                        var dbSetProperties = clsNode
                            .DescendantNodes()
                            .OfType<PropertyDeclarationSyntax>()
                            .Where(p => p.Type is GenericNameSyntax)
                            .Where(p => ((GenericNameSyntax)p.Type).Identifier.ValueText == "DbSet");
                        
                    }
                }
            }
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
                            var diagnostic = Diagnostic.Create(CodeFirstClassReadOnlyRule, propNode.GetLocation(), propNode.Identifier.ValueText, declCls.Identifier.ValueText);
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
                            var diagnostic = Diagnostic.Create(CodeFirstClassReadOnlyRule, propNode.GetLocation(), propNode.Identifier.ValueText, declCls.Identifier.ValueText);
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
                                            .Where(p => p?.Type?.Name == "DbSet");

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

        static void ValidateLinqToEntitiesExpression(LambdaExpressionSyntax lambda, EFCodeFirstClassInfo rootQueryableType, SyntaxNodeAnalysisContext context)
        {
            var accessNodes = lambda.DescendantNodes().OfType<MemberAccessExpressionSyntax>();
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
                    var diagnostic = Diagnostic.Create(CodeFirstClassReadOnlyPropertyUsage, node.GetLocation(), memberName.Identifier.ValueText, rootQueryableType.Name);
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }

        private static void AnalyzeLinqExpression(SyntaxNodeAnalysisContext context)
        {
            //Check if the lambda is part of an IQueryable call chain. If so, check that it only contains valid
            //EF LINQ constructs (initializers, entity members, entity navigation properties)
            var lambda = context.Node as LambdaExpressionSyntax;
            if (lambda != null)
            {
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
                                    case "Select": //Projection
                                    case "Where": //Filter
                                        {
                                            var si = context.SemanticModel.GetSymbolInfo(memberExpr.Expression);
                                            //Is this Where() called on a property?
                                            var pts = si.Symbol as IPropertySymbol;
                                            if (pts != null)
                                            {
                                                var nts = pts.Type as INamedTypeSymbol;
                                                if (nts != null)
                                                {
                                                    //Like a DbSet<T>?
                                                    if (nts.Name == "DbSet" && nts.TypeArguments.Length == 1)
                                                    {
                                                        //That is part of a class derived from DbContext?
                                                        if (pts?.ContainingType?.BaseType.Name == "DbContext")
                                                        {
                                                            var typeArg = nts.TypeArguments[0];
                                                            //Let's give our method some assistance, by checking what T actually is
                                                            var clsSymbol = context.SemanticModel
                                                                                   .LookupNamespacesAndTypes(typeArg.Locations.First().SourceSpan.Start, null, typeArg.Name)
                                                                                   .OfType<INamedTypeSymbol>()
                                                                                   .FirstOrDefault();

                                                            var clsSymbols = context.SemanticModel
                                                                                    .LookupSymbols(clsSymbol.Locations.First().SourceSpan.Start, clsSymbol);

                                                            //If it has potential EF LINQ minefields then do the actual check
                                                            var clsInfo = new EFCodeFirstClassInfo(clsSymbol);
                                                            clsInfo.AddProperties(clsSymbols.OfType<IPropertySymbol>());
                                                            if (clsInfo.HasReadOnlyProperties())
                                                            {
                                                                //Okay now let's see if this lambda is valid in the EF context
                                                                ValidateLinqToEntitiesExpression(lambda, clsInfo, context);
                                                            }
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
            }
        }
    }
}
