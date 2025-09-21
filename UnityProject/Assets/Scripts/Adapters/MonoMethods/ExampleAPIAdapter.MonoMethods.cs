/*
 * JEngine自动生成的Mono方法脚本，作者已经代替你掉了头发，帮你写出了这个Mono适配器脚本，让你能够直接调用全部Mono类
 */
using System;
using UnityEngine;
using JEngine.Core;
using System.Reflection;
using ILRuntime.CLR.Method;

namespace ProjectAdapter
{
    public partial class ExampleAPIAdapter
    {
        public partial class Adapter
        {
            #region Generate For Mono Events from template

            /*
             * JEngine作者匠心打造的适配mono方法模板
             * 这里开始都是JEngine提供的模板自动生成的，注册了全部Mono的Event Methods，共几十个
             * 这么多框架，只有JEngine如此贴心，为你想到了一切可能
             * 你还有什么理由不去用JEngine？
             */
            private bool _destoryed;
            
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
                if (instance != null)
                {
                    if (isAwaking) return;
                    isAwaking = true;
                    LifeCycleMgr.Instance.AddTask(instance, () =>
                    {
                        if (_destoryed) return;
                        var type = instance.Type.ReflectionType;
                        //直接Invoke
                        GetMethodInfo(type, "Awake")?.Invoke(instance, ConstMgr.NullObjects);
                        LifeCycleMgr.Instance.AddAwakeItem(instance, null); //这一帧空出来
                        //就mono订阅start和update事件
                        LifeCycleMgr.Instance.AddStartItem(instance, GetMethodInfo(type, "Start"));
                        LifeCycleMgr.Instance.AddFixedUpdateItem(instance, GetMethodInfo(type, "FixedUpdate"),
                            gameObject);
                        LifeCycleMgr.Instance.AddUpdateItem(instance, GetMethodInfo(type, "Update"), gameObject);
                        LifeCycleMgr.Instance.AddLateUpdateItem(instance, GetMethodInfo(type, "LateUpdate"),
                            gameObject);
                
                        isAwaking = false;
                        awaked = true;
                    }, () => Application.isPlaying && !_destoryed && gameObject.activeInHierarchy);
                }
            }
    
            /// <summary>
            /// 只注册没参数的方法，且必须在热更层定义（如Awake,Start,OnEnable,Update等）
            /// </summary>
            /// <param name="type"></param>
            /// <param name="funcName"></param>
            /// <returns></returns>
            private MethodInfo GetMethodInfo(Type type, string funcName)
            {
                if (instance.Type.GetMethod(funcName, 0) != null)
                {
                    return type.GetMethod(funcName);
                }
    
                return null;
            }
    
            IMethod _mOnEnableMethod;
            bool _mOnEnableMethodGot;
    
            void OnEnable()
            {
                LifeCycleMgr.Instance.AddTask(() =>
                {
                    if (instance != null)
                    {
                        if (!_mOnEnableMethodGot)
                        {
                            _mOnEnableMethod = instance.Type.GetMethod("OnEnable", 0);
                            _mOnEnableMethodGot = true;
                        }
    
                        if (_mOnEnableMethod != null)
                        {
                            if (_destoryed || !Application.isPlaying)
                            {
                                return;
                            }
                            appdomain.Invoke(_mOnEnableMethod, instance, ConstMgr.NullObjects);
                        }
                    }
                }, () => Application.isPlaying && awaked);
            }
    
            IMethod _mOnDisableMethod;
            bool _mOnDisableMethodGot;
    
            void OnDisable()
            {
                if (instance != null)
                {
                    if (!_mOnDisableMethodGot)
                    {
                        _mOnDisableMethod = instance.Type.GetMethod("OnDisable", 0);
                        _mOnDisableMethodGot = true;
                    }
    
                    if (_mOnDisableMethod != null)
                    {
                        appdomain.Invoke(_mOnDisableMethod, instance, ConstMgr.NullObjects);
                    }
                }
            }
    
            IMethod _mDestroyMethod;
            bool _mDestroyMethodGot;
    
            void OnDestroy()
            {
                _destoryed = true;
                
                if (!_mDestroyMethodGot)
                {
                    _mDestroyMethod = instance?.Type?.GetMethod("OnDestroy", 0);
                    _mDestroyMethodGot = true;
                }
    
                if (_mDestroyMethod != null)
                {
                    appdomain.Invoke(_mDestroyMethod, instance, ConstMgr.NullObjects);
                }
                

                if (Application.isPlaying)
                {
                    LifeCycleMgr.Instance.RemoveUpdateItem(instance);
                    LifeCycleMgr.Instance.RemoveFixedUpdateItem(instance);
                    LifeCycleMgr.Instance.RemoveLateUpdateItem(instance);
                }
                
                //销毁ILTypeIns
                instance = null;
            }
    
            IMethod _mOnTriggerEnterMethod;
            bool _mOnTriggerEnterMethodGot;
    
            void OnTriggerEnter(Collider other)
            {
                if (!_mOnTriggerEnterMethodGot)
                {
                    _mOnTriggerEnterMethod = instance.Type.GetMethod("OnTriggerEnter", 1);
                    _mOnTriggerEnterMethodGot = true;
                }
    
                if (_mOnTriggerEnterMethod != null)
                {
                    appdomain.Invoke(_mOnTriggerEnterMethod, instance, other);
                }
            }
    
            IMethod _mOnTriggerStayMethod;
            bool _mOnTriggerStayMethodGot;
    
            void OnTriggerStay(Collider other)
            {
                if (!_mOnTriggerStayMethodGot)
                {
                    _mOnTriggerStayMethod = instance.Type.GetMethod("OnTriggerStay", 1);
                    _mOnTriggerStayMethodGot = true;
                }
    
                if (_mOnTriggerStayMethod != null)
                {
                    appdomain.Invoke(_mOnTriggerStayMethod, instance, other);
                }
            }
    
            IMethod _mOnTriggerExitMethod;
            bool _mOnTriggerExitMethodGot;
    
            void OnTriggerExit(Collider other)
            {
                if (!_mOnTriggerExitMethodGot)
                {
                    _mOnTriggerExitMethod = instance.Type.GetMethod("OnTriggerExit", 1);
                    _mOnTriggerExitMethodGot = true;
                }
    
                if (_mOnTriggerExitMethod != null)
                {
                    appdomain.Invoke(_mOnTriggerExitMethod, instance, other);
                }
            }
    
            IMethod _mOnCollisionEnterMethod;
            bool _mOnCollisionEnterMethodGot;
    
            void OnCollisionEnter(Collision other)
            {
                if (!_mOnCollisionEnterMethodGot)
                {
                    _mOnCollisionEnterMethod = instance.Type.GetMethod("OnCollisionEnter", 1);
                    _mOnCollisionEnterMethodGot = true;
                }
    
                if (_mOnCollisionEnterMethod != null)
                {
                    appdomain.Invoke(_mOnCollisionEnterMethod, instance, other);
                }
            }
    
            IMethod _mOnCollisionStayMethod;
            bool _mOnCollisionStayMethodGot;
    
            void OnCollisionStay(Collision other)
            {
                if (!_mOnCollisionStayMethodGot)
                {
                    _mOnCollisionStayMethod = instance.Type.GetMethod("OnCollisionStay", 1);
                    _mOnCollisionStayMethodGot = true;
                }
    
                if (_mOnCollisionStayMethod != null)
                {
                    appdomain.Invoke(_mOnCollisionStayMethod, instance, other);
                }
            }
    
            IMethod _mOnCollisionExitMethod;
            bool _mOnCollisionExitMethodGot;
    
            void OnCollisionExit(Collision other)
            {
                if (!_mOnCollisionExitMethodGot)
                {
                    _mOnCollisionExitMethod = instance.Type.GetMethod("OnCollisionExit", 1);
                    _mOnCollisionExitMethodGot = true;
                }
    
                if (_mOnCollisionExitMethod != null)
                {
                    appdomain.Invoke(_mOnCollisionExitMethod, instance, other);
                }
            }
    
    
            IMethod _mOnValidateMethod;
            bool _mOnValidateMethodGot;
    
            void OnValidate()
            {
                if (instance != null)
                {
                    if (!_mOnValidateMethodGot)
                    {
                        _mOnValidateMethod = instance.Type.GetMethod("OnValidate", 0);
                        _mOnValidateMethodGot = true;
                    }
    
                    if (_mOnValidateMethod != null)
                    {
                        appdomain.Invoke(_mOnValidateMethod, instance, ConstMgr.NullObjects);
                    }
                }
            }
    
            IMethod _mOnAnimatorMoveMethod;
            bool _mOnAnimatorMoveMethodGot;
    
            void OnAnimatorMove()
            {
                if (instance != null)
                {
                    if (!_mOnAnimatorMoveMethodGot)
                    {
                        _mOnAnimatorMoveMethod = instance.Type.GetMethod("OnAnimatorMove", 0);
                        _mOnAnimatorMoveMethodGot = true;
                    }
    
                    if (_mOnAnimatorMoveMethod != null)
                    {
                        appdomain.Invoke(_mOnAnimatorMoveMethod, instance, ConstMgr.NullObjects);
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
                if (instance != null)
                {
                    if (!_mOnApplicationFocusMethodGot)
                    {
                        _mOnApplicationFocusMethod = instance.Type.GetMethod("OnApplicationFocus", 1);
                        _mOnApplicationFocusMethodGot = true;
                    }
    
                    if (_mOnApplicationFocusMethod != null)
                    {
                        appdomain.Invoke(_mOnApplicationFocusMethod, instance, hasFocus);
                    }
                }
            }
    
            IMethod _mOnApplicationPauseMethod;
            bool _mOnApplicationPauseMethodGot;
    
            void OnApplicationPause(bool pauseStatus)
            {
                if (instance != null)
                {
                    if (!_mOnApplicationPauseMethodGot)
                    {
                        _mOnApplicationPauseMethod = instance.Type.GetMethod("OnApplicationPause", 1);
                        _mOnApplicationPauseMethodGot = true;
                    }
    
                    if (_mOnApplicationPauseMethod != null)
                    {
                        appdomain.Invoke(_mOnApplicationPauseMethod, instance, pauseStatus);
                    }
                }
            }
    
            IMethod _mOnApplicationQuitMethod;
            bool _mOnApplicationQuitMethodGot;
    
            void OnApplicationQuit()
            {
                if (instance != null)
                {
                    if (!_mOnApplicationQuitMethodGot)
                    {
                        _mOnApplicationQuitMethod = instance.Type.GetMethod("OnApplicationQuit", 0);
                        _mOnApplicationQuitMethodGot = true;
                    }
    
                    if (_mOnApplicationQuitMethod != null)
                    {
                        appdomain.Invoke(_mOnApplicationQuitMethod, instance, ConstMgr.NullObjects);
                    }
                }
            }
    
            IMethod _mOnBecameInvisibleMethod;
            bool _mOnBecameInvisibleMethodGot;
    
            void OnBecameInvisible()
            {
                if (instance != null)
                {
                    if (!_mOnBecameInvisibleMethodGot)
                    {
                        _mOnBecameInvisibleMethod = instance.Type.GetMethod("OnBecameInvisible", 0);
                        _mOnBecameInvisibleMethodGot = true;
                    }
    
                    if (_mOnBecameInvisibleMethod != null)
                    {
                        appdomain.Invoke(_mOnBecameInvisibleMethod, instance, ConstMgr.NullObjects);
                    }
                }
            }
    
            IMethod _mOnBecameVisibleMethod;
            bool _mOnBecameVisibleMethodGot;
    
            void OnBecameVisible()
            {
                if (instance != null)
                {
                    if (!_mOnBecameVisibleMethodGot)
                    {
                        _mOnBecameVisibleMethod = instance.Type.GetMethod("OnBecameVisible", 0);
                        _mOnBecameVisibleMethodGot = true;
                    }
    
                    if (_mOnBecameVisibleMethod != null)
                    {
                        appdomain.Invoke(_mOnBecameVisibleMethod, instance, ConstMgr.NullObjects);
                    }
                }
            }
    
            IMethod _mOnDrawGizmosMethod;
            bool _mOnDrawGizmosMethodGot;
    
            void OnDrawGizmos()
            {
                if (instance != null)
                {
                    if (!_mOnDrawGizmosMethodGot)
                    {
                        _mOnDrawGizmosMethod = instance.Type.GetMethod("OnDrawGizmos", 0);
                        _mOnDrawGizmosMethodGot = true;
                    }
    
                    if (_mOnDrawGizmosMethod != null)
                    {
                        appdomain.Invoke(_mOnDrawGizmosMethod, instance, ConstMgr.NullObjects);
                    }
                }
            }
    
            IMethod _mOnJointBreakMethod;
            bool _mOnJointBreakMethodGot;
    
            void OnJointBreak(float breakForce)
            {
                if (instance != null)
                {
                    if (!_mOnJointBreakMethodGot)
                    {
                        _mOnJointBreakMethod = instance.Type.GetMethod("OnJointBreak", 1);
                        _mOnJointBreakMethodGot = true;
                    }
    
                    if (_mOnJointBreakMethod != null)
                    {
                        appdomain.Invoke(_mOnJointBreakMethod, instance, breakForce);
                    }
                }
            }
    
            IMethod _mOnMouseDownMethod;
            bool _mOnMouseDownMethodGot;
    
            void OnMouseDown()
            {
                if (instance != null)
                {
                    if (!_mOnMouseDownMethodGot)
                    {
                        _mOnMouseDownMethod = instance.Type.GetMethod("OnMouseDown", 0);
                        _mOnMouseDownMethodGot = true;
                    }
    
                    if (_mOnMouseDownMethod != null)
                    {
                        appdomain.Invoke(_mOnMouseDownMethod, instance, ConstMgr.NullObjects);
                    }
                }
            }
    
            IMethod _mOnMouseDragMethod;
            bool _mOnMouseDragMethodGot;
    
            void OnMouseDrag()
            {
                if (instance != null)
                {
                    if (!_mOnMouseDragMethodGot)
                    {
                        _mOnMouseDragMethod = instance.Type.GetMethod("OnMouseDrag", 0);
                        _mOnMouseDragMethodGot = true;
                    }
    
                    if (_mOnMouseDragMethod != null)
                    {
                        appdomain.Invoke(_mOnMouseDragMethod, instance, ConstMgr.NullObjects);
                    }
                }
            }
    
            IMethod _mOnMouseEnterMethod;
            bool _mOnMouseEnterMethodGot;
    
            void OnMouseEnter()
            {
                if (instance != null)
                {
                    if (!_mOnMouseEnterMethodGot)
                    {
                        _mOnMouseEnterMethod = instance.Type.GetMethod("OnMouseEnter", 0);
                        _mOnMouseEnterMethodGot = true;
                    }
    
                    if (_mOnMouseEnterMethod != null)
                    {
                        appdomain.Invoke(_mOnMouseEnterMethod, instance, ConstMgr.NullObjects);
                    }
                }
            }
    
            IMethod _mOnMouseExitMethod;
            bool _mOnMouseExitMethodGot;
    
            void OnMouseExit()
            {
                if (instance != null)
                {
                    if (!_mOnMouseExitMethodGot)
                    {
                        _mOnMouseExitMethod = instance.Type.GetMethod("OnMouseExit", 0);
                        _mOnMouseExitMethodGot = true;
                    }
    
                    if (_mOnMouseExitMethod != null)
                    {
                        appdomain.Invoke(_mOnMouseExitMethod, instance, ConstMgr.NullObjects);
                    }
                }
            }
    
            IMethod _mOnMouseOverMethod;
            bool _mOnMouseOverMethodGot;
    
            void OnMouseOver()
            {
                if (instance != null)
                {
                    if (!_mOnMouseOverMethodGot)
                    {
                        _mOnMouseOverMethod = instance.Type.GetMethod("OnMouseOver", 0);
                        _mOnMouseOverMethodGot = true;
                    }
    
                    if (_mOnMouseOverMethod != null)
                    {
                        appdomain.Invoke(_mOnMouseOverMethod, instance, ConstMgr.NullObjects);
                    }
                }
            }
    
            IMethod _mOnMouseUpMethod;
            bool _mOnMouseUpMethodGot;
    
            void OnMouseUp()
            {
                if (instance != null)
                {
                    if (!_mOnMouseUpMethodGot)
                    {
                        _mOnMouseUpMethod = instance.Type.GetMethod("OnMouseUp", 0);
                        _mOnMouseUpMethodGot = true;
                    }
    
                    if (_mOnMouseUpMethod != null)
                    {
                        appdomain.Invoke(_mOnMouseUpMethod, instance, ConstMgr.NullObjects);
                    }
                }
            }
    
            IMethod _mOnParticleCollisionMethod;
            bool _mOnParticleCollisionMethodGot;
    
            void OnParticleCollision(GameObject other)
            {
                if (instance != null)
                {
                    if (!_mOnParticleCollisionMethodGot)
                    {
                        _mOnParticleCollisionMethod = instance.Type.GetMethod("OnParticleCollision", 1);
                        _mOnParticleCollisionMethodGot = true;
                    }
    
                    if (_mOnParticleCollisionMethod != null)
                    {
                        appdomain.Invoke(_mOnParticleCollisionMethod, instance, other);
                    }
                }
            }
    
            IMethod _mOnParticleTriggerMethod;
            bool _mOnParticleTriggerMethodGot;
    
            void OnParticleTrigger()
            {
                if (instance != null)
                {
                    if (!_mOnParticleTriggerMethodGot)
                    {
                        _mOnParticleTriggerMethod = instance.Type.GetMethod("OnParticleTrigger", 0);
                        _mOnParticleTriggerMethodGot = true;
                    }
    
                    if (_mOnParticleTriggerMethod != null)
                    {
                        appdomain.Invoke(_mOnParticleTriggerMethod, instance, ConstMgr.NullObjects);
                    }
                }
            }
    
            IMethod _mOnPostRenderMethod;
            bool _mOnPostRenderMethodGot;
    
            void OnPostRender()
            {
                if (instance != null)
                {
                    if (!_mOnPostRenderMethodGot)
                    {
                        _mOnPostRenderMethod = instance.Type.GetMethod("OnPostRender", 0);
                        _mOnPostRenderMethodGot = true;
                    }
    
                    if (_mOnPostRenderMethod != null)
                    {
                        appdomain.Invoke(_mOnPostRenderMethod, instance, ConstMgr.NullObjects);
                    }
                }
            }
    
            IMethod _mOnPreCullMethod;
            bool _mOnPreCullMethodGot;
    
            void OnPreCull()
            {
                if (instance != null)
                {
                    if (!_mOnPreCullMethodGot)
                    {
                        _mOnPreCullMethod = instance.Type.GetMethod("OnPreCull", 0);
                        _mOnPreCullMethodGot = true;
                    }
    
                    if (_mOnPreCullMethod != null)
                    {
                        appdomain.Invoke(_mOnPreCullMethod, instance, ConstMgr.NullObjects);
                    }
                }
            }
    
            IMethod _mOnPreRenderMethod;
            bool _mOnPreRenderMethodGot;
    
            void OnPreRender()
            {
                if (instance != null)
                {
                    if (!_mOnPreRenderMethodGot)
                    {
                        _mOnPreRenderMethod = instance.Type.GetMethod("OnPreRender", 0);
                        _mOnPreRenderMethodGot = true;
                    }
    
                    if (_mOnPreRenderMethod != null)
                    {
                        appdomain.Invoke(_mOnPreRenderMethod, instance, ConstMgr.NullObjects);
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
                }            if (instance != null)
                {
                    if (!_mOnRenderImageMethodGot)
                    {
                        _mOnRenderImageMethod = instance.Type.GetMethod("OnRenderImage", 2);
                        _mOnRenderImageMethodGot = true;
                    }
    
                    if (_mOnRenderImageMethod != null)
                    {
                        appdomain.Invoke(_mOnRenderImageMethod, instance, src, dest);
                    }
                }
            }
    
            IMethod _mOnRenderObjectMethod;
            bool _mOnRenderObjectMethodGot;
    
            void OnRenderObject()
            {
                if (instance != null)
                {
                    if (!_mOnRenderObjectMethodGot)
                    {
                        _mOnRenderObjectMethod = instance.Type.GetMethod("OnRenderObject", 0);
                        _mOnRenderObjectMethodGot = true;
                    }
    
                    if (_mOnRenderObjectMethod != null)
                    {
                        appdomain.Invoke(_mOnRenderObjectMethod, instance, ConstMgr.NullObjects);
                    }
                }
            }
    
            IMethod _mOnServerInitializedMethod;
            bool _mOnServerInitializedMethodGot;
    
            void OnServerInitialized()
            {
                if (instance != null)
                {
                    if (!_mOnServerInitializedMethodGot)
                    {
                        _mOnServerInitializedMethod = instance.Type.GetMethod("OnServerInitialized", 0);
                        _mOnServerInitializedMethodGot = true;
                    }
    
                    if (_mOnServerInitializedMethod != null)
                    {
                        appdomain.Invoke(_mOnServerInitializedMethod, instance, ConstMgr.NullObjects);
                    }
                }
            }
    
            IMethod _mOnAnimatorIKMethod;
            bool _mOnAnimatorIKMethodGot;
    
            void OnAnimatorIK(int layerIndex)
            {
                if (instance != null)
                {
                    if (!_mOnAnimatorIKMethodGot)
                    {
                        _mOnAnimatorIKMethod = instance.Type.GetMethod("OnAnimatorIK", 1);
                        _mOnAnimatorIKMethodGot = true;
                    }
    
                    if (_mOnAnimatorIKMethod != null)
                    {
                        appdomain.Invoke(_mOnAnimatorIKMethod, instance, layerIndex);
                    }
                }
            }
    
            IMethod _mOnAudioFilterReadMethod;
            bool _mOnAudioFilterReadMethodGot;
    
            void OnAudioFilterRead(float[] data, int channels)
            {
                if (instance != null)
                {
                    if (!_mOnAudioFilterReadMethodGot)
                    {
                        _mOnAudioFilterReadMethod = instance.Type.GetMethod("OnAudioFilterRead", 2);
                        _mOnAudioFilterReadMethodGot = true;
                    }
    
                    if (_mOnAudioFilterReadMethod != null)
                    {
                        appdomain.Invoke(_mOnAudioFilterReadMethod, instance, data, channels);
                    }
                }
            }
    
    
            IMethod _mOnCanvasGroupChangedMethod;
            bool _mOnCanvasGroupChangedMethodGot;
    
            void OnCanvasGroupChanged()
            {
                if (instance != null)
                {
                    if (!_mOnCanvasGroupChangedMethodGot)
                    {
                        _mOnCanvasGroupChangedMethod = instance.Type.GetMethod("OnCanvasGroupChanged", 0);
                        _mOnCanvasGroupChangedMethodGot = true;
                    }
    
                    if (_mOnCanvasGroupChangedMethod != null)
                    {
                        appdomain.Invoke(_mOnCanvasGroupChangedMethod, instance, ConstMgr.NullObjects);
                    }
                }
            }
    
            IMethod _mOnCanvasHierarchyChangedMethod;
            bool _mOnCanvasHierarchyChangedMethodGot;
    
            void OnCanvasHierarchyChanged()
            {
                if (instance != null)
                {
                    if (!_mOnCanvasHierarchyChangedMethodGot)
                    {
                        _mOnCanvasHierarchyChangedMethod = instance.Type.GetMethod("OnCanvasHierarchyChanged", 0);
                        _mOnCanvasHierarchyChangedMethodGot = true;
                    }
    
                    if (_mOnCanvasHierarchyChangedMethod != null)
                    {
                        appdomain.Invoke(_mOnCanvasHierarchyChangedMethod, instance, ConstMgr.NullObjects);
                    }
                }
            }
    
            IMethod _mOnCollisionEnter2DMethod;
            bool _mOnCollisionEnter2DMethodGot;
    
            void OnCollisionEnter2D(Collision2D other)
            {
                if (!_mOnCollisionEnter2DMethodGot)
                {
                    _mOnCollisionEnter2DMethod = instance.Type.GetMethod("OnCollisionEnter2D", 1);
                    _mOnCollisionEnter2DMethodGot = true;
                }
    
                if (_mOnCollisionEnter2DMethod != null)
                {
                    appdomain.Invoke(_mOnCollisionEnter2DMethod, instance, other);
                }
            }
    
            IMethod _mOnCollisionExit2DMethod;
            bool _mOnCollisionExit2DMethodGot;
    
            void OnCollisionExit2D(Collision2D other)
            {
                if (!_mOnCollisionExit2DMethodGot)
                {
                    _mOnCollisionExit2DMethod = instance.Type.GetMethod("OnCollisionExit2D", 1);
                    _mOnCollisionExit2DMethodGot = true;
                }
    
                if (_mOnCollisionExit2DMethod != null)
                {
                    appdomain.Invoke(_mOnCollisionExit2DMethod, instance, other);
                }
            }
    
            IMethod _mOnCollisionStay2DMethod;
            bool _mOnCollisionStay2DMethodGot;
    
            void OnCollisionStay2D(Collision2D other)
            {
                if (!_mOnCollisionStay2DMethodGot)
                {
                    _mOnCollisionStay2DMethod = instance.Type.GetMethod("OnCollisionStay2D", 1);
                    _mOnCollisionStay2DMethodGot = true;
                }
    
                if (_mOnCollisionStay2DMethod != null)
                {
                    appdomain.Invoke(_mOnCollisionStay2DMethod, instance, other);
                }
            }
    
            IMethod _mOnConnectedToServerMethod;
            bool _mOnConnectedToServerMethodGot;
    
            void OnConnectedToServer()
            {
                if (instance != null)
                {
                    if (!_mOnConnectedToServerMethodGot)
                    {
                        _mOnConnectedToServerMethod = instance.Type.GetMethod("OnConnectedToServer", 0);
                        _mOnConnectedToServerMethodGot = true;
                    }
    
                    if (_mOnConnectedToServerMethod != null)
                    {
                        appdomain.Invoke(_mOnConnectedToServerMethod, instance, ConstMgr.NullObjects);
                    }
                }
            }
    
            IMethod _mOnControllerColliderHitMethod;
            bool _mOnControllerColliderHitMethodGot;
    
            void OnControllerColliderHit(ControllerColliderHit hit)
            {
                if (instance != null)
                {
                    if (!_mOnControllerColliderHitMethodGot)
                    {
                        _mOnControllerColliderHitMethod = instance.Type.GetMethod("OnControllerColliderHit", 1);
                        _mOnControllerColliderHitMethodGot = true;
                    }
    
                    if (_mOnControllerColliderHitMethod != null)
                    {
                        appdomain.Invoke(_mOnControllerColliderHitMethod, instance, hit);
                    }
                }
            }
    
            IMethod _mOnDrawGizmosSelectedMethod;
            bool _mOnDrawGizmosSelectedMethodGot;
    
            void OnDrawGizmosSelected()
            {
                if (instance != null)
                {
                    if (!_mOnDrawGizmosSelectedMethodGot)
                    {
                        _mOnDrawGizmosSelectedMethod = instance.Type.GetMethod("OnDrawGizmosSelected", 0);
                        _mOnDrawGizmosSelectedMethodGot = true;
                    }
    
                    if (_mOnDrawGizmosSelectedMethod != null)
                    {
                        appdomain.Invoke(_mOnDrawGizmosSelectedMethod, instance, ConstMgr.NullObjects);
                    }
                }
            }
    
            IMethod _mOnGUIMethod;
            bool _mOnGUIMethodGot;
    
            void OnGUI()
            {
                if (instance != null)
                {
                    if (!_mOnGUIMethodGot)
                    {
                        _mOnGUIMethod = instance.Type.GetMethod("OnGUI", 0);
                        _mOnGUIMethodGot = true;
                    }
    
                    if (_mOnGUIMethod != null)
                    {
                        appdomain.Invoke(_mOnGUIMethod, instance, ConstMgr.NullObjects);
                    }
                }
            }
    
            IMethod _mOnJointBreak2DMethod;
            bool _mOnJointBreak2DMethodGot;
    
            void OnJointBreak2D(Joint2D brokenJoint)
            {
                if (instance != null)
                {
                    if (!_mOnJointBreak2DMethodGot)
                    {
                        _mOnJointBreak2DMethod = instance.Type.GetMethod("OnJointBreak2D", 1);
                        _mOnJointBreak2DMethodGot = true;
                    }
    
                    if (_mOnJointBreak2DMethod != null)
                    {
                        appdomain.Invoke(_mOnJointBreak2DMethod, instance, brokenJoint);
                    }
                }
            }
    
            IMethod _mOnParticleSystemStoppedMethod;
            bool _mOnParticleSystemStoppedMethodGot;
    
            void OnParticleSystemStopped()
            {
                if (instance != null)
                {
                    if (!_mOnParticleSystemStoppedMethodGot)
                    {
                        _mOnParticleSystemStoppedMethod = instance.Type.GetMethod("OnParticleSystemStopped", 0);
                        _mOnParticleSystemStoppedMethodGot = true;
                    }
    
                    if (_mOnParticleSystemStoppedMethod != null)
                    {
                        appdomain.Invoke(_mOnParticleSystemStoppedMethod, instance, ConstMgr.NullObjects);
                    }
                }
            }
    
            IMethod _mOnTransformChildrenChangedMethod;
            bool _mOnTransformChildrenChangedMethodGot;
    
            void OnTransformChildrenChanged()
            {
                if (instance != null)
                {
                    if (!_mOnTransformChildrenChangedMethodGot)
                    {
                        _mOnTransformChildrenChangedMethod = instance.Type.GetMethod("OnTransformChildrenChanged", 0);
                        _mOnTransformChildrenChangedMethodGot = true;
                    }
    
                    if (_mOnTransformChildrenChangedMethod != null)
                    {
                        appdomain.Invoke(_mOnTransformChildrenChangedMethod, instance, ConstMgr.NullObjects);
                    }
                }
            }
    
            IMethod _mOnTransformParentChangedMethod;
            bool _mOnTransformParentChangedMethodGot;
    
            void OnTransformParentChanged()
            {
                if (instance != null)
                {
                    if (!_mOnTransformParentChangedMethodGot)
                    {
                        _mOnTransformParentChangedMethod = instance.Type.GetMethod("OnTransformParentChanged", 0);
                        _mOnTransformParentChangedMethodGot = true;
                    }
    
                    if (_mOnTransformParentChangedMethod != null)
                    {
                        appdomain.Invoke(_mOnTransformParentChangedMethod, instance, ConstMgr.NullObjects);
                    }
                }
            }
    
            IMethod _mOnTriggerEnter2DMethod;
            bool _mOnTriggerEnter2DMethodGot;
    
            void OnTriggerEnter2D(Collider2D other)
            {
                if (instance != null)
                {
                    if (!_mOnTriggerEnter2DMethodGot)
                    {
                        _mOnTriggerEnter2DMethod = instance.Type.GetMethod("OnTriggerEnter2D", 1);
                        _mOnTriggerEnter2DMethodGot = true;
                    }
    
                    if (_mOnTriggerEnter2DMethod != null)
                    {
                        appdomain.Invoke(_mOnTriggerEnter2DMethod, instance, other);
                    }
                }
            }
    
            IMethod _mOnTriggerExit2DMethod;
            bool _mOnTriggerExit2DMethodGot;
    
            void OnTriggerExit2D(Collider2D other)
            {
                if (instance != null)
                {
                    if (!_mOnTriggerExit2DMethodGot)
                    {
                        _mOnTriggerExit2DMethod = instance.Type.GetMethod("OnTriggerExit2D", 1);
                        _mOnTriggerExit2DMethodGot = true;
                    }
    
                    if (_mOnTriggerExit2DMethod != null)
                    {
                        appdomain.Invoke(_mOnTriggerExit2DMethod, instance, other);
                    }
                }
            }
    
            IMethod _mOnTriggerStay2DMethod;
            bool _mOnTriggerStay2DMethodGot;
    
            void OnTriggerStay2D(Collider2D other)
            {
                if (instance != null)
                {
                    if (!_mOnTriggerStay2DMethodGot)
                    {
                        _mOnTriggerStay2DMethod = instance.Type.GetMethod("OnTriggerStay2D", 1);
                        _mOnTriggerStay2DMethodGot = true;
                    }
    
                    if (_mOnTriggerStay2DMethod != null)
                    {
                        appdomain.Invoke(_mOnTriggerStay2DMethod, instance, other);
                    }
                }
            }
    
            IMethod _mOnWillRenderObjectMethod;
            bool _mOnWillRenderObjectMethodGot;
    
            void OnWillRenderObject()
            {
                if (instance != null)
                {
                    if (!_mOnWillRenderObjectMethodGot)
                    {
                        _mOnWillRenderObjectMethod = instance.Type.GetMethod("OnWillRenderObject", 0);
                        _mOnWillRenderObjectMethodGot = true;
                    }
    
                    if (_mOnWillRenderObjectMethod != null)
                    {
                        appdomain.Invoke(_mOnWillRenderObjectMethod, instance, ConstMgr.NullObjects);
                    }
                }
            }
    
            IMethod _mOnBeforeTransformParentChangedMethod;
            bool _mOnBeforeTransformParentChangedMethodGot;
    
            void OnBeforeTransformParentChanged()
            {
                if (instance != null)
                {
                    if (!_mOnBeforeTransformParentChangedMethodGot)
                    {
                        _mOnBeforeTransformParentChangedMethod =
                            instance.Type.GetMethod("OnBeforeTransformParentChanged", 0);
                        _mOnBeforeTransformParentChangedMethodGot = true;
                    }
    
                    if (_mOnBeforeTransformParentChangedMethod != null)
                    {
                        appdomain.Invoke(_mOnBeforeTransformParentChangedMethod, instance, ConstMgr.NullObjects);
                    }
                }
            }
    
            IMethod _mOnDidApplyAnimationPropertiesMethod;
            bool _mOnDidApplyAnimationPropertiesMethodGot;
    
            void OnDidApplyAnimationProperties()
            {
                if (instance != null)
                {
                    if (!_mOnDidApplyAnimationPropertiesMethodGot)
                    {
                        _mOnDidApplyAnimationPropertiesMethod = instance.Type.GetMethod("OnDidApplyAnimationProperties", 0);
                        _mOnDidApplyAnimationPropertiesMethodGot = true;
                    }
    
                    if (_mOnDidApplyAnimationPropertiesMethod != null)
                    {
                        appdomain.Invoke(_mOnDidApplyAnimationPropertiesMethod, instance, ConstMgr.NullObjects);
                    }
                }
            }
    
            IMethod _mOnMouseUpAsButtonMethod;
            bool _mOnMouseUpAsButtonMethodGot;
    
            void OnMouseUpAsButton()
            {
                if (instance != null)
                {
                    if (!_mOnMouseUpAsButtonMethodGot)
                    {
                        _mOnMouseUpAsButtonMethod = instance.Type.GetMethod("OnMouseUpAsButton", 0);
                        _mOnMouseUpAsButtonMethodGot = true;
                    }
    
                    if (_mOnMouseUpAsButtonMethod != null)
                    {
                        appdomain.Invoke(_mOnMouseUpAsButtonMethod, instance, ConstMgr.NullObjects);
                    }
                }
            }
    
            IMethod _mOnParticleUpdateJobScheduledMethod;
            bool _mOnParticleUpdateJobScheduledMethodGot;
    
            void OnParticleUpdateJobScheduled()
            {
                if (instance != null)
                {
                    if (!_mOnParticleUpdateJobScheduledMethodGot)
                    {
                        _mOnParticleUpdateJobScheduledMethod = instance.Type.GetMethod("OnParticleUpdateJobScheduled", 0);
                        _mOnParticleUpdateJobScheduledMethodGot = true;
                    }
    
                    if (_mOnParticleUpdateJobScheduledMethod != null)
                    {
                        appdomain.Invoke(_mOnParticleUpdateJobScheduledMethod, instance, ConstMgr.NullObjects);
                    }
                }
            }
    
            IMethod _mOnRectTransformDimensionsChangeMethod;
            bool _mOnRectTransformDimensionsChangeMethodGot;
    
            void OnRectTransformDimensionsChange()
            {
                if (instance != null)
                {
                    if (!_mOnRectTransformDimensionsChangeMethodGot)
                    {
                        _mOnRectTransformDimensionsChangeMethod =
                            instance.Type.GetMethod("OnRectTransformDimensionsChange", 0);
                        _mOnRectTransformDimensionsChangeMethodGot = true;
                    }
    
                    if (_mOnRectTransformDimensionsChangeMethod != null)
                    {
                        appdomain.Invoke(_mOnRectTransformDimensionsChangeMethod, instance, ConstMgr.NullObjects);
                    }
                }
            }
            #endregion
        }
    }
}
