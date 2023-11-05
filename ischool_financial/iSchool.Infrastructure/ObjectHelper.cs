using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace iSchool.Infrastructure
{
	public static partial class ObjectHelper
	{
        public static Exception Try(Action action)
        {
            Exception _ex = null;
            if (action != null)
            {
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    _ex = ex;
                }
            }
            return _ex;
        }

        public static void TryThrow(this Exception ex)
        {
            if (ex != null) throw ex;
        }

        public static T Tryv<T>(Func<T> func, T defv = default)
        {
            try { return func(); }
            catch { return defv; }
        }

        public static T Tryv<T>(Func<T> func, Func<T> funcDefv)
        {
            try { return func(); }
            catch { return funcDefv(); }
        }

        public static T Tryv0<T>(Func<T> func, T defv = default)
        {
            try
            {
                var v = func();
                if (ReferenceEquals(v, null)) return defv;
                return v;
            }
            catch { return defv; }
        }

        public static T Todo<T>(this T obj, Action<T> todo)
        {
            todo(obj);
            return obj;
        }
    }
}