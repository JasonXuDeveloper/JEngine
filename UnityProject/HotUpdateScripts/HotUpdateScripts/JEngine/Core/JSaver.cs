//
// JSaver.cs
//
// Author:
//       JasonXuDeveloper（傑） <jasonxudeveloper@gmail.com>
//
// Copyright (c) 2020 
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
using System;
using UnityEngine;

namespace JEngine.Core
{
    public class JSaver
    {
        /// <summary>
        /// Save a data to local storage as string
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="val"></param>
        /// <param name="encryptKey"></param>
        /// <returns>Saved value</returns>
        public static string SaveAsString<T>(T val, string dataName, string encryptKey)
        {
            if (encryptKey.Length != 16)
            {
                var ex = new Exception("encryptKey needs to be 16 characters!");
                Log.PrintError(ex);
                return null;
            }
            string strData = val.ToString();
            var result = CryptoHelper.EncryptStr(strData, encryptKey);
            PlayerPrefs.SetString(dataName, result);
            return result;
        }

        /// <summary>
        /// Save a data to local storage as JSON
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="val"></param>
        /// <param name="encryptKey"></param>
        /// <returns>Saved value</returns>
        public static string SaveAsJSON<T>(T val, string dataName, string encryptKey)
        {
            if (encryptKey.Length != 16)
            {
                var ex = new Exception("encryptKey needs to be 16 characters!");
                Log.PrintError(ex);
                return null;
            }
            string strData = StringifyHelper.JSONSerliaze(val);
            var result = CryptoHelper.EncryptStr(strData, encryptKey);
            PlayerPrefs.SetString(dataName, result);
            return result;
        }

        /// <summary>
        /// Get string from local storage
        /// </summary>
        /// <param name="val"></param>
        /// <param name="encryptKey"></param>
        /// <returns>Saved value</returns>
        public static string GetString(string dataName, string encryptKey)
        {
            if (encryptKey.Length != 16)
            {
                var ex = new Exception("encryptKey needs to be 16 characters!");
                Log.PrintError(ex);
                return null;
            }
            var result = PlayerPrefs.GetString(dataName);
            try
            {
                result = CryptoHelper.DecryptStr(result, encryptKey);
                return result;
            }
            catch (Exception ex)
            {
                Log.PrintError(ex);
                return null;
            }
        }

        /// <summary>
        /// Get int from local storage
        /// </summary>
        /// <param name="val"></param>
        /// <param name="encryptKey"></param>
        /// <returns>Saved value</returns>
        public static int GetInt(string dataName, string encryptKey)
        {
            return int.Parse(GetString(dataName, encryptKey));
        }

        /// <summary>
        /// Get short from local storage
        /// </summary>
        /// <param name="val"></param>
        /// <param name="encryptKey"></param>
        /// <returns>Saved value</returns>
        public static short GetShort(string dataName, string encryptKey)
        {
            return short.Parse(GetString(dataName, encryptKey));
        }

        /// <summary>
        /// Get long from local storage
        /// </summary>
        /// <param name="val"></param>
        /// <param name="encryptKey"></param>
        /// <returns>Saved value</returns>
        public static long GetLong(string dataName, string encryptKey)
        {
            return long.Parse(GetString(dataName, encryptKey));
        }

        /// <summary>
        /// Get decimal from local storage
        /// </summary>
        /// <param name="val"></param>
        /// <param name="encryptKey"></param>
        /// <returns>Saved value</returns>
        public static decimal GetDecimal(string dataName, string encryptKey)
        {
            return decimal.Parse(GetString(dataName, encryptKey));
        }

        /// <summary>
        /// Get double from local storage
        /// </summary>
        /// <param name="val"></param>
        /// <param name="encryptKey"></param>
        /// <returns>Saved value</returns>
        public static double GetDouble(string dataName, string encryptKey)
        {
            return double.Parse(GetString(dataName, encryptKey));
        }

        /// <summary>
        /// Get float from local storage
        /// </summary>
        /// <param name="val"></param>
        /// <param name="encryptKey"></param>
        /// <returns>Saved value</returns>
        public static float GetFloat(string dataName, string encryptKey)
        {
            return float.Parse(GetString(dataName, encryptKey));
        }

        /// <summary>
        /// Get bool from local storage
        /// </summary>
        /// <param name="val"></param>
        /// <param name="encryptKey"></param>
        /// <returns>Saved value</returns>
        public static bool GetBool(string dataName, string encryptKey)
        {
            return bool.Parse(GetString(dataName, encryptKey));
        }

        /// <summary>
        /// Get object from local storage
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="val"></param>
        /// <param name="encryptKey"></param>
        /// <returns>Saved value</returns>
        public static T GetObject<T>(string dataName, string encryptKey)
        {
            if (encryptKey.Length != 16)
            {
                throw new Exception("encryptKey needs to be 16 characters!");
            }
            var result = PlayerPrefs.GetString(dataName);
            try
            {
                result = CryptoHelper.DecryptStr(result, encryptKey);
                return StringifyHelper.JSONDeSerliaze<T>(result);
            }
            catch (Exception ex)
            {
                Log.PrintError(ex);
                return default(T);
            }
        }

        /// <summary>
        /// Whether or not has specific data
        /// </summary>
        /// <param name="dataName"></param>
        /// <returns></returns>
        public static bool HasData(string dataName)
        {
            return PlayerPrefs.HasKey(dataName);
        }

        /// <summary>
        /// Delete specific data
        /// </summary>
        /// <param name="dataName"></param>
        public static void DeleteData(string dataName)
        {
            PlayerPrefs.DeleteKey(dataName);
        }

        /// <summary>
        /// Delete all data
        /// </summary>
        public static void DeleteAll()
        {
            PlayerPrefs.DeleteAll();
        }
    }
}
