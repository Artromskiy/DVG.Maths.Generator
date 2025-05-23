﻿using DVG.GLSH.Generator.Members;
using System.Collections.Generic;
using System.Linq;
using static System.Linq.Enumerable;
using static System.String;

namespace DVG.GLSH.Generator.Types
{
    internal partial class MatrixType
    {
        /// <summary>
        /// Refers to GLSL 450 specs.
        /// 5 Operators and Expressions.
        /// 5.9 Expressions.
        /// </summary>
        /// <returns></returns>
        private IEnumerable<Member> Operators()
        {
            var p = " + ";
            var c = ", ";
            // matrix-matrix-multiplication
            for (var rcols = 2; rcols <= 4; ++rcols)
            {
                var rhs = new MatrixType(BaseType, rcols, Columns);
                var resultType = new MatrixType(BaseType, rcols, Rows);
                var ctrParams1 = Join(c, FieldsHelper(rcols, Rows).Select(f => Join(p, Range(0, Columns).Select(i => $"lhs[{i}, {f[2]}] * rhs[{f[1]}, {i}]"))));
                yield return new Operator(resultType, "*")
                {
                    Comment = $"Executes a matrix-matrix-multiplication {Name} * {rhs.Name} -> {resultType.Name}.",
                    Parameters = new string[] { $"{Name} lhs", $"{rhs.Name} rhs" },
                    Code = new string[] { $"new {resultType.Name}({ctrParams1})" },
                    GlslName = "op_Multiply",
                };
            }

            string ctrParams2 = Join(c, Range(0, Rows).Select(r => Join(p, Range(0, Columns).Select(c => $"m[{c}, {r}] * v.{"xyzw"[c]}"))));
            var retType = new VectorType(BaseType, Rows);
            var inpType = new VectorType(BaseType, Columns);
            yield return new Operator(retType, "*")
            {
                Comment = "Executes a matrix-vector-multiplication.",
                Parameters = new string[] { $"{Name} m", $"{inpType.Name} v" },
                Code = new string[] { $"new {retType.Name}({ctrParams2})" },
                GlslName = "op_Multiply",
            };

            // arithmetic operators
            var operatorToComment = new Dictionary<string, string>
            {
                {"+", "+ (addition)"},
                {"-", "- (subtraction)"},
                {"/", "/ (division)"},
                {"*", "* (multiplication)"} // only scalar
            };
            var operatorToName = new Dictionary<string, string>
            {
                {"+", "op_Addition"},
                {"-", "op_Subtraction"},
                {"/", "op_Division"},
                {"*", "op_Multiply"} // only scalar
            };
            foreach (var kvp in operatorToComment)
            {
                var op = kvp.Key;
                var opComment = kvp.Value;

                if (op != "*")
                    yield return new Operator(this, op) // matrix * matrix
                    {
                        Comment = $"Executes a component-wise {opComment}.",
                        Parameters = this.LhsRhs(),
                        Code = new string[] { $"new {Name}({Fields.Select(f => $"lhs{f} {op} rhs{f}").CommaSeparated()})" },
                        GlslName = operatorToName[op]
                    };
                yield return new Operator(this, op) // scalar * matrix
                {
                    Comment = $"Executes a component-wise {opComment} with scalar.",
                    Parameters = new string[] { $"{BaseTypeName} s", $"{Name} m" },
                    Code = new string[] { $"new {Name}({Fields.Select(f => $"s {op} m{f}").CommaSeparated()})" },
                    GlslName = operatorToName[op]
                };
                yield return new Operator(this, op) // matrix * scalar
                {
                    Comment = $"Executes a component-wise {opComment} with scalar.",
                    Parameters = new string[] { $"{Name} m", $"{BaseTypeName} s" },
                    Code = new string[] { $"new {Name}({Fields.Select(f => $"m{f} {op} s").CommaSeparated()})" },
                    GlslName = operatorToName[op]
                };
            }
        }
    }
}
