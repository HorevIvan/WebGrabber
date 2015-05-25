using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WebGrabber
{
    public static class ExtensionMethods
    {
        public static Boolean Is<T>(this Object o)
        {
            return (o is T);
        }

        public static T To<T>(this Object o)
        {
            return ((T)o);
        }

        public static Boolean Not(this Boolean value)
        {
            return !(value);
        }

        public static Boolean IsNull(this Object o)
        {
            return ReferenceEquals(o, null);
        }

        public static Boolean IsNotNull(this Object o)
        {
            return o.IsNull().Not();
        }

        public static Boolean IsEmpty<T>(this IEnumerable<T> enumerable)
        {
            return (enumerable.IsNull() || enumerable.Any().Not());
        }

        public static Boolean IsNotEmpty<T>(this IEnumerable<T> enumerable)
        {
            return enumerable.IsEmpty().Not();
        }

        public static String AsString(this Object o)
        {
            return (o + String.Empty);
        }

        public static String Set(this String pattern, params Object[] parameters)
        {
            return String.Format(pattern, parameters);
        }

        #region Loop

        public static T[] Loop<T>(this T[] array, Action<T> action)
        {
            array.AsEnumerable().Loop(action);

            return array;
        }

        public static IEnumerable<T> Loop<T>(this IEnumerable<T> enumerable, Action<T> action)
        {
            foreach (var item in enumerable)
            {
                action(item);
            }

            return enumerable;
        }

        #endregion

    }
}
