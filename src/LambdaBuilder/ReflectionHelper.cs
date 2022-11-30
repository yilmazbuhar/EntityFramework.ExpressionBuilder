using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime;

namespace LambdaBuilder.Infra
{
    public static class ReflectionHelper
    {
        

        public static IEnumerable<object> GetInstances<TType>()
        {
            foreach (var item in GetTypeOf<TType>())
            {
                yield return (TType)Activator.CreateInstance(item);
            }
        }

        public static IEnumerable<Type> GetTypeOf<TType>()
        {
            var sourcetype = typeof(TType);
            return sourcetype.Assembly.GetTypes()
                 .Where(type => sourcetype.IsAssignableFrom(type) && !type.IsAbstract && !type.IsInterface);
        }

        public static PropertyInfo GetProperty(Type type, string propertyName)
        {
            string typeName = string.Empty;
            if (propertyName.Contains("."))
            {
                //name was specified with typename - so pull out the typename
                typeName = propertyName.Substring(0, propertyName.IndexOf("."));
                propertyName = propertyName.Substring(propertyName.IndexOf(".") + 1);
            }

            PropertyInfo prop = type.GetProperty(propertyName, BindingFlags.IgnoreCase | BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public);
            if (prop == null)
            {
                var baseTypesAndInterfaces = new List<Type>();
                if (type.BaseType != null) baseTypesAndInterfaces.Add(type.BaseType);
                baseTypesAndInterfaces.AddRange(type.GetInterfaces());
                foreach (Type t in baseTypesAndInterfaces)
                {
                    prop = GetProperty(t, propertyName);
                    if (prop != null)
                    {
                        if (!string.IsNullOrEmpty(typeName) && t.Name != typeName)
                            continue; //keep looking as the typename was not found
                        break;
                    }
                }
            }
            return prop;
        }
    }
}
