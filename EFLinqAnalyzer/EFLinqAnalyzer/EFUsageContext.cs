using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFLinqAnalyzer
{
    /// <summary>
    /// An Entity Framework specific view of the current semantic model
    /// 
    /// Symbols of interest to Entity Framework are stashed for easy lookup
    /// </summary>
    public class EFUsageContext
    {
        private SyntaxNodeAnalysisContext _context;
        private ReadOnlyCollection<INamedTypeSymbol> _dbContextSymbols;
        private ReadOnlyCollection<ITypeSymbol> _entityTypeSymbols;

        private Dictionary<ITypeSymbol, EFCodeFirstClassInfo> _clsInfo;
        private Dictionary<string, List<EFCodeFirstClassInfo>> _propertiesToCls;

        public EFUsageContext(SyntaxNodeAnalysisContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Builds the EF-specific view of the current semantic model
        /// </summary>
        /// <returns>false if no DbContext or known entity types were discovered, true otherwise</returns>
        public bool Build()
        {
            //TODO: Can we check if Entity Framework is even referenced? We could just
            //bail out right here right now if it isn't referenced.

            var typeSymbols = _context.SemanticModel
                                      .LookupSymbols(_context.Node
                                                             .GetLocation()
                                                             .SourceSpan
                                                             .Start)
                                      .OfType<INamedTypeSymbol>();

            var symbols = typeSymbols.Where(t => TypeUltimatelyDerivesFromDbContext(t));

            _dbContextSymbols = new ReadOnlyCollection<INamedTypeSymbol>(symbols.ToList());

            if (_dbContextSymbols.Count == 0)
                return false;

            var entityTypes = _dbContextSymbols.SelectMany(dbc =>
            {
                var dbSetProps = _context.SemanticModel
                                         .LookupSymbols(dbc.Locations
                                                           .First()
                                                           .SourceSpan
                                                           .Start, dbc)
                                         .OfType<IPropertySymbol>()
                                         .Where(t => t.Type.MetadataName == "DbSet`1");
                return dbSetProps;
            })
            .Select(t => t.Type as INamedTypeSymbol)
            .Where(t => t != null)
            .Select(t => t.TypeArguments.First());

            _entityTypeSymbols = new ReadOnlyCollection<ITypeSymbol>(entityTypes.ToList());

            _clsInfo = new Dictionary<ITypeSymbol, EFCodeFirstClassInfo>();
            _propertiesToCls = new Dictionary<string, List<EFCodeFirstClassInfo>>();
            foreach (var et in _entityTypeSymbols)
            {
                var clsSymbol = _context.SemanticModel
                                        .LookupNamespacesAndTypes(et.Locations
                                                                    .First()
                                                                    .SourceSpan
                                                                    .Start, null, et.Name)
                                        .OfType<INamedTypeSymbol>()
                                        .FirstOrDefault();

                var clsSymbols = _context.SemanticModel
                                         .LookupSymbols(clsSymbol.Locations
                                                                 .First()
                                                                 .SourceSpan
                                                                 .Start, clsSymbol);

                var clsInfo = new EFCodeFirstClassInfo(clsSymbol);
                clsInfo.AddProperties(clsSymbols.OfType<IPropertySymbol>(), (sym) => {
                    if (!_propertiesToCls.ContainsKey(sym.Name))
                        _propertiesToCls[sym.Name] = new List<EFCodeFirstClassInfo>();
                    _propertiesToCls[sym.Name].Add(clsInfo);
                });

                _clsInfo[et] = clsInfo;
            }
            return true;
        }

        internal static bool TypeUltimatelyDerivesFromDbContext(ITypeSymbol type)
        {
            //Walk up the inheritance chain until we encounter DbContext or null (ie. We're at System.Object)
            var bt = type.BaseType;
            while (bt != null)
            {
                if (bt.Name == "DbContext")
                    return true;

                bt = bt.BaseType;
            }
            return false;
        }

        /// <summary>
        /// Gets the EF class info for the given type symbol. Returns null if no such class found
        /// </summary>
        /// <param name="typeArg"></param>
        /// <returns></returns>
        public EFCodeFirstClassInfo GetClassInfo(ITypeSymbol typeArg) => _clsInfo.ContainsKey(typeArg) ? _clsInfo[typeArg] : null;

        /// <summary>
        /// Gets the applicable EF classes for the given property name.
        /// </summary>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public ImmutableArray<EFCodeFirstClassInfo> GetClassForProperty(string propertyName) => _propertiesToCls.ContainsKey(propertyName) ? _propertiesToCls[propertyName].ToImmutableArray() : ImmutableArray.Create<EFCodeFirstClassInfo>();

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
