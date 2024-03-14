using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CmnUtilLib
{
    public static class UtilPath
    {
        public static string GetStartUpPath()
        {
            string exePath = Environment.GetCommandLineArgs()[0];
            string exeFullPath = System.IO.Path.GetFullPath(exePath);

            return exeFullPath;
        }

        public static string GetRootDir()
        {
            string? result = Path.GetDirectoryName(GetStartUpPath());

            return result ?? string.Empty;
        }
    }
}
