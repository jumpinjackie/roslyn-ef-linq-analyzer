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

        internal void AddProperties(IEnumerable<IPropertySymbol> symbols)
        {
            foreach (var sym in symbols.Where(p => p.SetMethod == null))
            {
                _props[sym.Name] = sym;
            }
        }

        public bool HasReadOnlyProperties()
        {
            return _props.Count > 0;
        }
        
        public bool IsReadOnly(string name)
        {
            return _props.ContainsKey(name);
        }
    }
}
