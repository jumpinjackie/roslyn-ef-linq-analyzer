using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFLinqAnalyzer
{
    public class EFCodeFirstClassInfo
    {
        public INamedTypeSymbol ClassType { get; }

        public string Name => this.ClassType.Name;

        private Dictionary<string, IPropertySymbol> _props;
        
        public EFCodeFirstClassInfo(INamedTypeSymbol clsType)
        {
            this.ClassType = clsType;
            _props = new Dictionary<string, IPropertySymbol>();
        }

        internal void AddProperties(IEnumerable<IPropertySymbol> symbols, Action<IPropertySymbol> regCallback)
        {
            foreach (var sym in symbols)
            {
                _props[sym.Name] = sym;
                regCallback?.Invoke(sym);
            }
        }

        public bool HasReadOnlyProperties()
        {
            return _props.Any(p => p.Value.SetMethod == null);
        }
        
        public bool IsReadOnly(string name)
        {
            return _props.ContainsKey(name) && _props[name].SetMethod == null;
        }

        public bool IsCollectionNavigationProperty(string propertyName)
        {
            if (_props.ContainsKey(propertyName))
            {
                var sym = _props[propertyName];
                return sym.IsVirtual && IsValidCollectionType(sym.Type.MetadataName);
            }
            return false;
        }

        private static bool IsValidCollectionType(string metadataName)
        {
            if (metadataName.EndsWith("`1", StringComparison.Ordinal))
            {
                //Are there any other valid collection types for EF navigation properties?
                return metadataName == "ICollection`1";
            }
            return false;
        }

        public override string ToString()
        {
            return this.ClassType.ToString();
        }
    }
}
