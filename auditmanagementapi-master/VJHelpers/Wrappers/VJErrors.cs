using System;
using System.Collections.Generic;
using System.Text;

namespace VJLiabraries.Wrappers
{
    public class VJErrors
    {
        public Exception Exception;
        public string CODE;
        public string Message;
        public VJErrors(Exception exception)
        {
            Initialize(null, exception, null);
        }
        public VJErrors(string message)
        {
            Initialize(null, null, message);
        }
        public VJErrors(string Code, Exception exception)
        {
            Initialize(Code, exception, null);
        }
        public VJErrors(string Code, string message)
        {
            Initialize(Code, null, message);
        }
        public VJErrors(string Code, Exception exception, string message)
        {
            Initialize(Code, exception, message);
        }

        private void Initialize(string Code, Exception exception, string message)
        {
            this.CODE = Code ?? string.Empty;
            this.Message = message ?? exception.Message;
            this.Exception = exception;
        }

        public override string ToString()
        {
            return $"CODE:{this.CODE} | Message:{this.Message} |Exception {this.Exception}";
        }
    }
}
