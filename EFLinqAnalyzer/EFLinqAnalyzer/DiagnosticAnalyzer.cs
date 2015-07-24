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

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return ImmutableArray.Create(CodeFirstClassReadOnlyRule);
            }
        }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeEFCodeFirstModelClassReadOnlyProperty, SyntaxKind.PropertyDeclaration);
            //context.RegisterSyntaxNodeAction(AnalyzeEFCodeFirstModelClassReadOnlyProperty, SyntaxKind.ClassDeclaration);
            //context.RegisterSyntaxNodeAction(AnalyzeLinqExpression, SyntaxKind.SimpleLambdaExpression, SyntaxKind.ParenthesizedLambdaExpression);
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

        private static void AnalyzeLinqExpression(SyntaxNodeAnalysisContext context)
        {
            //Check if the lambda is part of an IQueryable call chain. If so, check that it only contains valid
            //EF LINQ constructs (initializers, entity members, entity navigation properties)
        }
    }
}
