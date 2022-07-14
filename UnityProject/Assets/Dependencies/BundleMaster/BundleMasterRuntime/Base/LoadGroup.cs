using System.Collections.Generic;

namespace BM
{
    public class LoadGroup : LoadBase
    {
        /// <summary>
        /// 组Bundle里所有资源
        /// </summary>
        public List<string> FilePathList = new List<string>();

        /// <summary>
        /// 依赖的文件的名字
        /// </summary>
        public List<string> DependFileName = new List<string>();
        
        public LoadGroup()
        {
            base.LoadType = LoadType.Group;
        }
    }
}