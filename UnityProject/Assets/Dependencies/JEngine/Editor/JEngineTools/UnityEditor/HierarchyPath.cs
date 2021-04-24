using UnityEditor;
using UnityEngine;

namespace  JEngine.Editor
{
    internal class HierarchyPath : MonoBehaviour
    {
        private static readonly TextEditor CopyTool = new TextEditor();

        /// <summary>
        /// 将一个GameObject在Hierarchy中的完整路径拷贝的剪切板
        /// </summary>
        [MenuItem("GameObject/JEngine/Copy Path %#&C",priority = 0)]
        static void CopyTransPath()
        {
            Transform trans = Selection.activeTransform;
            if (null == trans) return;
            CopyTool.text = GetTransPath(trans);
            CopyTool.SelectAll();
            CopyTool.Copy();
        }

        /// <summary>
        /// 获得GameObject在Hierarchy中的完整路径
        /// </summary>
        public static string GetTransPath(Transform trans)
        {
            if (null == trans) return string.Empty;
            if (null == trans.parent) return trans.name;
            return GetTransPath(trans.parent) + "/" + trans.name;
        }
    }
}
