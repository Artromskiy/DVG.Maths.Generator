﻿using DVG.GLSH.Generator.Members;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace DVG.GLSH.Generator.Types
{
    internal class InfoGenerator
    {

        public static IEnumerable<string> InfoFile()
        {
            yield return "using System;";
            yield return "using System.Collections;";
            yield return "using System.Collections.Generic;";
            yield return "using System.Globalization;";
            yield return "using System.Runtime.InteropServices;";
            yield return "using System.Runtime.CompilerServices;";
            yield return "using System.Diagnostics.CodeAnalysis;";
            yield return "using System.Runtime.Serialization;";
            yield return "using System.Numerics;";
            yield return "using System.Linq;";
            yield return "";
            yield return "// ReSharper disable InconsistentNaming";
            yield return "";
            yield return "namespace " + AbstractType.Namespace;
            yield return "{";

            yield return "public static class GLSHInfo".Indent();
            yield return "{".Indent();

            foreach (var item in KnownTypesNames())
                yield return item.Indent(2);

            foreach (var item in KnownTypesToGlslTypes())
                yield return item.Indent(2);

            foreach (var item in KnownFunctons())
                yield return item.Indent(2);

            foreach (var item in KnownOperators())
                yield return item.Indent(2);

            foreach (var item in KnownFunctonsGlobal())
                yield return item.Indent(2);

            foreach (var item in KnownOperatorsGlobal())
                yield return item.Indent(2);

            yield return "}".Indent();
            yield return "}";
        }

        private static IEnumerable<string> KnownTypesNames()
        {
            yield return "public static readonly HashSet<string> knownTypes = new HashSet<string>()";
            yield return "{";
            foreach (var item in AbstractType.Types)
                yield return $"typeof({item.Key}).FullName!,".Indent();
            yield return "};";
        }

        private static IEnumerable<string> KnownTypesToGlslTypes()
        {
            yield return "public static readonly Dictionary<string, string> knownTypesToGlslTypes = new Dictionary<string, string>()";
            yield return "{";
            foreach (var item in AbstractType.Types)
                yield return $"{{typeof({item.Key}).FullName!, \"{item.Value.GlslName}\"}},".Indent();
            yield return "};";
        }

        private static IEnumerable<string> KnownFunctons()
        {
            foreach (var item in AbstractType.Types)
            {
                HashSet<string> writtenFunctionNames = new HashSet<string>();
                var allStaticFunctions = item.Value.members.Where
                    (member => (member.GetType() == typeof(Function) || member.GetType() == typeof(ComponentWiseStaticFunction)) &&
                    member.Static && !member.Extension);

                yield return $"private static readonly Dictionary<string, string> known{item.Key}Functions = new Dictionary<string, string>()";
                yield return "{";

                foreach (var member in allStaticFunctions)
                    if (writtenFunctionNames.Add(member.Name))
                        yield return $"{{nameof({item.Value.Name}.{member.Name}), \"{member.GlslName ?? ""}\"}},".Indent();

                yield return "};";
            }
        }

        private static IEnumerable<string> KnownOperators()
        {
            foreach (var item in AbstractType.Types)
            {
                HashSet<string> writtenOperatorNames = new HashSet<string>();
                var allOperators = item.Value.members.Where
                    (member => (member.GetType() == typeof(Operator) || member.GetType() == typeof(ComponentWiseOperator)) &&
                    member.Static && !member.Extension);

                yield return $"private static readonly Dictionary<string, string> known{item.Key}Operators = new Dictionary<string, string>()";
                yield return "{";

                foreach (var member in allOperators)
                {
                    var realName = member.Name[8..]; // remove "operator" in the beginning
                    if (writtenOperatorNames.Add(member.GlslName))
                    {
                        yield return $"{{\"{member.GlslName}\", \"{realName}\"}},".Indent();
                    }
                }

                yield return "};";
            }
        }

        private static IEnumerable<string> KnownFunctonsGlobal()
        {
            yield return $"public static readonly Dictionary<string, Dictionary<string, string>> knownFunctionsGlobal = new Dictionary<string, Dictionary<string, string>>()";
            yield return "{";
            foreach (var item in AbstractType.Types)
                yield return $"{{typeof({item.Value.Name}).FullName!, known{item.Key}Functions }},".Indent();
            yield return "};";
        }

        private static IEnumerable<string> KnownOperatorsGlobal()
        {
            yield return $"public static readonly Dictionary<string, Dictionary<string, string>> knownOperatorsGlobal = new Dictionary<string, Dictionary<string, string>>()";
            yield return "{";
            foreach (var item in AbstractType.Types)
                yield return $"{{typeof({item.Value.Name}).FullName!, known{item.Key}Operators }},".Indent();
            yield return "};";
        }
    }
}