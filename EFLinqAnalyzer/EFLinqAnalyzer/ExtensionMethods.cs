using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFLinqAnalyzer
{
    public static class ExtensionMethods
    {
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

            return
                prop?.Type ??
                param?.Type ??
                local?.Type ??
                field?.Type ??
                evt?.Type ??
                null;
        }
    }
}
