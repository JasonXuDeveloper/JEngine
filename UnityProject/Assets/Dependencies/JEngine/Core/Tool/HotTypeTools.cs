using System;
using System.IO;
using UnityEngine;
using System.Reflection;
using System.Threading.Tasks;
using ILRuntime.CLR.TypeSystem;
using ILRuntime.Mono.Cecil.Pdb;
using ILRuntime.Runtime.Intepreter;
using AppDomain = ILRuntime.Runtime.Enviorment.AppDomain;

namespace JEngine.Core
{
    public static partial class Tools
    {
        /// <summary>
        /// 缓存domain
        /// </summary>
        private static AppDomain _cacheDomain;

        /// <summary>
        /// ILRuntime的Appdomain
        /// </summary>
        public static AppDomain Domain
        {
            get
            {
                if (Application.isPlaying && InitJEngine.Appdomain != null)
                {
                    return InitJEngine.Appdomain;
                }

                if (_cacheDomain != null)
                {
                    _cacheDomain.Dispose();
                }

                _cacheDomain = new AppDomain();
                // 只有编辑器才会走到这
                Task.Run(async () =>
                {
                    _cacheDomain.LoadAssembly(new MemoryStream(await DllMgr.GetDllBytes(ConstMgr.MainHotDLLName, true)), null,
                        new PdbReaderProvider());
                    InitJEngine.InitializeILRuntime(_cacheDomain);
                }).Wait();
                return _cacheDomain;
            }
        }

        /// <summary>
        /// 通过字符串获取热更类型
        /// </summary>
        /// <param name="typename"></param>
        /// <returns></returns>
        public static Type GetHotType(string typename)
        {
            AppDomain ad = Domain;
            var t = ad.GetType(typename);
            return t?.ReflectionType;
        }

        /// <summary>
        /// 通过字符串获取热更IL类型
        /// </summary>
        /// <param name="typename"></param>
        /// <returns></returns>
        public static IType GetHotILType(string typename)
        {
            AppDomain ad = Domain;
            var t = ad.GetType(typename);
            return t;
        }

        /// <summary>
        /// 通过字符串获取热更类型实例
        /// </summary>
        /// <param name="typename"></param>
        /// <returns></returns>
        public static ILTypeInstance GetHotInstance(string typename)
        {
            AppDomain ad = Domain;
            var t = ad.GetType(typename);
            if (t == null) return null;
            return ad.Instantiate(typename);
        }

        /// <summary>
        /// 判断是否包含热更类型
        /// </summary>
        /// <param name="typename"></param>
        /// <returns></returns>
        public static bool HasHotType(string typename)
        {
            AppDomain ad = Domain;
            bool ret = ad.LoadedTypes.ContainsKey(typename);
            return ret;
        }

        /// <summary>
        /// 判断是否继承了JBehaviour
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsJBehaviourType(Type type)
        {
            Type jType = GetHotType("JEngine.Core.JBehaviour");
            if (jType == null)
            {
                return false;
            }

            return type.IsSubclassOf(jType);
        }

        /// <summary>
        /// 判断是不是继承了JBehaviour
        /// </summary>
        /// <param name="typename"></param>
        /// <returns></returns>
        public static bool IsJBehaviourType(string typename)
        {
            AppDomain ad = Domain;
            var t = ad.GetType(typename);
            var jb = ad.GetType("JEngine.Core.JBehaviour");
            bool ret = t.CanAssignTo(jb);
            return ret;
        }

        /// <summary>
        /// 调用热更方法
        /// </summary>
        /// <param name="type"></param>
        /// <param name="method"></param>
        public static void InvokeHotMethod(string type, string method)
        {
            InitJEngine.Appdomain.Invoke(type, method, ConstMgr.NullObjects, ConstMgr.NullObjects);
        }

        /// <summary>
        /// 调用热更方法
        /// </summary>
        /// <param name="type"></param>
        /// <param name="method"></param>
        /// <param name="instance"></param>
        /// <param name="param"></param>
        public static void InvokeHotMethod(string type, string method, object instance, params object[] param)
        {
            InitJEngine.Appdomain.Invoke(type, method, instance, param);
        }

        /// <summary>
        /// 获取热更方法的全部参数
        /// </summary>
        /// <param name="type"></param>
        /// <param name="methodName"></param>
        /// <returns></returns>
        public static ParameterInfo[] GetHotMethodParams(Type type, string methodName)
        {
            return type.GetMethod(methodName)?.GetParameters();
        }

        /// <summary>
        /// 判断是否可以被分配到类型
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="type"></param>
        public static bool CanAssignTo(this object instance, Type type)
        {
            return ((ILTypeInstance)instance).Type.CanAssignTo(InitJEngine.Appdomain.GetType(type.FullName));
        }
    }
}