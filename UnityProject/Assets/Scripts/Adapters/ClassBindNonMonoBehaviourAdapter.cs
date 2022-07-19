using System;
using System.Reflection;
using System.Threading.Tasks;
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

            public async void Awake()
            {
                if (awaked)
                {
                    return;
                }

                try
                {
                    //Unity会在ILRuntime准备好这个实例前调用Awake，所以这里暂时先不掉用
                    if (_instance != null)
                    {
                        if (!isAwaking)
                        {
                            isAwaking = true;
                            //没激活就别awake
                            try
                            {
                                while (Application.isPlaying && !_destoryed && !gameObject.activeInHierarchy)
                                {
                                    await TimeMgr.Delay(1);
                                }
                            }
                            catch (MissingReferenceException) //如果gameObject被删了，就会触发这个，这个时候就直接return了
                            {
                                return;
                            }

                            if (_destoryed || !Application.isPlaying)
                            {
                                return;
                            }

                            var type = _instance.Type.ReflectionType;
                            GetMethodInfo(type, "Awake")?.Invoke(_instance, ConstMgr.NullObjects);
                            if (isJBehaviour)
                            {
                                //JBehaviour额外处理
                                GetMethodInfo(type, "Check").Invoke(_instance, ConstMgr.NullObjects);
                                LifeCycleMgr.Instance.AddAwakeItem(_instance,  null);//这一帧空出来
                                LifeCycleMgr.Instance.AddOnEnableItem(_instance, GetMethodInfo(type, "OnEnable"));
                                LifeCycleMgr.Instance.AddStartItem(_instance, GetMethodInfo(type, "Start"));
                            }
                            isAwaking = false;
                            awaked = true;
                        }
                    }
                }
                catch (NullReferenceException)
                {
                    //如果出现了Null，那就重新Awake
                    Awake();
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