using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace CmnUtilLib
{
    /// <summary>
    /// ログファイル操作
    /// 日付をファイル名としたログファイル
    /// </summary>
    public class UtilLog
    {
        /// <summary>
        /// ファイル削除間隔(日)
        /// </summary>
        public int optDeleteSpan { get; set; } = 30;

        /// <summary>
        /// エンコーディング
        /// 例:Encoding.GetEncoding("Shift_JIS")
        /// </summary>
        public Encoding optEncoding { get; set; } = Encoding.UTF8;

        /// <summary>
        /// ファイル拡張子
        /// </summary>
        public string optFileExt { get; set; } = "txt";

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

        /// <summary>
        /// ログ種別
        /// 例:logtype-xxx.yyy
        /// </summary>
        private string _logType = string.Empty;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="rootPath"></param>
        public UtilLog(string rootPath, string? logtype = null)
        {
            //フォルダを作成
            Directory.CreateDirectory(rootPath);

            outPath = rootPath;
            _logType = string.IsNullOrEmpty(logtype) ? string.Empty : logtype;
        }

        /// <summary>
        /// ファイル出力処理
        /// </summary>
        /// <param name="strLog"></param>
        public void Output(string strLog)
        {
            lock (mLockObj)
            {
                string dateText = $"{DateTime.Now.ToString("yyyyMMdd")}";

                string fileName = string.IsNullOrEmpty(_logType) ? 
                    $"{dateText}.{optFileExt}" : 
                    $"{_logType}-{dateText}.{optFileExt}";

                string strPath = Path.Combine(outPath, fileName);

                using (StreamWriter sw = new StreamWriter(strPath, true, optEncoding))
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
                DateTime delDateTime = DateTime.Now.AddDays(-1 * optDeleteSpan);

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

    /// <summary>
    /// ログ出力クラス
    /// 日付フォルダにログ出力する
    /// </summary>
    public class UtilLogDateDir
    {
        /// <summary>
        /// エンコーディング
        /// default:UTF8
        /// </summary>
        public Encoding optEncode { get; set; } = Encoding.UTF8;

        /// <summary>
        /// ログ削除期間
        /// default:30day
        /// </summary>
        public int optDelSpanDay { get; set; } = 30;

        /// <summary>
        /// ログファイル名
        /// </summary>
        private string _fileName = string.Empty;

        /// <summary>
        /// ログ出力先
        /// </summary>
        private string _rootPath = string.Empty;

        /// <summary>
        /// 排他ロックオブジェクト
        /// </summary>
        private object _lock = new object();

        public UtilLogDateDir(string rootPath, string fileName)
        {
            //不正文字が含まれていないか確認
            if (!UtilPath.CheckPath(rootPath))
            {
                throw new ArgumentException("ディレクトリに不正文字列が含まれています");
            }

            //ファイル名の不正チェック
            if (!UtilPath.CheckName(fileName))
            {
                throw new ArgumentException("ファイル名に不正文字列が含まれています");
            }
            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentException("ファイル名が不正です");
            }

            //ログの出力先となるルートディレクトリを作成する
            Directory.CreateDirectory(rootPath);

            _fileName = fileName;
            _rootPath = rootPath;
        }

        /// <summary>
        /// ログ出力処理
        /// </summary>
        /// <param name="msg"></param>
        public void Output(string msg)
        {
            lock(_lock)
            {
                //日付をフォーマット指定して文字列で取り出し
                string dateText = $"{DateTime.Now:yyyyMMdd}";
                string setPath = Path.Combine(_rootPath, dateText);

                //ディレクトリ有無を確認してFlagとして保持
                bool createFlag = Directory.Exists(setPath);

                if (!createFlag)
                {
                    Directory.CreateDirectory(setPath);
                }

                string strPath = Path.Combine(_rootPath, _fileName);

                using (StreamWriter sw = new StreamWriter(strPath, true, optEncode))
                {
                    //時間情報を付与して出力テキストを生成
                    string strOut = $"[{DateTime.Now:HH:mm:ss}] {msg}";

                    sw.WriteLine(strOut);
                }

                //ディレクトリを作成していたら削除処理を実施
                if(createFlag)
                {
                    DeleteLog();
                }
            }
        }

        private void DeleteLog()
        {
            Task.Run(() =>
            {
                //指定日数分マイナスした値を取得
                DateTime delDateTime = DateTime.Now.AddDays(-1 * optDelSpanDay);

                //時間部分を0としたDateTime値を生成
                DateTime delDate = new DateTime(delDateTime.Year, delDateTime.Month, delDateTime.Day);

                //フォルダ内のファイルを取得
                DirectoryInfo dirInfo = new DirectoryInfo(_rootPath);
                var logDirs = dirInfo.GetDirectories();

                //作成日が指定日以前のディレクトリを列挙
                var delInfos = logDirs.Where(x => x.CreationTime <= delDate);

                //対象のディレクトリを削除
                foreach (var item in delInfos)
                {
                    try
                    {
                        Directory.Delete(item.FullName, true);
                    }
                    catch (Exception ex)
                    {
                        Output($"[UtilLogDate] ファイル削除例外 msg:{ex.Message}");
                    }
                }
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
