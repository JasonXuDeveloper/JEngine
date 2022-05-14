using UnityEngine;

namespace JEngine.Core
{
    public class GameStats : MonoBehaviour
    {
        public static float FPS => _backupFrames;
        public static bool Debug;
        public static long TotalFrames => _totalFrames;

        private static int _frames;
        private static int _backupFrames;
        private static float _timer = 1;
        private static long _totalFrames;
        private static long _encryptedCounts;

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
            if (Debug && InitJEngine.Success)
            {
                //仅限于Editor的部分
#if UNITY_EDITOR
                //待插入脚本
#endif
            }
        }


        void FixedUpdate()
        {
            //增加帧率
            ++_frames;
            ++_backupFrames;
            ++_totalFrames;
            
            //计时器刷新
            _timer -= Time.deltaTime;
            
            //如果计时器时间到了，就更新
            if (!(_timer <= 0)) return;
            _backupFrames = _frames;
            _frames = 0;
            _timer = 1;
        }
    }
}