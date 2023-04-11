namespace JEngine.Core
{
    public interface IUpdater
    {
        void OnMessage(string msg);

        void OnProgress(float progress);

        void OnVersion(string ver);

        void OnUpdateFinish(bool result);
    }
}