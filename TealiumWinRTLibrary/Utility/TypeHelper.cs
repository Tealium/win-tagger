using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Tealium.Utility
{
    internal class TypeHelper
    {

        public static object LookupProperty(string p, object parameter, StringComparison comparison = StringComparison.OrdinalIgnoreCase)
        {
            //check for properties w/ that name first
            var props = GetProperties(parameter);
            foreach (var item in props)
            {
                if (string.Equals(item.Name, p, comparison))
                {
                    var s = item.GetValue(null, null);
                    if (s != null)
                        return s.ToString();
                }

            }
            //if not found, check the fields
            var fields = GetFields(parameter);
            foreach (var item in props)
            {
                if (string.Equals(item.Name, p, comparison))
                {
                    var s = item.GetValue(null, null);
                    if (s != null)
                        return s.ToString();
                }

            }
            return null;
        }

        public static IEnumerable<T> GetAttributes<T>(object o) where T : Attribute
        {
            if (o == null)
                return null;
#if NETFX_CORE
            return o.GetType().GetTypeInfo().GetCustomAttributes<T>();
#else
            return (IEnumerable<T>)o.GetType().GetCustomAttributes(typeof(T), true);
#endif
        }

        public static T GetAttribute<T>(object o) where T : Attribute
        {
            if (o == null)
                return null;
#if NETFX_CORE
            return o.GetType().GetTypeInfo().GetCustomAttribute<T>();
#else
            return (T)o.GetType().GetCustomAttributes(typeof(T), true).FirstOrDefault();
#endif
        }

        public static IEnumerable<PropertyInfo> GetProperties(object o)
        {
            if (o == null)
                return null;

#if NETFX_CORE
            return o.GetType().GetRuntimeProperties();
#else
            return o.GetType().GetProperties();
#endif
        }

        public static IEnumerable<FieldInfo> GetFields(object o)
        {
            if (o == null)
                return null;
#if NETFX_CORE
            return o.GetType().GetRuntimeFields();
#else
            return o.GetType().GetFields();
#endif
        }


    }
}
