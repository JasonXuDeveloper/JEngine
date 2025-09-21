using ILRuntime.Runtime.Enviorment;
using UnityEngine;

namespace JEngine.Helper
{
    public static class HotUpdateLoadedHelper
    {
        public static void Init(AppDomain appDomain)
        {
            Debug.Log("<color=orange>[HotUpdateLoadedHelper.Init] 这个周期在主工程，在RunGame周期后被调用，需要自行修改Assets/Scripts/Helpers/HotUpdateLoadedHelper.cs文件的Init方法</color>");
        }
    }
}