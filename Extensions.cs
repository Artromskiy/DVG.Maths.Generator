﻿using GLSHGenerator.Types;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GLSHGenerator
{
    internal static class Extensions
    {
        public static bool WriteToFileIfChanged(this IEnumerable<string> flines, string filename)
        {
            var lines = flines.ToArray();
            var currLines = File.Exists(filename) ? File.ReadAllLines(filename) : new string[] { };
            if (lines.SequenceEqual(currLines)) return false;
            File.WriteAllLines(filename, lines);
            return true;
        }

        public static IEnumerable<T> ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach (var item in source)
                action(item);
            return source;
        }

        public static string ParameterNameExtract(this string s)
        {
            return s.Split('=')[0].Trim().Split(' ').Last().Trim();
        }

        public static IEnumerable<string> ArgNames(this IEnumerable<string> paras) => paras.CommaSeparated().Split(',').Select(p => p.ParameterNameExtract()).ToArray();
        public static IEnumerable<string> ParasRecovered(this IEnumerable<string> paras) => paras.CommaSeparated().Split(',').ToArray();

        private static string NestedSymmetricFunction(IReadOnlyList<string> fields, string funcFormat, int start, int end)
        {
            if (start == end)
                return fields[start];

            var mid = (start + end) / 2;
            return string.Format(funcFormat,
                NestedSymmetricFunction(fields, funcFormat, start, mid),
                NestedSymmetricFunction(fields, funcFormat, mid + 1, end));
        }

        private static string NestedSymmetricFunction(IEnumerable<string> ffs, string funcFormat)
        {
            var fs = ffs.ToArray();
            return NestedSymmetricFunction(fs, funcFormat, 0, fs.Length - 1);
        }

        public static string Indent(this string s, int lvl = 1)
        {
            return new string(' ', lvl * 4) + s;
        }

        public static IEnumerable<string> TypedArgs(this IEnumerable<string> ss, AbstractType type)
        {
            return ss.Select(s => type.Name + " " + s);
        }

        public static string CommaSeparated<T>(this IEnumerable<T> coll)
        {
            var cc = coll.Select(c => c.ToString()).ToArray();
            return cc.Length == 0 ? "" : cc.Aggregate((s1, s2) => s1 + ", " + s2);
        }
        public static string Aggregated<T>(this IEnumerable<T> coll, string seperator)
        {
            var cc = coll.Select(c => c.ToString()).ToArray();
            return cc.Length == 0 ? "" : NestedSymmetricFunction(cc, "({0}" + seperator + "{1})");
        }

        public static IEnumerable<string> LhsRhs(this AbstractType type)
        {
            yield return type.Name + " lhs";
            yield return type.Name + " rhs";
        }

        public static IEnumerable<string> TypedArgs(this AbstractType type, params string[] args)
        {
            return args.Select(a => type.Name + " " + a);
        }

        public static IEnumerable<string> SConcat(this IEnumerable<string> coll, params string[] args)
        {
            return coll.Concat(args);
        }

        public static string Capitalized(this string s)
        {
            if (string.IsNullOrEmpty(s))
                return s;
            return char.ToUpper(s[0]) + s.Substring(1);
        }

        public static IEnumerable<string> AsComment(this string s, bool withTrailingEmptyLine = true)
        {
            if (withTrailingEmptyLine)
                yield return "";
            if (string.IsNullOrEmpty(s))
                yield break;
            yield return "/// <summary>";
            yield return "/// " + s;
            yield return "/// </summary>";
        }

        public static IEnumerable<string> RepeatTimes(this string s, int times)
        {
            for (var i = 0; i < times; ++i)
                yield return s;
        }

        public static IEnumerable<string> DotComp(this string s, int maxComp = 4)
        {
            for (var i = 0; i < maxComp; ++i)
                yield return s + "." + "xyzw"[i];
        }

        public static IEnumerable<string> ImpulseString(this int arg, string imp, string nonimp, int maxComp = 4)
        {
            for (var i = 0; i < maxComp; ++i)
                yield return i == arg ? imp : nonimp;
        }

        public static IEnumerable<T> ExactlyN<T>(this IEnumerable<T> coll, int n, T obj)
        {
            var it = coll.GetEnumerator();
            for (var i = 0; i < n; ++i)
                yield return it.MoveNext() ? it.Current : obj;
        }

        public static IEnumerable<T> ForIndexUpTo<T>(this int n, Func<int, T> f)
        {
            for (var i = 0; i < n; ++i)
                yield return f(i);
        }
    }
}
