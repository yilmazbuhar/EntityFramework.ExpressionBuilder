using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LambdaBuilder.Infra
{
    public static class ReflectionHelper
    {
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
