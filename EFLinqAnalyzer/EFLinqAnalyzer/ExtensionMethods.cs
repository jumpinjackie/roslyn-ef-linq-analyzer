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
    public static class ExtensionMethods
    {
        public static bool IsDbSetProperty(this IPropertySymbol p)
        {
            return p?.Type?.MetadataName == EFSpecialIdentifiers.DbSet ||
                   p?.Type?.MetadataName == EFSpecialIdentifiers.IDbSet;
        }

        public static bool IsDbSet(this ITypeSymbol nts)
        {
            return nts?.MetadataName == EFSpecialIdentifiers.DbSet ||
                   nts?.MetadataName == EFSpecialIdentifiers.IDbSet;
        }

        public static bool IsQueryable(this ITypeSymbol nts)
        {
            return nts?.MetadataName == EFSpecialIdentifiers.IQueryable;
        }

        /// <summary>
        /// Attempts to get the type of the given symbol
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        public static ITypeSymbol TryGetType(this ISymbol symbol)
        {
            //Psst. Hey Microsoft: Maybe you could have a ISymbolWithType interface
            //that has a Type property that all these interfaces below inherit from
            //so I don't have to do this myself!
            var prop = symbol as IPropertySymbol;
            var param = symbol as IParameterSymbol;
            var local = symbol as ILocalSymbol;
            var field = symbol as IFieldSymbol;
            var evt = symbol as IEventSymbol;
            var nts = symbol as INamedTypeSymbol;

            return
                prop?.Type ??
                param?.Type ??
                local?.Type ??
                field?.Type ??
                evt?.Type ??
                nts;
        }

        /// <summary>
        /// Gets the method declaration for the given method identifier
        /// </summary>
        /// <param name="identifier"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public static MethodDeclarationSyntax GetDeclaringMethod(this IdentifierNameSyntax identifier, SyntaxNodeAnalysisContext context)
        {
            var si = context.SemanticModel.GetSymbolInfo(identifier);
            return si.Symbol?.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() as MethodDeclarationSyntax;
        }

        /// <summary>
        /// Indicates if this type symbol ultimately derives from DbContext
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool UltimatelyDerivesFromDbContext(this ITypeSymbol type)
        {
            //Walk up the inheritance chain until we encounter DbContext or null (ie. We're at System.Object)
            var bt = type.BaseType;
            while (bt != null)
            {
                if (bt.Name == EFSpecialIdentifiers.DbContext)
                    return true;

                bt = bt.BaseType;
            }
            return false;
        }

        /// <summary>
        /// Indicates if this expression is a DbContext-derived object instance or some expression that returns one
        /// </summary>
        /// <param name="syntax"></param>
        /// <returns></returns>
        public static bool IsDbContextInstance(this ExpressionSyntax syntax, SyntaxNodeAnalysisContext context)
        {
            var si = context.SemanticModel.GetSymbolInfo(syntax);
            if (si.Symbol != null)
            {
                return si.Symbol.TryGetType()?.UltimatelyDerivesFromDbContext() == true;
            }
            return false;
        }
    }
}
