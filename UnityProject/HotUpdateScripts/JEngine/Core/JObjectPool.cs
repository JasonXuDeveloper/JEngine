//
// JObjectPool.cs
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
using System.Collections.Generic;
using UnityEngine;

namespace JEngine.Core
{
    public class JGameObjectPool : MonoBehaviour
    {
        public GameObject OriginalObj;
        public Transform Parent;
        public int pooledAmount = 10;
        public bool lockPool;


        private List<GameObject> pooledObjects;
        private int currentIndex;

        /// <summary>
        /// 初始化
        /// Initialize
        /// </summary>
        private void Awake()
        {
            if (Parent == null)
            {
                Parent = this.gameObject.transform;
            }
            pooledObjects = new List<GameObject>();
            for (int i = 0; i < pooledAmount; ++i)
            {
                GameObject obj = Create();
                obj.SetActive(false);
                pooledObjects.Add(obj);
            }
        }

        /// <summary>
        /// Get an object from the pool
        /// </summary>
        /// <returns></returns>
        public GameObject PoolObject
        {
            get
            {
                for (int i = 0; i < pooledObjects.Count; ++i)
                {
                    int tempIndex = (currentIndex + i) % pooledObjects.Count;
                    if (!pooledObjects[tempIndex].activeInHierarchy)
                    {
                        currentIndex = (tempIndex + 1) % pooledObjects.Count;
                        return pooledObjects[tempIndex];
                    }
                }

                if (!lockPool)
                {
                    GameObject obj = Create();
                    pooledObjects.Add(obj);
                    return obj;
                }

                return null;
            }
        }


        /// <summary>
        /// Private methods that create GameObject
        /// </summary>
        /// <returns></returns>
        private GameObject Create()
        {
            return Instantiate(OriginalObj, parent: Parent);
        }
    }
}
