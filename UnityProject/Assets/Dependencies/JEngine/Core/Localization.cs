//
// Localization.cs
//
// Author:
//       JasonXuDeveloper（傑） <jasonxudeveloper@gmail.com>
//
// Copyright (c) 2020 JEngine
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
using System.Collections.Generic;
using System.Linq;
using libx;
using UnityEngine;
using System.Text.RegularExpressions;


namespace JEngine.Core
{
    public class Localization
    {
        private static List<LocalizedText> _texts;
        
        private static Dictionary<string, Dictionary<string, string>> _phrases;//language,key,value

        private static string _language;

        public static string CurrentLanguage
        {
            get
            {
                return _language;
            }
        }

        private static void Init()
        {
            if (!MonoBehaviour.FindObjectOfType<Assets>())
            {
                Log.PrintError("请先初始化XAsset");
                return;
            }
            
            _phrases = new Dictionary<string, Dictionary<string, string>>(0);
            ChangeLanguage(PlayerPrefs.GetString("JEngine.Core.Localization.language",System.Globalization.CultureInfo.InstalledUICulture.Name));
            
            var req = Assets.LoadAsset("Localization.csv",typeof(TextAsset));
            TextAsset file = (TextAsset)req.asset;
            
            //获取全部行
            string[] AllRows = file.text.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries); //忽略空行

            string pattern = ",(?=(?:[^\\" + '"' + "]*\\" + '"' + "[^\\" + '"' + "]*\\" + '"' + ")*[^\\" + '"' + "]*$)";

            //获取语言
            var header = Regex.Split(AllRows[0],pattern);

            for (int i = 1; i < header.Length; i++)
            {
                //某语言
                string lang = _getExactValue(header[i]).ToLower();
                //获取key和value
                Dictionary<string,string> _p = new Dictionary<string, string>();
                for (int j = 1; j < AllRows.Length; j++)
                {
                    //某一行
                    string row = AllRows[j];
                    //切割
                    var cells = Regex.Split(row, pattern);
                    //添加key-value
                    _p.Add(_getExactValue(cells[0]),_getExactValue(cells[i]));
                }
                //添加lang
                _phrases.Add(lang,_p);
            }

        }

        private static string _getExactValue(string val)
        {
            if (val[0] == '"' && val[val.Length - 1] == '"')
            {
                val = val.Substring(1, val.Length - 2);
            }

            char p = '"';
            string pattern = p.ToString() + p.ToString();
            string p2 = p+"";
            val = val.Replace(pattern, p2);
            return val;
        }

        /// <summary>
        /// 更改语言
        /// Change language
        /// </summary>
        /// <param name="lang"></param>
        public static void ChangeLanguage(string lang)
        {
            _language = lang.ToLower();
            PlayerPrefs.SetString("JEngine.Core.Localization.language", _language);
        }

        public static void AddText(LocalizedText lt)
        {
            if (_texts == null)
            {
                _texts = new List<LocalizedText>();
            }
            _texts.Add(lt);
        }

        /// <summary>
        /// 获取key的文字
        /// Find string of key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string GetString(string key)
        {
            if (_phrases == null)
            {
                Init();
            }

            if (!_phrases.ContainsKey(_language))
            {
                string newLang = _phrases.Keys.ToList().Find(k => k.Split('-')[0] == _language.Split('-')[0]);
                if (newLang == null)
                {
                    newLang = "zh-cn";
                }
                Log.PrintError($"不存在语言{_language}，自动替换为{newLang}");
                ChangeLanguage(newLang);
            }

            if (!_phrases[_language].ContainsKey(key))
            {
                return $"[invalid key: {key}]";
            }

            return _phrases[_language][key];
        }
    }
}