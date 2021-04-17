using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.IO.Compression;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

namespace JEngine.Editor
{
    public class Tools
    {
        
        /// <summary>
        /// 获取场景下全部的类
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static List<T> FindObjectsOfTypeAll<T>()
        {
            return new List<Scene>{SceneManager.GetActiveScene()}.SelectMany(scene => scene.GetRootGameObjects())
                .SelectMany(g => g.GetComponentsInChildren<T>(true))
                .ToList();
        }
        
        /// <summary>
        /// Post请求
        /// </summary>
        /// <param name="url"></param>
        /// <param name="dic"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public static async Task<string> Post(string url, Dictionary<string, string> dic,int timeout = 30000)
        {
            string result = "";

            Thread t = new Thread(() =>
            {
                try
                {
                    //创建请求
                    HttpWebRequest req = (HttpWebRequest) WebRequest.Create(url);
                    req.Method = "POST";
                    req.ContentType = "application/x-www-form-urlencoded";

                    //参数
                    StringBuilder builder = new StringBuilder();
                    int i = 0;
                    foreach (var item in dic)
                    {
                        if (i > 0)
                            builder.Append("&");
                        builder.AppendFormat("{0}={1}", item.Key, item.Value);
                        i++;
                    }

                    //开始请求
                    byte[] data = Encoding.UTF8.GetBytes(builder.ToString());
                    req.ContentLength = data.Length;
                    using (Stream reqStream = req.GetRequestStream())
                    {
                        reqStream.Write(data, 0, data.Length);
                        reqStream.Close();
                    }


                    HttpWebResponse resp = (HttpWebResponse) req.GetResponse();
                    Stream stream = resp.GetResponseStream();
                    //获取响应内容
                    using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                    {
                        result = reader.ReadToEnd();
                    }
                }
                catch (Exception e)
                {
                    result = e.GetType()+": "+e.Message;
                }
            });
            
            t.Start();
            
            while (string.IsNullOrEmpty(result))
            {
                if (timeout <= 0)
                {
                    t.Abort();
                    throw new TimeoutException("Post request time out");
                }
                timeout--;
                await Task.Delay(1);
            }
            return result;
        }

        public static void Unzip(string file, string path)
        {
            ZipFile.ExtractToDirectory(file, path);
        }

        public static async Task<bool> Download(string url, string path)
        {
            var result = false;
            //下载文件  
            WebClient client = new WebClient();
            try
            {
                Pop("Downloading", "下载中");
                await client.DownloadFileTaskAsync(url, path);
                result = true;
            }
            catch (Exception ex)
            {
                EditorUtility.DisplayDialog("Error", $"下载文件失败：{ex.Message}", "OK");
            }

            client.Dispose();
            isPopping = false;
            return result;
        }

        private static bool isPopping;

        private static async void Pop(string title, string info)
        {
            isPopping = true;
            while (isPopping)
            {
                EditorUtility.DisplayProgressBar(title, info, Random.Range(0f, 1f));
                await Task.Delay(1);
            }

            EditorUtility.ClearProgressBar();
        }
    }
}