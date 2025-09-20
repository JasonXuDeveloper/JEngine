using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace JEngine.Core.Editor
{
    public static class StyleSheetLoader
    {
        private const string CommonStyleSheetName = "JEngineCommon.uss";
        public static StyleSheet LoadPackageStyleSheet(string fileName)
        {
            var packageInfo = UnityEditor.PackageManager.PackageInfo.FindForAssembly(Assembly.GetExecutingAssembly());

            if (packageInfo != null)
            {
                var ussPath = $"{packageInfo.assetPath}/Editor/CustomEditor/{fileName}";
                var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(ussPath);
                if (styleSheet != null)
                    return styleSheet;
            }

            Debug.LogWarning($"Could not load USS file: {fileName} from package");
            return ScriptableObject.CreateInstance<StyleSheet>();
        }

        public static StyleSheet LoadPackageStyleSheet<T>() where T : class
        {
            var fileName = typeof(T).Name + ".uss";
            return LoadPackageStyleSheet(fileName);
        }

        public static StyleSheet LoadCommonStyleSheet()
        {
            return LoadPackageStyleSheet(CommonStyleSheetName);
        }
    }
}