using UnityEditor;

namespace JEngine.Editor
{
    [InitializeOnLoad]
    internal class EditorUpdate
    {
        /// <summary>
        /// 注册各种Update
        /// </summary>
        static EditorUpdate()
        {
            if (!Clean.hasAdded)
            {
                EditorApplication.update += Clean.Update; //处理DLL
            }

            if (!AllowUnsafe.hasAdded)
            {
                EditorApplication.update += AllowUnsafe.Update; //允许Unsafe code
            }

            if (!SetData.hasAdded)
            {
                EditorApplication.update += SetData.Update; //设置某些需要在后台设置的东西
            }
        }
    }
}