using System;
using System.Collections.Generic;

namespace iSchool
{
	public class FuncComparer<T> : IComparer<T>
    {
        public FuncComparer(Func<T, T, int> func) => _func = func;

        private readonly Func<T, T, int> _func;

		public int Compare(T x, T y) => _func(x, y);
    }
}