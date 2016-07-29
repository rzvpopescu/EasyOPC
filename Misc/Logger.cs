using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// Class used for console loggin purpose
/// </summary>
namespace EasyOPC.Misc
{
    internal static class Logger
    {        
        public static void LogInfo(string sMessage)
        {
            Console.WriteLine(string.Format("#LOG -INFO- {0}", sMessage));
        }

        public static void LogError(string sMessage)
        {
            Console.WriteLine(string.Format("#LOG -ERROR- {0}", sMessage));
        }

        public static void LogWarning(string sMessage)
        {
            Console.WriteLine(string.Format("#LOG -WARNING- {0}", sMessage));
        }
    }
}
