using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace VJLiabraries.Wrappers
{
    /// <summary>
    /// Return single object if passed or retrun Null element
    /// User Linq singleOrDefault
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class VJMaybe<T> : IEnumerable<T>
    {
        private readonly IEnumerable<T> Values;

        public VJMaybe()
        {
            this.Values = new T[0];
        }

        public VJMaybe(T value)
        {
            this.Values = new[] { value };
        }

        public IEnumerator<T> GetEnumerator()
        {
            return this.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
