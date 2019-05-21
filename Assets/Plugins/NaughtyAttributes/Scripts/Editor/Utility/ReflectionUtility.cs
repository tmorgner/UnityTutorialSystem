using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace NaughtyAttributes.Editor
{
    public static class ReflectionUtility
    {
        public static IEnumerable<FieldInfo> GetAllFields(object target, Func<FieldInfo, bool> predicate)
        {
            List<Type> types = new List<Type>()
            {
                target.GetType()
            };

            while (types.Last().BaseType != null)
            {
                types.Add(types.Last().BaseType);
            }

            for (int i = types.Count - 1; i >= 0; i--)
            {
                IEnumerable<FieldInfo> fieldInfos = types[i]
                                                    .GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public |
                                                               BindingFlags.DeclaredOnly)
                                                    .Where(predicate);

                foreach (var fieldInfo in fieldInfos)
                {
                    yield return fieldInfo;
                }
            }
        }

        public static List<FieldInfo> GetAllFieldsEfficiently(object target, Func<FieldInfo, bool> predicate, List<FieldInfo> buffer)
        {
            if (buffer != null)
            {
                buffer.Clear();
            }
            else
            {
                buffer = new List<FieldInfo>();
            }

            var types = new List<Type>()
            {
                target.GetType()
            };

            while (types.Last().BaseType != null)
            {
                types.Add(types.Last().BaseType);
            }

            for (int i = types.Count - 1; i >= 0; i--)
            {
                var fieldInfos = types[i]
                    .GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly);

                foreach (var fi in fieldInfos)
                {
                    if (!predicate(fi))
                    {
                        continue;
                    }

                    buffer.Add(fi);
                }
            }

            return buffer;
        }

        public static IEnumerable<PropertyInfo> GetAllProperties(object target, Func<PropertyInfo, bool> predicate)
        {
            List<Type> types = new List<Type>()
            {
                target.GetType()
            };

            while (types.Last().BaseType != null)
            {
                types.Add(types.Last().BaseType);
            }

            for (int i = types.Count - 1; i >= 0; i--)
            {
                IEnumerable<PropertyInfo> propertyInfos = types[i]
                                                          .GetProperties(BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public |
                                                                         BindingFlags.DeclaredOnly)
                                                          .Where(predicate);

                foreach (var propertyInfo in propertyInfos)
                {
                    yield return propertyInfo;
                }
            }
        }

        public static List<PropertyInfo> GetAllPropertiesEfficiently(object target, Func<PropertyInfo, bool> predicate, List<PropertyInfo> buffer)
        {
            if (buffer != null)
            {
                buffer.Clear();
            }
            else
            {
                buffer = new List<PropertyInfo>();
            }

            List<Type> types = new List<Type>()
            {
                target.GetType()
            };

            while (types.Last().BaseType != null)
            {
                types.Add(types.Last().BaseType);
            }

            for (int i = types.Count - 1; i >= 0; i--)
            {
                var propertyInfos = types[i].GetProperties(BindingFlags.Instance |
                                                           BindingFlags.Static |
                                                           BindingFlags.NonPublic |
                                                           BindingFlags.Public |
                                                           BindingFlags.DeclaredOnly);

                foreach (var propertyInfo in propertyInfos)
                {
                    if (predicate(propertyInfo))
                    {
                        buffer.Add(propertyInfo);
                    }
                }
            }

            return buffer;
        }

        public static IEnumerable<MethodInfo> GetAllMethods(object target, Func<MethodInfo, bool> predicate)
        {
            var methodInfos = target.GetType()
                                    .GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public)
                                    .Where(predicate);

            return methodInfos;
        }

        public static List<MethodInfo> GetAllMethodsEfficiently(object target, Func<MethodInfo, bool> predicate, List<MethodInfo> buffer)
        {
            
            if (buffer != null)
            {
                buffer.Clear();
            }
            else
            {
                buffer = new List<MethodInfo>();
            }

            var methodInfos = target.GetType()
                                    .GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);

            foreach (var m in methodInfos)
            {
                if (predicate(m))
                {
                    buffer.Add(m);
                }
            }
            return buffer;
        }

        public static FieldInfo GetField(object target, string fieldName)
        {
            return GetAllFields(target, f => f.Name.Equals(fieldName, StringComparison.InvariantCulture)).FirstOrDefault();
        }

        public static PropertyInfo GetProperty(object target, string propertyName)
        {
            return GetAllProperties(target, p => p.Name.Equals(propertyName, StringComparison.InvariantCulture)).FirstOrDefault();
        }

        public static MethodInfo GetMethod(object target, string methodName)
        {
            return GetAllMethods(target, m => m.Name.Equals(methodName, StringComparison.InvariantCulture)).FirstOrDefault();
        }
    }
}