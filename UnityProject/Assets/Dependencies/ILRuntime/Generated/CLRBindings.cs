using System;
using System.Collections.Generic;
using System.Reflection;

namespace ILRuntime.Runtime.Generated
{
    class CLRBindings
    {

//will auto register in unity
#if UNITY_5_3_OR_NEWER
        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.BeforeSceneLoad)]
#endif
        static private void RegisterBindingAction()
        {
            ILRuntime.Runtime.CLRBinding.CLRBindingUtils.RegisterBindingAction(Initialize);
        }


        /// <summary>
        /// Initialize the CLR binding, please invoke this AFTER CLR Redirection registration
        /// </summary>
        public static void Initialize(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            System_String_Binding.Register(app);
            JEngine_Core_Log_Binding.Register(app);
            System_Type_Binding.Register(app);
            JEngine_Core_BindableProperty_1_Int64_Binding.Register(app);
            UnityEngine_Object_Binding.Register(app);
            System_Action_3_ILTypeInstance_Object_Object_Binding.Register(app);
            System_Action_1_ILTypeInstance_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_Type_UIBehaviour_Binding.Register(app);
            System_Action_2_ILTypeInstance_Int64_Binding.Register(app);
            UnityEngine_ColorUtility_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_Transform_Text_Binding.Register(app);
            UnityEngine_Component_Binding.Register(app);
            UnityEngine_GameObject_Binding.Register(app);
            UnityEngine_Transform_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_Transform_Button_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_Transform_Outline_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_Transform_Shadow_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_Transform_Image_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_Transform_RectTransform_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_Transform_RawImage_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_Transform_Slider_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_Transform_Toggle_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_Transform_Scrollbar_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_Transform_Dropdown_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_Transform_InputField_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_Transform_Canvas_Binding.Register(app);
            System_Collections_Generic_List_1_ILTypeInstance_Binding.Register(app);
            System_Object_Binding.Register(app);
            GenericExample_1_ProjectAdapter_GenericExampleAdapter2_Binding_Adapter_Binding.Register(app);
            GenericExample_1_ILTypeInstance_Binding.Register(app);
            ExampleAPI_Binding.Register(app);
            UnityEngine_MonoBehaviour_Binding.Register(app);
            System_Runtime_CompilerServices_AsyncVoidMethodBuilder_Binding.Register(app);
            System_Threading_Tasks_Task_Binding.Register(app);
            System_Runtime_CompilerServices_TaskAwaiter_Binding.Register(app);
            UnityEngine_Quaternion_Binding.Register(app);
            System_Linq_Enumerable_Binding.Register(app);
            UnityEngine_UI_InputField_Binding.Register(app);
            UnityEngine_Events_UnityEvent_1_String_Binding.Register(app);
            UnityEngine_UI_Toggle_Binding.Register(app);
            UnityEngine_Events_UnityEvent_1_Boolean_Binding.Register(app);
            UnityEngine_UI_Text_Binding.Register(app);
            System_Int32_Binding.Register(app);
            UnityEngine_UI_Button_Binding.Register(app);
            UnityEngine_Events_UnityEvent_Binding.Register(app);
            JSONTest_Binding.Register(app);
            LitJson_JsonMapper_Binding.Register(app);
            UnityEngine_PlayerPrefs_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_String_String_Binding.Register(app);
            JEngine_AntiCheat_JFloat_Binding.Register(app);
            JEngine_AntiCheat_JLong_Binding.Register(app);
            WebSocketSharp_MessageEventArgs_Binding.Register(app);
            JEngine_Net_SocketIOEvent_Binding.Register(app);
            System_Threading_Thread_Binding.Register(app);
            JSONObject_Binding.Register(app);
            System_Threading_Tasks_Task_1_Boolean_Binding.Register(app);
            System_Runtime_CompilerServices_TaskAwaiter_1_Boolean_Binding.Register(app);
            UnityEngine_Random_Binding.Register(app);
            GTest_1_ILTypeInstance_Binding.Register(app);
            JEngine_AntiCheat_JInt_Binding.Register(app);
            UnityEngine_Debug_Binding.Register(app);
            JEngine_Core_ClassData_Binding.Register(app);
            JEngine_Core_ClassBind_Binding.Register(app);
            UnityEngine_TextAsset_Binding.Register(app);
            System_Boolean_Binding.Register(app);
            JEngine_Net_SocketIOComponent_Binding.Register(app);
            JEngine_Net_JSocketConfig_Binding.Register(app);
            WebSocketSharp_WebSocket_Binding.Register(app);
            System_Runtime_CompilerServices_AsyncTaskMethodBuilder_1_Boolean_Binding.Register(app);
            System_Action_1_SocketIOEvent_Binding.Register(app);
            System_Action_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_Object_List_1_ValueTuple_2_String_ILTypeInstance_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_Object_List_1_ValueTuple_2_String_ILTypeInstance_Binding_Enumerator_Binding.Register(app);
            System_Collections_Generic_KeyValuePair_2_Object_List_1_ValueTuple_2_String_ILTypeInstance_Binding.Register(app);
            System_Collections_Generic_List_1_ValueTuple_2_String_ILTypeInstance_Binding.Register(app);
            System_Collections_Generic_List_1_Object_Binding.Register(app);
            System_ValueTuple_2_String_ILTypeInstance_Binding.Register(app);
            JEngine_Core_Loom_Binding.Register(app);
            System_IDisposable_Binding.Register(app);
            System_Reflection_MemberInfo_Binding.Register(app);
            System_Reflection_MethodBase_Binding.Register(app);
            System_Threading_Tasks_Task_1_CoroutineAdapter_Binding_Adaptor_Binding.Register(app);
            System_Runtime_CompilerServices_TaskAwaiter_1_CoroutineAdapter_Binding_Adaptor_Binding.Register(app);
            System_Exception_Binding.Register(app);
            System_Collections_IDictionary_Binding.Register(app);
            System_Reflection_ParameterInfo_Binding.Register(app);
            System_Char_Binding.Register(app);
            System_Collections_Generic_List_1_String_Binding.Register(app);
            System_Text_StringBuilder_Binding.Register(app);
            JEngine_Core_CryptoHelper_Binding.Register(app);
            System_Convert_Binding.Register(app);
            System_Int16_Binding.Register(app);
            System_Int64_Binding.Register(app);
            System_Decimal_Binding.Register(app);
            System_Double_Binding.Register(app);
            System_Single_Binding.Register(app);
            InitJEngine_Binding.Register(app);
            System_Collections_Generic_List_1_GameObject_Binding.Register(app);
            UnityEngine_Application_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_String_ILTypeInstance_Binding.Register(app);
            System_Activator_Binding.Register(app);
            JEngine_Core_Tools_Binding.Register(app);
            System_Threading_CancellationTokenSource_Binding.Register(app);
            System_Guid_Binding.Register(app);
            System_Collections_Generic_KeyValuePair_2_String_ILTypeInstance_Binding.Register(app);
            System_NotSupportedException_Binding.Register(app);
            System_Diagnostics_Stopwatch_Binding.Register(app);
            JEngine_Core_GameStats_Binding.Register(app);
            JEngine_Core_AssetMgr_Binding.Register(app);
            System_Collections_Generic_List_1_Action_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_Int32_Single_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_Int32_Int32_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_Int32_Func_1_Boolean_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_Int32_Action_Binding.Register(app);
            System_Runtime_CompilerServices_AsyncTaskMethodBuilder_1_CoroutineAdapter_Binding_Adaptor_Binding.Register(app);
            System_Collections_Generic_List_1_CoroutineAdapter_Binding_Adaptor_Binding.Register(app);
            System_GC_Binding.Register(app);
            System_Func_1_Boolean_Binding.Register(app);
            System_TimeoutException_Binding.Register(app);
            System_IO_MemoryStream_Binding.Register(app);
            ProtoBuf_Serializer_Binding.Register(app);
            System_Runtime_CompilerServices_AsyncTaskMethodBuilder_Binding.Register(app);
            System_Threading_Tasks_Task_1_Object_Binding.Register(app);
            System_Runtime_CompilerServices_TaskAwaiter_1_Object_Binding.Register(app);
            System_Action_2_Boolean_CoroutineAdapter_Binding_Adaptor_Binding.Register(app);
            System_ArgumentException_Binding.Register(app);
        }

        /// <summary>
        /// Release the CLR binding, please invoke this BEFORE ILRuntime Appdomain destroy
        /// </summary>
        public static void Shutdown(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
        }
    }
}
