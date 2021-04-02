//
// GameStats.cs
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

using UnityEngine;

namespace JEngine.Core
{
    public class GameStats : MonoBehaviour
    {
        public static float FPS => _frames;
        public static bool Debug;
        public static long TotalFrames => _totalFrames;

        private static int _frames = 0;
        private static float _timer = 1;
        private static long _totalFrames = 0;
        private static long _encryptedCounts = 0;

        public static void Initialize()
        {
            if (GameObject.Find("GameStats")) return;
            var go = new GameObject("GameStats");
            go.AddComponent<GameStats>();
            DontDestroyOnLoad(go);
        }
        
        private void Update()
        {
            //进入热更了再开始
            if (Debug && Init.Success)
            {
                //仅限于Editor的部分
                #if UNITY_EDITOR
                if (_encryptedCounts != Init.EncryptedCounts)
                {
                    var diff = Init.EncryptedCounts - _encryptedCounts;
                    Log.Print($"第{_totalFrames}帧JStream总共将热更DLL分为了{Init.EncryptedCounts}块，新增{diff}块，进行解释执行");
                    _encryptedCounts = Init.EncryptedCounts;
                }
                #endif
            }
        }


        void FixedUpdate()
        {
            //增加帧率
            ++_frames;
            ++_totalFrames;
            
            //计时器刷新
            _timer -= Time.deltaTime;
            
            //如果计时器时间到了，就更新
            if (!(_timer <= 0)) return;
            _frames = 0;
            _timer = 1;
        }
    }
}