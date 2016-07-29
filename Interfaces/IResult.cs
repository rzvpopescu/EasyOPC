using System;
using System.Runtime.InteropServices;

namespace EasyOPC
{
    [ComVisible(true)]
    [Guid("34D38B7F-F696-49F3-9291-43C49C1199C6")]
    public interface IResult
    {
        /// <summary>
        /// Indicates that the result had completed with success
        /// </summary>
        bool Success { get; }

        /// <summary>
        /// The result exception in case of a failure
        /// </summary>
        Exception Exception { get; }
    }
}
