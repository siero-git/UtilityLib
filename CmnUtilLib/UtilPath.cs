using System;
using System.Collections.Generic;
using System.IO;
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

        /// <summary>
        /// ディレクトリの不正文字列判定
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static bool CheckPath(string path)
        {
            var chkChars = Path.GetInvalidPathChars();

            return path.IndexOfAny(chkChars) < 0;
        }

        /// <summary>
        /// ファイルの不正文字列判定
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static bool CheckName(string name)
        {
            var chkChars = Path.GetInvalidFileNameChars();

            return name.IndexOfAny(chkChars) < 0;
        }
    }
}
