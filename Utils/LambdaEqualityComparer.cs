using System;
using System.Collections.Generic;

namespace Schedules.Utils
{
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