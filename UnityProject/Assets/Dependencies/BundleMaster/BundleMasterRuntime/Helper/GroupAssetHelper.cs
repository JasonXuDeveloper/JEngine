using System.Collections.Generic;

namespace BM
{
    public static class GroupAssetHelper
    {
        /// <summary>
        /// 判断是否是颗粒化组的文件
        /// </summary>
        /// <returns>组路径</returns>
        public static string IsGroupAsset(string assetPath, List<string> loadGroupKey)
        {
            for (int i = 0; i < loadGroupKey.Count; i++)
            {
                if (assetPath.Contains(loadGroupKey[i]))
                {
                    return loadGroupKey[i];
                }
            }
            return null;
        }
        
    }
}