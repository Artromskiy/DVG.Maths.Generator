﻿using DVG.GLSH.Generator.Members;
using System.Collections.Generic;
using System.Linq;

namespace DVG.GLSH.Generator.Types
{
    internal partial class VectorType
    {
        private IEnumerable<Member> CastFunctions()
        {
            // implicit upcasts
            var implicits = new HashSet<BuiltinType>();
            var upcasts = BuiltinType.Upcasts;
            foreach (var ukvp in upcasts.Where(k => k.Key == BaseType))
            {
                var otherType = ukvp.Value;
                implicits.Add(otherType);
                var targetType = new VectorType(otherType, Length);

                yield return new ImplicitOperator(targetType)
                {
                    ParameterString = Name + " v",
                    CodeString = Construct(targetType, CompString.Select(c => TypeCast(otherType, "v." + c)).ExactlyN(Length, otherType.ZeroValue)),
                    Comment = $"Implicitly converts this to a {targetType.Name}.",
                };
            }

            yield break;
            // explicit casts
            foreach (var oType in BuiltinType.BaseTypes)
            {
                var otherType = oType;

                for (var comps = 2; comps <= 4; ++comps)
                {
                    if (otherType == BaseType && comps == Length)
                        continue; // same type and comps not useful

                    if (comps == Length && implicits.Contains(otherType))
                        continue; // already has an implicit conversion

                    var commentAppendix = "";
                    if (comps > Length)
                        commentAppendix = " (Higher components are zeroed)";
                    var targetType = new VectorType(otherType, comps);
                    yield return new ExplicitOperator(targetType)
                    {
                        ParameterString = Name + " v",
                        CodeString = Construct(targetType, CompString.Select(c => TypeCast(otherType, "v." + c)).ExactlyN(comps, otherType.ZeroValue)),
                        Comment = $"Explicitly converts this to a {targetType.Name}.{commentAppendix}"
                    };
                }
            }
        }
    }
}
