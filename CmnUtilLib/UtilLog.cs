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
        private string rootPath = string.Empty;

        public UtilLog(string rootPath)
        {
            this.rootPath = rootPath;
            Directory.CreateDirectory(rootPath);
        }

        public void SetOption()
        {
        }

        /// <summary>
        /// ファイル出力処理
        /// </summary>
        /// <param name="strLog"></param>
        public void Output(string strLog)
        {
            lock (mLockObj)
            {
                string strPath = Path.Combine(rootPath, $"{DateTime.Now.ToString("yyyyMMdd")}.{fileExt}");

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
                DirectoryInfo dirInfo = new DirectoryInfo(rootPath);
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
}
