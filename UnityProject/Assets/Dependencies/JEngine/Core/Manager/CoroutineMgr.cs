using System.Collections;
using System.Collections.Concurrent;
using UnityEngine;

namespace JEngine.Core
{
    public class CoroutineMgr: MonoBehaviour
    {
        /// <summary>
        /// 初始化
        /// </summary>
        public static void Initialize()
        {
            if (_instance != null)
            {
                return;
            }

            _instance = new GameObject("CoroutineMgr").AddComponent<CoroutineMgr>();
            DontDestroyOnLoad(_instance);
        }

        /// <summary>
        /// 单例
        /// </summary>
        private static CoroutineMgr _instance;

        /// <summary>
        /// 单例
        /// </summary>
        public static CoroutineMgr Instance => _instance;
        
        /// <summary>
        /// 执行过的迭代器
        /// </summary>
        private readonly ConcurrentDictionary<IEnumerator, Coroutine> _coroutineMap = new ConcurrentDictionary<IEnumerator, Coroutine>();

        /// <summary>
        /// Start a coroutine
        /// </summary>
        /// <param name="enumerator"></param>
        /// <returns></returns>
        public new Coroutine StartCoroutine(IEnumerator enumerator)
        {
            var ret = base.StartCoroutine(enumerator);
            _coroutineMap[enumerator] = ret;
            return ret;
        }
        
        /// <summary>
        /// Stop a coroutine
        /// </summary>
        /// <param name="enumerator"></param>
        public new void StopCoroutine(IEnumerator enumerator)
        {
            if(_coroutineMap.TryGetValue(enumerator,out var coroutine))
            {
                base.StopCoroutine(coroutine);
                _coroutineMap.TryRemove(enumerator,out _);
            }
            base.StopCoroutine(enumerator);
        }
        
        /// <summary>
        /// Stop a coroutine
        /// </summary>
        /// <param name="coroutine"></param>
        public new void StopCoroutine(Coroutine coroutine)
        {
            base.StopCoroutine(coroutine);
            foreach (var item in _coroutineMap)
            {
                if(item.Value == coroutine)
                {
                    _coroutineMap.TryRemove(item.Key,out _);
                    break;
                }
            }
        }
        
        /// <summary>
        /// Stop all coroutines
        /// </summary>
        public new void StopAllCoroutines()
        {
            foreach (var coroutine in _coroutineMap.Values)
            {
                base.StopCoroutine(coroutine);
            }
            _coroutineMap.Clear();
            base.StopAllCoroutines();
        }
    }
}