using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis;

namespace EFLinqAnalyzer
{
    /// <summary>
    /// The names of all canonical methods that are supported by Entity Framework
    /// 
    /// <see cref="https://msdn.microsoft.com/en-us/library/bb738626.aspx"/>
    /// <see cref="https://msdn.microsoft.com/en-us/library/bb738681.aspx"/>
    /// </summary>
    public static class CanonicalMethodNames
    {
        class PropertyInfo
        {
            public string Name { get; }

            private List<PropertySignature> _signatures;

            public PropertyInfo(string name)
            {
                this.Name = name;
                _signatures = new List<PropertySignature>();
            }

            private PropertyInfo(string name, IEnumerable<PropertySignature> sigs)
                : this(name)
            {   
                AddSignatures(sigs);
            }

            public IEnumerable<PropertySignature> Signatures => _signatures;

            /// <summary>
            /// Indicates that this is a stub definition. Only name validation is performed
            /// </summary>
            public bool IsStub => _signatures.Count == 0;

            /// <summary>
            /// Adds the given signature to this instance. If this was a stub, it is no longer a stub
            /// </summary>
            /// <param name="sig"></param>
            public void AddSignature(PropertySignature sig)
            {
                _signatures.Add(sig);
                sig.Parent = this;
            }

            /// <summary>
            /// Adds the given signatures to this instance. If this was a stub, it is no longer a stub
            /// </summary>
            /// <param name="sigs"></param>
            public void AddSignatures(IEnumerable<PropertySignature> sigs)
            {
                foreach (var sig in sigs)
                {
                    AddSignature(sig);
                }
            }
        }

        class PropertySignature
        {
            public PropertySignature(string invokingType, string returnType, bool isStatic)
            {
                this.InvokingType = invokingType;
                this.ReturnType = returnType;
                this.IsStatic = isStatic;
            }

            /// <summary>
            /// The name of the type that this method belongs to
            /// </summary>
            public string InvokingType { get; }

            /// <summary>
            /// The return type of this method
            /// </summary>
            public string ReturnType { get; }

            /// <summary>
            /// Gets whether this method is static
            /// </summary>
            public bool IsStatic { get; }
            public PropertyInfo Parent { get; internal set; }
        }

        class MethodInfo
        {
            private List<MethodSignature> _signatures;

            public string Name { get; }
            
            private MethodInfo(string name, IEnumerable<MethodSignature> sigs)
            {
                this.Name = name;
                _signatures = new List<MethodSignature>();
                AddSignatures(sigs);
            }

            public IEnumerable<MethodSignature> Signatures => _signatures;

            /// <summary>
            /// Indicates that this is a stub definition. Only name validation is performed
            /// </summary>
            public bool IsStub => _signatures.Count == 0;

            /// <summary>
            /// Adds the given signature to this instance. If this was a stub, it is no longer a stub
            /// </summary>
            /// <param name="sig"></param>
            public MethodInfo AddSignature(MethodSignature sig)
            {
                _signatures.Add(sig);
                sig.Parent = this;
                return this;
            }

            /// <summary>
            /// Adds the given signatures to this instance. If this was a stub, it is no longer a stub
            /// </summary>
            /// <param name="sigs"></param>
            public MethodInfo AddSignatures(IEnumerable<MethodSignature> sigs)
            {
                foreach (var sig in sigs)
                {
                    AddSignature(sig);
                }
                return this;
            }

            /// <summary>
            /// Creates a name-only method definition stub
            /// </summary>
            /// <param name="name"></param>
            /// <returns></returns>
            internal static MethodInfo Stub(string name) => new MethodInfo(name, Enumerable.Empty<MethodSignature>());

            /// <summary>
            /// Creates a method with signatures
            /// </summary>
            /// <param name="name"></param>
            /// <param name="sigs"></param>
            /// <returns></returns>
            internal static MethodInfo WithSignatures(string name, IEnumerable<MethodSignature> sigs) => new MethodInfo(name, sigs);
        }

        class MethodSignature
        {
            /// <summary>
            /// The name of the type that this method belongs to
            /// </summary>
            public string InvokingType { get; }

            /// <summary>
            /// The return type of this method
            /// </summary>
            public string ReturnType { get; }

            /// <summary>
            /// Gets whether this method is static
            /// </summary>
            public bool IsStatic { get; }

            /// <summary>
            /// A list of arguments this method is expecting
            /// </summary>
            public List<MethodArgument> Arguments { get; }
            
            public MethodInfo Parent { get; internal set; }

            public MethodSignature(string invokingType, string returnType, bool isStatic, IEnumerable<MethodArgument> args)
            {
                this.InvokingType = invokingType;
                this.IsStatic = isStatic;
                this.ReturnType = returnType;
                this.Arguments = new List<MethodArgument>(args);
            }

            public static IEnumerable<MethodSignature> Create(params MethodSignature[] sigs) => sigs;

            public override string ToString() => $"{(IsStatic ? "[static]" : "[instance]")} {this.ReturnType} {(this.Parent != null ? this.Parent.Name : "?")}({string.Join(", ", this.Arguments.Select(arg => arg.ToString()))})";
        }

        class MethodArgument
        {
            public string Name { get; }

            public string Type { get; }

            public MethodArgument(string name)
            {
                this.Name = name;
                this.Type = null;
            }

            public MethodArgument(string name, string type)
                : this(name)
            {
                this.Type = type;
            }

            public static IEnumerable<MethodArgument> Create(params MethodArgument[] args) => args;

            public override string ToString() => $"{(!string.IsNullOrEmpty(this.Type) ? this.Type : "any")} {this.Name}";
        }

        static Dictionary<string, string> _types;
        static readonly Dictionary<string, MethodInfo> _methods;

        #region Aggregate
        public const string Avg = nameof(Avg);
        public const string BigCount = nameof(BigCount);
        public const string Count = nameof(Count);
        public const string Max = nameof(Max);
        public const string Min = nameof(Min);
        public const string StDev = nameof(StDev);
        public const string StDevP = nameof(StDevP);
        public const string Sum = nameof(Sum);
        public const string Var = nameof(Var);
        public const string VarP = nameof(VarP);
        #endregion

        #region Math
        public const string Abs = nameof(Abs);
        public const string Ceiling = nameof(Ceiling);
        public const string Floor = nameof(Floor);
        public const string Power = nameof(Power);
        public const string Round = nameof(Round);
        public const string Truncate = nameof(Truncate);
        #endregion

        #region String
        public const string Concat = nameof(Concat);
        public const string Contains = nameof(Contains);
        public const string EndsWith = nameof(EndsWith);
        public const string IndexOf = nameof(IndexOf);
        public const string Left = nameof(Left);
        public const string Length = nameof(Length);
        public const string LTrim = nameof(LTrim);
        public const string Replace = nameof(Replace);
        public const string Reverse = nameof(Reverse);
        public const string Right = nameof(Right);
        public const string RTrim = nameof(RTrim);
        public const string Substring = nameof(Substring);
        public const string StartsWith = nameof(StartsWith);
        public const string ToLower = nameof(ToLower);
        public const string ToUpper = nameof(ToUpper);
        public const string Trim = nameof(Trim);
        #endregion

        #region Date and Time
        public const string AddNanoseconds = nameof(AddNanoseconds);
        public const string AddMicroseconds = nameof(AddMicroseconds);
        public const string AddMilliseconds = nameof(AddMilliseconds);
        public const string AddSeconds = nameof(AddSeconds);
        public const string AddMinutes = nameof(AddMinutes);
        public const string AddHours = nameof(AddHours);
        public const string AddDays = nameof(AddDays);
        public const string AddMonths = nameof(AddMonths);
        public const string AddYears = nameof(AddYears);
        public const string CreateDateTime = nameof(CreateDateTime);
        public const string CreateDateTimeOffset = nameof(CreateDateTimeOffset);
        public const string CreateTime = nameof(CreateTime);
        public const string CurrentDateTime = nameof(CurrentDateTime);
        public const string CurrentDateTimeOffset = nameof(CurrentDateTimeOffset);
        public const string CurrentUtcDateTime = nameof(CurrentUtcDateTime);
        public const string Day = nameof(Day);
        public const string DayOfYear = nameof(DayOfYear);
        public const string DiffNanoseconds = nameof(DiffNanoseconds);
        public const string DiffMilliseconds = nameof(DiffMilliseconds);
        public const string DiffMicroseconds = nameof(DiffMicroseconds);
        public const string DiffSeconds = nameof(DiffSeconds);
        public const string DiffMinutes = nameof(DiffMinutes);
        public const string DiffHours = nameof(DiffHours);
        public const string DiffDays = nameof(DiffDays);
        public const string DiffMonths = nameof(DiffMonths);
        public const string DiffYears = nameof(DiffYears);
        public const string GetTotalOffsetMinutes = nameof(GetTotalOffsetMinutes);
        public const string Hour = nameof(Hour);
        public const string Millisecond = nameof(Millisecond);
        public const string Minute = nameof(Minute);
        public const string Month = nameof(Month);
        public const string Second = nameof(Second);
        public const string TruncateTime = nameof(TruncateTime);
        public const string Year = nameof(Year);
        #endregion

        #region Bitwise
        public const string BitWiseAnd = nameof(BitWiseAnd);
        public const string BitWiseNot = nameof(BitWiseNot);
        public const string BitWiseOr = nameof(BitWiseOr);
        public const string BitWiseXor = nameof(BitWiseXor);
        #endregion

        #region Spatial
        public const string Area = nameof(Area);
        public const string AsBinary = nameof(AsBinary);
        public const string AsGml = nameof(AsGml);
        public const string AsText = nameof(AsText);
        public const string Centroid = nameof(Centroid);
        public const string CoordinateSystemId = nameof(CoordinateSystemId);
        public const string Distance = nameof(Distance);
        public const string Elevation = nameof(Elevation);
        public const string EndPoint = nameof(EndPoint);
        public const string ExteriorRing = nameof(ExteriorRing);
        public const string GeographyCollectionFromBinary = nameof(GeographyCollectionFromBinary);
        public const string GeographyCollectionFromText = nameof(GeographyCollectionFromText);
        public const string GeographyFromBinary = nameof(GeographyFromBinary);
        public const string GeographyFromGml = nameof(GeographyFromGml);
        public const string GeographyFromText = nameof(GeographyFromText);
        public const string GeographyLineFromBinary = nameof(GeographyLineFromBinary);
        public const string GeographyLineFromText = nameof(GeographyLineFromText);
        public const string GeographyMultiLineFromBinary = nameof(GeographyMultiLineFromBinary);
        public const string GeographyMultiLineFromText = nameof(GeographyMultiLineFromText);
        public const string GeographyMultiPointFromBinary = nameof(GeographyMultiPointFromBinary);
        public const string GeographyMultiPointFromText = nameof(GeographyMultiPointFromText);
        public const string GeographyMultiPolygonFromBinary = nameof(GeographyMultiPolygonFromBinary);
        public const string GeographyMultiPolygonFromText = nameof(GeographyMultiPolygonFromText);
        public const string GeographyPointFromBinary = nameof(GeographyPointFromBinary);
        public const string GeographyPointFromText = nameof(GeographyPointFromText);
        public const string GeographyPolygonFromBinary = nameof(GeographyPolygonFromBinary);
        public const string GeographyPolygonFromText = nameof(GeographyPolygonFromText);
        public const string GeometryCollectionFromBinary = nameof(GeometryCollectionFromBinary);
        public const string GeometryCollectionFromText = nameof(GeometryCollectionFromText);
        public const string GeometryFromBinary = nameof(GeometryFromBinary);
        public const string GeometryFromGml = nameof(GeometryFromGml);
        public const string GeometryFromText = nameof(GeometryFromText);
        public const string GeometryLineFromBinary = nameof(GeometryLineFromBinary);
        public const string GeometryLineFromText = nameof(GeometryLineFromText);
        public const string GeometryMultiLineFromBinary = nameof(GeometryMultiLineFromBinary);
        public const string GeometryMultiLineFromText = nameof(GeometryMultiLineFromText);
        public const string GeometryMultiPointFromBinary = nameof(GeometryMultiPointFromBinary);
        public const string GeometryMultiPointFromText = nameof(GeometryMultiPointFromText);
        public const string GeometryMultiPolygonFromBinary = nameof(GeometryMultiPolygonFromBinary);
        public const string GeometryMultiPolygonFromText = nameof(GeometryMultiPolygonFromText);
        public const string GeometryPointFromBinary = nameof(GeometryPointFromBinary);
        public const string GeometryPointFromText = nameof(GeometryPointFromText);
        public const string GeometryPolygonFromBinary = nameof(GeometryPolygonFromBinary);
        public const string GeometryPolygonFromText = nameof(GeometryPolygonFromText);
        public const string InteriorRingAt = nameof(InteriorRingAt);
        public const string InteriorRingCount = nameof(InteriorRingCount);
        public const string IsClosedSpatial = nameof(IsClosedSpatial);
        public const string IsEmptySpatial = nameof(IsEmptySpatial);
        public const string IsRing = nameof(IsRing);
        public const string IsSimpleGeometry = nameof(IsSimpleGeometry);
        public const string IsValidGeometry = nameof(IsValidGeometry);
        public const string Latitude = nameof(Latitude);
        public const string Longitude = nameof(Longitude);
        public const string Measure = nameof(Measure);
        public const string PointAt = nameof(PointAt);
        public const string PointCount = nameof(PointCount);
        public const string PointOnSurface = nameof(PointOnSurface);
        public const string SpatialBoundary = nameof(SpatialBoundary);
        public const string SpatialBuffer = nameof(SpatialBuffer);
        public const string SpatialContains = nameof(SpatialContains);
        public const string SpatialConvexHull = nameof(SpatialConvexHull);
        public const string SpatialCrosses = nameof(SpatialCrosses);
        public const string SpatialDifference = nameof(SpatialDifference);
        public const string SpatialDimension = nameof(SpatialDimension);
        public const string SpatialDisjoint = nameof(SpatialDisjoint);
        public const string SpatialElementAt = nameof(SpatialElementAt);
        public const string SpatialElementCount = nameof(SpatialElementCount);
        public const string SpatialEnvelope = nameof(SpatialEnvelope);
        public const string SpatialEquals = nameof(SpatialEquals);
        public const string SpatialIntersection = nameof(SpatialIntersection);
        public const string SpatialIntersects = nameof(SpatialIntersects);
        public const string SpatialLength = nameof(SpatialLength);
        public const string SpatialOverlaps = nameof(SpatialOverlaps);
        public const string SpatialRelate = nameof(SpatialRelate);
        public const string SpatialSymmetricDifference = nameof(SpatialSymmetricDifference);
        public const string SpatialTouches = nameof(SpatialTouches);
        public const string SpatialTypeName = nameof(SpatialTypeName);
        public const string SpatialUnion = nameof(SpatialUnion);
        public const string SpatialWithin = nameof(SpatialWithin);
        public const string StartPoint = nameof(StartPoint);
        public const string XCoordinate = nameof(XCoordinate);
        public const string YCooridnate = nameof(YCooridnate);
        #endregion

        #region Other
        public const string NewGuid = nameof(NewGuid);
        #endregion

        public static class LinqOperators
        {
            public const string Where = nameof(Where);
            public const string Select = nameof(Select);
            public const string SelectMany = nameof(SelectMany);
            public const string GroupBy = nameof(GroupBy);
            public const string Join = nameof(Join);
            public const string GroupJoin = nameof(GroupJoin);
            //public const string Aggregate = nameof(Aggregate);
            public const string All = nameof(All);
            public const string Any = nameof(Any);
            public const string Contains = nameof(Contains);
            public const string Concat = nameof(Concat);
            public const string DefaultIfEmpty = nameof(DefaultIfEmpty);
            public const string Distinct = nameof(Distinct);
            public const string Except = nameof(Except);
            public const string Intersect = nameof(Intersect);
            public const string Union = nameof(Union);
            public const string OrderBy = nameof(OrderBy);
            public const string OrderByDescending = nameof(OrderByDescending);
            public const string ThenBy = nameof(ThenBy);
            public const string ThenByDescending = nameof(ThenByDescending);
            //public const string Reverse = nameof(Reverse);
            public const string Average = nameof(Average);
            public const string Count = nameof(Count);
            public const string LongCount = nameof(LongCount);
            public const string Max = nameof(Max);
            public const string Min = nameof(Min);
            public const string Sum = nameof(Sum);
            public const string Cast = nameof(Cast);
            public const string OfType = nameof(OfType);
            //public const string ElementAt = nameof(ElementAt);
            //public const string ElementAtOrDefault = nameof(ElementAtOrDefault);
            public const string First = nameof(First);
            public const string FirstOrDefault = nameof(FirstOrDefault);
            //public const string Last = nameof(Last);
            //public const string LastOrDefault = nameof(LastOrDefault);
            public const string Single = nameof(Single);
            public const string SingleOrDefault = nameof(SingleOrDefault);
            public const string Skip = nameof(Skip);
            //public const string SkipWhile = nameof(SkipWhile);
            public const string Take = nameof(Take);
            //public const string TakeWhile = nameof(TakeWhile);
        }

        static void RegisterKnownType(string name)
        {
            _types[name] = name;
        }

        static IEnumerable<MethodArgument> ProcessArguments(string[] args)
        {
            for (int i = 0; i < args.Length; i += 2)
            {
                string type = args[i];
                string name = args[i + 1];
                yield return new MethodArgument(name, type);
            }
        }

        static PropertyInfo RegisterProperty(string invokeType, string name, string returnType, bool isStatic)
        {
            if (!_properties.ContainsKey(name))
                _properties[name] = new PropertyInfo(name);

            _properties[name].AddSignature(new PropertySignature(invokeType, returnType, isStatic));

            return _properties[name];
        }
        
        /// <summary>
        /// Registers a CLR method that has a known mapping to a canonical Entity Framework function
        /// 
        /// <see cref="https://msdn.microsoft.com/en-us/library/bb738681.aspx"/>
        /// </summary>
        /// <param name="invokeType"></param>
        /// <param name="name"></param>
        /// <param name="returnType"></param>
        /// <param name="isStatic"></param>
        /// <param name="arguments"></param>
        /// <returns></returns>
        static MethodInfo RegisterMethod(string invokeType, string name, string returnType, bool isStatic, params string[] arguments)
        {
            if (!_methods.ContainsKey(name))
                _methods[name] = MethodInfo.Stub(name);

            var signature = new MethodSignature(invokeType, returnType, isStatic, ProcessArguments(arguments));
            _methods[name].AddSignature(signature);

            return _methods[name];
        }

        static MethodInfo RegisterMethod(string name, bool isLinqOperator = false) => RegisterMethod(name, MethodInfo.Stub(name), isLinqOperator);

        static MethodInfo RegisterMethod(string name, MethodInfo info, bool isLinqOperator = false)
        {
            if (!_methods.ContainsKey(name))
                _methods[name] = info;
            else if (!info.IsStub)
                _methods[name].AddSignatures(info.Signatures);

            if (isLinqOperator)
                _linqOperators[name] = _methods[name];

            return _methods[name];
        }

        static CanonicalMethodNames()
        {
            _types = new Dictionary<string, string>();
            _methods = new Dictionary<string, MethodInfo>();
            _linqOperators = new Dictionary<string, MethodInfo>();
            _properties = new Dictionary<string, PropertyInfo>();

            //= operator
            RegisterMethod("DateTime", "Equals", "Boolean", true, "DateTime", "t1", "DateTime", "t2");
            RegisterMethod("DateTime", "Equals", "Boolean", false, "DateTime", "value");
            RegisterMethod("String", "Equals", "Boolean", false, "String", "value");
            RegisterMethod("String", "Equals", "Boolean", true, "String", "a", "String", "b");

            //Abs
            foreach (string type in new string[] { "Int16", "Int32", "Int64", "Byte", "Single", "Double", "Decimal" })
            {
                RegisterMethod("Math", Abs, type, true, type, "value");
            }

            //Ceiling
            foreach (string type in new string[] { "Double", "Decimal" })
            {
                RegisterMethod("Math", Ceiling, type, true, type, "value");
            }
            RegisterMethod("Decimal", Ceiling, "Decimal", true, "Decimal", "value");

            //Floor
            foreach (string type in new string[] { "Double", "Decimal" })
            {
                RegisterMethod("Math", Floor, type, true, type, "value");
            }
            RegisterMethod("Decimal", Floor, "Decimal", true, "Decimal", "value");

            //Round
            RegisterMethod("Math", Round, "Double", true, "Double", "value", "Int16", "digits");
            RegisterMethod("Math", Round, "Double", true, "Double", "value", "Int32", "digits");
            RegisterMethod("Math", Round, "Decimal", true, "Decimal", "value", "Int16", "digits");
            RegisterMethod("Math", Round, "Decimal", true, "Decimal", "value", "Int32", "digits");

            //Truncate
            RegisterMethod("Math", Truncate, "Double", true, "Double", "value");
            RegisterMethod("Math", Truncate, "Decimal", true, "Decimal", "value");
            //WTF: Such overloads do not exist, but according to MSDN they do
            RegisterMethod("Math", Truncate, "Double", true, "Double", "value", "Int16", "digits");
            RegisterMethod("Math", Truncate, "Double", true, "Double", "value", "Int32", "digits");
            RegisterMethod("Math", Truncate, "Decimal", true, "Decimal", "value", "Int16", "digits");
            RegisterMethod("Math", Truncate, "Decimal", true, "Decimal", "value", "Int32", "digits");

            //Power
            //WTF: Such overloads do not exist, but according to MSDN they do
            RegisterMethod("Math", "Pow", "Double", true, "Int32", "value", "Int64", "exponent");
            RegisterMethod("Math", "Pow", "Double", true, "Int32", "value", "Double", "exponent");
            RegisterMethod("Math", "Pow", "Double", true, "Int32", "value", "Decimal", "exponent");
            RegisterMethod("Math", "Pow", "Double", true, "Int64", "value", "Int64", "exponent");
            RegisterMethod("Math", "Pow", "Double", true, "Int64", "value", "Double", "exponent");
            RegisterMethod("Math", "Pow", "Double", true, "Int64", "value", "Decimal", "exponent");
            RegisterMethod("Math", "Pow", "Double", true, "Double", "value", "Int64", "exponent");
            RegisterMethod("Math", "Pow", "Double", true, "Double", "value", "Double", "exponent");
            RegisterMethod("Math", "Pow", "Double", true, "Double", "value", "Decimal", "exponent");
            RegisterMethod("Math", "Pow", "Double", true, "Decimal", "value", "Int64", "exponent");
            RegisterMethod("Math", "Pow", "Double", true, "Decimal", "value", "Double", "exponent");
            RegisterMethod("Math", "Pow", "Double", true, "Decimal", "value", "Decimal", "exponent");

            //Year
            RegisterProperty("DateTime", "Year", "Int32", false);
            RegisterProperty("DateTimeOffset", "Year", "Int32", false);
            
            //Month
            RegisterProperty("DateTime", "Month", "Int32", false);
            RegisterProperty("DateTimeOffset", "Month", "Int32", false);

            //Day
            RegisterProperty("DateTime", "Day", "Int32", false);
            RegisterProperty("DateTimeOffset", "Day", "Int32", false);
            RegisterProperty("DateTimeOffset", "Day", "Int32", false);

            //Hour
            RegisterProperty("DateTime", "Hour", "Int32", false);
            RegisterProperty("TimeSpan", "Hours", "Int32", false);
            RegisterProperty("DateTimeOffset", "Hour", "Int32", false);

            //Minute
            RegisterProperty("DateTime", "Minute", "Int32", false);
            RegisterProperty("TimeSpan", "Minutes", "Int32", false);
            RegisterProperty("DateTimeOffset", "Minute", "Int32", false);

            //Second
            RegisterProperty("DateTime", "Second", "Int32", false);
            RegisterProperty("TimeSpan", "Seconds", "Int32", false);
            RegisterProperty("DateTimeOffset", "Second", "Int32", false);

            //Milliseconds
            RegisterProperty("TimeSpan", "Milliseconds", "Int32", false);
            RegisterProperty("DateTimeOffset", "Millisecond", "Int32", false);

            //CurrentDateTimeOffset
            RegisterMethod("DateTimeOffset", "Now", "DateTimeOffset", true);

            //CurrentDateTime
            RegisterProperty("DateTime", "Now", "DateTime", true);

            //CurrentUtcDateTime
            RegisterProperty("DateTime", "UtcNow", "DateTime", true);

            //LTrim
            RegisterMethod("String", "TrimStart", "String", false);

            //RTrim
            RegisterMethod("String", "TrimEnd", "String", false);

            //Trim
            RegisterMethod("String", "Trim", "String", false);

            //ToUpper
            RegisterMethod("String", "ToUpper", "String", false);

            //ToLower
            RegisterMethod("String", "ToLower", "String", false);

            //Substring
            RegisterMethod("String", "Substring", "String", false, "Int32", "startIndex", "Int32", "length");
            RegisterMethod("String", "Substring", "String", false, "Int32", "startIndex");
            RegisterMethod("String", "Remove", "String", false, "Int32", "startIndex");

            //Replace
            RegisterMethod("String", "Replace", "String", false, "String", "oldValue", "String", "newValue");

            //Concat
            RegisterMethod("String", "Remove", "String", false, "Int32", "startIndex", "Int32", "count");
            RegisterMethod("String", "Insert", "String", false, "Int32", "startIndex", "String", "value");
            RegisterMethod("String", "Concat", "String", true, "String", "str0", "String", "str1");
            RegisterMethod("String", "Concat", "String", true, "String", "str0", "String", "str1", "String", "str2");
            RegisterMethod("String", "Concat", "String", true, "String", "str0", "String", "str1", "String", "str2", "String", "str3");

            //IndexOf
            RegisterMethod("String", "IndexOf", "Int32", false, "String", "value");

            //Length
            RegisterProperty("String", "Length", "Int32", false);

            //LIKE operators
            RegisterMethod("String", "StartsWith", "Boolean", false, "String", "value");
            RegisterMethod("String", "EndsWith", "Boolean", false, "String", "value");
            RegisterMethod("String", "Contains", "Boolean", false, "String", "value");

            //IsNullOrEmpty
            RegisterMethod("String", "IsNullOrEmpty", "Boolean", true, "String", "value");

            //NewGuid
            RegisterMethod("Guid", "NewGuid", "Guid", true);

            //LINQ operators
            RegisterMethod(LinqOperators.All, true);
            RegisterMethod(LinqOperators.Any, true);
            RegisterMethod(LinqOperators.Average, true);
            RegisterMethod(LinqOperators.Cast, true);
            RegisterMethod(LinqOperators.Concat, true);
            RegisterMethod(LinqOperators.Contains, true);
            RegisterMethod(LinqOperators.Count, true);
            RegisterMethod(LinqOperators.DefaultIfEmpty, true);
            RegisterMethod(LinqOperators.Distinct, true);
            RegisterMethod(LinqOperators.Except, true);
            RegisterMethod(LinqOperators.First, true);
            RegisterMethod(LinqOperators.FirstOrDefault, true);
            RegisterMethod(LinqOperators.GroupBy, true);
            RegisterMethod(LinqOperators.GroupJoin, true);
            RegisterMethod(LinqOperators.Intersect, true);
            RegisterMethod(LinqOperators.Join, true);
            RegisterMethod(LinqOperators.LongCount, true);
            RegisterMethod(LinqOperators.Max, true);
            RegisterMethod(LinqOperators.Min, true);
            RegisterMethod(LinqOperators.OfType, true);
            RegisterMethod(LinqOperators.OrderBy, true);
            RegisterMethod(LinqOperators.OrderByDescending, true);
            RegisterMethod(LinqOperators.Select, true);
            RegisterMethod(LinqOperators.SelectMany, true);
            RegisterMethod(LinqOperators.Single, true);
            RegisterMethod(LinqOperators.SingleOrDefault, true);
            RegisterMethod(LinqOperators.Skip, true);
            RegisterMethod(LinqOperators.Sum, true);
            RegisterMethod(LinqOperators.Take, true);
            RegisterMethod(LinqOperators.ThenBy, true);
            RegisterMethod(LinqOperators.ThenByDescending, true);
            RegisterMethod(LinqOperators.Union, true);
            RegisterMethod(LinqOperators.Where, true);
            
            //Known Types
            RegisterKnownType(/* System.Data.Spatial */ "DbGeometry");
            RegisterKnownType(/* System.Data.Spatial */ "DbGeography");
            RegisterKnownType(/* System.Data.Objects.SqlClient */ "SqlFunctions");
            RegisterKnownType(/* System.Data.Entity */ "DbFunctions");
        }

        private static Dictionary<string, MethodInfo> _linqOperators;
        private static Dictionary<string, PropertyInfo> _properties;

        internal static bool IsLinqOperator(string methodName) => _linqOperators.ContainsKey(methodName);

        static bool IsSupportedType(string name) => _types.ContainsKey(name);

        internal static bool IsKnownMethod(IdentifierNameSyntax identExpr)
        {
            string methodName = identExpr.Identifier.ValueText;

            if (_methods.ContainsKey(methodName))
            {
                var mi = _methods[methodName];
                if (mi.IsStub)
                    return true;

                //TODO: Based on the given syntax nodes, validate it against the known signatures and/or
                //the allowable types
                return true;
            }
            return false;
        }

        internal static bool IsKnownMethod(InvocationExpressionSyntax node, MemberAccessExpressionSyntax memberExpr, EFCodeFirstClassInfo rootQueryableType, EFUsageContext efContext, SyntaxNodeAnalysisContext context)
        {
            var member = memberExpr.Expression as MemberAccessExpressionSyntax;
            var identifier = memberExpr.Expression as IdentifierNameSyntax;
            if (identifier != null)
            {
                //If it's a supported type, then any static method under it (predicated by identifier 
                //not being null, which hints at a static call. ie. If we get here, this is a static method
                //call) is considered supported
                if (IsSupportedType(identifier.Identifier.ValueText))
                    return true;
            }

            string methodName = memberExpr?.Name?.Identifier.ValueText;

            if (_methods.ContainsKey(methodName))
            {
                var mi = _methods[methodName];
                if (mi.IsStub)
                    return true;
                
                //TODO: Based on the given syntax nodes, validate it against the known signatures and/or
                //the allowable types
                return true;
            }

            //The method is an instance method on some member
            if (member != null)
            {
                var si = context.SemanticModel.GetSymbolInfo(member.Name);
                //Is this on a property member?
                var pts = si.Symbol as IPropertySymbol;
                if (pts != null)
                {
                    //Of a type that we know is an EF class?
                    var cls = efContext.GetClassInfo(pts.ContainingType);
                    if (cls != null)
                    {
                        //Is the property of a type that is EF whole heartedly
                        //supports?
                        if (IsSupportedType(pts.Type.MetadataName))
                            return true;
                    }
                }
            }

            //Last chance, does this method have a special [DbFunction] attribute that
            //tells EF that there will be a DB server-side equivalent function?
            var symInfo = context.SemanticModel.GetSymbolInfo(memberExpr.Name);
            var miSym = symInfo.Symbol as IMethodSymbol;
            if (miSym != null)
            {
                if (miSym.GetAttributes().Any(a => a.AttributeClass.MetadataName == "DbFunctionAttribute"))
                    return true;
            }

            return false;
        }
    }
}
