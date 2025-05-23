﻿using DVG.GLSH.Generator.Types;
using System.Collections.Generic;

namespace DVG.GLSH.Generator.Members
{
    internal abstract class Member
    {
        /// <summary>
        /// Original type ref
        /// </summary>
        public AbstractType OriginalType { get; set; }

        /// <summary>
        /// Is this member has corresponding member in GLSL
        /// </summary>
        public bool GlslBuiltIn => !string.IsNullOrEmpty(GlslName);

        /// <summary>
        /// Name of corresponding member in GLSL
        /// </summary>
        public string GlslName { get; set; }

        /// <summary>
        /// Name of the member
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Comment of the member
        /// </summary>
        public string Comment { get; set; }

        /// <summary>
        /// Visibility modifier
        /// </summary>
        public string Visibility { get; set; } = "public";

        /// <summary>
        /// True iff member is static
        /// </summary>
        public bool Static { get; set; }

        /// <summary>
        /// True if member is extension
        /// </summary>
        public bool Extension { get; set; }

        /// <summary>
        /// Attributes of this member
        /// </summary>
        public string[] Attributes = new string[] { };

        /// <summary>
        /// All lines of this member
        /// </summary>
        public virtual IEnumerable<string> Lines
        {
            get
            {
                foreach (var line in Comment.AsComment())
                    yield return line;
                foreach (var attribute in Attributes)
                    yield return $"[{attribute}]";
            }
        }

        /// <summary>
        /// Prefix for members (visibility, static)
        /// </summary>
        public virtual string MemberPrefix => Visibility + (Static ? " static" : "");

        /// <summary>
        /// Returns an enumeration of members used for the "glsh" class
        /// </summary>
        public virtual IEnumerable<Member> GlshMembers() { yield break; }

        /// <summary>
        /// If true, does not generate glm versions
        /// </summary>
        public bool DisableGlmGen { get; set; }
    }
}
