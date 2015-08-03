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
        
        internal EFCodeFirstClassInfo(INamedTypeSymbol clsType)
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

        /// <summary>
        /// Returns true if this class has a property of the given name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool HasProperty(string name) => _props.ContainsKey(name);

        /// <summary>
        /// Returns true if this class has any collection navigation properties
        /// </summary>
        /// <returns></returns>
        public bool HasCollectionNavigationProperties() => _props.Any(p => IsCollectionNavigationProperty(p.Key));

        /// <summary>
        /// Returns true if this class has any read-only properties
        /// </summary>
        /// <returns></returns>
        public bool HasReadOnlyProperties() => _props.Any(p => p.Value.SetMethod == null);

        /// <summary>
        /// Returns true if the given property name is read-only
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool IsReadOnly(string name) => _props.ContainsKey(name) && _props[name].SetMethod == null;

        /// <summary>
        /// Returns true if the given property name has a NotMappedAttribute applied
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool IsExplicitlyUnmapped(string name) => _props.ContainsKey(name) && _props[name].GetAttributes().Any(attr => attr.AttributeClass.Name == "NotMappedAttribute");

        /// <summary>
        /// Returns true if the given property has the NotMappedAttribute applied
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool IsUnmapped(string name)
        {
            if (_props.ContainsKey(name))
            {
                return _props[name].GetAttributes().Any(ad => ad.AttributeClass.Name == "NotMappedAttribute");
            }
            return false;
        }

        /// <summary>
        /// Returns true if the given property is a collection navigation property
        /// </summary>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public bool IsCollectionNavigationProperty(string propertyName)
        {
            if (_props.ContainsKey(propertyName))
            {
                var sym = _props[propertyName];
                return sym.IsVirtual && IsValidCollectionType(sym.Type.MetadataName);
            }
            return false;
        }

        private static bool IsValidCollectionType(string metadataName) => metadataName == "ICollection`1"; //Are there any other valid collection types for EF navigation properties?

        public override string ToString() => this.ClassType.ToString();
    }
}
