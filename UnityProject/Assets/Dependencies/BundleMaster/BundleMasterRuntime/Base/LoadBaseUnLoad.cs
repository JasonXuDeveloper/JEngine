namespace BM
{
    public partial class LoadBase
    {
        /// <summary>
        /// 卸载AssetBundle
        /// </summary>
        public void UnLoad()
        {
            _assetBundleCreateRequest = null;
            AssetBundle.Unload(true);
            _loadState = LoadState.NoLoad;
        }
    }
}