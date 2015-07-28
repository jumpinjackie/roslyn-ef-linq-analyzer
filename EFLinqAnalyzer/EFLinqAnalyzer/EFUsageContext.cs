using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFLinqAnalyzer
{
    /// <summary>
    /// An Entity Framework specific view of the current semantic model
    /// 
    /// Symbols of interest to Entity Framework are pre-cached for easy lookup
    /// </summary>
    public class EFUsageContext
    {
        private SyntaxNodeAnalysisContext _context;
        private ReadOnlyCollection<INamedTypeSymbol> _dbContextSymbols;
        private ReadOnlyCollection<ITypeSymbol> _entityTypeSymbols;

        public EFUsageContext(SyntaxNodeAnalysisContext context)
        {
            _context = context;
            var symbols = context.SemanticModel
                                 .LookupSymbols(context.Node
                                                       .GetLocation()
                                                       .SourceSpan
                                                       .Start)
                                 .OfType<INamedTypeSymbol>()
                                 .Where(t => t?.BaseType?.Name == "DbContext");

            _dbContextSymbols = new ReadOnlyCollection<INamedTypeSymbol>(symbols.ToList());

            var entityTypes = _dbContextSymbols.SelectMany(dbc =>
            {
                return _context.SemanticModel
                               .LookupSymbols(dbc.Locations
                                                 .First()
                                                 .SourceSpan
                                                 .Start, dbc)
                               .OfType<IPropertySymbol>()
                               .Where(t => t.Type.MetadataName == "DbSet`1");
            })
            .Select(t => t.Type as INamedTypeSymbol)
            .Where(t => t != null)
            .Select(t => t.TypeArguments.First());

            _entityTypeSymbols = new ReadOnlyCollection<ITypeSymbol>(entityTypes.ToList());
        }

        public EFCodeFirstClassInfo BuildEFClassInfo(ITypeSymbol typeArg)
        {
            var clsSymbol = _context.SemanticModel
                                    .LookupNamespacesAndTypes(typeArg.Locations
                                                                     .First()
                                                                     .SourceSpan
                                                                     .Start, null, typeArg.Name)
                                    .OfType<INamedTypeSymbol>()
                                    .FirstOrDefault();

            var clsSymbols = _context.SemanticModel
                                     .LookupSymbols(clsSymbol.Locations
                                                             .First()
                                                             .SourceSpan
                                                             .Start, clsSymbol);

            var clsInfo = new EFCodeFirstClassInfo(clsSymbol);
            clsInfo.AddProperties(clsSymbols.OfType<IPropertySymbol>());
            return clsInfo;
        }

        /// <summary>
        /// Gets the list of known DbContext derived type symbols from the current semantic model
        /// </summary>
        public IReadOnlyCollection<INamedTypeSymbol> DbContexts => _dbContextSymbols;

        /// <summary>
        /// Gets the list of known EF code first class type symbols
        /// </summary>
        public IReadOnlyCollection<ITypeSymbol> EntityTypes => _entityTypeSymbols;
    }
}
