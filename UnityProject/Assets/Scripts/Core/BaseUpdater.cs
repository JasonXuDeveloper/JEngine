using System;

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

    public Action onLoadSceneFinish;
    public void OnLoadSceneFinish()
    {
        onLoadSceneFinish?.Invoke();
    }
        
    public Action onUpdateFailed;
    public void OnUpdateFailed()
    {
        onUpdateFailed?.Invoke();
    }
        

    public BaseUpdater(Action<string> onMessage, Action<float> onProgress, Action<string> onVersion, Action<float> onLoadSceneProgress, Action onLoadSceneFinish, Action onUpdateFailed)
    {
        this.onMessage = onMessage;
        this.onProgress = onProgress;
        this.onVersion = onVersion;
        this.onLoadSceneProgress = onLoadSceneProgress;
        this.onLoadSceneFinish = onLoadSceneFinish;
        this.onUpdateFailed = onUpdateFailed;
    }
}