using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFLinqAnalyzer
{
    public static class DiagnosticCodes
    {
        /// <summary>
        /// EFLINQ001: (Info) Read-Only EF code first property
        /// </summary>
        public static DiagnosticDescriptor EFLINQ001 = new DiagnosticDescriptor(
            id: nameof(EFLINQ001),
            title: new LocalizableResourceString(nameof(Resources.EFLINQ001_TITLE), Resources.ResourceManager, typeof(Resources)),
            messageFormat: new LocalizableResourceString(nameof(Resources.EFLINQ001_MSGFORMAT), Resources.ResourceManager, typeof(Resources)),
            category: Resources.DIAGNOSTIC_CATEGORY,
            defaultSeverity: DiagnosticSeverity.Info,
            isEnabledByDefault: true,
            description: new LocalizableResourceString(nameof(Resources.EFLINQ001_DESC), Resources.ResourceManager, typeof(Resources)));

        /// <summary>
        /// EFLINQ002: (Error) Read-Only property used within LINQ to Entities expression
        /// </summary>
        public static DiagnosticDescriptor EFLINQ002 = new DiagnosticDescriptor(
            id: nameof(EFLINQ002),
            title: new LocalizableResourceString(nameof(Resources.EFLINQ002_TITLE), Resources.ResourceManager, typeof(Resources)),
            messageFormat: new LocalizableResourceString(nameof(Resources.EFLINQ002_MSGFORMAT), Resources.ResourceManager, typeof(Resources)),
            category: Resources.DIAGNOSTIC_CATEGORY,
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: new LocalizableResourceString(nameof(Resources.EFLINQ002_DESC), Resources.ResourceManager, typeof(Resources)));

        /// <summary>
        /// EFLINQ003: (Error) Invalid static method call within LINQ to Entities expression
        /// </summary>
        public static DiagnosticDescriptor EFLINQ003 = new DiagnosticDescriptor(
            id: nameof(EFLINQ003),
            title: new LocalizableResourceString(nameof(Resources.EFLINQ003_TITLE), Resources.ResourceManager, typeof(Resources)),
            messageFormat: new LocalizableResourceString(nameof(Resources.EFLINQ003_MSGFORMAT), Resources.ResourceManager, typeof(Resources)),
            category: Resources.DIAGNOSTIC_CATEGORY,
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: new LocalizableResourceString(nameof(Resources.EFLINQ003_DESC), Resources.ResourceManager, typeof(Resources)));

        /// <summary>
        /// EFLINQ004: (Error) Invalid method call on instance within LINQ to Entities expression
        /// </summary>
        public static DiagnosticDescriptor EFLINQ004 = new DiagnosticDescriptor(
            id: nameof(EFLINQ004),
            title: new LocalizableResourceString(nameof(Resources.EFLINQ004_TITLE), Resources.ResourceManager, typeof(Resources)),
            messageFormat: new LocalizableResourceString(nameof(Resources.EFLINQ004_MSGFORMAT), Resources.ResourceManager, typeof(Resources)),
            category: Resources.DIAGNOSTIC_CATEGORY,
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: new LocalizableResourceString(nameof(Resources.EFLINQ004_DESC), Resources.ResourceManager, typeof(Resources)));

        /// <summary>
        /// EFLINQ005: (Warning) Read-Only property potentially used within LINQ to Entities expression
        /// </summary>
        public static DiagnosticDescriptor EFLINQ005 = new DiagnosticDescriptor(
            id: nameof(EFLINQ005),
            title: new LocalizableResourceString(nameof(Resources.EFLINQ005_TITLE), Resources.ResourceManager, typeof(Resources)),
            messageFormat: new LocalizableResourceString(nameof(Resources.EFLINQ005_MSGFORMAT), Resources.ResourceManager, typeof(Resources)),
            category: Resources.DIAGNOSTIC_CATEGORY,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: new LocalizableResourceString(nameof(Resources.EFLINQ005_DESC), Resources.ResourceManager, typeof(Resources)));

        /// <summary>
        /// EFLINQ006: (Warning) Potential invalid static method call within LINQ to Entities expression
        /// </summary>
        public static DiagnosticDescriptor EFLINQ006 = new DiagnosticDescriptor(
            id: nameof(EFLINQ006),
            title: new LocalizableResourceString(nameof(Resources.EFLINQ006_TITLE), Resources.ResourceManager, typeof(Resources)),
            messageFormat: new LocalizableResourceString(nameof(Resources.EFLINQ006_MSGFORMAT), Resources.ResourceManager, typeof(Resources)),
            category: Resources.DIAGNOSTIC_CATEGORY,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: new LocalizableResourceString(nameof(Resources.EFLINQ006_DESC), Resources.ResourceManager, typeof(Resources)));

        /// <summary>
        /// EFLINQ007: (Warning) Potential unsupported method call on instance within LINQ to Entities expression
        /// </summary>
        public static DiagnosticDescriptor EFLINQ007 = new DiagnosticDescriptor(
            id: nameof(EFLINQ007),
            title: new LocalizableResourceString(nameof(Resources.EFLINQ007_TITLE), Resources.ResourceManager, typeof(Resources)),
            messageFormat: new LocalizableResourceString(nameof(Resources.EFLINQ007_MSGFORMAT), Resources.ResourceManager, typeof(Resources)),
            category: Resources.DIAGNOSTIC_CATEGORY,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: new LocalizableResourceString(nameof(Resources.EFLINQ007_DESC), Resources.ResourceManager, typeof(Resources)));

        /// <summary>
        /// EFLINQ008: (Error) Collection navigation property not queryable
        /// </summary>
        public static DiagnosticDescriptor EFLINQ008 = new DiagnosticDescriptor(
            id: nameof(EFLINQ008),
            title: new LocalizableResourceString(nameof(Resources.EFLINQ008_TITLE), Resources.ResourceManager, typeof(Resources)),
            messageFormat: new LocalizableResourceString(nameof(Resources.EFLINQ008_MSGFORMAT), Resources.ResourceManager, typeof(Resources)),
            category: Resources.DIAGNOSTIC_CATEGORY,
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: new LocalizableResourceString(nameof(Resources.EFLINQ008_DESC), Resources.ResourceManager, typeof(Resources)));

        /// <summary>
        /// EFLINQ009: (Warning) Collection navigation property in a potential LINQ to Entities expression is not queryable
        /// </summary>
        public static DiagnosticDescriptor EFLINQ009 = new DiagnosticDescriptor(
            id: nameof(EFLINQ009),
            title: new LocalizableResourceString(nameof(Resources.EFLINQ009_TITLE), Resources.ResourceManager, typeof(Resources)),
            messageFormat: new LocalizableResourceString(nameof(Resources.EFLINQ009_MSGFORMAT), Resources.ResourceManager, typeof(Resources)),
            category: Resources.DIAGNOSTIC_CATEGORY,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: new LocalizableResourceString(nameof(Resources.EFLINQ009_DESC), Resources.ResourceManager, typeof(Resources)));

        /// <summary>
        /// EFLINQ010: (Warning) Collection navigation property in a potential LINQ to Entities expression is not queryable (ambiguous entity type)
        /// </summary>
        public static DiagnosticDescriptor EFLINQ010 = new DiagnosticDescriptor(
            id: nameof(EFLINQ010),
            title: new LocalizableResourceString(nameof(Resources.EFLINQ010_TITLE), Resources.ResourceManager, typeof(Resources)),
            messageFormat: new LocalizableResourceString(nameof(Resources.EFLINQ010_MSGFORMAT), Resources.ResourceManager, typeof(Resources)),
            category: Resources.DIAGNOSTIC_CATEGORY,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: new LocalizableResourceString(nameof(Resources.EFLINQ010_DESC), Resources.ResourceManager, typeof(Resources)));

        /// <summary>
        /// EFLINQ011: (Error) Interpolated strings cannot be used within a LINQ to Entities expression
        /// </summary>
        public static DiagnosticDescriptor EFLINQ011 = new DiagnosticDescriptor(
            id: nameof(EFLINQ011),
            title: new LocalizableResourceString(nameof(Resources.EFLINQ011_TITLE), Resources.ResourceManager, typeof(Resources)),
            messageFormat: new LocalizableResourceString(nameof(Resources.EFLINQ011_MSGFORMAT), Resources.ResourceManager, typeof(Resources)),
            category: Resources.DIAGNOSTIC_CATEGORY,
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: new LocalizableResourceString(nameof(Resources.EFLINQ011_DESC), Resources.ResourceManager, typeof(Resources)));

        /// <summary>
        /// EFLINQ012: (Warning) Interpolated string potentially used within a LINQ to Entities expression
        /// </summary>
        public static DiagnosticDescriptor EFLINQ012 = new DiagnosticDescriptor(
            id: nameof(EFLINQ012),
            title: new LocalizableResourceString(nameof(Resources.EFLINQ012_TITLE), Resources.ResourceManager, typeof(Resources)),
            messageFormat: new LocalizableResourceString(nameof(Resources.EFLINQ012_MSGFORMAT), Resources.ResourceManager, typeof(Resources)),
            category: Resources.DIAGNOSTIC_CATEGORY,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: new LocalizableResourceString(nameof(Resources.EFLINQ012_DESC), Resources.ResourceManager, typeof(Resources)));

        public static ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(
            EFLINQ001,
            EFLINQ002,
            EFLINQ003,
            EFLINQ004,
            EFLINQ005,
            EFLINQ006,
            EFLINQ007,
            EFLINQ008,
            EFLINQ009,
            EFLINQ010,
            EFLINQ011,
            EFLINQ012);
    }
}
