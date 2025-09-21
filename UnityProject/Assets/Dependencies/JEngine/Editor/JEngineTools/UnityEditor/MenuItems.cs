//
// MenuItems.cs
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

#if UNITY_EDITOR
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using ILRuntime.CLR.TypeSystem;
using ILRuntime.Mono.Cecil.Pdb;
using ILRuntime.Runtime.Enviorment;
using JEngine.Core;
using UnityEditor;
using UnityEngine;

namespace JEngine.Editor
{
    [Obfuscation(Exclude = true)]
    internal class MenuItems
    {
        [MenuItem("JEngine/Open Documentation", priority = 1999)]
        public static void OpenDocument()
        {
            Application.OpenURL("https://docs.xgamedev.net/");
        }

        [MenuItem("JEngine/Open on Github", priority = 2000)]
        public static void OpenGithub()
        {
            Application.OpenURL("https://github.com/JasonXuDeveloper/JEngine");
        }
    }
}
#endif