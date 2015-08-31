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
    public enum ContextualLinqParameterType
    {
        Unknown,
        Queryable,
        Parameter
    }

    public class ContextualLinqParameter
    {
        public string Name { get; }

        public ITypeSymbol Type { get; }

        public EFCodeFirstClassInfo QueryableType { get; }

        public ContextualLinqParameterType ParameterType { get; }

        private ContextualLinqParameter(string name)
        {
            this.Name = name;
            this.ParameterType = ContextualLinqParameterType.Unknown;
        }

        private ContextualLinqParameter(string name, ITypeSymbol type)
        {
            this.Name = name;
            this.Type = type;
            this.ParameterType = ContextualLinqParameterType.Parameter;
        }

        private ContextualLinqParameter(string name, EFCodeFirstClassInfo type)
        {
            this.Name = name;
            this.QueryableType = type;
            this.ParameterType = ContextualLinqParameterType.Queryable;
        }
        
        private ContextualLinqParameter(ParameterSyntax syntax)
            : this(syntax.Identifier)
        {
            
        }

        private ContextualLinqParameter(SyntaxToken token)
        {
            this.Name = token.ValueText;
        }

        public static Dictionary<string, ContextualLinqParameter> BuildContext(IEnumerable<ParameterSyntax> parameters, SyntaxNodeAnalysisContext context, EFUsageContext efContext)
        {
            var cparams = new Dictionary<string, ContextualLinqParameter>();
            foreach (var p in parameters)
            {
                string name = p.Identifier.ValueText;
                var si = context.SemanticModel.GetSymbolInfo(p);
                var type = si.Symbol?.TryGetType();
                if (type != null)
                    cparams[name] = new ContextualLinqParameter(name, type);
                else
                    cparams[name] = new ContextualLinqParameter(name);
            }
            return cparams;
        }

        public static Dictionary<string, ContextualLinqParameter> BuildContext(QueryExpressionSyntax query, SyntaxNodeAnalysisContext context, EFUsageContext efContext)
        {
            var cparams = new Dictionary<string, ContextualLinqParameter>();

            //From x
            var fromExpr = query.FromClause.Identifier;
            //in <expr>
            var inExpr = query.FromClause.Expression;
            string name = fromExpr.ValueText;

            var memberExpr = inExpr as MemberAccessExpressionSyntax;
            if (memberExpr != null)
            {
                EFCodeFirstClassInfo cls;
                if (LinqExpressionValidator.MemberAccessIsAccessingDbContext(memberExpr, context, efContext, out cls))
                {
                    cparams[name] = new ContextualLinqParameter(name, cls);
                }
            }

            //Still not set, just set as a contextual parameter with no known type
            if (!cparams.ContainsKey(name))
                cparams[name] = new ContextualLinqParameter(name);

            return cparams;
        }
    }
}
