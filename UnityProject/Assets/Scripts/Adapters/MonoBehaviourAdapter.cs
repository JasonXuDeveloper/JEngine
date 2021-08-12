using System;
using System.Threading.Tasks;
using ILRuntime.CLR.Method;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.Runtime.Intepreter;
using UnityEngine;
using AppDomain = ILRuntime.Runtime.Enviorment.AppDomain;

public class MonoBehaviourAdapter : CrossBindingAdaptor
{
    public override Type BaseCLRType
    {
        get { return typeof(MonoBehaviour); }
    }

    public override Type AdaptorType
    {
        get { return typeof(Adaptor); }
    }

    public override object CreateCLRInstance(AppDomain appdomain, ILTypeInstance instance)
    {
        return new Adaptor(appdomain, instance);
    }

    //为了完整实现MonoBehaviour的所有特性，这个Adapter还得扩展，这里只抛砖引玉，只实现了最常用的Awake, Start和Update
    public class Adaptor : MonoBehaviour, CrossBindingAdaptorType
    {
        ILTypeInstance instance;
        AppDomain appdomain;

        public bool isMonoBehaviour = true;

        public Adaptor()
        {

        }

        public Adaptor(AppDomain appdomain, ILTypeInstance instance)
        {
            this.appdomain = appdomain;
            this.instance = instance;
        }

        public ILTypeInstance ILInstance
        {
            get { return instance; }
            set { instance = value; }
        }

        public AppDomain AppDomain
        {
            get { return appdomain; }
            set { appdomain = value; }
        }

        public void Reset()
        {
            mAwakeMethodGot = false;
            mStartMethodGot = false;
            mUpdateMethodGot = false;
            mFixedUpdateMethodGot = false;
            mLateUpdateMethodGot = false;
            mOnEnableMethodGot = false;
            mOnDisableMethodGot = false;
            mDestroyMethodGot = false;
            mOnTriggerEnterMethodGot = false;
            mOnTriggerStayMethodGot = false;
            mOnTriggerExitMethodGot = false;
            mOnCollisionEnterMethodGot = false;
            mOnCollisionStayMethodGot = false;
            mOnCollisionExitMethodGot = false;
            mOnValidateMethodGot = false;
            mOnAnimatorMoveMethodGot = false;
            mOnApplicationFocusMethodGot = false;
            mOnApplicationPauseMethodGot = false;
            mOnApplicationQuitMethodGot = false;
            mOnBecameInvisibleMethodGot = false;
            mOnBecameVisibleMethodGot = false;
            mOnDrawGizmosMethodGot = false;
            mOnJointBreakMethodGot = false;
            mOnMouseDownMethodGot = false;
            mOnMouseDragMethodGot = false;
            mOnMouseEnterMethodGot = false;
            mOnMouseExitMethodGot = false;
            mOnMouseOverMethodGot = false;
            mOnMouseUpMethodGot = false;
            mOnParticleCollisionMethodGot = false;
            mOnParticleTriggerMethodGot = false;
            mOnPostRenderMethodGot = false;
            mOnPreCullMethodGot = false;
            mOnPreRenderMethodGot = false;
            mOnRenderImageMethodGot = false;
            mOnRenderObjectMethodGot = false;
            mOnServerInitializedMethodGot = false;
            mOnAnimatorIKMethodGot = false;
            mOnAudioFilterReadMethodGot = false;
            mOnCanvasGroupChangedMethodGot = false;
            mOnCanvasHierarchyChangedMethodGot = false;
            mOnCollisionEnter2DMethodGot = false;
            mOnCollisionExit2DMethodGot = false;
            mOnCollisionStay2DMethodGot = false;
            mOnConnectedToServerMethodGot = false;
            mOnControllerColliderHitMethodGot = false;
            mOnDrawGizmosSelectedMethodGot = false;
            mOnGUIMethodGot = false;
            mOnJointBreak2DMethodGot = false;
            mOnParticleSystemStoppedMethodGot = false;
            mOnTransformChildrenChangedMethodGot = false;
            mOnTransformParentChangedMethodGot = false;
            mOnTriggerEnter2DMethodGot = false;
            mOnTriggerExit2DMethodGot = false;
            mOnTriggerStay2DMethodGot = false;
            mOnWillRenderObjectMethodGot = false;
            mOnBeforeTransformParentChangedMethodGot = false;
            mOnDidApplyAnimationPropertiesMethodGot = false;
            mOnMouseUpAsButtonMethodGot = false;
            mOnParticleUpdateJobScheduledMethodGot = false;
            mOnRectTransformDimensionsChangeMethodGot = false;
            instance = null;
            appdomain = null;
            destoryed = false;
            awaked = false;
            isAwaking = false;
        }

        object[] param0 = new object[0];
        private bool destoryed;

        IMethod mAwakeMethod;
        bool mAwakeMethodGot;
        public bool awaked;
        public bool isAwaking;

        public async void Awake()
        {
            try
            {
                //Unity会在ILRuntime准备好这个实例前调用Awake，所以这里暂时先不掉用
                if (instance != null)
                {
                    if (!mAwakeMethodGot)
                    {
                        mAwakeMethod = instance.Type.GetMethod("Awake", 0);
                        mAwakeMethodGot = true;
                    }

                    if (!isAwaking)
                    {
                        isAwaking = true;
                        //没激活就别awake
                        try
                        {
                            while (Application.isPlaying && !destoryed && !gameObject.activeInHierarchy)
                            {
                                await Task.Delay(20);
                            }
                        }
                        catch (MissingReferenceException) //如果gameObject被删了，就会触发这个，这个时候就直接return了
                        {
                            return;
                        }

                        if (destoryed || !Application.isPlaying)
                        {
                            return;
                        }

                        if (mAwakeMethod != null) 
                        {
                            appdomain.Invoke(mAwakeMethod, instance, param0);
                        }
                        
                        isAwaking = false;
                        awaked = true;
                        OnEnable();
                    }
                }
            }
            catch (NullReferenceException)
            {
                //如果出现了Null，那就重新Awake
                Awake();
            }
        }

        IMethod mStartMethod;
        bool mStartMethodGot;

        async void Start()
        {
            if (!isMonoBehaviour) return;

            if (instance == null)
            {
                await Task.Delay(1);
            }

            if (!mStartMethodGot)
            {
                mStartMethod = instance.Type.GetMethod("Start", 0);
                mStartMethodGot = true;
            }

            if (mStartMethod != null)
            {
                appdomain.Invoke(mStartMethod, instance, param0);
            }
        }

        IMethod mUpdateMethod;
        bool mUpdateMethodGot;

        void Update()
        {
            if (!isMonoBehaviour) return;

            if (!mUpdateMethodGot)
            {
                mUpdateMethod = instance.Type.GetMethod("Update", 0);
                mUpdateMethodGot = true;
            }

            if (mUpdateMethod != null)
            {
                appdomain.Invoke(mUpdateMethod, instance, param0);
            }

        }

        IMethod mFixedUpdateMethod;
        bool mFixedUpdateMethodGot;

        void FixedUpdate()
        {
            if (!isMonoBehaviour) return;

            if (!mFixedUpdateMethodGot)
            {
                mFixedUpdateMethod = instance.Type.GetMethod("FixedUpdate", 0);
                mFixedUpdateMethodGot = true;
            }

            if (mFixedUpdateMethod != null)
            {
                appdomain.Invoke(mFixedUpdateMethod, instance, param0);
            }
        }

        IMethod mLateUpdateMethod;
        bool mLateUpdateMethodGot;

        void LateUpdate()
        {
            if (!isMonoBehaviour) return;

            if (!mLateUpdateMethodGot)
            {
                mLateUpdateMethod = instance.Type.GetMethod("LateUpdate", 0);
                mLateUpdateMethodGot = true;
            }

            if (mLateUpdateMethod != null)
            {
                appdomain.Invoke(mLateUpdateMethod, instance, param0);
            }
        }

        IMethod mOnEnableMethod;
        bool mOnEnableMethodGot;

        void OnEnable()
        {
            if (!isMonoBehaviour) return;

            if (instance != null)
            {
                if (!mOnEnableMethodGot)
                {
                    mOnEnableMethod = instance.Type.GetMethod("OnEnable", 0);
                    mOnEnableMethodGot = true;
                }

                if (mOnEnableMethod != null && awaked)
                {
                    appdomain.Invoke(mOnEnableMethod, instance, param0);
                }
            }
        }

        IMethod mOnDisableMethod;
        bool mOnDisableMethodGot;

        void OnDisable()
        {
            if (!isMonoBehaviour) return;

            if (instance != null)
            {
                if (!mOnDisableMethodGot)
                {
                    mOnDisableMethod = instance.Type.GetMethod("OnDisable", 0);
                    mOnDisableMethodGot = true;
                }

                if (mOnDisableMethod != null)
                {
                    appdomain.Invoke(mOnDisableMethod, instance, param0);
                }
            }
        }

        IMethod mDestroyMethod;
        bool mDestroyMethodGot;

        void OnDestroy()
        {
            destoryed = true;

            if (!isMonoBehaviour) return;

            if (!mDestroyMethodGot)
            {
                mDestroyMethod = instance.Type.GetMethod("OnDestroy", 0);
                mDestroyMethodGot = true;
            }

            if (mDestroyMethod != null)
            {
                appdomain.Invoke(mDestroyMethod, instance, param0);
            }
        }

        IMethod mOnTriggerEnterMethod;
        bool mOnTriggerEnterMethodGot;

        void OnTriggerEnter(Collider other)
        {
            if (!isMonoBehaviour) return;

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
            if (!isMonoBehaviour) return;

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
            if (!isMonoBehaviour) return;

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
            if (!isMonoBehaviour) return;

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
            if (!isMonoBehaviour) return;

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
            if (!isMonoBehaviour) return;

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


        IMethod mOnValidateMethod;
        bool mOnValidateMethodGot;

        void OnValidate()
        {
            if (!isMonoBehaviour) return;

            if (instance != null)
            {
                if (!mOnValidateMethodGot)
                {
                    mOnValidateMethod = instance.Type.GetMethod("OnValidate", 0);
                    mOnValidateMethodGot = true;
                }

                if (mOnValidateMethod != null)
                {
                    appdomain.Invoke(mOnValidateMethod, instance, param0);
                }
            }
        }

        IMethod mOnAnimatorMoveMethod;
        bool mOnAnimatorMoveMethodGot;

        void OnAnimatorMove()
        {
            if (!isMonoBehaviour) return;

            if (instance != null)
            {
                if (!mOnAnimatorMoveMethodGot)
                {
                    mOnAnimatorMoveMethod = instance.Type.GetMethod("OnAnimatorMove", 0);
                    mOnAnimatorMoveMethodGot = true;
                }

                if (mOnAnimatorMoveMethod != null)
                {
                    appdomain.Invoke(mOnAnimatorMoveMethod, instance, param0);
                }
            }
        }

        IMethod mOnApplicationFocusMethod;
        bool mOnApplicationFocusMethodGot;

        void OnApplicationFocus(bool hasFocus)
        {
            if (!isMonoBehaviour) return;

            if (instance != null)
            {
                if (!mOnApplicationFocusMethodGot)
                {
                    mOnApplicationFocusMethod = instance.Type.GetMethod("OnApplicationFocus", 1);
                    mOnApplicationFocusMethodGot = true;
                }

                if (mOnApplicationFocusMethod != null)
                {
                    appdomain.Invoke(mOnApplicationFocusMethod, instance, hasFocus);
                }
            }
        }

        IMethod mOnApplicationPauseMethod;
        bool mOnApplicationPauseMethodGot;

        void OnApplicationPause(bool pauseStatus)
        {
            if (!isMonoBehaviour) return;

            if (instance != null)
            {
                if (!mOnApplicationPauseMethodGot)
                {
                    mOnApplicationPauseMethod = instance.Type.GetMethod("OnApplicationPause", 1);
                    mOnApplicationPauseMethodGot = true;
                }

                if (mOnApplicationPauseMethod != null)
                {
                    appdomain.Invoke(mOnApplicationPauseMethod, instance, pauseStatus);
                }
            }
        }

        IMethod mOnApplicationQuitMethod;
        bool mOnApplicationQuitMethodGot;

        void OnApplicationQuit()
        {
            if (!isMonoBehaviour) return;

            if (instance != null)
            {
                if (!mOnApplicationQuitMethodGot)
                {
                    mOnApplicationQuitMethod = instance.Type.GetMethod("OnApplicationQuit", 0);
                    mOnApplicationQuitMethodGot = true;
                }

                if (mOnApplicationQuitMethod != null)
                {
                    appdomain.Invoke(mOnApplicationQuitMethod, instance, param0);
                }
            }
        }

        IMethod mOnBecameInvisibleMethod;
        bool mOnBecameInvisibleMethodGot;

        void OnBecameInvisible()
        {
            if (!isMonoBehaviour) return;

            if (instance != null)
            {
                if (!mOnBecameInvisibleMethodGot)
                {
                    mOnBecameInvisibleMethod = instance.Type.GetMethod("OnBecameInvisible", 0);
                    mOnBecameInvisibleMethodGot = true;
                }

                if (mOnBecameInvisibleMethod != null)
                {
                    appdomain.Invoke(mOnBecameInvisibleMethod, instance, param0);
                }
            }
        }

        IMethod mOnBecameVisibleMethod;
        bool mOnBecameVisibleMethodGot;

        void OnBecameVisible()
        {
            if (!isMonoBehaviour) return;

            if (instance != null)
            {
                if (!mOnBecameVisibleMethodGot)
                {
                    mOnBecameVisibleMethod = instance.Type.GetMethod("OnBecameVisible", 0);
                    mOnBecameVisibleMethodGot = true;
                }

                if (mOnBecameVisibleMethod != null)
                {
                    appdomain.Invoke(mOnBecameVisibleMethod, instance, param0);
                }
            }
        }

        IMethod mOnDrawGizmosMethod;
        bool mOnDrawGizmosMethodGot;

        void OnDrawGizmos()
        {
            if (!isMonoBehaviour) return;

            if (instance != null)
            {
                if (!mOnDrawGizmosMethodGot)
                {
                    mOnDrawGizmosMethod = instance.Type.GetMethod("OnDrawGizmos", 0);
                    mOnDrawGizmosMethodGot = true;
                }

                if (mOnDrawGizmosMethod != null)
                {
                    appdomain.Invoke(mOnDrawGizmosMethod, instance, param0);
                }
            }
        }

        IMethod mOnJointBreakMethod;
        bool mOnJointBreakMethodGot;

        void OnJointBreak(float breakForce)
        {
            if (!isMonoBehaviour) return;

            if (instance != null)
            {
                if (!mOnJointBreakMethodGot)
                {
                    mOnJointBreakMethod = instance.Type.GetMethod("OnJointBreak", 1);
                    mOnJointBreakMethodGot = true;
                }

                if (mOnJointBreakMethod != null)
                {
                    appdomain.Invoke(mOnJointBreakMethod, instance, breakForce);
                }
            }
        }

        IMethod mOnMouseDownMethod;
        bool mOnMouseDownMethodGot;

        void OnMouseDown()
        {
            if (!isMonoBehaviour) return;

            if (instance != null)
            {
                if (!mOnMouseDownMethodGot)
                {
                    mOnMouseDownMethod = instance.Type.GetMethod("OnMouseDown", 0);
                    mOnMouseDownMethodGot = true;
                }

                if (mOnMouseDownMethod != null)
                {
                    appdomain.Invoke(mOnMouseDownMethod, instance, param0);
                }
            }
        }

        IMethod mOnMouseDragMethod;
        bool mOnMouseDragMethodGot;

        void OnMouseDrag()
        {
            if (!isMonoBehaviour) return;

            if (instance != null)
            {
                if (!mOnMouseDragMethodGot)
                {
                    mOnMouseDragMethod = instance.Type.GetMethod("OnMouseDrag", 0);
                    mOnMouseDragMethodGot = true;
                }

                if (mOnMouseDragMethod != null)
                {
                    appdomain.Invoke(mOnMouseDragMethod, instance, param0);
                }
            }
        }

        IMethod mOnMouseEnterMethod;
        bool mOnMouseEnterMethodGot;

        void OnMouseEnter()
        {
            if (!isMonoBehaviour) return;

            if (instance != null)
            {
                if (!mOnMouseEnterMethodGot)
                {
                    mOnMouseEnterMethod = instance.Type.GetMethod("OnMouseEnter", 0);
                    mOnMouseEnterMethodGot = true;
                }

                if (mOnMouseEnterMethod != null)
                {
                    appdomain.Invoke(mOnMouseEnterMethod, instance, param0);
                }
            }
        }

        IMethod mOnMouseExitMethod;
        bool mOnMouseExitMethodGot;

        void OnMouseExit()
        {
            if (!isMonoBehaviour) return;

            if (instance != null)
            {
                if (!mOnMouseExitMethodGot)
                {
                    mOnMouseExitMethod = instance.Type.GetMethod("OnMouseExit", 0);
                    mOnMouseExitMethodGot = true;
                }

                if (mOnMouseExitMethod != null)
                {
                    appdomain.Invoke(mOnMouseExitMethod, instance, param0);
                }
            }
        }

        IMethod mOnMouseOverMethod;
        bool mOnMouseOverMethodGot;

        void OnMouseOver()
        {
            if (!isMonoBehaviour) return;

            if (instance != null)
            {
                if (!mOnMouseOverMethodGot)
                {
                    mOnMouseOverMethod = instance.Type.GetMethod("OnMouseOver", 0);
                    mOnMouseOverMethodGot = true;
                }

                if (mOnMouseOverMethod != null)
                {
                    appdomain.Invoke(mOnMouseOverMethod, instance, param0);
                }
            }
        }

        IMethod mOnMouseUpMethod;
        bool mOnMouseUpMethodGot;

        void OnMouseUp()
        {
            if (!isMonoBehaviour) return;

            if (instance != null)
            {
                if (!mOnMouseUpMethodGot)
                {
                    mOnMouseUpMethod = instance.Type.GetMethod("OnMouseUp", 0);
                    mOnMouseUpMethodGot = true;
                }

                if (mOnMouseUpMethod != null)
                {
                    appdomain.Invoke(mOnMouseUpMethod, instance, param0);
                }
            }
        }

        IMethod mOnParticleCollisionMethod;
        bool mOnParticleCollisionMethodGot;

        void OnParticleCollision(GameObject other)
        {
            if (!isMonoBehaviour) return;

            if (instance != null)
            {
                if (!mOnParticleCollisionMethodGot)
                {
                    mOnParticleCollisionMethod = instance.Type.GetMethod("OnParticleCollision", 1);
                    mOnParticleCollisionMethodGot = true;
                }

                if (mOnParticleCollisionMethod != null)
                {
                    appdomain.Invoke(mOnParticleCollisionMethod, instance, other);
                }
            }
        }

        IMethod mOnParticleTriggerMethod;
        bool mOnParticleTriggerMethodGot;

        void OnParticleTrigger()
        {
            if (!isMonoBehaviour) return;

            if (instance != null)
            {
                if (!mOnParticleTriggerMethodGot)
                {
                    mOnParticleTriggerMethod = instance.Type.GetMethod("OnParticleTrigger", 0);
                    mOnParticleTriggerMethodGot = true;
                }

                if (mOnParticleTriggerMethod != null)
                {
                    appdomain.Invoke(mOnParticleTriggerMethod, instance, param0);
                }
            }
        }

        IMethod mOnPostRenderMethod;
        bool mOnPostRenderMethodGot;

        void OnPostRender()
        {
            if (!isMonoBehaviour) return;

            if (instance != null)
            {
                if (!mOnPostRenderMethodGot)
                {
                    mOnPostRenderMethod = instance.Type.GetMethod("OnPostRender", 0);
                    mOnPostRenderMethodGot = true;
                }

                if (mOnPostRenderMethod != null)
                {
                    appdomain.Invoke(mOnPostRenderMethod, instance, param0);
                }
            }
        }

        IMethod mOnPreCullMethod;
        bool mOnPreCullMethodGot;

        void OnPreCull()
        {
            if (!isMonoBehaviour) return;

            if (instance != null)
            {
                if (!mOnPreCullMethodGot)
                {
                    mOnPreCullMethod = instance.Type.GetMethod("OnPreCull", 0);
                    mOnPreCullMethodGot = true;
                }

                if (mOnPreCullMethod != null)
                {
                    appdomain.Invoke(mOnPreCullMethod, instance, param0);
                }
            }
        }

        IMethod mOnPreRenderMethod;
        bool mOnPreRenderMethodGot;

        void OnPreRender()
        {
            if (!isMonoBehaviour) return;

            if (instance != null)
            {
                if (!mOnPreRenderMethodGot)
                {
                    mOnPreRenderMethod = instance.Type.GetMethod("OnPreRender", 0);
                    mOnPreRenderMethodGot = true;
                }

                if (mOnPreRenderMethod != null)
                {
                    appdomain.Invoke(mOnPreRenderMethod, instance, param0);
                }
            }
        }

        IMethod mOnRenderImageMethod;
        bool mOnRenderImageMethodGot;

        void OnRenderImage(RenderTexture src, RenderTexture dest)
        {
            if (!isMonoBehaviour) return;

            if (instance != null)
            {
                if (!mOnRenderImageMethodGot)
                {
                    mOnRenderImageMethod = instance.Type.GetMethod("OnRenderImage", 2);
                    mOnRenderImageMethodGot = true;
                }

                if (mOnRenderImageMethod != null)
                {
                    appdomain.Invoke(mOnRenderImageMethod, instance, src, dest);
                }
            }
        }

        IMethod mOnRenderObjectMethod;
        bool mOnRenderObjectMethodGot;

        void OnRenderObject()
        {
            if (!isMonoBehaviour) return;

            if (instance != null)
            {
                if (!mOnRenderObjectMethodGot)
                {
                    mOnRenderObjectMethod = instance.Type.GetMethod("OnRenderObject", 0);
                    mOnRenderObjectMethodGot = true;
                }

                if (mOnRenderObjectMethod != null)
                {
                    appdomain.Invoke(mOnRenderObjectMethod, instance, param0);
                }
            }
        }

        IMethod mOnServerInitializedMethod;
        bool mOnServerInitializedMethodGot;

        void OnServerInitialized()
        {
            if (!isMonoBehaviour) return;

            if (instance != null)
            {
                if (!mOnServerInitializedMethodGot)
                {
                    mOnServerInitializedMethod = instance.Type.GetMethod("OnServerInitialized", 0);
                    mOnServerInitializedMethodGot = true;
                }

                if (mOnServerInitializedMethod != null)
                {
                    appdomain.Invoke(mOnServerInitializedMethod, instance, param0);
                }
            }
        }

        IMethod mOnAnimatorIKMethod;
        bool mOnAnimatorIKMethodGot;

        void OnAnimatorIK(int layerIndex)
        {
            if (!isMonoBehaviour) return;

            if (instance != null)
            {
                if (!mOnAnimatorIKMethodGot)
                {
                    mOnAnimatorIKMethod = instance.Type.GetMethod("OnAnimatorIK", 1);
                    mOnAnimatorIKMethodGot = true;
                }

                if (mOnAnimatorIKMethod != null)
                {
                    appdomain.Invoke(mOnAnimatorIKMethod, instance, layerIndex);
                }
            }
        }

        IMethod mOnAudioFilterReadMethod;
        bool mOnAudioFilterReadMethodGot;

        void OnAudioFilterRead(float[] data, int channels)
        {
            if (!isMonoBehaviour) return;

            if (instance != null)
            {
                if (!mOnAudioFilterReadMethodGot)
                {
                    mOnAudioFilterReadMethod = instance.Type.GetMethod("OnAudioFilterRead", 2);
                    mOnAudioFilterReadMethodGot = true;
                }

                if (mOnAudioFilterReadMethod != null)
                {
                    appdomain.Invoke(mOnAudioFilterReadMethod, instance, data, channels);
                }
            }
        }


        IMethod mOnCanvasGroupChangedMethod;
        bool mOnCanvasGroupChangedMethodGot;

        void OnCanvasGroupChanged()
        {
            if (!isMonoBehaviour) return;

            if (instance != null)
            {
                if (!mOnCanvasGroupChangedMethodGot)
                {
                    mOnCanvasGroupChangedMethod = instance.Type.GetMethod("OnCanvasGroupChanged", 0);
                    mOnCanvasGroupChangedMethodGot = true;
                }

                if (mOnCanvasGroupChangedMethod != null)
                {
                    appdomain.Invoke(mOnCanvasGroupChangedMethod, instance, param0);
                }
            }
        }

        IMethod mOnCanvasHierarchyChangedMethod;
        bool mOnCanvasHierarchyChangedMethodGot;

        void OnCanvasHierarchyChanged()
        {
            if (!isMonoBehaviour) return;

            if (instance != null)
            {
                if (!mOnCanvasHierarchyChangedMethodGot)
                {
                    mOnCanvasHierarchyChangedMethod = instance.Type.GetMethod("OnCanvasHierarchyChanged", 0);
                    mOnCanvasHierarchyChangedMethodGot = true;
                }

                if (mOnCanvasHierarchyChangedMethod != null)
                {
                    appdomain.Invoke(mOnCanvasHierarchyChangedMethod, instance, param0);
                }
            }
        }

        IMethod mOnCollisionEnter2DMethod;
        bool mOnCollisionEnter2DMethodGot;

        void OnCollisionEnter2D(Collision2D other)
        {
            if (!isMonoBehaviour) return;

            if (!mOnCollisionEnter2DMethodGot)
            {
                mOnCollisionEnter2DMethod = instance.Type.GetMethod("OnCollisionEnter2D", 1);
                mOnCollisionEnter2DMethodGot = true;
            }

            if (mOnCollisionEnter2DMethod != null)
            {
                appdomain.Invoke(mOnCollisionEnter2DMethod, instance, other);
            }
        }

        IMethod mOnCollisionExit2DMethod;
        bool mOnCollisionExit2DMethodGot;

        void OnCollisionExit2D(Collision2D other)
        {
            if (!isMonoBehaviour) return;

            if (!mOnCollisionExit2DMethodGot)
            {
                mOnCollisionExit2DMethod = instance.Type.GetMethod("OnCollisionExit2D", 1);
                mOnCollisionExit2DMethodGot = true;
            }

            if (mOnCollisionExit2DMethod != null)
            {
                appdomain.Invoke(mOnCollisionExit2DMethod, instance, other);
            }
        }

        IMethod mOnCollisionStay2DMethod;
        bool mOnCollisionStay2DMethodGot;

        void OnCollisionStay2D(Collision2D other)
        {
            if (!isMonoBehaviour) return;

            if (!mOnCollisionStay2DMethodGot)
            {
                mOnCollisionStay2DMethod = instance.Type.GetMethod("OnCollisionStay2D", 1);
                mOnCollisionStay2DMethodGot = true;
            }

            if (mOnCollisionStay2DMethod != null)
            {
                appdomain.Invoke(mOnCollisionStay2DMethod, instance, other);
            }
        }

        IMethod mOnConnectedToServerMethod;
        bool mOnConnectedToServerMethodGot;

        void OnConnectedToServer()
        {
            if (!isMonoBehaviour) return;

            if (instance != null)
            {
                if (!mOnConnectedToServerMethodGot)
                {
                    mOnConnectedToServerMethod = instance.Type.GetMethod("OnConnectedToServer", 0);
                    mOnConnectedToServerMethodGot = true;
                }

                if (mOnConnectedToServerMethod != null)
                {
                    appdomain.Invoke(mOnConnectedToServerMethod, instance, param0);
                }
            }
        }

        IMethod mOnControllerColliderHitMethod;
        bool mOnControllerColliderHitMethodGot;

        void OnControllerColliderHit(ControllerColliderHit hit)
        {
            if (!isMonoBehaviour) return;

            if (instance != null)
            {
                if (!mOnControllerColliderHitMethodGot)
                {
                    mOnControllerColliderHitMethod = instance.Type.GetMethod("OnControllerColliderHit", 1);
                    mOnControllerColliderHitMethodGot = true;
                }

                if (mOnControllerColliderHitMethod != null)
                {
                    appdomain.Invoke(mOnControllerColliderHitMethod, instance, hit);
                }
            }
        }

        IMethod mOnDrawGizmosSelectedMethod;
        bool mOnDrawGizmosSelectedMethodGot;

        void OnDrawGizmosSelected()
        {
            if (!isMonoBehaviour) return;

            if (instance != null)
            {
                if (!mOnDrawGizmosSelectedMethodGot)
                {
                    mOnDrawGizmosSelectedMethod = instance.Type.GetMethod("OnDrawGizmosSelected", 0);
                    mOnDrawGizmosSelectedMethodGot = true;
                }

                if (mOnDrawGizmosSelectedMethod != null)
                {
                    appdomain.Invoke(mOnDrawGizmosSelectedMethod, instance, param0);
                }
            }
        }

        IMethod mOnGUIMethod;
        bool mOnGUIMethodGot;

        void OnGUI()
        {
            if (!isMonoBehaviour) return;

            if (instance != null)
            {
                if (!mOnGUIMethodGot)
                {
                    mOnGUIMethod = instance.Type.GetMethod("OnGUI", 0);
                    mOnGUIMethodGot = true;
                }

                if (mOnGUIMethod != null)
                {
                    appdomain.Invoke(mOnGUIMethod, instance, param0);
                }
            }
        }

        IMethod mOnJointBreak2DMethod;
        bool mOnJointBreak2DMethodGot;

        void OnJointBreak2D(Joint2D brokenJoint)
        {
            if (!isMonoBehaviour) return;

            if (instance != null)
            {
                if (!mOnJointBreak2DMethodGot)
                {
                    mOnJointBreak2DMethod = instance.Type.GetMethod("OnJointBreak2D", 1);
                    mOnJointBreak2DMethodGot = true;
                }

                if (mOnJointBreak2DMethod != null)
                {
                    appdomain.Invoke(mOnJointBreak2DMethod, instance, brokenJoint);
                }
            }
        }

        IMethod mOnParticleSystemStoppedMethod;
        bool mOnParticleSystemStoppedMethodGot;

        void OnParticleSystemStopped()
        {
            if (!isMonoBehaviour) return;

            if (instance != null)
            {
                if (!mOnParticleSystemStoppedMethodGot)
                {
                    mOnParticleSystemStoppedMethod = instance.Type.GetMethod("OnParticleSystemStopped", 0);
                    mOnParticleSystemStoppedMethodGot = true;
                }

                if (mOnParticleSystemStoppedMethod != null)
                {
                    appdomain.Invoke(mOnParticleSystemStoppedMethod, instance, param0);
                }
            }
        }

        IMethod mOnTransformChildrenChangedMethod;
        bool mOnTransformChildrenChangedMethodGot;

        void OnTransformChildrenChanged()
        {
            if (!isMonoBehaviour) return;

            if (instance != null)
            {
                if (!mOnTransformChildrenChangedMethodGot)
                {
                    mOnTransformChildrenChangedMethod = instance.Type.GetMethod("OnTransformChildrenChanged", 0);
                    mOnTransformChildrenChangedMethodGot = true;
                }

                if (mOnTransformChildrenChangedMethod != null)
                {
                    appdomain.Invoke(mOnTransformChildrenChangedMethod, instance, param0);
                }
            }
        }

        IMethod mOnTransformParentChangedMethod;
        bool mOnTransformParentChangedMethodGot;

        void OnTransformParentChanged()
        {
            if (!isMonoBehaviour) return;

            if (instance != null)
            {
                if (!mOnTransformParentChangedMethodGot)
                {
                    mOnTransformParentChangedMethod = instance.Type.GetMethod("OnTransformParentChanged", 0);
                    mOnTransformParentChangedMethodGot = true;
                }

                if (mOnTransformParentChangedMethod != null)
                {
                    appdomain.Invoke(mOnTransformParentChangedMethod, instance, param0);
                }
            }
        }

        IMethod mOnTriggerEnter2DMethod;
        bool mOnTriggerEnter2DMethodGot;

        void OnTriggerEnter2D(Collider2D other)
        {
            if (!isMonoBehaviour) return;

            if (instance != null)
            {
                if (!mOnTriggerEnter2DMethodGot)
                {
                    mOnTriggerEnter2DMethod = instance.Type.GetMethod("OnTriggerEnter2D", 1);
                    mOnTriggerEnter2DMethodGot = true;
                }

                if (mOnTriggerEnter2DMethod != null)
                {
                    appdomain.Invoke(mOnTriggerEnter2DMethod, instance, other);
                }
            }
        }

        IMethod mOnTriggerExit2DMethod;
        bool mOnTriggerExit2DMethodGot;

        void OnTriggerExit2D(Collider2D other)
        {
            if (!isMonoBehaviour) return;

            if (instance != null)
            {
                if (!mOnTriggerExit2DMethodGot)
                {
                    mOnTriggerExit2DMethod = instance.Type.GetMethod("OnTriggerExit2D", 1);
                    mOnTriggerExit2DMethodGot = true;
                }

                if (mOnTriggerExit2DMethod != null)
                {
                    appdomain.Invoke(mOnTriggerExit2DMethod, instance, other);
                }
            }
        }

        IMethod mOnTriggerStay2DMethod;
        bool mOnTriggerStay2DMethodGot;

        void OnTriggerStay2D(Collider2D other)
        {
            if (!isMonoBehaviour) return;

            if (instance != null)
            {
                if (!mOnTriggerStay2DMethodGot)
                {
                    mOnTriggerStay2DMethod = instance.Type.GetMethod("OnTriggerStay2D", 1);
                    mOnTriggerStay2DMethodGot = true;
                }

                if (mOnTriggerStay2DMethod != null)
                {
                    appdomain.Invoke(mOnTriggerStay2DMethod, instance, other);
                }
            }
        }

        IMethod mOnWillRenderObjectMethod;
        bool mOnWillRenderObjectMethodGot;

        void OnWillRenderObject()
        {
            if (!isMonoBehaviour) return;

            if (instance != null)
            {
                if (!mOnWillRenderObjectMethodGot)
                {
                    mOnWillRenderObjectMethod = instance.Type.GetMethod("OnWillRenderObject", 0);
                    mOnWillRenderObjectMethodGot = true;
                }

                if (mOnWillRenderObjectMethod != null)
                {
                    appdomain.Invoke(mOnWillRenderObjectMethod, instance, param0);
                }
            }
        }

        IMethod mOnBeforeTransformParentChangedMethod;
        bool mOnBeforeTransformParentChangedMethodGot;

        void OnBeforeTransformParentChanged()
        {
            if (!isMonoBehaviour) return;

            if (instance != null)
            {
                if (!mOnBeforeTransformParentChangedMethodGot)
                {
                    mOnBeforeTransformParentChangedMethod =
                        instance.Type.GetMethod("OnBeforeTransformParentChanged", 0);
                    mOnBeforeTransformParentChangedMethodGot = true;
                }

                if (mOnBeforeTransformParentChangedMethod != null)
                {
                    appdomain.Invoke(mOnBeforeTransformParentChangedMethod, instance, param0);
                }
            }
        }

        IMethod mOnDidApplyAnimationPropertiesMethod;
        bool mOnDidApplyAnimationPropertiesMethodGot;

        void OnDidApplyAnimationProperties()
        {
            if (!isMonoBehaviour) return;

            if (instance != null)
            {
                if (!mOnDidApplyAnimationPropertiesMethodGot)
                {
                    mOnDidApplyAnimationPropertiesMethod = instance.Type.GetMethod("OnDidApplyAnimationProperties", 0);
                    mOnDidApplyAnimationPropertiesMethodGot = true;
                }

                if (mOnDidApplyAnimationPropertiesMethod != null)
                {
                    appdomain.Invoke(mOnDidApplyAnimationPropertiesMethod, instance, param0);
                }
            }
        }

        IMethod mOnMouseUpAsButtonMethod;
        bool mOnMouseUpAsButtonMethodGot;

        void OnMouseUpAsButton()
        {
            if (!isMonoBehaviour) return;

            if (instance != null)
            {
                if (!mOnMouseUpAsButtonMethodGot)
                {
                    mOnMouseUpAsButtonMethod = instance.Type.GetMethod("OnMouseUpAsButton", 0);
                    mOnMouseUpAsButtonMethodGot = true;
                }

                if (mOnMouseUpAsButtonMethod != null)
                {
                    appdomain.Invoke(mOnMouseUpAsButtonMethod, instance, param0);
                }
            }
        }

        IMethod mOnParticleUpdateJobScheduledMethod;
        bool mOnParticleUpdateJobScheduledMethodGot;

        void OnParticleUpdateJobScheduled()
        {
            if (!isMonoBehaviour) return;

            if (instance != null)
            {
                if (!mOnParticleUpdateJobScheduledMethodGot)
                {
                    mOnParticleUpdateJobScheduledMethod = instance.Type.GetMethod("OnParticleUpdateJobScheduled", 0);
                    mOnParticleUpdateJobScheduledMethodGot = true;
                }

                if (mOnParticleUpdateJobScheduledMethod != null)
                {
                    appdomain.Invoke(mOnParticleUpdateJobScheduledMethod, instance, param0);
                }
            }
        }

        IMethod mOnRectTransformDimensionsChangeMethod;
        bool mOnRectTransformDimensionsChangeMethodGot;

        void OnRectTransformDimensionsChange()
        {
            if (!isMonoBehaviour) return;

            if (instance != null)
            {
                if (!mOnRectTransformDimensionsChangeMethodGot)
                {
                    mOnRectTransformDimensionsChangeMethod =
                        instance.Type.GetMethod("OnRectTransformDimensionsChange", 0);
                    mOnRectTransformDimensionsChangeMethodGot = true;
                }

                if (mOnRectTransformDimensionsChangeMethod != null)
                {
                    appdomain.Invoke(mOnRectTransformDimensionsChangeMethod, instance, param0);
                }
            }
        }

        IMethod mToStringMethod;
        bool mToStringMethodGot;
        public override string ToString()
        {
            if (instance != null)
            {
                if (!mToStringMethodGot)
                {
                    mToStringMethod =
                        instance.Type.GetMethod("ToString", 0);
                    mToStringMethodGot = true;
                }

                if (mToStringMethod != null)
                {
                    appdomain.Invoke(mToStringMethod, instance, param0);
                }
            }

            return instance?.Type?.FullName ?? base.ToString();
        }
    }
}