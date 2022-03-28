namespace BM
{
    /// <summary>
    /// 一个文件的依赖项
    /// </summary>
    public class LoadDepend : LoadBase
    {
        public LoadDepend()
        {
            base.LoadType = LoadType.Depend;
        }
    }
}