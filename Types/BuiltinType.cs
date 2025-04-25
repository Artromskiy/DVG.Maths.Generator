﻿using GLSHGenerator.Members;
using System;
using System.Collections.Generic;

namespace GLSHGenerator.Types
{
    internal class BuiltinType : AbstractType
    {
        public static IEnumerable<BuiltinType> BaseTypes
        {
            get
            {
                yield return TypeInt;
                yield return TypeUint;
                yield return TypeFloat;
                if (GenerateHalfs)
                    yield return TypeHalf;
                yield return TypeDouble;
                if (GenerateDecimals)
                    yield return TypeDecimal;
                //yield return TypeComplex;
                if (GenerateLongs)
                    yield return TypeLong;
                yield return TypeBool;
                //yield return TypeGeneric;
            }
        }

        public static List<KeyValuePair<BuiltinType, BuiltinType>> Upcasts
        {
            get
            {
                // from -> to
                var dic = new List<KeyValuePair<BuiltinType, BuiltinType>>
                {
                    new KeyValuePair<BuiltinType, BuiltinType>(TypeInt, TypeUint),
                    new KeyValuePair<BuiltinType, BuiltinType>(TypeInt, TypeFloat),
                    new KeyValuePair<BuiltinType, BuiltinType>(TypeInt, TypeDouble),

                    new KeyValuePair<BuiltinType, BuiltinType>(TypeUint, TypeFloat),
                    new KeyValuePair<BuiltinType, BuiltinType>(TypeUint, TypeDouble),

                    new KeyValuePair<BuiltinType, BuiltinType>(TypeFloat, TypeDouble),

                };
                if (GenerateLongs)
                {
                    dic.Add(new KeyValuePair<BuiltinType, BuiltinType>(TypeInt, TypeLong));
                    dic.Add(new KeyValuePair<BuiltinType, BuiltinType>(TypeUint, TypeLong));
                }
                if (GenerateHalfs)
                {
                    dic.Add(new KeyValuePair<BuiltinType, BuiltinType>(TypeInt, TypeHalf));
                    dic.Add(new KeyValuePair<BuiltinType, BuiltinType>(TypeUint, TypeHalf));
                    dic.Add(new KeyValuePair<BuiltinType, BuiltinType>(TypeHalf, TypeFloat));
                    dic.Add(new KeyValuePair<BuiltinType, BuiltinType>(TypeHalf, TypeDouble));
                }
                if (GenerateDecimals)
                {
                    dic.Add(new KeyValuePair<BuiltinType, BuiltinType>(TypeInt, TypeDecimal));
                    dic.Add(new KeyValuePair<BuiltinType, BuiltinType>(TypeUint, TypeDecimal));
                }
                if (GenerateDecimals && GenerateLongs)
                    dic.Add(new KeyValuePair<BuiltinType, BuiltinType>(TypeLong, TypeDecimal));
                return dic;
            }
        }

        public static readonly BuiltinType TypeInt = new BuiltinType
        {
            TypeName = "int",
            Prefix = "i",
            IsInteger = true,
            TypeConstants = new[] { "MaxValue", "MinValue" }
        };
        public static readonly BuiltinType TypeUint = new BuiltinType
        {
            TypeName = "uint",
            Prefix = "u",
            OneValueConstant = "1u",
            ZeroValueConstant = "0u",
            IsSigned = false,
            IsInteger = true,
            TypeConstants = new[] { "MaxValue", "MinValue" }
        };
        public static readonly BuiltinType TypeHalf = new BuiltinType
        {
            TypeName = "Half",
            Prefix = "h",
            OneValueConstant = "Half.One",
            ZeroValueConstant = "Half.Zero",
            HasOwnFunctions = true,
            IsFloatingPoint = true,
            TypeConstants = new[] { "MaxValue", "MinValue", "Epsilon", "NaN", "NegativeInfinity", "PositiveInfinity" },
        };
        public static readonly BuiltinType TypeFloat = new BuiltinType
        {
            TypeName = "float",
            OneValueConstant = "1f",
            ZeroValueConstant = "0f",
            IsFloatingPoint = true,
            TypeConstants = new[] { "MaxValue", "MinValue", "Epsilon", "NaN", "NegativeInfinity", "PositiveInfinity" },
        };
        public static readonly BuiltinType TypeDouble = new BuiltinType
        {
            TypeName = "double",
            Prefix = "d",
            LengthType = "double",
            OneValueConstant = "1.0",
            ZeroValueConstant = "0.0",
            IsFloatingPoint = true,
            TypeConstants = new[] { "MaxValue", "MinValue", "Epsilon", "NaN", "NegativeInfinity", "PositiveInfinity" },
        };
        public static readonly BuiltinType TypeDecimal = new BuiltinType
        {
            TypeName = "decimal",
            Prefix = "dec",
            LengthType = "decimal",
            OneValueConstant = "1m",
            ZeroValueConstant = "0m",
            Decimal = true,
            IsFloatingPoint = true,
            EpsilonFormat = "double.Epsilon",
            TypeConstants = new[] { "MaxValue", "MinValue", "MinusOne" }
        };
        public static readonly BuiltinType TypeLong = new BuiltinType
        {
            TypeName = "long",
            Prefix = "l",
            LengthType = "double",
            IsInteger = true,
            TypeConstants = new[] { "MaxValue", "MinValue" }
        };
        public static readonly BuiltinType TypeBool = new BuiltinType
        {
            TypeName = "bool",
            Prefix = "b",
            HasArithmetics = false,
            HashCodeMultiplier = 2,
            OneValueConstant = "true",
            ZeroValueConstant = "false",
            HasFormatString = false,
            IsBool = true,
            IsSigned = false
        };

        public string TypeName { get; set; }
        public string Prefix { get; set; }
        public bool Decimal { get; set; }

        public bool HasArithmetics { get; set; } = true;
        public bool HasComparisons => HasArithmetics;
        public string LengthType { get; set; } = "float";
        public string AbsOverrideType { get; set; }
        public string AbsOverrideTypePrefix { get; set; }
        public bool IsSigned { get; set; } = true;
        public bool IsInteger { get; set; } = false;
        public bool IsFloatingPoint { get; set; }
        public bool HasOwnFunctions { get; set; }

        public string EpsilonFormat { get; set; } = "{0}.Epsilon";


        public string EqualFormat { get; set; } = "{0} == {1}";
        public string NotEqualFormat { get; set; } = "{0} != {1}";

        public bool HasFormatString { get; set; } = false;

        public bool IsBool { get; set; }

        public string OneValueConstant { get; set; } = "1";
        public string ZeroValueConstant { get; set; } = "0";

        public override string OneValue => OneValueConstant;
        public override string ZeroValue => ZeroValueConstant;

        public int HashCodeMultiplier { get; set; } = 397;

        public override string Name => TypeName;

        public override string TypeComment => "Builtin " + Name;

        public override IEnumerable<Member> GenerateMembers()
        {
            yield break; // no-op
        }

        protected override IEnumerable<string> Body
        {
            get { throw new InvalidOperationException("No body for builtin types"); }
        }

        public string[] TypeConstants { get; set; } = new string[] { };
    }
}
