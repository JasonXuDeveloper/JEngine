using ET;

namespace BM
{
    public abstract class LoadHandlerBase
    {
        /// <summary>
        /// 是否是通过路径加载的
        /// </summary>
        internal bool HaveHandler = true;
        
        /// <summary>
        /// 对应的加载的资源的路径
        /// </summary>
        protected string AssetPath;

        /// <summary>
        /// 唯一ID
        /// </summary>
        public uint UniqueId = 0;
        
        /// <summary>
        /// 所属分包的名称
        /// </summary>
        protected string BundlePackageName;
        
        /// <summary>
        /// 加载完成的计数
        /// </summary>
        protected int RefLoadFinishCount = 0;
        
        /// <summary>
        /// 是否卸载标记位
        /// </summary>
        protected bool UnloadFinish = false;
        
        /// <summary>
        /// 加载计数器(负责完成所有依赖的Bundle加载完成)
        /// </summary>
        protected async ETTask LoadAsyncLoader(LoadBase loadBase, ETTask baseTcs)
        {
            ETTask tcs = ETTask.Create(true);
            loadBase.LoadAssetBundleAsync(tcs, BundlePackageName).Coroutine();
            await tcs;
            RefLoadFinishCount--;
            if (RefLoadFinishCount == 0)
            {
                baseTcs.SetResult();
            }
            if (RefLoadFinishCount < 0)
            {
                AssetLogHelper.LogError("资源加载引用计数不正确: " + RefLoadFinishCount);
            }
        }
        
        public void UnLoad()
        {
            if (AssetComponentConfig.AssetLoadMode == AssetLoadMode.Develop)
            {
                return;
            }
            if (UnloadFinish)
            {
                AssetLogHelper.LogError(AssetPath + "已经卸载完了");
                return;
            }
            AssetComponent.BundleNameToRuntimeInfo[BundlePackageName].UnLoadHandler.Remove(UniqueId);
            //减少引用数量
            ClearAsset();
            UnloadFinish = true;
        }
        
        /// <summary>
        /// 子类需要实现清理资源引用的逻辑
        /// </summary>
        protected abstract void ClearAsset();
    }
}