using YooAsset;

namespace JEngine.Core
{
    public class QueryStreamingAssetsFileServices : IQueryServices
    {
        public bool QueryStreamingAssets(string fileName)
        {
            // StreamingAssetsHelper.cs是太空战机里提供的一个查询脚本。
            string buildinFolderName = YooAssets.GetStreamingAssetBuildinFolderName();
            return StreamingAssetsHelper.FileExists($"{buildinFolderName}/{fileName}");
        }
    }
}