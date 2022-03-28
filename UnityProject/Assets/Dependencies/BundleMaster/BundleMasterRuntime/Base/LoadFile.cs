namespace BM
{
    /// <summary>
    /// 一个文件以及它的bundle包信息
    /// </summary>
    public class LoadFile : LoadBase
    {
        /// <summary>
        /// 依赖的文件的名字
        /// </summary>
        public string[] DependFileName;
        
        public LoadFile()
        {
            base.LoadType = LoadType.File;
        }

    }
}
