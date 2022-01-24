using System;
using System.Threading.Tasks;
using ILRuntime.CLR.Method;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.Runtime.Intepreter;
using JEngine.Core;
using UnityEngine;
using AppDomain = ILRuntime.Runtime.Enviorment.AppDomain;

public class MonoBehaviourAdapter : CrossBindingAdaptor
{
    public override Type BaseCLRType => typeof(MonoBehaviour);

    public override Type AdaptorType => typeof(Adaptor);

    public override object CreateCLRInstance(AppDomain appdomain, ILTypeInstance instance)
    {
        return new Adaptor(appdomain, instance);
    }

    //为了完整实现MonoBehaviour的所有特性，这个Adapter还得扩展，这里只抛砖引玉，只实现了最常用的Awake, Start和Update
    public class Adaptor : MonoBehaviour, CrossBindingAdaptorType
    {
        ILTypeInstance _instance;
        AppDomain _appdomain;

        public bool isMonoBehaviour = true;

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

        public void Reset()
        {
            _mAwakeMethodGot = false;
            _mStartMethodGot = false;
            _mUpdateMethodGot = false;
            _mFixedUpdateMethodGot = false;
            _mLateUpdateMethodGot = false;
            _mOnEnableMethodGot = false;
            _mOnDisableMethodGot = false;
            _mDestroyMethodGot = false;
            _mOnTriggerEnterMethodGot = false;
            _mOnTriggerStayMethodGot = false;
            _mOnTriggerExitMethodGot = false;
            _mOnCollisionEnterMethodGot = false;
            _mOnCollisionStayMethodGot = false;
            _mOnCollisionExitMethodGot = false;
            _mOnValidateMethodGot = false;
            _mOnAnimatorMoveMethodGot = false;
            _mOnApplicationFocusMethodGot = false;
            _mOnApplicationPauseMethodGot = false;
            _mOnApplicationQuitMethodGot = false;
            _mOnBecameInvisibleMethodGot = false;
            _mOnBecameVisibleMethodGot = false;
            _mOnDrawGizmosMethodGot = false;
            _mOnJointBreakMethodGot = false;
            _mOnMouseDownMethodGot = false;
            _mOnMouseDragMethodGot = false;
            _mOnMouseEnterMethodGot = false;
            _mOnMouseExitMethodGot = false;
            _mOnMouseOverMethodGot = false;
            _mOnMouseUpMethodGot = false;
            _mOnParticleCollisionMethodGot = false;
            _mOnParticleTriggerMethodGot = false;
            _mOnPostRenderMethodGot = false;
            _mOnPreCullMethodGot = false;
            _mOnPreRenderMethodGot = false;
            _mOnRenderImageMethodGot = false;
            _mOnRenderObjectMethodGot = false;
            _mOnServerInitializedMethodGot = false;
            _mOnAnimatorIKMethodGot = false;
            _mOnAudioFilterReadMethodGot = false;
            _mOnCanvasGroupChangedMethodGot = false;
            _mOnCanvasHierarchyChangedMethodGot = false;
            _mOnCollisionEnter2DMethodGot = false;
            _mOnCollisionExit2DMethodGot = false;
            _mOnCollisionStay2DMethodGot = false;
            _mOnConnectedToServerMethodGot = false;
            _mOnControllerColliderHitMethodGot = false;
            _mOnDrawGizmosSelectedMethodGot = false;
            _mOnGUIMethodGot = false;
            _mOnJointBreak2DMethodGot = false;
            _mOnParticleSystemStoppedMethodGot = false;
            _mOnTransformChildrenChangedMethodGot = false;
            _mOnTransformParentChangedMethodGot = false;
            _mOnTriggerEnter2DMethodGot = false;
            _mOnTriggerExit2DMethodGot = false;
            _mOnTriggerStay2DMethodGot = false;
            _mOnWillRenderObjectMethodGot = false;
            _mOnBeforeTransformParentChangedMethodGot = false;
            _mOnDidApplyAnimationPropertiesMethodGot = false;
            _mOnMouseUpAsButtonMethodGot = false;
            _mOnParticleUpdateJobScheduledMethodGot = false;
            _mOnRectTransformDimensionsChangeMethodGot = false;
            _instance = null;
            _appdomain = null;
            _destoryed = false;
            awaked = false;
            isAwaking = false;
        }

        private bool _destoryed;

        IMethod _mAwakeMethod;
        bool _mAwakeMethodGot;
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
                    if (!_mAwakeMethodGot)
                    {
                        _mAwakeMethod = _instance.Type.GetMethod("Awake", 0);
                        _mAwakeMethodGot = true;
                    }

                    if (!isAwaking)
                    {
                        isAwaking = true;
                        //没激活就别awake
                        try
                        {
                            while (Application.isPlaying && !_destoryed && !gameObject.activeInHierarchy)
                            {
                                await Task.Delay(20);
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

                        if (_mAwakeMethod != null) 
                        {
                            _appdomain.Invoke(_mAwakeMethod, _instance, Tools.Param0);
                        }
                        
                        isAwaking = false;
                        awaked = true;
                        OnEnable();
                        Start();
                    }
                }
            }
            catch (NullReferenceException)
            {
                //如果出现了Null，那就重新Awake
                Awake();
            }
        }

        IMethod _mStartMethod;
        bool _mStartMethodGot;
        private bool started;

        void Start()
        {
            if (!awaked || started)
            {
                return;
            }
            
            if (!isMonoBehaviour) return;

            if (!_mStartMethodGot)
            {
                _mStartMethod = _instance.Type.GetMethod("Start", 0);
                _mStartMethodGot = true;
            }

            if (_mStartMethod != null)
            {
                _appdomain.Invoke(_mStartMethod, _instance, Tools.Param0);
                started = true;
            }
        }

        IMethod _mUpdateMethod;
        bool _mUpdateMethodGot;

        void Update()
        {
            if (!isMonoBehaviour) return;

            if (!_mUpdateMethodGot)
            {
                _mUpdateMethod = _instance.Type.GetMethod("Update", 0);
                _mUpdateMethodGot = true;
            }

            if (_mUpdateMethod != null)
            {
                _appdomain.Invoke(_mUpdateMethod, _instance, Tools.Param0);
            }

        }

        IMethod _mFixedUpdateMethod;
        bool _mFixedUpdateMethodGot;

        void FixedUpdate()
        {
            if (!isMonoBehaviour) return;

            if (!_mFixedUpdateMethodGot)
            {
                _mFixedUpdateMethod = _instance.Type.GetMethod("FixedUpdate", 0);
                _mFixedUpdateMethodGot = true;
            }

            if (_mFixedUpdateMethod != null)
            {
                _appdomain.Invoke(_mFixedUpdateMethod, _instance, Tools.Param0);
            }
        }

        IMethod _mLateUpdateMethod;
        bool _mLateUpdateMethodGot;

        void LateUpdate()
        {
            if (!isMonoBehaviour) return;

            if (!_mLateUpdateMethodGot)
            {
                _mLateUpdateMethod = _instance.Type.GetMethod("LateUpdate", 0);
                _mLateUpdateMethodGot = true;
            }

            if (_mLateUpdateMethod != null)
            {
                _appdomain.Invoke(_mLateUpdateMethod, _instance, Tools.Param0);
            }
        }

        IMethod _mOnEnableMethod;
        bool _mOnEnableMethodGot;

        void OnEnable()
        {
            if (!isMonoBehaviour) return;

            if (_instance != null)
            {
                if (!_mOnEnableMethodGot)
                {
                    _mOnEnableMethod = _instance.Type.GetMethod("OnEnable", 0);
                    _mOnEnableMethodGot = true;
                }

                if (_mOnEnableMethod != null && awaked)
                {
                    _appdomain.Invoke(_mOnEnableMethod, _instance, Tools.Param0);
                }
            }
        }

        IMethod _mOnDisableMethod;
        bool _mOnDisableMethodGot;

        void OnDisable()
        {
            if (!isMonoBehaviour) return;

            if (_instance != null)
            {
                if (!_mOnDisableMethodGot)
                {
                    _mOnDisableMethod = _instance.Type.GetMethod("OnDisable", 0);
                    _mOnDisableMethodGot = true;
                }

                if (_mOnDisableMethod != null)
                {
                    _appdomain.Invoke(_mOnDisableMethod, _instance, Tools.Param0);
                }
            }
        }

        IMethod _mDestroyMethod;
        bool _mDestroyMethodGot;

        void OnDestroy()
        {
            _destoryed = true;

            if (!isMonoBehaviour) return;

            if (!_mDestroyMethodGot)
            {
                _mDestroyMethod = _instance.Type.GetMethod("OnDestroy", 0);
                _mDestroyMethodGot = true;
            }

            if (_mDestroyMethod != null)
            {
                _appdomain.Invoke(_mDestroyMethod, _instance, Tools.Param0);
            }
        }

        IMethod _mOnTriggerEnterMethod;
        bool _mOnTriggerEnterMethodGot;

        void OnTriggerEnter(Collider other)
        {
            if (!isMonoBehaviour) return;

            if (!_mOnTriggerEnterMethodGot)
            {
                _mOnTriggerEnterMethod = _instance.Type.GetMethod("OnTriggerEnter", 1);
                _mOnTriggerEnterMethodGot = true;
            }

            if (_mOnTriggerEnterMethod != null)
            {
                _appdomain.Invoke(_mOnTriggerEnterMethod, _instance, other);
            }
        }

        IMethod _mOnTriggerStayMethod;
        bool _mOnTriggerStayMethodGot;

        void OnTriggerStay(Collider other)
        {
            if (!isMonoBehaviour) return;

            if (!_mOnTriggerStayMethodGot)
            {
                _mOnTriggerStayMethod = _instance.Type.GetMethod("OnTriggerStay", 1);
                _mOnTriggerStayMethodGot = true;
            }

            if (_mOnTriggerStayMethod != null)
            {
                _appdomain.Invoke(_mOnTriggerStayMethod, _instance, other);
            }
        }

        IMethod _mOnTriggerExitMethod;
        bool _mOnTriggerExitMethodGot;

        void OnTriggerExit(Collider other)
        {
            if (!isMonoBehaviour) return;

            if (!_mOnTriggerExitMethodGot)
            {
                _mOnTriggerExitMethod = _instance.Type.GetMethod("OnTriggerExit", 1);
                _mOnTriggerExitMethodGot = true;
            }

            if (_mOnTriggerExitMethod != null)
            {
                _appdomain.Invoke(_mOnTriggerExitMethod, _instance, other);
            }
        }

        IMethod _mOnCollisionEnterMethod;
        bool _mOnCollisionEnterMethodGot;

        void OnCollisionEnter(Collision other)
        {
            if (!isMonoBehaviour) return;

            if (!_mOnCollisionEnterMethodGot)
            {
                _mOnCollisionEnterMethod = _instance.Type.GetMethod("OnCollisionEnter", 1);
                _mOnCollisionEnterMethodGot = true;
            }

            if (_mOnCollisionEnterMethod != null)
            {
                _appdomain.Invoke(_mOnCollisionEnterMethod, _instance, other);
            }
        }

        IMethod _mOnCollisionStayMethod;
        bool _mOnCollisionStayMethodGot;

        void OnCollisionStay(Collision other)
        {
            if (!isMonoBehaviour) return;

            if (!_mOnCollisionStayMethodGot)
            {
                _mOnCollisionStayMethod = _instance.Type.GetMethod("OnCollisionStay", 1);
                _mOnCollisionStayMethodGot = true;
            }

            if (_mOnCollisionStayMethod != null)
            {
                _appdomain.Invoke(_mOnCollisionStayMethod, _instance, other);
            }
        }

        IMethod _mOnCollisionExitMethod;
        bool _mOnCollisionExitMethodGot;

        void OnCollisionExit(Collision other)
        {
            if (!isMonoBehaviour) return;

            if (!_mOnCollisionExitMethodGot)
            {
                _mOnCollisionExitMethod = _instance.Type.GetMethod("OnCollisionExit", 1);
                _mOnCollisionExitMethodGot = true;
            }

            if (_mOnCollisionExitMethod != null)
            {
                _appdomain.Invoke(_mOnCollisionExitMethod, _instance, other);
            }
        }


        IMethod _mOnValidateMethod;
        bool _mOnValidateMethodGot;

        void OnValidate()
        {
            if (!isMonoBehaviour) return;

            if (_instance != null)
            {
                if (!_mOnValidateMethodGot)
                {
                    _mOnValidateMethod = _instance.Type.GetMethod("OnValidate", 0);
                    _mOnValidateMethodGot = true;
                }

                if (_mOnValidateMethod != null)
                {
                    _appdomain.Invoke(_mOnValidateMethod, _instance, Tools.Param0);
                }
            }
        }

        IMethod _mOnAnimatorMoveMethod;
        bool _mOnAnimatorMoveMethodGot;

        void OnAnimatorMove()
        {
            if (!isMonoBehaviour) return;

            if (_instance != null)
            {
                if (!_mOnAnimatorMoveMethodGot)
                {
                    _mOnAnimatorMoveMethod = _instance.Type.GetMethod("OnAnimatorMove", 0);
                    _mOnAnimatorMoveMethodGot = true;
                }

                if (_mOnAnimatorMoveMethod != null)
                {
                    _appdomain.Invoke(_mOnAnimatorMoveMethod, _instance, Tools.Param0);
                }
                else
                {
                    var animator = gameObject.GetComponent<Animator>();
                    if (animator != null)
                    {
                        animator.ApplyBuiltinRootMotion();
                    }
                }
            }
        }

        IMethod _mOnApplicationFocusMethod;
        bool _mOnApplicationFocusMethodGot;

        void OnApplicationFocus(bool hasFocus)
        {
            if (!isMonoBehaviour) return;

            if (_instance != null)
            {
                if (!_mOnApplicationFocusMethodGot)
                {
                    _mOnApplicationFocusMethod = _instance.Type.GetMethod("OnApplicationFocus", 1);
                    _mOnApplicationFocusMethodGot = true;
                }

                if (_mOnApplicationFocusMethod != null)
                {
                    _appdomain.Invoke(_mOnApplicationFocusMethod, _instance, hasFocus);
                }
            }
        }

        IMethod _mOnApplicationPauseMethod;
        bool _mOnApplicationPauseMethodGot;

        void OnApplicationPause(bool pauseStatus)
        {
            if (!isMonoBehaviour) return;

            if (_instance != null)
            {
                if (!_mOnApplicationPauseMethodGot)
                {
                    _mOnApplicationPauseMethod = _instance.Type.GetMethod("OnApplicationPause", 1);
                    _mOnApplicationPauseMethodGot = true;
                }

                if (_mOnApplicationPauseMethod != null)
                {
                    _appdomain.Invoke(_mOnApplicationPauseMethod, _instance, pauseStatus);
                }
            }
        }

        IMethod _mOnApplicationQuitMethod;
        bool _mOnApplicationQuitMethodGot;

        void OnApplicationQuit()
        {
            if (!isMonoBehaviour) return;

            if (_instance != null)
            {
                if (!_mOnApplicationQuitMethodGot)
                {
                    _mOnApplicationQuitMethod = _instance.Type.GetMethod("OnApplicationQuit", 0);
                    _mOnApplicationQuitMethodGot = true;
                }

                if (_mOnApplicationQuitMethod != null)
                {
                    _appdomain.Invoke(_mOnApplicationQuitMethod, _instance, Tools.Param0);
                }
            }
        }

        IMethod _mOnBecameInvisibleMethod;
        bool _mOnBecameInvisibleMethodGot;

        void OnBecameInvisible()
        {
            if (!isMonoBehaviour) return;

            if (_instance != null)
            {
                if (!_mOnBecameInvisibleMethodGot)
                {
                    _mOnBecameInvisibleMethod = _instance.Type.GetMethod("OnBecameInvisible", 0);
                    _mOnBecameInvisibleMethodGot = true;
                }

                if (_mOnBecameInvisibleMethod != null)
                {
                    _appdomain.Invoke(_mOnBecameInvisibleMethod, _instance, Tools.Param0);
                }
            }
        }

        IMethod _mOnBecameVisibleMethod;
        bool _mOnBecameVisibleMethodGot;

        void OnBecameVisible()
        {
            if (!isMonoBehaviour) return;

            if (_instance != null)
            {
                if (!_mOnBecameVisibleMethodGot)
                {
                    _mOnBecameVisibleMethod = _instance.Type.GetMethod("OnBecameVisible", 0);
                    _mOnBecameVisibleMethodGot = true;
                }

                if (_mOnBecameVisibleMethod != null)
                {
                    _appdomain.Invoke(_mOnBecameVisibleMethod, _instance, Tools.Param0);
                }
            }
        }

        IMethod _mOnDrawGizmosMethod;
        bool _mOnDrawGizmosMethodGot;

        void OnDrawGizmos()
        {
            if (!isMonoBehaviour) return;

            if (_instance != null)
            {
                if (!_mOnDrawGizmosMethodGot)
                {
                    _mOnDrawGizmosMethod = _instance.Type.GetMethod("OnDrawGizmos", 0);
                    _mOnDrawGizmosMethodGot = true;
                }

                if (_mOnDrawGizmosMethod != null)
                {
                    _appdomain.Invoke(_mOnDrawGizmosMethod, _instance, Tools.Param0);
                }
            }
        }

        IMethod _mOnJointBreakMethod;
        bool _mOnJointBreakMethodGot;

        void OnJointBreak(float breakForce)
        {
            if (!isMonoBehaviour) return;

            if (_instance != null)
            {
                if (!_mOnJointBreakMethodGot)
                {
                    _mOnJointBreakMethod = _instance.Type.GetMethod("OnJointBreak", 1);
                    _mOnJointBreakMethodGot = true;
                }

                if (_mOnJointBreakMethod != null)
                {
                    _appdomain.Invoke(_mOnJointBreakMethod, _instance, breakForce);
                }
            }
        }

        IMethod _mOnMouseDownMethod;
        bool _mOnMouseDownMethodGot;

        void OnMouseDown()
        {
            if (!isMonoBehaviour) return;

            if (_instance != null)
            {
                if (!_mOnMouseDownMethodGot)
                {
                    _mOnMouseDownMethod = _instance.Type.GetMethod("OnMouseDown", 0);
                    _mOnMouseDownMethodGot = true;
                }

                if (_mOnMouseDownMethod != null)
                {
                    _appdomain.Invoke(_mOnMouseDownMethod, _instance, Tools.Param0);
                }
            }
        }

        IMethod _mOnMouseDragMethod;
        bool _mOnMouseDragMethodGot;

        void OnMouseDrag()
        {
            if (!isMonoBehaviour) return;

            if (_instance != null)
            {
                if (!_mOnMouseDragMethodGot)
                {
                    _mOnMouseDragMethod = _instance.Type.GetMethod("OnMouseDrag", 0);
                    _mOnMouseDragMethodGot = true;
                }

                if (_mOnMouseDragMethod != null)
                {
                    _appdomain.Invoke(_mOnMouseDragMethod, _instance, Tools.Param0);
                }
            }
        }

        IMethod _mOnMouseEnterMethod;
        bool _mOnMouseEnterMethodGot;

        void OnMouseEnter()
        {
            if (!isMonoBehaviour) return;

            if (_instance != null)
            {
                if (!_mOnMouseEnterMethodGot)
                {
                    _mOnMouseEnterMethod = _instance.Type.GetMethod("OnMouseEnter", 0);
                    _mOnMouseEnterMethodGot = true;
                }

                if (_mOnMouseEnterMethod != null)
                {
                    _appdomain.Invoke(_mOnMouseEnterMethod, _instance, Tools.Param0);
                }
            }
        }

        IMethod _mOnMouseExitMethod;
        bool _mOnMouseExitMethodGot;

        void OnMouseExit()
        {
            if (!isMonoBehaviour) return;

            if (_instance != null)
            {
                if (!_mOnMouseExitMethodGot)
                {
                    _mOnMouseExitMethod = _instance.Type.GetMethod("OnMouseExit", 0);
                    _mOnMouseExitMethodGot = true;
                }

                if (_mOnMouseExitMethod != null)
                {
                    _appdomain.Invoke(_mOnMouseExitMethod, _instance, Tools.Param0);
                }
            }
        }

        IMethod _mOnMouseOverMethod;
        bool _mOnMouseOverMethodGot;

        void OnMouseOver()
        {
            if (!isMonoBehaviour) return;

            if (_instance != null)
            {
                if (!_mOnMouseOverMethodGot)
                {
                    _mOnMouseOverMethod = _instance.Type.GetMethod("OnMouseOver", 0);
                    _mOnMouseOverMethodGot = true;
                }

                if (_mOnMouseOverMethod != null)
                {
                    _appdomain.Invoke(_mOnMouseOverMethod, _instance, Tools.Param0);
                }
            }
        }

        IMethod _mOnMouseUpMethod;
        bool _mOnMouseUpMethodGot;

        void OnMouseUp()
        {
            if (!isMonoBehaviour) return;

            if (_instance != null)
            {
                if (!_mOnMouseUpMethodGot)
                {
                    _mOnMouseUpMethod = _instance.Type.GetMethod("OnMouseUp", 0);
                    _mOnMouseUpMethodGot = true;
                }

                if (_mOnMouseUpMethod != null)
                {
                    _appdomain.Invoke(_mOnMouseUpMethod, _instance, Tools.Param0);
                }
            }
        }

        IMethod _mOnParticleCollisionMethod;
        bool _mOnParticleCollisionMethodGot;

        void OnParticleCollision(GameObject other)
        {
            if (!isMonoBehaviour) return;

            if (_instance != null)
            {
                if (!_mOnParticleCollisionMethodGot)
                {
                    _mOnParticleCollisionMethod = _instance.Type.GetMethod("OnParticleCollision", 1);
                    _mOnParticleCollisionMethodGot = true;
                }

                if (_mOnParticleCollisionMethod != null)
                {
                    _appdomain.Invoke(_mOnParticleCollisionMethod, _instance, other);
                }
            }
        }

        IMethod _mOnParticleTriggerMethod;
        bool _mOnParticleTriggerMethodGot;

        void OnParticleTrigger()
        {
            if (!isMonoBehaviour) return;

            if (_instance != null)
            {
                if (!_mOnParticleTriggerMethodGot)
                {
                    _mOnParticleTriggerMethod = _instance.Type.GetMethod("OnParticleTrigger", 0);
                    _mOnParticleTriggerMethodGot = true;
                }

                if (_mOnParticleTriggerMethod != null)
                {
                    _appdomain.Invoke(_mOnParticleTriggerMethod, _instance, Tools.Param0);
                }
            }
        }

        IMethod _mOnPostRenderMethod;
        bool _mOnPostRenderMethodGot;

        void OnPostRender()
        {
            if (!isMonoBehaviour) return;

            if (_instance != null)
            {
                if (!_mOnPostRenderMethodGot)
                {
                    _mOnPostRenderMethod = _instance.Type.GetMethod("OnPostRender", 0);
                    _mOnPostRenderMethodGot = true;
                }

                if (_mOnPostRenderMethod != null)
                {
                    _appdomain.Invoke(_mOnPostRenderMethod, _instance, Tools.Param0);
                }
            }
        }

        IMethod _mOnPreCullMethod;
        bool _mOnPreCullMethodGot;

        void OnPreCull()
        {
            if (!isMonoBehaviour) return;

            if (_instance != null)
            {
                if (!_mOnPreCullMethodGot)
                {
                    _mOnPreCullMethod = _instance.Type.GetMethod("OnPreCull", 0);
                    _mOnPreCullMethodGot = true;
                }

                if (_mOnPreCullMethod != null)
                {
                    _appdomain.Invoke(_mOnPreCullMethod, _instance, Tools.Param0);
                }
            }
        }

        IMethod _mOnPreRenderMethod;
        bool _mOnPreRenderMethodGot;

        void OnPreRender()
        {
            if (!isMonoBehaviour) return;

            if (_instance != null)
            {
                if (!_mOnPreRenderMethodGot)
                {
                    _mOnPreRenderMethod = _instance.Type.GetMethod("OnPreRender", 0);
                    _mOnPreRenderMethodGot = true;
                }

                if (_mOnPreRenderMethod != null)
                {
                    _appdomain.Invoke(_mOnPreRenderMethod, _instance, Tools.Param0);
                }
            }
        }

        IMethod _mOnRenderImageMethod;
        bool _mOnRenderImageMethodGot;
        private bool _isCamera;
        private bool _hasChecked;
        void OnRenderImage(RenderTexture src, RenderTexture dest)
        {
            if (!_hasChecked)
            {
                _isCamera = GetComponent<Camera>() != null;
                _hasChecked = true;
            }

            if (_isCamera)
            {
                Graphics.Blit(src, dest);
            }
            if (!isMonoBehaviour) return;
            if (_instance != null)
            {
                if (!_mOnRenderImageMethodGot)
                {
                    _mOnRenderImageMethod = _instance.Type.GetMethod("OnRenderImage", 2);
                    _mOnRenderImageMethodGot = true;
                }

                if (_mOnRenderImageMethod != null)
                {
                    _appdomain.Invoke(_mOnRenderImageMethod, _instance, src, dest);
                }
            }
        }

        IMethod _mOnRenderObjectMethod;
        bool _mOnRenderObjectMethodGot;

        void OnRenderObject()
        {
            if (!isMonoBehaviour) return;

            if (_instance != null)
            {
                if (!_mOnRenderObjectMethodGot)
                {
                    _mOnRenderObjectMethod = _instance.Type.GetMethod("OnRenderObject", 0);
                    _mOnRenderObjectMethodGot = true;
                }

                if (_mOnRenderObjectMethod != null)
                {
                    _appdomain.Invoke(_mOnRenderObjectMethod, _instance, Tools.Param0);
                }
            }
        }

        IMethod _mOnServerInitializedMethod;
        bool _mOnServerInitializedMethodGot;

        void OnServerInitialized()
        {
            if (!isMonoBehaviour) return;

            if (_instance != null)
            {
                if (!_mOnServerInitializedMethodGot)
                {
                    _mOnServerInitializedMethod = _instance.Type.GetMethod("OnServerInitialized", 0);
                    _mOnServerInitializedMethodGot = true;
                }

                if (_mOnServerInitializedMethod != null)
                {
                    _appdomain.Invoke(_mOnServerInitializedMethod, _instance, Tools.Param0);
                }
            }
        }

        IMethod _mOnAnimatorIKMethod;
        bool _mOnAnimatorIKMethodGot;

        void OnAnimatorIK(int layerIndex)
        {
            if (!isMonoBehaviour) return;

            if (_instance != null)
            {
                if (!_mOnAnimatorIKMethodGot)
                {
                    _mOnAnimatorIKMethod = _instance.Type.GetMethod("OnAnimatorIK", 1);
                    _mOnAnimatorIKMethodGot = true;
                }

                if (_mOnAnimatorIKMethod != null)
                {
                    _appdomain.Invoke(_mOnAnimatorIKMethod, _instance, layerIndex);
                }
            }
        }

        IMethod _mOnAudioFilterReadMethod;
        bool _mOnAudioFilterReadMethodGot;

        void OnAudioFilterRead(float[] data, int channels)
        {
            if (!isMonoBehaviour) return;

            if (_instance != null)
            {
                if (!_mOnAudioFilterReadMethodGot)
                {
                    _mOnAudioFilterReadMethod = _instance.Type.GetMethod("OnAudioFilterRead", 2);
                    _mOnAudioFilterReadMethodGot = true;
                }

                if (_mOnAudioFilterReadMethod != null)
                {
                    _appdomain.Invoke(_mOnAudioFilterReadMethod, _instance, data, channels);
                }
            }
        }


        IMethod _mOnCanvasGroupChangedMethod;
        bool _mOnCanvasGroupChangedMethodGot;

        void OnCanvasGroupChanged()
        {
            if (!isMonoBehaviour) return;

            if (_instance != null)
            {
                if (!_mOnCanvasGroupChangedMethodGot)
                {
                    _mOnCanvasGroupChangedMethod = _instance.Type.GetMethod("OnCanvasGroupChanged", 0);
                    _mOnCanvasGroupChangedMethodGot = true;
                }

                if (_mOnCanvasGroupChangedMethod != null)
                {
                    _appdomain.Invoke(_mOnCanvasGroupChangedMethod, _instance, Tools.Param0);
                }
            }
        }

        IMethod _mOnCanvasHierarchyChangedMethod;
        bool _mOnCanvasHierarchyChangedMethodGot;

        void OnCanvasHierarchyChanged()
        {
            if (!isMonoBehaviour) return;

            if (_instance != null)
            {
                if (!_mOnCanvasHierarchyChangedMethodGot)
                {
                    _mOnCanvasHierarchyChangedMethod = _instance.Type.GetMethod("OnCanvasHierarchyChanged", 0);
                    _mOnCanvasHierarchyChangedMethodGot = true;
                }

                if (_mOnCanvasHierarchyChangedMethod != null)
                {
                    _appdomain.Invoke(_mOnCanvasHierarchyChangedMethod, _instance, Tools.Param0);
                }
            }
        }

        IMethod _mOnCollisionEnter2DMethod;
        bool _mOnCollisionEnter2DMethodGot;

        void OnCollisionEnter2D(Collision2D other)
        {
            if (!isMonoBehaviour) return;

            if (!_mOnCollisionEnter2DMethodGot)
            {
                _mOnCollisionEnter2DMethod = _instance.Type.GetMethod("OnCollisionEnter2D", 1);
                _mOnCollisionEnter2DMethodGot = true;
            }

            if (_mOnCollisionEnter2DMethod != null)
            {
                _appdomain.Invoke(_mOnCollisionEnter2DMethod, _instance, other);
            }
        }

        IMethod _mOnCollisionExit2DMethod;
        bool _mOnCollisionExit2DMethodGot;

        void OnCollisionExit2D(Collision2D other)
        {
            if (!isMonoBehaviour) return;

            if (!_mOnCollisionExit2DMethodGot)
            {
                _mOnCollisionExit2DMethod = _instance.Type.GetMethod("OnCollisionExit2D", 1);
                _mOnCollisionExit2DMethodGot = true;
            }

            if (_mOnCollisionExit2DMethod != null)
            {
                _appdomain.Invoke(_mOnCollisionExit2DMethod, _instance, other);
            }
        }

        IMethod _mOnCollisionStay2DMethod;
        bool _mOnCollisionStay2DMethodGot;

        void OnCollisionStay2D(Collision2D other)
        {
            if (!isMonoBehaviour) return;

            if (!_mOnCollisionStay2DMethodGot)
            {
                _mOnCollisionStay2DMethod = _instance.Type.GetMethod("OnCollisionStay2D", 1);
                _mOnCollisionStay2DMethodGot = true;
            }

            if (_mOnCollisionStay2DMethod != null)
            {
                _appdomain.Invoke(_mOnCollisionStay2DMethod, _instance, other);
            }
        }

        IMethod _mOnConnectedToServerMethod;
        bool _mOnConnectedToServerMethodGot;

        void OnConnectedToServer()
        {
            if (!isMonoBehaviour) return;

            if (_instance != null)
            {
                if (!_mOnConnectedToServerMethodGot)
                {
                    _mOnConnectedToServerMethod = _instance.Type.GetMethod("OnConnectedToServer", 0);
                    _mOnConnectedToServerMethodGot = true;
                }

                if (_mOnConnectedToServerMethod != null)
                {
                    _appdomain.Invoke(_mOnConnectedToServerMethod, _instance, Tools.Param0);
                }
            }
        }

        IMethod _mOnControllerColliderHitMethod;
        bool _mOnControllerColliderHitMethodGot;

        void OnControllerColliderHit(ControllerColliderHit hit)
        {
            if (!isMonoBehaviour) return;

            if (_instance != null)
            {
                if (!_mOnControllerColliderHitMethodGot)
                {
                    _mOnControllerColliderHitMethod = _instance.Type.GetMethod("OnControllerColliderHit", 1);
                    _mOnControllerColliderHitMethodGot = true;
                }

                if (_mOnControllerColliderHitMethod != null)
                {
                    _appdomain.Invoke(_mOnControllerColliderHitMethod, _instance, hit);
                }
            }
        }

        IMethod _mOnDrawGizmosSelectedMethod;
        bool _mOnDrawGizmosSelectedMethodGot;

        void OnDrawGizmosSelected()
        {
            if (!isMonoBehaviour) return;

            if (_instance != null)
            {
                if (!_mOnDrawGizmosSelectedMethodGot)
                {
                    _mOnDrawGizmosSelectedMethod = _instance.Type.GetMethod("OnDrawGizmosSelected", 0);
                    _mOnDrawGizmosSelectedMethodGot = true;
                }

                if (_mOnDrawGizmosSelectedMethod != null)
                {
                    _appdomain.Invoke(_mOnDrawGizmosSelectedMethod, _instance, Tools.Param0);
                }
            }
        }

        IMethod _mOnGUIMethod;
        bool _mOnGUIMethodGot;

        void OnGUI()
        {
            if (!isMonoBehaviour) return;

            if (_instance != null)
            {
                if (!_mOnGUIMethodGot)
                {
                    _mOnGUIMethod = _instance.Type.GetMethod("OnGUI", 0);
                    _mOnGUIMethodGot = true;
                }

                if (_mOnGUIMethod != null)
                {
                    _appdomain.Invoke(_mOnGUIMethod, _instance, Tools.Param0);
                }
            }
        }

        IMethod _mOnJointBreak2DMethod;
        bool _mOnJointBreak2DMethodGot;

        void OnJointBreak2D(Joint2D brokenJoint)
        {
            if (!isMonoBehaviour) return;

            if (_instance != null)
            {
                if (!_mOnJointBreak2DMethodGot)
                {
                    _mOnJointBreak2DMethod = _instance.Type.GetMethod("OnJointBreak2D", 1);
                    _mOnJointBreak2DMethodGot = true;
                }

                if (_mOnJointBreak2DMethod != null)
                {
                    _appdomain.Invoke(_mOnJointBreak2DMethod, _instance, brokenJoint);
                }
            }
        }

        IMethod _mOnParticleSystemStoppedMethod;
        bool _mOnParticleSystemStoppedMethodGot;

        void OnParticleSystemStopped()
        {
            if (!isMonoBehaviour) return;

            if (_instance != null)
            {
                if (!_mOnParticleSystemStoppedMethodGot)
                {
                    _mOnParticleSystemStoppedMethod = _instance.Type.GetMethod("OnParticleSystemStopped", 0);
                    _mOnParticleSystemStoppedMethodGot = true;
                }

                if (_mOnParticleSystemStoppedMethod != null)
                {
                    _appdomain.Invoke(_mOnParticleSystemStoppedMethod, _instance, Tools.Param0);
                }
            }
        }

        IMethod _mOnTransformChildrenChangedMethod;
        bool _mOnTransformChildrenChangedMethodGot;

        void OnTransformChildrenChanged()
        {
            if (!isMonoBehaviour) return;

            if (_instance != null)
            {
                if (!_mOnTransformChildrenChangedMethodGot)
                {
                    _mOnTransformChildrenChangedMethod = _instance.Type.GetMethod("OnTransformChildrenChanged", 0);
                    _mOnTransformChildrenChangedMethodGot = true;
                }

                if (_mOnTransformChildrenChangedMethod != null)
                {
                    _appdomain.Invoke(_mOnTransformChildrenChangedMethod, _instance, Tools.Param0);
                }
            }
        }

        IMethod _mOnTransformParentChangedMethod;
        bool _mOnTransformParentChangedMethodGot;

        void OnTransformParentChanged()
        {
            if (!isMonoBehaviour) return;

            if (_instance != null)
            {
                if (!_mOnTransformParentChangedMethodGot)
                {
                    _mOnTransformParentChangedMethod = _instance.Type.GetMethod("OnTransformParentChanged", 0);
                    _mOnTransformParentChangedMethodGot = true;
                }

                if (_mOnTransformParentChangedMethod != null)
                {
                    _appdomain.Invoke(_mOnTransformParentChangedMethod, _instance, Tools.Param0);
                }
            }
        }

        IMethod _mOnTriggerEnter2DMethod;
        bool _mOnTriggerEnter2DMethodGot;

        void OnTriggerEnter2D(Collider2D other)
        {
            if (!isMonoBehaviour) return;

            if (_instance != null)
            {
                if (!_mOnTriggerEnter2DMethodGot)
                {
                    _mOnTriggerEnter2DMethod = _instance.Type.GetMethod("OnTriggerEnter2D", 1);
                    _mOnTriggerEnter2DMethodGot = true;
                }

                if (_mOnTriggerEnter2DMethod != null)
                {
                    _appdomain.Invoke(_mOnTriggerEnter2DMethod, _instance, other);
                }
            }
        }

        IMethod _mOnTriggerExit2DMethod;
        bool _mOnTriggerExit2DMethodGot;

        void OnTriggerExit2D(Collider2D other)
        {
            if (!isMonoBehaviour) return;

            if (_instance != null)
            {
                if (!_mOnTriggerExit2DMethodGot)
                {
                    _mOnTriggerExit2DMethod = _instance.Type.GetMethod("OnTriggerExit2D", 1);
                    _mOnTriggerExit2DMethodGot = true;
                }

                if (_mOnTriggerExit2DMethod != null)
                {
                    _appdomain.Invoke(_mOnTriggerExit2DMethod, _instance, other);
                }
            }
        }

        IMethod _mOnTriggerStay2DMethod;
        bool _mOnTriggerStay2DMethodGot;

        void OnTriggerStay2D(Collider2D other)
        {
            if (!isMonoBehaviour) return;

            if (_instance != null)
            {
                if (!_mOnTriggerStay2DMethodGot)
                {
                    _mOnTriggerStay2DMethod = _instance.Type.GetMethod("OnTriggerStay2D", 1);
                    _mOnTriggerStay2DMethodGot = true;
                }

                if (_mOnTriggerStay2DMethod != null)
                {
                    _appdomain.Invoke(_mOnTriggerStay2DMethod, _instance, other);
                }
            }
        }

        IMethod _mOnWillRenderObjectMethod;
        bool _mOnWillRenderObjectMethodGot;

        void OnWillRenderObject()
        {
            if (!isMonoBehaviour) return;

            if (_instance != null)
            {
                if (!_mOnWillRenderObjectMethodGot)
                {
                    _mOnWillRenderObjectMethod = _instance.Type.GetMethod("OnWillRenderObject", 0);
                    _mOnWillRenderObjectMethodGot = true;
                }

                if (_mOnWillRenderObjectMethod != null)
                {
                    _appdomain.Invoke(_mOnWillRenderObjectMethod, _instance, Tools.Param0);
                }
            }
        }

        IMethod _mOnBeforeTransformParentChangedMethod;
        bool _mOnBeforeTransformParentChangedMethodGot;

        void OnBeforeTransformParentChanged()
        {
            if (!isMonoBehaviour) return;

            if (_instance != null)
            {
                if (!_mOnBeforeTransformParentChangedMethodGot)
                {
                    _mOnBeforeTransformParentChangedMethod =
                        _instance.Type.GetMethod("OnBeforeTransformParentChanged", 0);
                    _mOnBeforeTransformParentChangedMethodGot = true;
                }

                if (_mOnBeforeTransformParentChangedMethod != null)
                {
                    _appdomain.Invoke(_mOnBeforeTransformParentChangedMethod, _instance, Tools.Param0);
                }
            }
        }

        IMethod _mOnDidApplyAnimationPropertiesMethod;
        bool _mOnDidApplyAnimationPropertiesMethodGot;

        void OnDidApplyAnimationProperties()
        {
            if (!isMonoBehaviour) return;

            if (_instance != null)
            {
                if (!_mOnDidApplyAnimationPropertiesMethodGot)
                {
                    _mOnDidApplyAnimationPropertiesMethod = _instance.Type.GetMethod("OnDidApplyAnimationProperties", 0);
                    _mOnDidApplyAnimationPropertiesMethodGot = true;
                }

                if (_mOnDidApplyAnimationPropertiesMethod != null)
                {
                    _appdomain.Invoke(_mOnDidApplyAnimationPropertiesMethod, _instance, Tools.Param0);
                }
            }
        }

        IMethod _mOnMouseUpAsButtonMethod;
        bool _mOnMouseUpAsButtonMethodGot;

        void OnMouseUpAsButton()
        {
            if (!isMonoBehaviour) return;

            if (_instance != null)
            {
                if (!_mOnMouseUpAsButtonMethodGot)
                {
                    _mOnMouseUpAsButtonMethod = _instance.Type.GetMethod("OnMouseUpAsButton", 0);
                    _mOnMouseUpAsButtonMethodGot = true;
                }

                if (_mOnMouseUpAsButtonMethod != null)
                {
                    _appdomain.Invoke(_mOnMouseUpAsButtonMethod, _instance, Tools.Param0);
                }
            }
        }

        IMethod _mOnParticleUpdateJobScheduledMethod;
        bool _mOnParticleUpdateJobScheduledMethodGot;

        void OnParticleUpdateJobScheduled()
        {
            if (!isMonoBehaviour) return;

            if (_instance != null)
            {
                if (!_mOnParticleUpdateJobScheduledMethodGot)
                {
                    _mOnParticleUpdateJobScheduledMethod = _instance.Type.GetMethod("OnParticleUpdateJobScheduled", 0);
                    _mOnParticleUpdateJobScheduledMethodGot = true;
                }

                if (_mOnParticleUpdateJobScheduledMethod != null)
                {
                    _appdomain.Invoke(_mOnParticleUpdateJobScheduledMethod, _instance, Tools.Param0);
                }
            }
        }

        IMethod _mOnRectTransformDimensionsChangeMethod;
        bool _mOnRectTransformDimensionsChangeMethodGot;

        void OnRectTransformDimensionsChange()
        {
            if (!isMonoBehaviour) return;

            if (_instance != null)
            {
                if (!_mOnRectTransformDimensionsChangeMethodGot)
                {
                    _mOnRectTransformDimensionsChangeMethod =
                        _instance.Type.GetMethod("OnRectTransformDimensionsChange", 0);
                    _mOnRectTransformDimensionsChangeMethodGot = true;
                }

                if (_mOnRectTransformDimensionsChangeMethod != null)
                {
                    _appdomain.Invoke(_mOnRectTransformDimensionsChangeMethod, _instance, Tools.Param0);
                }
            }
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
                    _appdomain.Invoke(_mToStringMethod, _instance, Tools.Param0);
                }
            }

            return _instance?.Type?.FullName ?? base.ToString();
        }
    }
}