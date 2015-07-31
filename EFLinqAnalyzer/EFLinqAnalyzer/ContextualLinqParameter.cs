using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFLinqAnalyzer
{
    public class ContextualLinqParameter
    {
        public string Name { get; }

        public ContextualLinqParameter(ParameterSyntax syntax)
            : this(syntax.Identifier)
        {
            
        }

        public ContextualLinqParameter(SyntaxToken token)
        {
            this.Name = token.ValueText;
        }
    }
}
