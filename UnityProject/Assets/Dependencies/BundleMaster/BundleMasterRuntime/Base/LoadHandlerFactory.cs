using System.Collections.Generic;

namespace BM
{
    internal static class LoadHandlerFactory
    {
        private static Queue<LoadHandler> _loadHandlerPool = new Queue<LoadHandler>();

        internal static LoadHandler GetLoadHandler(string assetPath, string bundlePackageName, bool haveHandler, bool isPool)
        {
            LoadHandler loadHandler;
            if (_loadHandlerPool.Count > 0 && isPool)
            {
                loadHandler = _loadHandlerPool.Dequeue();
                loadHandler.Init(assetPath, bundlePackageName, haveHandler);
                return loadHandler;
            }
            loadHandler = CreateLoadHandler(assetPath, bundlePackageName, haveHandler, isPool);
            return loadHandler;
        }

        private static LoadHandler CreateLoadHandler(string assetPath, string bundlePackageName, bool haveHandler, bool isPool)
        {
            LoadHandler loadHandler = new LoadHandler(isPool);
            loadHandler.Init(assetPath, bundlePackageName, haveHandler);
            return loadHandler;
        }
        
        internal static void EnterPool(LoadHandler loadHandler)
        {
            _loadHandlerPool.Enqueue(loadHandler);
        }
    }
}