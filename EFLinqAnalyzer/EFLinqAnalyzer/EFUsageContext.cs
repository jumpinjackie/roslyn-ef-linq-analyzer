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
        private ReadOnlyCollection<INamedTypeSymbol> _entityTypeSymbols;

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
            //Bail immediately if EntityFramework is not referenced
            if (!_context.SemanticModel
                         .Compilation
                         .ReferencedAssemblyNames
                         .Any(asm => asm.Name == "EntityFramework" 
                                  && asm.Version.Major == 6))
            {
                return false;
            }
            
            var typeSymbols = _context.SemanticModel
                                      .LookupSymbols(_context.Node
                                                             .GetLocation()
                                                             .SourceSpan
                                                             .Start)
                                      .OfType<INamedTypeSymbol>();

            var symbols = typeSymbols.Where(t => t.UltimatelyDerivesFromDbContext());

            _dbContextSymbols = new ReadOnlyCollection<INamedTypeSymbol>(symbols.ToList());

            if (_dbContextSymbols.Count == 0)
                return false;

            //TODO: Need to follow any related types that hang off of the primary entities (referenced via DbSet<T>)
            var entityTypes = _dbContextSymbols.SelectMany(dbc =>
            {
                var dbSetProps = dbc.GetMembers()
                    .Where(m => m.Kind == SymbolKind.Property)
                    .Cast<IPropertySymbol>()
                    .Where(t => t.IsDbSetProperty());

                return dbSetProps;
            })
            .Select(p => p.Type)
            .OfType<INamedTypeSymbol>()
            .Where(t => t.TypeArguments.Length == 1)
            .Select(t => t.TypeArguments.First())
            .OfType<INamedTypeSymbol>();

            _entityTypeSymbols = new ReadOnlyCollection<INamedTypeSymbol>(entityTypes.ToList());

            _clsInfo = new Dictionary<ITypeSymbol, EFCodeFirstClassInfo>();
            _propertiesToCls = new Dictionary<string, List<EFCodeFirstClassInfo>>();
            foreach (var clsSymbol in _entityTypeSymbols)
            {
                var clsSymbols = clsSymbol.GetMembers()
                                              .Where(m => m.Kind == SymbolKind.Property)
                                              .Cast<IPropertySymbol>();
                var clsInfo = new EFCodeFirstClassInfo(clsSymbol);
                clsInfo.AddProperties(clsSymbols, (sym) =>
                {
                    if (!_propertiesToCls.ContainsKey(sym.Name))
                        _propertiesToCls[sym.Name] = new List<EFCodeFirstClassInfo>();
                    _propertiesToCls[sym.Name].Add(clsInfo);
                });

                _clsInfo[clsSymbol] = clsInfo;
            }
            return _clsInfo.Count > 0;
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
