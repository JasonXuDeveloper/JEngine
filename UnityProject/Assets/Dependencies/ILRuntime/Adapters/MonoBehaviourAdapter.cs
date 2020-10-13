using UnityEngine;
using System.Collections.Generic;
using ILRuntime.Other;
using System;
using System.Collections;
using System.Reflection;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.Runtime.Intepreter;
using ILRuntime.CLR.Method;
using JEngine.Core;

public class MonoBehaviourAdapter : CrossBindingAdaptor
{
    public override Type BaseCLRType
    {
        get
        {
            return typeof(MonoBehaviour);
        }
    }

    public override Type AdaptorType
    {
        get
        {
            return typeof(Adaptor);
        }
    }

    public override object CreateCLRInstance(ILRuntime.Runtime.Enviorment.AppDomain appdomain, ILTypeInstance instance)
    {
        return new Adaptor(appdomain, instance);
    }
    //为了完整实现MonoBehaviour的所有特性，这个Adapter还得扩展，这里只抛砖引玉，只实现了最常用的Awake, Start和Update
    public class Adaptor : MonoBehaviour, CrossBindingAdaptorType
    {
        ILTypeInstance instance;
        ILRuntime.Runtime.Enviorment.AppDomain appdomain;

        public Adaptor()
        {

        }

        public Adaptor(ILRuntime.Runtime.Enviorment.AppDomain appdomain, ILTypeInstance instance)
        {
            this.appdomain = appdomain;
            this.instance = instance;
        }

        public ILTypeInstance ILInstance { get { return instance; } set { instance = value; } }

        public ILRuntime.Runtime.Enviorment.AppDomain AppDomain { get { return appdomain; } set { appdomain = value; } }

        IMethod mAwakeMethod;
        bool mAwakeMethodGot;
        public void Awake()
        {
            //Unity会在ILRuntime准备好这个实例前调用Awake，所以这里暂时先不掉用
            if (instance != null)
            {
                if (!mAwakeMethodGot)
                {
                    mAwakeMethod = instance.Type.GetMethod("Awake", 0);
                    mAwakeMethodGot = true;
                }

                if (mAwakeMethod != null)
                {
                    appdomain.Invoke(mAwakeMethod, instance, null);
                }
            }
        }

        IMethod mStartMethod;
        bool mStartMethodGot;
        void Start()
        {
            if (!mStartMethodGot)
            {
                mStartMethod = instance.Type.GetMethod("Start", 0);
                mStartMethodGot = true;
            }

            if (mStartMethod != null)
            {
                appdomain.Invoke(mStartMethod, instance, null);
            }
        }

        IMethod mUpdateMethod;
        bool mUpdateMethodGot;
        void Update()
        {
            if (!mUpdateMethodGot)
            {
                mUpdateMethod = instance.Type.GetMethod("Update", 0);
                mUpdateMethodGot = true;
            }

            if (mUpdateMethod != null)
            {
                appdomain.Invoke(mUpdateMethod, instance, null);
            }
            
        }

        IMethod mFixedUpdateMethod;
        bool mFixedUpdateMethodGot;
        void FixedUpdate()
        {
            if (!mFixedUpdateMethodGot)
            {
                mFixedUpdateMethod = instance.Type.GetMethod("FixedUpdate", 0);
                mFixedUpdateMethodGot = true;
            }

            if (mFixedUpdateMethod != null)
            {
                appdomain.Invoke(mFixedUpdateMethod, instance, null);
            }
        }
        
        IMethod mLateUpdateMethod;
        bool mLateUpdateMethodGot;
        void LateUpdate()
        {
            if (!mLateUpdateMethodGot)
            {
                mLateUpdateMethod = instance.Type.GetMethod("LateUpdate", 0);
                mLateUpdateMethodGot = true;
            }

            if (mLateUpdateMethod != null)
            {
                appdomain.Invoke(mLateUpdateMethod, instance, null);
            }
        }
        
        IMethod mOnEnableMethod;
        bool mOnEnableMethodGot;
        void OnEnable()
        {
            if (instance != null)
            {
                if (!mOnEnableMethodGot)
                {
                    mOnEnableMethod = instance.Type.GetMethod("OnEnable", 0);
                    mOnEnableMethodGot = true;
                }

                if (mOnEnableMethod != null)
                {
                    appdomain.Invoke(mOnEnableMethod, instance, null);
                }
            }
        }
        
        IMethod mOnDisableMethod;
        bool mOnDisableMethodGot;
        void OnDisable()
        {
            if (instance != null)
            {
                if (!mOnDisableMethodGot)
                {
                    mOnDisableMethod = instance.Type.GetMethod("OnDisable", 0);
                    mOnDisableMethodGot = true;
                }

                if (mOnDisableMethod != null)
                {
                    appdomain.Invoke(mOnDisableMethod, instance, null);
                }
            }
        }
        
        IMethod mDestroyMethod;
        bool mDestroyMethodGot;
        void OnDestroy()
        {
            if (!mDestroyMethodGot)
            {
                mDestroyMethod = instance.Type.GetMethod("OnDestroy", 0);
                mDestroyMethodGot = true;
            }

            if (mDestroyMethod != null)
            {
                appdomain.Invoke(mDestroyMethod, instance, null);
            }
        }
        
        IMethod mOnTriggerEnterMethod;
        bool mOnTriggerEnterMethodGot;
        void OnTriggerEnter(Collider other)
        {
            if (!mOnTriggerEnterMethodGot)
            {
                mOnTriggerEnterMethod = instance.Type.GetMethod("OnTriggerEnter", 1);
                mOnTriggerEnterMethodGot = true;
            }

            if (mOnTriggerEnterMethod != null)
            {
                appdomain.Invoke(mOnTriggerEnterMethod, instance, other);
            }
        }
        
        IMethod mOnTriggerStayMethod;
        bool mOnTriggerStayMethodGot;
        void OnTriggerStay(Collider other)
        {
            if (!mOnTriggerStayMethodGot)
            {
                mOnTriggerStayMethod = instance.Type.GetMethod("OnTriggerStay", 1);
                mOnTriggerStayMethodGot = true;
            }

            if (mOnTriggerStayMethod != null)
            {
                appdomain.Invoke(mOnTriggerStayMethod, instance, other);
            }
        }
        
        IMethod mOnTriggerExitMethod;
        bool mOnTriggerExitMethodGot;
        void OnTriggerExit(Collider other)
        {
            if (!mOnTriggerExitMethodGot)
            {
                mOnTriggerExitMethod = instance.Type.GetMethod("OnTriggerExit", 1);
                mOnTriggerExitMethodGot = true;
            }

            if (mOnTriggerExitMethod != null)
            {
                appdomain.Invoke(mOnTriggerExitMethod, instance, other);
            }
        }
        
        IMethod mOnCollisionEnterMethod;
        bool mOnCollisionEnterMethodGot;
        void OnCollisionEnter(Collision other)
        {
            if (!mOnCollisionEnterMethodGot)
            {
                mOnCollisionEnterMethod = instance.Type.GetMethod("OnCollisionEnter", 1);
                mOnCollisionEnterMethodGot = true;
            }

            if (mOnCollisionEnterMethod != null)
            {
                appdomain.Invoke(mOnCollisionEnterMethod, instance, other);
            }
        }

        IMethod mOnCollisionStayMethod;
        bool mOnCollisionStayMethodGot;
        void OnCollisionStay(Collision other)
        {
            if (!mOnCollisionStayMethodGot)
            {
                mOnCollisionStayMethod = instance.Type.GetMethod("OnCollisionStay", 1);
                mOnCollisionStayMethodGot = true;
            }

            if (mOnCollisionStayMethod != null)
            {
                appdomain.Invoke(mOnCollisionStayMethod, instance, other);
            }
        }
        
        IMethod mOnCollisionExitMethod;
        bool mOnCollisionExitMethodGot;
        void OnCollisionExit(Collision other)
        {
            if (!mOnCollisionExitMethodGot)
            {
                mOnCollisionExitMethod = instance.Type.GetMethod("OnCollisionExit", 1);
                mOnCollisionExitMethodGot = true;
            }

            if (mOnCollisionExitMethod != null)
            {
                appdomain.Invoke(mOnCollisionExitMethod, instance, other);
            }
        }
        public override string ToString()
        {
            IMethod m = appdomain.ObjectType.GetMethod("ToString", 0);
            m = instance.Type.GetVirtualMethod(m);
            if (m == null || m is ILMethod)
            {
                return instance.ToString();
            }
            else
                return instance.Type.FullName;
        }
    }
}
