//
// JExtensions.cs
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
using JEngine.UI;
using UnityEngine;
using System.Reflection;

namespace JEngine.Core
{
    public static class JExtensions
    {
        /// <summary>
        /// Get a class created by classbind on a gameObject
        /// 获取通过classbind给gameObject挂载的脚本对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="gameObject"></param>
        /// <returns></returns>
        public static T GetHotClass<T>(this GameObject gameObject) where T : class
        {
            if (typeof(T) == typeof(Component) || typeof(T).IsSubclassOf(typeof(MonoBehaviour)))
            {
                return gameObject.GetComponent<T>();
            }
            else
            {
                object result = Tools.GetHotComponent(gameObject, typeof(T).FullName);
                if (result == null)
                {
                    return null;
                }
                if(((T[])result).Length == 0)
                {
                    return null;
                }
                return ((T[])result)[0];
            }
        }

        /// <summary>
        /// Get all class created by classbind on a gameObject
        /// 获取全部通过classbind给gameObject挂载的脚本对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="gameObject"></param>
        /// <returns></returns>
        public static T[] GetHotClasses<T>(this GameObject gameObject) where T : class
        {
            if (typeof(T) == typeof(Component) || typeof(T).IsSubclassOf(typeof(MonoBehaviour)))
            {
                return gameObject.GetComponents<T>();
            }
            else
            {
                object result = Tools.GetHotComponent(gameObject, typeof(T).FullName);
                if (result == null)
                {
                    return null;
                }
                if (((T[])result).Length == 0)
                {
                    return null;
                }
                return (T[])result;
            }
        }

        /// <summary>
        /// Get a class created by classbind on a gameObject's children
        /// 获取通过classbind给gameObject的子物体挂载的脚本对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="gameObject"></param>
        /// <returns></returns>
        public static T GetHotClassInChildren<T>(this GameObject gameObject) where T : class
        {
            if (typeof(T) == typeof(Component) || typeof(T).IsSubclassOf(typeof(MonoBehaviour)))
            {
                return gameObject.GetComponentInChildren<T>();
            }
            else
            {
                object result = Tools.GetHotComponentInChildren(gameObject, typeof(T).FullName);
                if (result == null)
                {
                    return null;
                }
                if (((T[])result).Length == 0)
                {
                    return null;
                }
                return ((T[])result)[0];
            }
        }

        /// <summary>
        /// Get all class created by classbind on a gameObject's children
        /// 获取全部通过classbind给gameObject的子物体挂载的脚本对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="gameObject"></param>
        /// <returns></returns>
        public static T[] GetHotClassesInChildren<T>(this GameObject gameObject) where T : class
        {
            if (typeof(T) == typeof(Component) || typeof(T).IsSubclassOf(typeof(MonoBehaviour)))
            {
                return gameObject.GetComponentsInChildren<T>();
            }
            else
            {
                object result = Tools.GetHotComponentInChildren(gameObject, typeof(T).FullName);
                if (result == null)
                {
                    return null;
                }
                if (((T[])result).Length == 0)
                {
                    return null;
                }
                return (T[])result;
            }
        }

        /// <summary>
        /// Create a JBehaviour on a gameObject
        /// 在gameobject上创建JBehaviour
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="gameObject"></param>
        /// <param name="activeAfter"></param>
        /// <returns></returns>
        public static T CreateJBehaviour<T>(this GameObject gameObject, bool activeAfter = true) where T : JBehaviour
        {
            return JBehaviour.CreateOn<T>(gameObject, activeAfter);
        }

        /// <summary>
        /// Get a JBehaviour on a gameObject
        /// 在gameObject上获取JBehaviour
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="gameObject"></param>
        /// <returns></returns>
        public static T GetJBehaviour<T>(this GameObject gameObject) where T : JBehaviour
        {
            return JBehaviour.GetJBehaviour<T>(gameObject);
        }

        /// <summary>
        /// Get all JBehaviour on a gameObject
        /// 在gameObject上获取全部JBehaviour
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="gameObject"></param>
        /// <returns></returns>
        public static T[] GetJBehaviours<T>(this GameObject gameObject) where T : JBehaviour
        {
            return JBehaviour.GetJBehaviours<T>(gameObject);
        }

        /// <summary>
        /// Remove a JBehaviour
        /// 销毁JBehaviour
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="jBehaviour"></param>
        /// <returns></returns>
        public static void Remove(this JBehaviour jBehaviour)
        {
            JBehaviour.RemoveJBehaviour(jBehaviour);
        }

        /// <summary>
        /// Create JUI on a gameObject
        /// 在gameObject上创建JUI
        /// </summary>
        /// <param name="gameObject"></param>
        /// <returns></returns>
        public static JUI CreateJUI(this GameObject gameObject)
        {
            return JBehaviour.CreateOn<JUI>(gameObject, false);
        }

        /// <summary>
        /// Get a JUI on a gameObject
        /// 在gameObject上获取JUI
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="gameObject"></param>
        /// <returns></returns>
        public static JUI GetJUI(this GameObject gameObject)
        {
            return JBehaviour.GetJBehaviour<JUI>(gameObject);
        }

        /// <summary>
        /// Get a JUI on a gameObject
        /// 在gameObject上获取全部JUI
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="gameObject"></param>
        /// <returns></returns>
        public static JUI[] GetJUIs(this GameObject gameObject)
        {
            return JBehaviour.GetJBehaviours<JUI>(gameObject);
        }


        /// <summary>
        /// Remove a JUI
        /// 销毁JUI
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="jBehaviour"></param>
        /// <returns></returns>
        public static void Remove(this JUI jUI)
        {
            JBehaviour.RemoveJBehaviour(jUI);
        }
    }
}
