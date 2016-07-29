using System;
using System.Runtime.InteropServices;

namespace EasyOPC
{
    [ComVisible(true)]
    [Guid("C3ABAB23-011E-49D1-B30C-7EC0EA16308C")]
    public class Result : IResult
    {
        private Exception _exception;        
        private bool _success;
        
        public Result(bool success)
        {
            _success = success;
        }

       

        public Result(bool success, Exception exception) : this(success)
        {
            _exception = exception;
        }


        public Exception Exception
        {
            get
            {
                return _exception;
            }
        }

       
        public bool Success
        {
            get
            {
                return _success;
            }
        }
    }
}
