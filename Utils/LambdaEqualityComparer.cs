using System;
using System.Collections.Generic;

namespace Schedules.Utils
{
    /// <summary>
    /// IEqualityComparer that takes a function to specify which field(s) to compare.
    /// </summary>
    public class LambdaEqualityComparer<T, R> : IEqualityComparer<T>
    {
        private readonly Func<T, R> valueExtractor;

        public LambdaEqualityComparer(Func<T, R> valueExtractor)
        {
            this.valueExtractor = valueExtractor;
        }

        public bool Equals(T x, T y)
        {
            return object.Equals(valueExtractor(x), valueExtractor(y));
        }

        public int GetHashCode(T obj)
        {
            return valueExtractor(obj).GetHashCode();
        }
    }
}