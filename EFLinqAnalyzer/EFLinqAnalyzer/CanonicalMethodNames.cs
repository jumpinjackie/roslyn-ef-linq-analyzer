using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace EFLinqAnalyzer
{
    /// <summary>
    /// The names of all canonical methods that are supported by Entity Framework
    /// 
    /// <see cref="https://msdn.microsoft.com/en-us/library/bb738626.aspx"/>
    /// </summary>
    public class CanonicalMethodNames
    {
        class MethodInfo
        {
            public List<MethodSignature> Signatures { get; }

            public string Name { get; }
            
            public MethodInfo(string name, IEnumerable<MethodSignature> sigs)
            {
                this.Name = name;
                this.Signatures = new List<MethodSignature>(sigs);
                this.IsStub = false;
            }

            /// <summary>
            /// Indicates that this is a stub definition. Only name validation is performed
            /// </summary>
            public bool IsStub { get; private set; }
            
            /// <summary>
            /// Creates a name-only method definition stub
            /// </summary>
            /// <param name="name"></param>
            /// <returns></returns>
            internal static MethodInfo Stub(string name)
            {
                return new MethodInfo(name, Enumerable.Empty<MethodSignature>())
                {
                    IsStub = true
                };
            }
        }

        class MethodSignature
        {
            public string ReturnType { get; }

            public List<MethodArgument> Arguments { get; }

            public MethodSignature(IEnumerable<MethodArgument> args)
                : this("void", args)
            { }
            
            public MethodSignature(string returnType, IEnumerable<MethodArgument> args)
            {
                this.ReturnType = returnType;
                this.Arguments = new List<MethodArgument>(args);
            }
        }

        class MethodArgument
        {
            public string Name { get; }

            public ICollection<string> AllowedTypes { get; }

            public MethodArgument(string name)
            {
                this.Name = name;
                this.AllowedTypes = new HashSet<string>();
            }

            public MethodArgument(string name, string type)
                : this(name)
            {
                this.AllowedTypes.Add(type);
            }

            public MethodArgument(string name, IEnumerable<string> types)
                : this(name)
            {
                foreach (var t in types)
                {
                    this.AllowedTypes.Add(t);
                }
            }
        }

        static Dictionary<string, MethodInfo> _methods;

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
            public const string GroupBy = nameof(GroupBy);
            public const string Join = nameof(Join);
            public const string Aggregate = nameof(Aggregate);
        }

        static CanonicalMethodNames()
        {
            _methods = new Dictionary<string, MethodInfo>();

            #region Aggregate

            _methods[Avg] = new MethodInfo(Avg,  
                new string[] {
                    nameof(Int32), nameof(Int64), nameof(Double), nameof(Decimal)
                }.Select(type => new MethodSignature(type, new MethodArgument[]
                {
                    new MethodArgument("expression", type)
                })));

            _methods[BigCount] = new MethodInfo(BigCount, new MethodSignature[] {
                new MethodSignature(nameof(Int64), new MethodArgument[] {
                    new MethodArgument("expression", nameof(Object))
                })
            });

            _methods[Count] = new MethodInfo(Count, new MethodSignature[] {
                new MethodSignature(nameof(Int32), new MethodArgument[] {
                    new MethodArgument("expression", nameof(Object))
                })
            });

            _methods[Max] = new MethodInfo(Max,
                new string[] {
                    nameof(Byte), nameof(Int16), nameof(Int32), nameof(Int64),
                    nameof(Single), nameof(Double), nameof(Decimal), nameof(DateTime),
                    nameof(DateTimeOffset), /* nameof(Time), */ nameof(String) /*, nameof(Binary) */
                }.Select(type => new MethodSignature(type, new MethodArgument[]
                {
                    new MethodArgument("expression", type)
                })));

            _methods[Min] = new MethodInfo(Min,
                new string[] {
                    nameof(Byte), nameof(Int16), nameof(Int32), nameof(Int64),
                    nameof(Single), nameof(Double), nameof(Decimal), nameof(DateTime),
                    nameof(DateTimeOffset), /* nameof(Time), */ nameof(String) /*, nameof(Binary) */
                }.Select(type => new MethodSignature(type, new MethodArgument[]
                {
                    new MethodArgument("expression", type)
                })));

            _methods[StDev] = new MethodInfo(StDev,
                new string[] {
                    nameof(Int32), nameof(Int64), nameof(Double), nameof(Decimal)
                }.Select(type => new MethodSignature(nameof(Double), new MethodArgument[]
                {
                    new MethodArgument("expression", type)
                })));

            _methods[StDevP] = new MethodInfo(StDevP,
                new string[] {
                    nameof(Int32), nameof(Int64), nameof(Double), nameof(Decimal)
                }.Select(type => new MethodSignature(nameof(Double), new MethodArgument[]
                {
                    new MethodArgument("expression", type)
                })));

            _methods[Sum] = new MethodInfo(Sum,
                new string[] {
                    nameof(Int32), nameof(Int64), nameof(Double), nameof(Decimal)
                }.Select(type => new MethodSignature(nameof(Double), new MethodArgument[]
                {
                    new MethodArgument("expression", type)
                })));

            _methods[Var] = new MethodInfo(Var,
                new string[] {
                    nameof(Int32), nameof(Int64), nameof(Double), nameof(Decimal)
                }.Select(type => new MethodSignature(nameof(Double), new MethodArgument[]
                {
                    new MethodArgument("expression", type)
                })));

            _methods[VarP] = new MethodInfo(VarP,
                new string[] {
                    nameof(Int32), nameof(Int64), nameof(Double), nameof(Decimal)
                }.Select(type => new MethodSignature(nameof(Double), new MethodArgument[]
                {
                    new MethodArgument("expression", type)
                })));
            #endregion

            #region Math

            _methods[Abs] = new MethodInfo(Abs,
                new string[] {
                    nameof(Int16), nameof(Int32), nameof(Int64), nameof(Double),
                    nameof(Decimal), nameof(Byte), nameof(Single)
                }.Select(type => new MethodSignature(type, new MethodArgument[]
                {
                    new MethodArgument("value", type)
                })));

            _methods[Ceiling] = new MethodInfo(Ceiling,
                new string[] {
                    nameof(Single), nameof(Double), nameof(Decimal)
                }.Select(type => new MethodSignature(type, new MethodArgument[]
                {
                    new MethodArgument("value", type)
                })));

            _methods[Floor] = new MethodInfo(Floor,
                new string[] {
                    nameof(Single), nameof(Double), nameof(Decimal)
                }.Select(type => new MethodSignature(type, new MethodArgument[]
                {
                    new MethodArgument("value", type)
                })));

            _methods[Power] = new MethodInfo(Power,
                new string[] {
                    nameof(Single), nameof(Double), nameof(Decimal)
                }.Select(type => new MethodSignature(type, new MethodArgument[]
                {
                    new MethodArgument("value", type)
                })));

            _methods[Round] = MethodInfo.Stub(Round);
            _methods[Truncate] = MethodInfo.Stub(Truncate);

            #endregion

            #region String

            _methods[Concat] = MethodInfo.Stub(Concat);
            _methods[Contains] = MethodInfo.Stub(Contains);
            _methods[EndsWith] = MethodInfo.Stub(EndsWith);
            _methods[IndexOf] = MethodInfo.Stub(IndexOf);
            _methods[Left] = MethodInfo.Stub(Left);
            _methods[Length] = MethodInfo.Stub(Length);
            _methods[LTrim] = MethodInfo.Stub(LTrim);
            _methods[Replace] = MethodInfo.Stub(Replace);
            _methods[Reverse] = MethodInfo.Stub(Reverse);
            _methods[Right] = MethodInfo.Stub(Right);
            _methods[RTrim] = MethodInfo.Stub(RTrim);
            _methods[Substring] = MethodInfo.Stub(Substring);
            _methods[StartsWith] = MethodInfo.Stub(StartsWith);
            _methods[ToLower] = MethodInfo.Stub(ToLower);
            _methods[ToUpper] = MethodInfo.Stub(ToUpper);
            _methods[Trim] = MethodInfo.Stub(Trim);

            #endregion

            #region Date and Time

            _methods[AddNanoseconds] = MethodInfo.Stub(AddNanoseconds);
            _methods[AddMicroseconds] = MethodInfo.Stub(AddMicroseconds);
            _methods[AddMilliseconds] = MethodInfo.Stub(AddMilliseconds);
            _methods[AddSeconds] = MethodInfo.Stub(AddSeconds);
            _methods[AddMinutes] = MethodInfo.Stub(AddMinutes);
            _methods[AddHours] = MethodInfo.Stub(AddHours);
            _methods[AddDays] = MethodInfo.Stub(AddDays);
            _methods[AddMonths] = MethodInfo.Stub(AddMonths);
            _methods[AddYears] = MethodInfo.Stub(AddYears);
            _methods[CreateDateTime] = MethodInfo.Stub(CreateDateTime);
            _methods[CreateDateTimeOffset] = MethodInfo.Stub(CreateDateTimeOffset);
            _methods[CreateTime] = MethodInfo.Stub(CreateTime);
            _methods[Day] = MethodInfo.Stub(Day);
            _methods[DayOfYear] = MethodInfo.Stub(DayOfYear);
            _methods[DiffNanoseconds] = MethodInfo.Stub(DiffNanoseconds);
            _methods[DiffMicroseconds] = MethodInfo.Stub(DiffMicroseconds);
            _methods[DiffMilliseconds] = MethodInfo.Stub(DiffMilliseconds);
            _methods[DiffSeconds] = MethodInfo.Stub(DiffSeconds);
            _methods[DiffMinutes] = MethodInfo.Stub(DiffMinutes);
            _methods[DiffHours] = MethodInfo.Stub(DiffHours);
            _methods[DiffDays] = MethodInfo.Stub(DiffDays);
            _methods[DiffMonths] = MethodInfo.Stub(DiffMonths);
            _methods[DiffYears] = MethodInfo.Stub(DiffYears);
            _methods[GetTotalOffsetMinutes] = MethodInfo.Stub(GetTotalOffsetMinutes);
            _methods[Hour] = MethodInfo.Stub(Hour);
            _methods[Millisecond] = MethodInfo.Stub(Millisecond);
            _methods[Minute] = MethodInfo.Stub(Minute);
            _methods[Month] = MethodInfo.Stub(Month);
            _methods[Second] = MethodInfo.Stub(Second);
            _methods[TruncateTime] = MethodInfo.Stub(TruncateTime);
            _methods[Truncate] = MethodInfo.Stub(Truncate);
            _methods[Year] = MethodInfo.Stub(Year);
            #endregion

            #region Bitwise
            _methods[BitWiseAnd] = MethodInfo.Stub(BitWiseAnd);
            _methods[BitWiseNot] = MethodInfo.Stub(BitWiseNot);
            _methods[BitWiseOr] = MethodInfo.Stub(BitWiseOr);
            _methods[BitWiseXor] = MethodInfo.Stub(BitWiseXor);
            #endregion

            #region Spatial
            _methods[Area] = MethodInfo.Stub(Area);
            _methods[AsBinary] = MethodInfo.Stub(AsBinary);
            _methods[AsGml] = MethodInfo.Stub(AsGml);
            _methods[AsText] = MethodInfo.Stub(AsText);
            _methods[Centroid] = MethodInfo.Stub(Centroid);
            _methods[CoordinateSystemId] = MethodInfo.Stub(CoordinateSystemId);
            _methods[Distance] = MethodInfo.Stub(Distance);
            _methods[Elevation] = MethodInfo.Stub(Elevation);
            _methods[EndPoint] = MethodInfo.Stub(EndPoint);
            _methods[ExteriorRing] = MethodInfo.Stub(ExteriorRing);
            _methods[GeographyCollectionFromBinary] = MethodInfo.Stub(GeographyCollectionFromBinary);
            _methods[GeographyCollectionFromText] = MethodInfo.Stub(GeographyCollectionFromText);
            _methods[GeographyFromBinary] = MethodInfo.Stub(GeographyFromBinary);
            _methods[GeographyFromGml] = MethodInfo.Stub(GeographyFromGml);
            _methods[GeographyFromText] = MethodInfo.Stub(GeographyFromText);
            _methods[GeographyLineFromBinary] = MethodInfo.Stub(GeographyLineFromBinary);
            _methods[GeographyLineFromText] = MethodInfo.Stub(GeographyLineFromText);
            _methods[GeographyMultiLineFromBinary] = MethodInfo.Stub(GeographyMultiLineFromBinary);
            _methods[GeographyMultiLineFromText] = MethodInfo.Stub(GeographyMultiLineFromText);
            _methods[GeographyMultiPointFromBinary] = MethodInfo.Stub(GeographyMultiPointFromBinary);
            _methods[GeographyMultiPointFromText] = MethodInfo.Stub(GeographyMultiPointFromText);
            _methods[GeographyMultiPolygonFromBinary] = MethodInfo.Stub(GeographyMultiPolygonFromBinary);
            _methods[GeographyMultiPolygonFromText] = MethodInfo.Stub(GeographyMultiPolygonFromText);
            _methods[GeographyPointFromBinary] = MethodInfo.Stub(GeographyPointFromBinary);
            _methods[GeographyPointFromText] = MethodInfo.Stub(GeographyPointFromText);
            _methods[GeographyPolygonFromBinary] = MethodInfo.Stub(GeographyPolygonFromBinary);
            _methods[GeographyPolygonFromText] = MethodInfo.Stub(GeographyPolygonFromText);
            _methods[GeometryCollectionFromBinary] = MethodInfo.Stub(GeometryCollectionFromBinary);
            _methods[GeometryCollectionFromText] = MethodInfo.Stub(GeometryCollectionFromText);
            _methods[GeometryFromBinary] = MethodInfo.Stub(GeometryFromBinary);
            _methods[GeometryFromGml] = MethodInfo.Stub(GeometryFromGml);
            _methods[GeometryFromText] = MethodInfo.Stub(GeometryFromText);
            _methods[GeometryLineFromBinary] = MethodInfo.Stub(GeometryLineFromBinary);
            _methods[GeometryLineFromText] = MethodInfo.Stub(GeometryLineFromText);
            _methods[GeometryMultiLineFromBinary] = MethodInfo.Stub(GeometryMultiLineFromBinary);
            _methods[GeometryMultiLineFromText] = MethodInfo.Stub(GeometryMultiLineFromText);
            _methods[GeometryMultiPointFromBinary] = MethodInfo.Stub(GeometryMultiPointFromBinary);
            _methods[GeometryMultiPointFromText] = MethodInfo.Stub(GeometryMultiPointFromText);
            _methods[GeometryMultiPolygonFromBinary] = MethodInfo.Stub(GeometryMultiPolygonFromBinary);
            _methods[GeometryMultiPolygonFromText] = MethodInfo.Stub(GeometryMultiPolygonFromText);
            _methods[GeometryPointFromBinary] = MethodInfo.Stub(GeometryPointFromBinary);
            _methods[GeometryPointFromText] = MethodInfo.Stub(GeometryPointFromText);
            _methods[GeometryPolygonFromBinary] = MethodInfo.Stub(GeometryPolygonFromBinary);
            _methods[GeometryPolygonFromText] = MethodInfo.Stub(GeometryPolygonFromText);
            _methods[InteriorRingAt] = MethodInfo.Stub(InteriorRingAt);
            _methods[InteriorRingCount] = MethodInfo.Stub(InteriorRingCount);
            _methods[IsClosedSpatial] = MethodInfo.Stub(IsClosedSpatial);
            _methods[IsEmptySpatial] = MethodInfo.Stub(IsEmptySpatial);
            _methods[IsRing] = MethodInfo.Stub(IsRing);
            _methods[IsSimpleGeometry] = MethodInfo.Stub(IsSimpleGeometry);
            _methods[IsValidGeometry] = MethodInfo.Stub(IsValidGeometry);
            _methods[Latitude] = MethodInfo.Stub(Latitude);
            _methods[Longitude] = MethodInfo.Stub(Longitude);
            _methods[Measure] = MethodInfo.Stub(Measure);
            _methods[PointAt] = MethodInfo.Stub(PointAt);
            _methods[PointCount] = MethodInfo.Stub(PointCount);
            _methods[PointOnSurface] = MethodInfo.Stub(PointOnSurface);
            _methods[SpatialBoundary] = MethodInfo.Stub(SpatialBoundary);
            _methods[SpatialBuffer] = MethodInfo.Stub(SpatialBuffer);
            _methods[SpatialContains] = MethodInfo.Stub(SpatialContains);
            _methods[SpatialConvexHull] = MethodInfo.Stub(SpatialConvexHull);
            _methods[SpatialCrosses] = MethodInfo.Stub(SpatialCrosses);
            _methods[SpatialDifference] = MethodInfo.Stub(SpatialDifference);
            _methods[SpatialDimension] = MethodInfo.Stub(SpatialDimension);
            _methods[SpatialDisjoint] = MethodInfo.Stub(SpatialDisjoint);
            _methods[SpatialElementAt] = MethodInfo.Stub(SpatialElementAt);
            _methods[SpatialElementCount] = MethodInfo.Stub(SpatialElementCount);
            _methods[SpatialEnvelope] = MethodInfo.Stub(SpatialEnvelope);
            _methods[SpatialEquals] = MethodInfo.Stub(SpatialEquals);
            _methods[SpatialIntersection] = MethodInfo.Stub(SpatialIntersection);
            _methods[SpatialIntersects] = MethodInfo.Stub(SpatialIntersects);
            _methods[SpatialLength] = MethodInfo.Stub(SpatialLength);
            _methods[SpatialOverlaps] = MethodInfo.Stub(SpatialOverlaps);
            _methods[SpatialRelate] = MethodInfo.Stub(SpatialRelate);
            _methods[SpatialSymmetricDifference] = MethodInfo.Stub(SpatialSymmetricDifference);
            _methods[SpatialTouches] = MethodInfo.Stub(SpatialTouches);
            _methods[SpatialTypeName] = MethodInfo.Stub(SpatialTypeName);
            _methods[SpatialUnion] = MethodInfo.Stub(SpatialUnion);
            _methods[SpatialWithin] = MethodInfo.Stub(SpatialWithin);
            _methods[StartPoint] = MethodInfo.Stub(StartPoint);
            _methods[XCoordinate] = MethodInfo.Stub(XCoordinate);
            _methods[YCooridnate] = MethodInfo.Stub(YCooridnate);
            #endregion

            #region Other
            _methods[NewGuid] = MethodInfo.Stub(NewGuid);
            #endregion

            #region LINQ operators
            _methods[LinqOperators.Where] = MethodInfo.Stub(LinqOperators.Where);
            _methods[LinqOperators.Select] = MethodInfo.Stub(LinqOperators.Select);
            _methods[LinqOperators.Aggregate] = MethodInfo.Stub(LinqOperators.Aggregate);
            _methods[LinqOperators.GroupBy] = MethodInfo.Stub(LinqOperators.GroupBy);
            _methods[LinqOperators.Join] = MethodInfo.Stub(LinqOperators.Join);

            _linqOperators = new Dictionary<string, MethodInfo>();

            _linqOperators[LinqOperators.Where] = _methods[LinqOperators.Where];
            _linqOperators[LinqOperators.Aggregate] = _methods[LinqOperators.Aggregate];
            _linqOperators[LinqOperators.GroupBy] = _methods[LinqOperators.GroupBy];
            _linqOperators[LinqOperators.Join] = _methods[LinqOperators.Join];
            _linqOperators[LinqOperators.Select] = _methods[LinqOperators.Select];
            #endregion
        }

        private static Dictionary<string, MethodInfo> _linqOperators;

        internal static bool IsLinqOperator(string methodName)
        {
            return _linqOperators.ContainsKey(methodName);
        }

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

        internal static bool IsKnownMethod(InvocationExpressionSyntax node, MemberAccessExpressionSyntax memberExpr)
        {
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
            return false;
        }
    }
}
