namespace JEngine.Editor
{
    internal static class SetData
    {
        public static bool hasAdded;
        
        public static void Update()
        {
            hasAdded = true;
            Setting.SetPrefix();
        }
    }
}