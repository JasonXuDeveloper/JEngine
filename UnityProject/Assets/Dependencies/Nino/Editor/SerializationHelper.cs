#if UNITY_2017_1_OR_NEWER
using System.IO;
using System.Linq;
using UnityEditor;
using System.Reflection;
using Nino.Serialization;

namespace Nino.Editor
{
    public static class SerializationHelper
    {
        /// <summary>
        /// External DLL, such as ILRuntime/huatuo hot update dll
        /// 外部dll，例如ILRuntime和huatuo的热更dll
        /// </summary>
        private static readonly string[] ExternalDLLPath = new string[]
        {
#if ILRuntime
            "Assets/Nino/Test/Editor/Serialization/Test11.bytes"
#endif
        };
        
        /// <summary>
        /// Export path, you can change this to export it anywhere under Assets directory
        /// If you want to export outside of Assets, you can use ../
        /// 导出目录，你可以修改这个路径，代码会生成到Assets目录下你指定的这个路径内
        /// 如果需要导出到Assets外部，可以在路径内写入../
        /// </summary>
        private const string ExportPath = "Nino/Generated";
        
        [MenuItem("Nino/Generator/Serialization Code")]
        public static void GenerateSerializationCode()
        {
            var assemblies = System.AppDomain.CurrentDomain.GetAssemblies().ToList();
            assemblies.AddRange(ExternalDLLPath.Select(s => Assembly.Load(File.ReadAllBytes(s))));
            // ReSharper disable RedundantArgumentDefaultValue
            CodeGenerator.GenerateSerializationCodeForAllTypePossible(ExportPath, assemblies.ToArray());
            AssetDatabase.Refresh();
            // ReSharper restore RedundantArgumentDefaultValue
        }
    }
}
#endif