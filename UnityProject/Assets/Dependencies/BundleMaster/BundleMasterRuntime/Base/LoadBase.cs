namespace BM
{
    /// <summary>
    /// 加载类型的基类
    /// </summary>
    public partial class LoadBase
    {
        public string FilePath;
        public string AssetBundleName;
        protected LoadType LoadType = LoadType.Null;
    }

    public enum LoadType
    {
        Null = 0,
        File = 1,
        Depend = 2,
        Group = 3
    }
}