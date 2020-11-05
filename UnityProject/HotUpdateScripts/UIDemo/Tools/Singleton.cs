namespace HotUpdateScripts
{
    /// <summary>
    /// 单例基类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Singleton<T> where T : new()
    {
        protected Singleton() { }

        protected static T _inst = new T();
        public static T Instance
        {
            get
            {
                if (null == _inst)
                    _inst = new T();
                return _inst;
            }
        }
    }
}