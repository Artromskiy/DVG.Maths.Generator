﻿using DVG.GLSH.Generator.Members;
using System.Collections.Generic;
using System.Linq;

namespace DVG.GLSH.Generator.Types
{
    internal abstract class AbstractType
    {
        public const bool GenerateHalfs = false;
        public const bool GenerateDecimals = false;
        public const bool GenerateLongs = false;
        public const bool GenerateQuaternions = false;
        public const bool GenerateMatrices = false;

        public const bool FullUnmanaged = true;

        public const bool SeparateUnmanagedAsExtensions = true;

        /// <summary>
        /// All known types
        /// </summary>
        public static readonly Dictionary<string, AbstractType> Types = new Dictionary<string, AbstractType>();

        /// <summary>
        /// Additional Attributes for type
        /// </summary>
        public virtual IEnumerable<string> Attributes => new string[]
        {
            "Serializable",
            "StructLayout(LayoutKind.Sequential)"
        };

        /// <summary>
        /// Name of corresponding type in GLSL
        /// </summary>
        public virtual string GlslName { get; }
        /// <summary>
        /// Name of the base type
        /// </summary>
        public string BaseTypeName => BaseType.Name;
        /// <summary>
        /// Cast to basetype
        /// </summary>
        public string BaseTypeCast => "(" + BaseTypeName + ")";
        /// <summary>
        /// Actual name of the type (e.g. the C# class name)
        /// </summary>
        public abstract string Name { get; }
        /// <summary>
        /// Reference to base type
        /// </summary>
        public BuiltinType BaseType { get; set; }

        /// <summary>
        /// Namespace of this type
        /// </summary>
        public static string Namespace { get; } = "DVG";

        /// <summary>
        /// Comment of this type
        /// </summary>
        public abstract string TypeComment { get; }

        /// <summary>
        /// List of C# base classes (mostly interfaces)
        /// </summary>
        public virtual IEnumerable<string> BaseClasses { get { yield break; } }

        /// <summary>
        /// All members
        /// </summary>
        public Member[] members;
        private Field[] fields;
        private Constructor[] constructors;
        private Property[] properties;
        private Property[] staticProperties;
        private ImplicitOperator[] implicitOperators;
        private ExplicitOperator[] explicitOperators;
        private Operator[] operators;
        private Function[] functions;
        private Function[] staticFunctions;
        private Indexer[] indexer;
        private ComponentWiseStaticFunction[] componentWiseStaticFunctions;
        private ComponentWiseOperator[] componentWiseOp;
        private Member[] glmMembers;
        private Function[] extensionFunctions;

        /// <summary>
        /// Generate all members
        /// </summary>
        public abstract IEnumerable<Member> GenerateMembers();

        /// <summary>
        /// Generates type members and sorts them
        /// </summary>
        public void Generate()
        {
            members = GenerateMembers().ToArray();

            //if (members.Any(m => string.IsNullOrEmpty(m.Comment)))
            //    throw new InvalidOperationException("Missing comment");

            string[] attributes = new string[] { "MethodImpl(MethodImplOptions.AggressiveInlining)" };

            foreach (var member in members)
                member.OriginalType = this;

            fields = members.OfType<Field>().ToArray();
            constructors = members.OfType<Constructor>().ForEach(e => e.Attributes = attributes).ToArray();
            properties = members.Where(m => !m.Static).OfType<Property>().ToArray();
            staticProperties = members.Where(m => m.Static).OfType<Property>().ToArray();
            implicitOperators = members.OfType<ImplicitOperator>().ForEach(e => e.Attributes = attributes).ToArray();
            explicitOperators = members.OfType<ExplicitOperator>().ForEach(e => e.Attributes = attributes).ToArray();
            operators = members.OfType<Operator>().ForEach(e => e.Attributes = attributes).ToArray();
            functions = members.Where(m => !m.Static && !m.Extension && m.GetType() == typeof(Function)).OfType<Function>().ForEach(e => e.Attributes = attributes).ToArray();
            staticFunctions = members.Where(m => m.Static && !m.Extension && m.GetType() == typeof(Function)).OfType<Function>().ForEach(e => e.Attributes = attributes).ToArray();
            indexer = members.OfType<Indexer>().ToArray();
            componentWiseStaticFunctions = members.OfType<ComponentWiseStaticFunction>().ForEach(e => e.Attributes = attributes).ToArray();
            componentWiseOp = members.OfType<ComponentWiseOperator>().ForEach(e => e.Attributes = attributes).ToArray();
            extensionFunctions = members.Where(m => m.Static && m.Extension && m.GetType() == typeof(Function)).OfType<Function>().ToArray();
            glmMembers = members.SelectMany(m => m.GlshMembers()).ForEach(e => e.Attributes = attributes).ToArray();
        }

        /// <summary>
        /// Constructs an object of a given type
        /// </summary>
        public string Construct(AbstractType type, IEnumerable<string> args) => $"new {type.Name}({args.CommaSeparated()})";
        /// <summary>
        /// Constructs an object of a given type
        /// </summary>
        public string Construct(AbstractType type, params string[] args) => $"new {type.Name}({args.CommaSeparated()})";


        public IEnumerable<string> GlmSharpFile
        {
            get
            {
                yield return "using System;";
                yield return "using System.Collections.Generic;";
                yield return "using System.Runtime.InteropServices;";
                yield return "using System.Numerics;";
                yield return "";
                yield return "";
                yield return "namespace " + Namespace;
                yield return "{";
                yield return "    /// <summary>";
                yield return "    /// Static class that contains static glsh functions";
                yield return "    /// </summary>";
                yield return "    public static partial class glsh";
                yield return "    {";
                foreach (var member in glmMembers)
                    foreach (var line in member.Lines)
                        yield return line.Indent(2);
                yield return "";
                yield return "    }";
                yield return "}";
            }
        }

        public IEnumerable<string> ExtCSharpFile
        {
            get
            {
                var baseclasses = BaseClasses.ToArray();
                yield return "using System;";
                yield return "using System.Runtime.InteropServices;";
                yield return "using System.Collections.Generic;";
                yield return "";
                yield return "";
                yield return "namespace " + Namespace + ".Extensions";
                yield return "{";
                foreach (var line in TypeComment.AsComment()) yield return line.Indent();
                yield return "    public static class " + Name + "Extensions";
                yield return "    {";


                if (extensionFunctions.Length > 0)
                {
                    yield return "";
                    yield return "        #region ExtensionFunctions";
                    foreach (var func in extensionFunctions)
                        foreach (var line in func.Lines)
                            yield return line.Indent(2);
                    yield return "";
                    yield return "        #endregion";
                    yield return "";
                }

                yield return "    }";
                yield return "}";
            }
        }

        public IEnumerable<string> CSharpFile
        {
            get
            {
                var baseclasses = BaseClasses.ToArray();
                yield return "#pragma warning disable IDE1006";
                yield return "using System;";
                yield return "using System.Runtime.InteropServices;";
                yield return "using System.Runtime.CompilerServices;";
                yield return "";
                yield return "";
                yield return "namespace " + Namespace;
                yield return "{";
                foreach (var line in TypeComment.AsComment()) yield return line.Indent();
                foreach (var item in Attributes)
                    yield return $"[{item}]".Indent();
                yield return "    public partial struct " + Name + (baseclasses.Length == 0 ? "" : " : " + baseclasses.CommaSeparated());
                yield return "    {";

                if (fields.Length > 0)
                {
                    yield return "";
                    yield return "        #region Fields";
                    foreach (var f in fields)
                        foreach (var line in f.Lines)
                            yield return line.Indent(2);
                    yield return "";
                    yield return "        #endregion";
                    yield return "";
                }

                if (constructors.Length > 0)
                {
                    yield return "";
                    yield return "        #region Constructors";
                    foreach (var ctor in constructors)
                        foreach (var line in ctor.Lines)
                            yield return line.Indent(2);
                    yield return "";
                    yield return "        #endregion";
                    yield return "";
                }

                if (implicitOperators.Length > 0)
                {
                    yield return "";
                    yield return "        #region Implicit Operators";
                    foreach (var op in implicitOperators)
                        foreach (var line in op.Lines)
                            yield return line.Indent(2);
                    yield return "";
                    yield return "        #endregion";
                    yield return "";
                }

                if (explicitOperators.Length > 0)
                {
                    yield return "";
                    yield return "        #region Explicit Operators";
                    foreach (var op in explicitOperators)
                        foreach (var line in op.Lines)
                            yield return line.Indent(2);
                    yield return "";
                    yield return "        #endregion";
                    yield return "";
                }

                if (indexer.Length > 0)
                {
                    yield return "";
                    yield return "        #region Indexer";
                    foreach (var index in indexer)
                        foreach (var line in index.Lines)
                            yield return line.Indent(2);
                    yield return "";
                    yield return "        #endregion";
                    yield return "";
                }

                if (properties.Length > 0)
                {
                    yield return "";
                    yield return "        #region Properties";
                    foreach (var prop in properties)
                        foreach (var line in prop.Lines)
                            yield return line.Indent(2);
                    yield return "";
                    yield return "        #endregion";
                    yield return "";
                }

                if (staticProperties.Length > 0)
                {
                    yield return "";
                    yield return "        #region Static Properties";
                    foreach (var prop in staticProperties)
                        foreach (var line in prop.Lines)
                            yield return line.Indent(2);
                    yield return "";
                    yield return "        #endregion";
                    yield return "";
                }

                if (operators.Length > 0)
                {
                    yield return "";
                    yield return "        #region Operators";
                    foreach (var op in operators)
                        foreach (var line in op.Lines)
                            yield return line.Indent(2);
                    yield return "";
                    yield return "        #endregion";
                    yield return "";
                }

                if (functions.Length > 0)
                {
                    yield return "";
                    yield return "        #region Functions";
                    foreach (var func in functions)
                        foreach (var line in func.Lines)
                            yield return line.Indent(2);
                    yield return "";
                    yield return "        #endregion";
                    yield return "";
                }

                if (staticFunctions.Length > 0)
                {
                    yield return "";
                    yield return "        #region Static Functions";
                    foreach (var func in staticFunctions)
                        foreach (var line in func.Lines)
                            yield return line.Indent(2);
                    yield return "";
                    yield return "        #endregion";
                    yield return "";
                }

                if (componentWiseStaticFunctions.Length > 0)
                {
                    yield return "";
                    yield return "        #region Component-Wise Static Functions";
                    foreach (var func in componentWiseStaticFunctions)
                        foreach (var line in func.Lines)
                            yield return line.Indent(2);
                    yield return "";
                    yield return "        #endregion";
                    yield return "";
                }

                if (componentWiseOp.Length > 0)
                {
                    yield return "";
                    yield return "        #region Component-Wise Operator Overloads";
                    foreach (var op in componentWiseOp)
                        foreach (var line in op.Lines)
                            yield return line.Indent(2);
                    yield return "";
                    yield return "        #endregion";
                    yield return "";
                }

                yield return "    }";
                yield return "}";
            }
        }

        public virtual string ZeroValue => BaseType.ZeroValue;
        public virtual string OneValue => BaseType.OneValue;

        public static void InitTypes()
        {
            Types.Clear();

            // vectors
            foreach (var type in BuiltinType.BaseTypes)
                for (var comp = 2; comp <= 4; ++comp)
                {
                    var vect = new VectorType(type, comp);
                    Types.Add(vect.Name, vect);
                }

            // matrices
            BuiltinType[] matrixTypes = new BuiltinType[] { BuiltinType.TypeFloat, BuiltinType.TypeDouble };
            foreach (var type in matrixTypes)
                for (var rows = 2; rows <= 4; ++rows)
                    for (var cols = 2; cols <= 4; ++cols)
                    {
                        var matt = new MatrixType(type, cols, rows);
                        if (GenerateMatrices)
                            Types.Add(matt.Name, matt);
                    }

            // generate types
            foreach (var type in Types.Values)
                type.Generate();
        }

        private static string NestedSymmetricFunction(IReadOnlyList<string> fields, string funcFormat, int start, int end)
        {
            if (start == end)
                return fields[start];

            var mid = (start + end) / 2;
            return string.Format(funcFormat,
                NestedSymmetricFunction(fields, funcFormat, start, mid),
                NestedSymmetricFunction(fields, funcFormat, mid + 1, end));
        }

        public string TypeCast(BuiltinType otherType, string c)
        {
            if (otherType.HasArithmetics && BaseType.IsBool)
                return $"{c} ? {otherType.OneValue} : {otherType.ZeroValue}";

            if (otherType.IsBool && BaseType.HasArithmetics)
                return $"{c} != {BaseType.ZeroValue}";

            return $"({otherType.Name}){c}";
        }
    }
}
