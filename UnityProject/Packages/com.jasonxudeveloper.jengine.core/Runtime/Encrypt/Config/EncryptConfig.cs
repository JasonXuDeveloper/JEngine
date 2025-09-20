// EncryptConfig.cs
// 
//  Author:
//        JasonXuDeveloper <jason@xgamedev.net>
// 
//  Copyright (c) 2025 JEngine
// 
//  Permission is hereby granted, free of charge, to any person obtaining a copy
//  of this software and associated documentation files (the "Software"), to deal
//  in the Software without restriction, including without limitation the rights
//  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//  copies of the Software, and to permit persons to whom the Software is
//  furnished to do so, subject to the following conditions:
// 
//  The above copyright notice and this permission notice shall be included in
//  all copies or substantial portions of the Software.
// 
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//  THE SOFTWARE.

using System.IO;
using UnityEditor;
using UnityEngine;

namespace JEngine.Core.Encrypt.Config
{
    public abstract class EncryptConfig<T, TKey> : ScriptableObject where T : EncryptConfig<T, TKey>
    {
        public TKey key;

        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    const string path = "EncryptConfigs/";
                    _instance = Resources.Load<T>(Path.Combine(path, typeof(T).Name));
                    if (_instance == null)
                    {
#if UNITY_EDITOR
                        var instance = CreateInstance<T>();
                        if (!AssetDatabase.IsValidFolder(Path.Combine("Assets/Resources", path)))
                        {
                            AssetDatabase.CreateFolder("Assets/Resources", "EncryptConfigs");
                        }

                        AssetDatabase.CreateAsset(instance,
                            Path.Combine("Assets/Resources", path, typeof(T).Name + ".asset"));
                        AssetDatabase.SaveAssets();
                        AssetDatabase.Refresh();
                        _instance = instance;
#else
                        Debug.LogError($"[JEngine] EncryptConfig<{typeof(T).Name}> not found in Resources/{path}. Please create it in the Editor first.");
#endif
                    }
                }

                return _instance;
            }
        }

        private static T _instance;
        
        public abstract void RegenerateKey();
    }
}