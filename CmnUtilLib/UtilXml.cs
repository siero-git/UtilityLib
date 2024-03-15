using System.Diagnostics;
using System.Xml.Serialization;
using System.Xml;

namespace CmnUtilLib
{
    public static class UtilXml
    {
        /// <summary>
        /// XMLデシリアライズ
        /// </summary>
        /// <param name="strPath">ロードするXMLファイル</param>
        /// <param name="tObj">読み込むクラスオブジェクト</param>
        /// <returns>読み込んだクラスオブジェクト</returns>
        public static T? LoadXml<T>(string strPath)
            where T : class
        {
            T? tObj = null;

            if (File.Exists(strPath) == false)
            {
                Console.WriteLine($"[{nameof(UtilXml)}] (LoadXml) ファイルパスが不正です");
                return null;
            }

            XmlSerializer serializer = new XmlSerializer(typeof(T));

            //usingでfinallyも包括
            using (FileStream fs = new FileStream(strPath, System.IO.FileMode.Open))
            {
                //as演算子による型変換は変換可能かどうかのチェックのみ
                //例外が発生する可能性がある場合、キャストすると遅くなるのでasで変換
                //失敗したらnullになる
                tObj = serializer.Deserialize(fs) as T;
            }

            return tObj;
        }

        /// <summary>
        /// XMLシリアライズ
        /// </summary>
        /// <param name="filePath">書き込むXMLファイルパス
        /// <param name="obj">シリアライズするオブジェクト
        /// <param name="type">シリアライズするオブジェクトの型
        public static int SaveXML(string filePath, object obj, Type type)
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(type, "");

                // 書き込む書式の設定
                XmlWriterSettings settings = new XmlWriterSettings();

                //名前空間の定義：xsi=XXX
                XmlSerializerNamespaces namesp = new XmlSerializerNamespaces();
                namesp.Add(String.Empty, String.Empty);

                //XML宣言を書き込む:falseに設定
                settings.OmitXmlDeclaration = false;

                settings.Indent = true;
                settings.IndentChars = "    ";

                // ファイルへオブジェクトを書き込み（シリアライズ）
                using (XmlWriter writer = XmlWriter.Create(filePath, settings))
                {
                    serializer.Serialize(writer, obj, namesp);
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
                return -1;
            }
            return 0;
        }
    }
}
