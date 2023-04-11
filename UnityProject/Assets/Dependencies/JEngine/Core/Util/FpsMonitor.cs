using UnityEngine;

namespace JEngine.Core
{
    public static class FpsMonitor
    {
        public static float FPS => _backupFrames;
        public static long TotalFrames => _totalFrames;

        private static int _frames;
        private static int _backupFrames;
        private static float _timer = 1;
        private static long _totalFrames;

        public static void Initialize()
        {
            _ = LifeCycleMgr.Instance.AddUpdateTask(Update);
        }

        private static void Update()
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
            //进入热更了再开始
            if (!InitJEngine.Success) return;
        }
    }
}