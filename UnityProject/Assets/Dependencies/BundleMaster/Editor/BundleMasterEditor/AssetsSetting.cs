using UnityEngine;

namespace BM
{
    public class AssetsSetting : ScriptableObject
    {
        [Header("分包名字")]
        [Tooltip("当前分包的包名(建议英文)")] public string BuildName;
        
        [Header("版本索引")]
        [Tooltip("表示当前Bundle的索引")] public int BuildIndex;

    }
}