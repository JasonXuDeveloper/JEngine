using UnityEngine.Networking;

namespace BM
{
    public class WebLoadProgress
    {
        internal UnityWebRequestAsyncOperation WeqOperation;

        internal float GetWebProgress()
        {
            if (WeqOperation != null)
            {
                return WeqOperation.progress;
            }
            return 1;
        }
    }
}