using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CmnUtilLib
{
    /// <summary>
    /// ログファイル操作
    /// </summary>
    public class UtilLog
    {
        /// <summary>
        /// ファイル削除間隔(日)
        /// </summary>
        public int delDaySpan { get; set; } = 30;

        /// <summary>
        /// ファイル追記フラグ
        /// </summary>
        public bool appedFlag { get; set;} = true;

        /// <summary>
        /// エンコーディング
        /// 例:Encoding.GetEncoding("Shift_JIS")
        /// </summary>
        public Encoding encoding { get; set; } = Encoding.UTF8;

        /// <summary>
        /// ファイル拡張子
        /// </summary>
        public string fileExt { get; set; } = "txt";

        /// <summary>
        /// 排他ロック用オブジェクト
        /// </summary>
        private object mLockObj = new object();

        /// <summary>
        /// 最終保存日時
        /// </summary>
        private DateTime LastDate = DateTime.MinValue;

        /// <summary>
        /// ログ出力先
        /// </summary>
        private string outPath = string.Empty;

        private string logType = string.Empty;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="rootPath"></param>
        public UtilLog(string rootPath, string? logtype = null)
        {
            //フォルダを作成
            Directory.CreateDirectory(rootPath);

            outPath = rootPath;
            logType = string.IsNullOrEmpty(logtype) ? string.Empty : logtype;
        }

        /// <summary>
        /// ファイル出力処理
        /// </summary>
        /// <param name="strLog"></param>
        public void Output(string strLog)
        {
            lock (mLockObj)
            {
                string fileName = string.IsNullOrEmpty(logType) ? 
                    $"{DateTime.Now.ToString("yyyyMMdd")}.{fileExt}" : 
                    $"{logType}-{DateTime.Now.ToString("yyyyMMdd")}.{fileExt}";

                string strPath = Path.Combine(outPath, fileName);

                using (StreamWriter sw = new StreamWriter(strPath, appedFlag, encoding))
                {
                    //時間情報を付与して出力テキストを生成
                    string strOut = $"[{DateTime.Now.ToString("HH:mm:ss")}] {strLog}";

                    sw.WriteLine(strOut);
                }
            }

            //年月日のいずれかに差異がある場合は削除処理実施
            if ((DateTime.Now.Year != LastDate.Year) ||
                (DateTime.Now.Month != LastDate.Month) ||
                (DateTime.Now.Day != LastDate.Day))
            {
                DeleteLog();
            }

        }

        private void DeleteLog()
        {
            Task.Run(() =>
            {
                //指定日数分マイナスした値を取得
                DateTime delDateTime = DateTime.Now.AddDays(-1 * delDaySpan);

                //時間部分を0としたDateTime値を生成
                DateTime delDate = new DateTime(delDateTime.Year, delDateTime.Month, delDateTime.Day);

                //フォルダ内のファイルを取得
                DirectoryInfo dirInfo = new DirectoryInfo(outPath);
                FileInfo[] fileInfos = dirInfo.GetFiles();

                //最終更新日が指定日以前のファイルをリストで取得
                List<FileInfo> delInfos = fileInfos.Where(x => x.LastWriteTime <= delDate).ToList();

                //対象のファイルを削除
                foreach (FileInfo info in delInfos)
                {
                    try
                    {
                        File.Delete(info.FullName);
                    }
                    catch (Exception ex)
                    {
                        Output($"[UtilLog] ファイル削除例外 msg:{ex.Message}");
                    }
                }
            })
            .ContinueWith((task) =>
            {
                LastDate = DateTime.Now;
            });
        }
    }

    public static class UtilConsole
    {
        private static int _cursorTop = 0;
        private static int _setLength = 0;

        public static void ConsoleLog(string msg, bool topUpdateFlag = false, ConsoleColor color = ConsoleColor.White)
        {
            Console.ForegroundColor = color;
            Console.SetCursorPosition(0, _cursorTop);

            if(msg.Length > _setLength)
            {
                Console.WriteLine(msg);
                _setLength = msg.Length;
            }
            else
            {
                string setMsg = msg.PadRight(_setLength);
                Console.WriteLine(setMsg);
                _setLength = setMsg.Trim().Length;
            }


            if (topUpdateFlag)
            {
                (int left, int top) = Console.GetCursorPosition();
                _cursorTop = top;
            }
        }
    }
}
