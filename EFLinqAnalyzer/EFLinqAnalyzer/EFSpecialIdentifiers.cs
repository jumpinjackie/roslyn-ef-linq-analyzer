using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFLinqAnalyzer
{
    public static class EFSpecialIdentifiers
    {
        /// <summary>
        /// AsQueryable()
        /// </summary>
        public const string AsQueryable = nameof(AsQueryable);

        /// <summary>
        /// Boolean
        /// </summary>
        public const string Boolean = nameof(Boolean);

        /// <summary>
        /// bool
        /// </summary>
        public const string BooleanShort = "bool";

        /// <summary>
        /// DbContext
        /// </summary>
        public const string DbContext = nameof(DbContext);

        /// <summary>
        /// <![CDATA[
        /// Expression<TFunc> where TFunc : Func<T, bool>
        /// ]]>
        /// </summary>
        public const string Expression = nameof(Expression);

        /// <summary>
        /// <![CDATA[
        /// Func<T, bool>
        /// ]]>
        /// </summary>
        public const string Func = nameof(Func);

        /// <summary>
        /// <![CDATA[
        /// DbSet<T>
        /// ]]>
        /// </summary>
        public const string DbSet = "DbSet`1";

        /// <summary>
        /// <![CDATA[
        /// IDbSet<T>
        /// ]]>
        /// </summary>
        public const string IDbSet = "IDbSet`1";

        /// <summary>
        /// <![CDATA[
        /// IQueryable<T>
        /// ]]>
        /// </summary>
        public const string IQueryable = "IQueryable`1";
    }
}
