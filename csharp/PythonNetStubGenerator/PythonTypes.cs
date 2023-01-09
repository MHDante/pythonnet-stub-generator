using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace PythonNetStubGenerator
{
    public static class PythonTypes
    {
        private static readonly HashSet<Type> AllExportedTypes = new HashSet<Type>();
        private static readonly HashSet<string> DirtyNamespaces = new HashSet<string>();
        private static readonly HashSet<Type> CurrentTypes = new HashSet<Type>();
        private static readonly HashSet<string> CurrentNamespaces = new HashSet<string>();
        private static readonly HashSet<Type> OverloadedNonGenericTypes = new HashSet<Type>();


        public static void CacheOverloadedNonGenericTypes(IEnumerable<Type> stubTypes)
        {
            var namesInSpace = new Dictionary<string, Dictionary<string, List<Type>>>();
            foreach (var type in stubTypes)
            {
                if (type.DeclaringType != null)
                {
                    if (type.IsGenericType) continue;
                    foreach (var siblingType in type.DeclaringType.GetNestedTypes())
                    {
                        if (!siblingType.IsGenericType) continue;
                        if (siblingType.NonGenericName() == type.Name)
                        {
                            OverloadedNonGenericTypes.Add(type);
                        }
                    }
                    continue;
                }

                var baseName = type.NonGenericName();
                var ns = type.Namespace ?? "";
                var typesByName = namesInSpace.TryGetValue(ns, out var val) ? val : namesInSpace[ns] = new Dictionary<string, List<Type>>();
                var typesWithName = typesByName.TryGetValue(baseName, out var val2) ? val2 : typesByName[baseName] = new List<Type>();

                typesWithName.Add(type);
                if (typesWithName.Count <= 1) continue;
                foreach (var overloadedType in typesWithName)
                {
                    if (!overloadedType.IsGenericType) OverloadedNonGenericTypes.Add(overloadedType);
                }
            }
        }

        public static bool IsOverloadedNonGenericType(this Type type) => OverloadedNonGenericTypes.Contains(type);


        public static bool CurrentUsedGenericArray { get; private set; }
        public static bool CurrentUsedBaseArray { get; private set; }

        public static void AddDependency(Type t)
        {
            var isNewAdd = AllExportedTypes.Add(t);
            if (isNewAdd) DirtyNamespaces.Add(t.Namespace);
            if (t != typeof(Nullable<>)) CurrentTypes.Add(t);
        }

        public static void AddArrayDependency(bool isGeneric)
        {
            AddDependency(typeof(Array));

            if (isGeneric) CurrentUsedGenericArray = true;
            else CurrentUsedBaseArray = true;
        }

        private static void AddNamespaceDependency(string typeNamespace) => CurrentNamespaces.Add(typeNamespace);

        public static List<Type> GetCurrentTypeDependencies() => new List<Type>(CurrentTypes);
        public static List<string> GetCurrentNamespaceDependencies() => new List<string>(CurrentNamespaces);
        public static void ClearCurrent()
        {
            CurrentTypes.Clear();
            CurrentNamespaces.Clear();

            CurrentUsedGenericArray = false;
            CurrentUsedBaseArray = false;
        }

        public static (string nameSpace, List<Type> types) RemoveDirtyNamespace()
        {
            var key = DirtyNamespaces.FirstOrDefault();
            DirtyNamespaces.Remove(key);
            if (key == null) return (null, new List<Type>());
            var results = AllExportedTypes.Where(it => it.Namespace == key).ToList();
            return (key, results);
        }


        internal static string SafePythonName(string s)
        {
            if (s == "from") return "from_";
            if (s == "del") return "del_";
            if (s == "None") return "None_";
            return s;
        }

        public static string NonGenericName(this Type t) =>
            t.Name.Split('`')[0];

        public static string NonGenericName(this MethodBase t) =>
            t.Name.Split('`')[0];


        public static string CleanName(this Type t)
        {
            var name = t.NonGenericName();
            if (t.IsGenericType) name = $"{name}_{t.GetGenericArguments().Length}";
            return name;
        }

        public static string CleanName(this MethodBase t)
        {
            var name = t.NonGenericName();
            if (t.IsGenericMethod) name = $"{name}_{t.GetGenericArguments().Length}";
            return name;
        }


        public static string ToPythonType(this Type t, bool withGenericParams = true)
        {
            if (t == null || t == typeof(void)) return "None";
            if (t == typeof(object)) return "typing.Any";
            if (t == typeof(string)) return "str";
            if (t == typeof(char)) return "str";
            if (t == typeof(double)) return "float";
            if (t == typeof(float)) return "float";
            if (t == typeof(bool)) return "bool";
            if (t == typeof(long)) return "int";
            if (t == typeof(int)) return "int";
            if (t == typeof(byte)) return "int";
            if (t == typeof(sbyte)) return "int";
            if (t == typeof(short)) return "int";
            if (t == typeof(uint)) return "int";
            if (t == typeof(ushort)) return "int";
            if (t == typeof(ulong)) return "int";
            if (t == typeof(IntPtr)) return "int";
            if (t == typeof(Type)) return "typing.Type[typing.Any]";
            if (t == typeof(Array))
            {
                AddArrayDependency(false);
                return "Array";
            }

            if (t.IsByRef || t.IsPointer)
                return !withGenericParams ? "clr.Reference" : $"clr.Reference[{t.GetElementType().ToPythonType()}]";

            if (t.IsArray)
            {
                AddArrayDependency(true);
                return !withGenericParams ? "Array_1" : $"Array_1[{t.GetElementType().ToPythonType()}]";
            }

            if (t.IsGenericParameter)
            {
                return GetGenericTypeParameterName(t);
            }


            var cleanName = t.CleanName();


            if (withGenericParams)
            {
                var generics = GetGenerics(t);
                if (generics.Count > 0)
                {
                    var pythonTypeArgs = generics.Select(it => it.ToPythonType()).CommaJoin();

                    if (t.IsConstructedGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>))
                    {
                        cleanName = "typing.Optional";
                    }


                    cleanName = $"{cleanName}[{pythonTypeArgs}]";
                }
            }

            var scope = GetScope(t);

            if (string.IsNullOrEmpty(scope))
            {
                AddDependency(t.IsGenericType ? t.GetGenericTypeDefinition() : t);
            }

            return scope + cleanName;
        }

        private static string GetScope(Type type)
        {
            var s = type.DeclaringType?.ToPythonType(false);
            if (s != null) return $"{s}.";

            var cleanName = type.CleanName();
            if (SymbolScope.Scopes.Any(it => it.HasConflict(cleanName, type.Namespace)))
            {
                AddNamespaceDependency(type.Namespace);
                return $"{type.Namespace}.";
            }

            return "";
        }



        private static List<Type> GetGenerics(Type type)
        {
            IEnumerable<Type> result = type.GetGenericArguments();
            if (type.IsGenericType) AddDependency(type.GetGenericTypeDefinition());
            return result.ToList();
        }

        private static string GetGenericTypeParameterName(Type t)
        {
            var currentScope = ClassScope.Current;

            var method = t.DeclaringMethod;
            var declType = t.DeclaringType;

            string basePrefix;
            if (method != null) basePrefix = method.CleanName();
            else if (declType != null) basePrefix = declType.CleanName();
            else throw new Exception("Where did this type come from?");

            var baseName = basePrefix + "_" + t.Name;

            var currentClassName = currentScope?.PythonClass;
            if (currentScope == null || currentClassName == basePrefix) return baseName;
            if (method == null) return currentClassName + "_" + baseName;

            return currentScope.PythonClass + "_" + baseName;
        }

        public static bool IsReservedWord(string propertyName)
        {
            switch (propertyName)
            {
                case "None": return true;
                default: return false;
            }
        }
    }
}
