using UnityEngine;

namespace JEngine.Core
{
    public partial class Updater : MonoBehaviour
    {
        [SerializeField] [Tooltip("热更资源下载地址")] private string resourceUrl = "http://127.0.0.1:7888/";

        [SerializeField] [Tooltip("热更资源下载备用地址")]
        private string fallbackUrl = "http://127.0.0.1:7888/";

        [SerializeField] private string gameScene = "Assets/HotUpdateResources/Main/Scene/Game.unity";
        [SerializeField] private string mainPackageName = "Main";

        [Tooltip("Simulate是开发模式，Standalone是离线模式，Remote是真机模式")] [SerializeField]
        private UpdateMode mode = UpdateMode.Simulate;

        /// <summary>
        /// 单例
        /// </summary>
        public static Updater Instance => _instance;

        /// <summary>
        /// 主包
        /// </summary>
        public static string MainPackageName => _instance.mainPackageName;

        /// <summary>
        /// 模式
        /// </summary>
        public static UpdateMode Mode => _instance.mode;

        /// <summary>
        /// 热更资源下载地址
        /// </summary>
        public static string ResourceUrl => _instance.resourceUrl;

        /// <summary>
        /// 热更资源下载备用地址
        /// </summary>
        public static string FallbackUrl => _instance.fallbackUrl;

        /// <summary>
        /// 更新模式
        /// </summary>
        public enum UpdateMode : byte
        {
            Simulate = 0,
            Standalone = 1,
            Remote = 2,
        }

        /// <summary>
        /// 单例
        /// </summary>
        private static Updater _instance;

        /// <summary>
        /// 更新配置
        /// </summary>
        private void Awake()
        {
            if (_instance != null)
            {
                DestroyImmediate(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        /// <summary>
        /// 给按钮拖拽赋值的点击事件，下载更新，用于初始化主包
        /// </summary>
        public void StartUpdate()
        {
            var updater = FindObjectOfType<UpdateScreen>();
            updater.sceneName = gameScene;
            _ = AssetMgr.UpdatePackage(mainPackageName, updater);
        }

        private void OnDestroy()
        {
            MessageBox.Dispose();
        }

        public static void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}