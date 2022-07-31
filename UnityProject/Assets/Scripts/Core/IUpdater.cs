public interface IUpdater
{
    void OnMessage(string msg);

    void OnProgress(float progress);

    void OnVersion(string ver);

    void OnLoadSceneProgress(float progress);

    void OnLoadSceneFinish();

    void OnUpdateFailed();
}