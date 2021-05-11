//
// UpdateScreen.cs
//
// Author:
//       fjy <jiyuan.feng@live.com>
//
// Copyright (c) 2020 fjy
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
using UnityEngine.UI;

namespace libx
{
    public class UpdateScreen : MonoBehaviour, IUpdater
    {
        public Button buttonStart;
        public Slider progressBar;
        public Text progressText;
        public Text version;

        private void Start()
        {
            try
            {
                version.text = "资源版本号: v" + Application.version + "res" +
                               Versions.LoadVersion(Application.persistentDataPath + "/DLC/" + Versions.Filename);
            }
            catch (Exception e)
            {
                version.text = "初始版本";
            }

            var updater = FindObjectOfType<Updater>();
            updater.listener = this;
        }

        #region IUpdateManager implementation

        public void OnStart()
        {
            buttonStart.gameObject.SetActive(false);
        }

        public void OnMessage(string msg)
        {
            progressText.text = msg;
        }

        public void OnProgress(float progress)
        {
            progressBar.value = progress;
        }

        public void OnVersion(string ver)
        {
            version.text = ver;
        }


        public void OnClear()
        {
            buttonStart.gameObject.SetActive(true);
        }

        #endregion
    }
}