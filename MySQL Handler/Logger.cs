using System;
using System.IO;
using System.Threading.Tasks;

namespace MySQL_Handler
{
    internal class Logger
    {
        internal static string Prefix { get; set; }

        private static void Log(string message)
        {
            if (Configuration.Out != null)
            {
                Task.Run(() => {
                    Configuration.Out.WriteLine($"{Prefix} {message}");
                });
            }
            else
            {
                Task.Run(() => {
                    Console.WriteLine($"{Prefix} {message}");
                });
            }
        }

        internal static void Verbose(string message)
        {
            if (Configuration.LogsEnabled && Configuration.VerboseMode)
            {
                Log(message);
            }
        }

        internal static void Error(Exception exception)
        {
            if (Configuration.LogsEnabled)
            {
                Log(exception.ToString());
            }
        }
    }
}
