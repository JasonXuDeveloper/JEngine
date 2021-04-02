using UnityEditor;

namespace JEngine.Editor
{
    internal static class AllowUnsafe
    {
        public static void Update()
        {
            if (!PlayerSettings.allowUnsafeCode)
            {
                PlayerSettings.allowUnsafeCode = true;
            }
        }
    }
}