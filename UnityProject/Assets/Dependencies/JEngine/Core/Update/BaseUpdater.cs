using System;

namespace JEngine.Core
{
    public class BaseUpdater : IUpdater
    {
        public Action<string> onMessage;

        public void OnMessage(string msg)
        {
            onMessage?.Invoke(msg);
        }

        public Action<float> onProgress;

        public void OnProgress(float progress)
        {
            onProgress?.Invoke(progress);
        }

        public Action<string> onVersion;

        public void OnVersion(string ver)
        {
            onVersion?.Invoke(ver);
        }

        public Action<float> onLoadSceneProgress;

        public void OnLoadSceneProgress(float progress)
        {
            onLoadSceneProgress?.Invoke(progress);
        }

        public Action<bool> onUpdateFinish;

        public void OnUpdateFinish(bool result)
        {
            onUpdateFinish?.Invoke(result);
        }

        public Action onLoadSceneFinish;

        public void OnLoadSceneFinish()
        {
            onLoadSceneFinish?.Invoke();
        }


        public BaseUpdater(Action<string> onMessage, Action<float> onProgress, Action<string> onVersion,
            Action<float> onLoadSceneProgress, Action onLoadSceneFinish, Action<bool> onUpdateFinish)
        {
            this.onMessage = onMessage;
            this.onProgress = onProgress;
            this.onVersion = onVersion;
            this.onLoadSceneProgress = onLoadSceneProgress;
            this.onUpdateFinish = onUpdateFinish;
            this.onLoadSceneFinish = onLoadSceneFinish;
        }
    }
}