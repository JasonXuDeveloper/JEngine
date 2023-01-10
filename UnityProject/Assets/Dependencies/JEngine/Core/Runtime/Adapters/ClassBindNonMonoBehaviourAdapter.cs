using System;
using System.Reflection;
using ILRuntime.CLR.Method;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.Runtime.Intepreter;
using UnityEngine;
using AppDomain = ILRuntime.Runtime.Enviorment.AppDomain;

namespace JEngine.Core.DO_NOT_USE
{
    public class ClassBindNonMonoBehaviour : MonoBehaviour
    {

    }

    public class ClassBindNonMonoBehaviourAdapter : CrossBindingAdaptor
    {
        public override Type BaseCLRType => typeof(ClassBindNonMonoBehaviour);

        public override Type AdaptorType => typeof(Adaptor);

        public override object CreateCLRInstance(AppDomain appdomain, ILTypeInstance instance)
        {
            return new Adaptor(appdomain, instance);
        }

        //为了完整实现MonoBehaviour的所有特性，这个Adapter还得扩展，这里只抛砖引玉，只实现了最常用的Awake, Start和Update
        public class Adaptor : ClassBindNonMonoBehaviour, CrossBindingAdaptorType
        {
            ILTypeInstance _instance;
            AppDomain _appdomain;

            public Adaptor()
            {

            }

            public Adaptor(AppDomain appdomain, ILTypeInstance instance)
            {
                this._appdomain = appdomain;
                this._instance = instance;
            }

            public ILTypeInstance ILInstance
            {
                get => _instance;
                set => _instance = value;
            }

            public AppDomain AppDomain
            {
                get => _appdomain;
                set => _appdomain = value;
            }

            private bool _destoryed;

            public bool isJBehaviour;
            IMethod _mAwakeMethod;
            public bool awaked;
            public bool isAwaking;

            public void Awake()
            {
                if (awaked)
                {
                    return;
                }

                //Unity会在ILRuntime准备好这个实例前调用Awake，所以这里暂时先不掉用
                if (_instance != null)
                {
                    if (isAwaking) return;
                    isAwaking = true;
                    LifeCycleMgr.Instance.AddTask(_instance, () =>
                    {
                        if (_destoryed) return;
                        var type = _instance.Type.ReflectionType;
                        GetMethodInfo(type, "Awake")?.Invoke(_instance, ConstMgr.NullObjects);
                        if (isJBehaviour)
                        {
                            //JBehaviour额外处理
                            GetMethodInfo(type, "Check").Invoke(_instance, ConstMgr.NullObjects);
                            LifeCycleMgr.Instance.AddAwakeItem(_instance, null); //这一帧空出来
                            GetMethodInfo(type, "OnEnable")?.Invoke(_instance, ConstMgr.NullObjects);
                            LifeCycleMgr.Instance.AddStartItem(_instance, GetMethodInfo(type, "Start"));
                        }

                        isAwaking = false;
                        awaked = true;
                    }, () => Application.isPlaying && !_destoryed && gameObject.activeInHierarchy);
                }
            }

            private MethodInfo GetMethodInfo(Type type, string funcName)
            {
                if (_instance.Type.GetMethod(funcName, 0) != null)
                {
                    return type.GetMethod(funcName);
                }

                return null;
            }
            

            void OnDestroy()
            {
                _destoryed = true;
                //销毁ILTypeIns
                _instance = null;
            }

            IMethod _mToStringMethod;
            bool _mToStringMethodGot;

            public override string ToString()
            {
                if (_instance != null)
                {
                    if (!_mToStringMethodGot)
                    {
                        _mToStringMethod =
                            _instance.Type.GetMethod("ToString", 0);
                        _mToStringMethodGot = true;
                    }

                    if (_mToStringMethod != null)
                    {
                        _appdomain.Invoke(_mToStringMethod, _instance, ConstMgr.NullObjects);
                    }
                }

                return _instance?.Type?.FullName ?? base.ToString();
            }
        }
    }
}