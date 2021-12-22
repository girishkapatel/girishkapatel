using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace VJLiabraries.Wrappers
{
    public class VJResult<T>
    {
        public Boolean IsSuccess;
        public T Value;
        public List<VJErrors> Errors;

        public VJResult()
        {
        }
        public VJResult(Boolean isSuccess, String message)
        {
            Initialize(default(T), isSuccess, message);
        }


        // returns an SQresult with  value set
        public VJResult(T v, Boolean isSuccess, String message)
        {
            Initialize(v, isSuccess, message);

        }

        public static VJResult<T> WrapSuccess(T obj)
        {
            return new VJResult<T>(obj, true, null);
        }

        public static VJResult<T> WrapSuccess()
        {
            return new VJResult<T>(true, null);
        }

        public VJResult<T> Initialize(T v, Boolean isSuccess, String message)
        {
            Errors = new List<VJErrors>();
            IsSuccess = isSuccess;
            Value = v;
            return this;
        }

        public static VJResult<T> AddError(string humanMessage)
        {
            VJErrors err = new VJErrors(humanMessage);
            return new VJResult<T>(false,humanMessage).AddError(err);
        }
        public static VJResult<T> AddError(string code,string humanMessage)
        {
            VJErrors err = new VJErrors(code,null,humanMessage);
            return new VJResult<T>(false, humanMessage).AddError(err);
        }
        public static VJResult<T> AddError(string code, string humanMessage,Exception ex)
        {
            VJErrors err = new VJErrors(code, ex, humanMessage);
            return new VJResult<T>(false, humanMessage).AddError(err);
        }
        public VJResult<T> AddError(VJErrors ex)
        {
            if (ex != null)
            {
                this.Errors.Add(ex);
            }
            return this;
        }

        public VJResult<T> AddErrors(IEnumerable<VJErrors> errors)
        {
            if (this.Errors == null) this.Errors = new List<VJErrors>();

            if (errors != null)
            {
                if (Errors == null) Errors = new List<VJErrors>();

                this.Errors.AddRange(errors);
            }
            return this;
        }
    }

    public static class MyExtensionMethods
    {
        public static string ToDelimitedString<T>(this IEnumerable<T> source)
        {
            return source.ToDelimitedString(x => x.ToString(),
                CultureInfo.CurrentCulture.TextInfo.ListSeparator);
        }

        public static string ToDelimitedString<T>(
            this IEnumerable<T> source, Func<T, string> converter)
        {
            return source.ToDelimitedString(converter,
                CultureInfo.CurrentCulture.TextInfo.ListSeparator);
        }

        public static string ToDelimitedString<T>(
            this IEnumerable<T> source, string separator)
        {
            return source.ToDelimitedString(x => x.ToString(), separator);
        }

        public static string ToDelimitedString<T>(this IEnumerable<T> source,
            Func<T, string> converter, string separator)
        {
            return string.Join(separator, source.Select(converter).ToArray());
        }
    }
}
