using System.Diagnostics;
using System.IO;
using UnityEditor;

namespace JEngine.Core
{
    public class DLLMgr
    {
        public static string DllPath = "Assets/HotUpdateResources/Dll/Hidden~/HotUpdateScripts.dll";
        
        [MenuItem("JEngine/XAsset/Bundles/Convert DLL")]
        public static void MakeBytes()
        {
            var watch = new Stopwatch();
            watch.Start();
            var bytes = FileToByte(DllPath);
            var result = ByteToFile(CryptoHelper.AesEncrypt(bytes, "DevelopmentMode."),
                "Assets/HotUpdateResources/Dll/HotUpdateScripts.bytes");
            watch.Stop();
            Log.Print("Convert Dlls in: " + watch.ElapsedMilliseconds + " ms.");
            if (!result)
            {
                Log.PrintError("DLL转Byte[]出错！");
            }
        }

        /// <summary>
        /// 删除文件或目录
        /// </summary>
        /// <param name="path"></param>
        public static void Delete(string path)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }

            if (Directory.Exists(path))
            {
                DirectoryInfo di = new DirectoryInfo(path);
                di.Delete(true);
            }
        }

        /// <summary>
        /// 将文件转换成byte[]数组
        /// </summary>
        /// <param name="fileUrl">文件路径文件名称</param>
        /// <returns>byte[]数组</returns>
        public static byte[] FileToByte(string fileUrl)
        {
            try
            {
                using (FileStream fs = new FileStream(fileUrl, FileMode.Open, FileAccess.Read))
                {
                    byte[] byteArray = new byte[fs.Length];
                    fs.Read(byteArray, 0, byteArray.Length);
                    return byteArray;
                }
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 将byte[]数组保存成文件
        /// </summary>
        /// <param name="byteArray">byte[]数组</param>
        /// <param name="fileName">保存至硬盘的文件路径</param>
        /// <returns></returns>
        public static bool ByteToFile(byte[] byteArray, string fileName)
        {
            bool result = false;
            try
            {
                using (FileStream fs = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.Write))
                {
                    fs.Write(byteArray, 0, byteArray.Length);
                    result = true;
                }
            }
            catch
            {
                result = false;
            }

            return result;
        }
    }
}
