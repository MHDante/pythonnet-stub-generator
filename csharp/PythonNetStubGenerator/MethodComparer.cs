using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace PythonNetStubGenerator
{
    internal class MethodComparer : IComparer<MethodInfo>
    {
        public int Compare(MethodInfo a, MethodInfo b)
        {
            if (a == null && b == null) return 0;
            if (a == null) return 1;
            if (b == null) return -1;

            var aName = a.NonGenericName();
            var bName = b.NonGenericName();
            
            var nameCompare = string.Compare(aName, bName, StringComparison.InvariantCulture);
            if (nameCompare != 0) return nameCompare;

            var aParams = a.GetParameters();
            var bParams = b.GetParameters();

            var paramCompare = aParams.Length.CompareTo(bParams.Length);
            if (paramCompare != 0) return paramCompare;

            float GetDepth(Type t, bool addGenerics)
            {
                if (t == null) return 0;
                var baseDepth = t.GetInterfaces().Append(t.BaseType).Select(it => GetDepth(it, false)).Max() + 1;

                if (addGenerics)
                {
                    var generics = new List<Type>();
                    if (t.HasElementType)
                    {
                        generics.Add(t.GetElementType());
                    }
                    if (t.IsGenericType)
                    {
                        generics.AddRange(t.GetGenericArguments());
                    }

                    if (generics.Count > 0)
                    {
                        var genericDepth = generics.Select(it => GetDepth(it, false)).Max();
                        baseDepth += genericDepth * .001f;
                    }
                }
                return baseDepth;
            }

            var aParamString = "";
            var bParamString = "";

            for (var i = 0; i < aParams.Length; i++)
            {
                var aParam = aParams[i];
                var bParam = bParams[i];

                var aParamName = aParam.Name;
                var bParamName = bParam.Name;

                aParamString += aParamName;
                bParamString += bParamName;

                var aType = aParam.ParameterType;
                var bType = bParam.ParameterType;

                var aDepth = GetDepth(aType, true);
                var bDepth = GetDepth(bType, true);

                if (aType == typeof(char) && bType == typeof(string)) continue;
                if (aType == typeof(string) && bType == typeof(char)) continue;

                // We invert this because we want the highest depth first
                // This allows overloads of more defined types to appear first in the method list
                // Allowing the type-checker to infer that type before the more general one
                var depthCompare = -aDepth.CompareTo(bDepth);
                if (depthCompare != 0) return depthCompare;
            }
            return string.Compare(aParamString, bParamString, StringComparison.Ordinal);
        }
    }
}