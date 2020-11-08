using System;
using System.Collections.Generic;
using System.Reflection;

namespace ILRuntime.Runtime.Generated
{
    class CLRBindings
    {


        /// <summary>
        /// Initialize the CLR binding, please invoke this AFTER CLR Redirection registration
        /// </summary>
        public static void Initialize(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            UnityEngine_Object_Binding.Register(app);
            UnityEngine_Component_Binding.Register(app);
            UnityEngine_GameObject_Binding.Register(app);
            System_Collections_Generic_List_1_GameObject_Binding.Register(app);
            System_Runtime_CompilerServices_AsyncVoidMethodBuilder_Binding.Register(app);
            System_String_Binding.Register(app);
            System_Action_Binding.Register(app);
            libx_Assets_Binding.Register(app);
            libx_AssetRequest_Binding.Register(app);
            System_Threading_Tasks_Task_Binding.Register(app);
            System_Runtime_CompilerServices_TaskAwaiter_Binding.Register(app);
            System_Collections_Generic_List_1_Action_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_Int32_Single_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_Int32_Func_1_Boolean_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_Int32_Action_Binding.Register(app);
            System_Runtime_CompilerServices_AsyncTaskMethodBuilder_1_CoroutineAdapter_Binding_Adaptor_Binding.Register(app);
            System_Threading_CancellationTokenSource_Binding.Register(app);
            UnityEngine_Application_Binding.Register(app);
            JEngine_Core_Log_Binding.Register(app);
            System_GC_Binding.Register(app);
            System_Func_1_Boolean_Binding.Register(app);
            System_Threading_Tasks_Task_1_CoroutineAdapter_Binding_Adaptor_Binding.Register(app);
            System_Runtime_CompilerServices_TaskAwaiter_1_CoroutineAdapter_Binding_Adaptor_Binding.Register(app);
            JEngine_Core_Loom_Binding.Register(app);
            System_TimeoutException_Binding.Register(app);
            Init_Binding.Register(app);
            System_Exception_Binding.Register(app);
            System_Object_Binding.Register(app);
            JEngine_Core_CryptoHelper_Binding.Register(app);
            UnityEngine_PlayerPrefs_Binding.Register(app);
            System_Convert_Binding.Register(app);
            System_Int32_Binding.Register(app);
            System_Int16_Binding.Register(app);
            System_Int64_Binding.Register(app);
            System_Decimal_Binding.Register(app);
            System_Double_Binding.Register(app);
            System_Single_Binding.Register(app);
            System_Boolean_Binding.Register(app);
            System_Type_Binding.Register(app);
            System_Collections_Generic_List_1_Type_Binding.Register(app);
            ProtoBuf_PType_Binding.Register(app);
            System_IO_MemoryStream_Binding.Register(app);
            ProtoBuf_Serializer_Binding.Register(app);
            LitJson_JsonMapper_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_String_ILTypeInstance_Binding.Register(app);
            JEngine_Core__ClassBind_Binding.Register(app);
            System_Reflection_MemberInfo_Binding.Register(app);
            JEngine_Core_ClassBind_Binding.Register(app);
            System_Linq_Enumerable_Binding.Register(app);
            System_Collections_Generic_List_1_MonoBehaviourAdapter_Binding_Adaptor_Binding.Register(app);
            System_Diagnostics_Stopwatch_Binding.Register(app);
            System_Guid_Binding.Register(app);
            UnityEngine_MonoBehaviour_Binding.Register(app);
            System_Collections_Generic_KeyValuePair_2_String_ILTypeInstance_Binding.Register(app);
            System_NotSupportedException_Binding.Register(app);
            MonoBehaviourAdapter_Binding_Adaptor_Binding.Register(app);
            System_Reflection_FieldInfo_Binding.Register(app);
            JEngine_Core_GameStats_Binding.Register(app);
            System_Action_1_ILTypeInstance_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_Type_UIBehaviour_Binding.Register(app);
            libx_Reference_Binding.Register(app);
            System_Text_StringBuilder_Binding.Register(app);
            UnityEngine_UI_Text_Binding.Register(app);
            UnityEngine_Random_Binding.Register(app);
            UnityEngine_UI_InputField_Binding.Register(app);
            UnityEngine_Events_UnityEvent_1_String_Binding.Register(app);
            UnityEngine_UI_Toggle_Binding.Register(app);
            UnityEngine_Events_UnityEvent_1_Boolean_Binding.Register(app);
            UnityEngine_TextAsset_Binding.Register(app);
            UnityEngine_UI_Button_Binding.Register(app);
            UnityEngine_Events_UnityEvent_Binding.Register(app);
            JEngine_AntiCheat_JFloat_Binding.Register(app);
            JEngine_AntiCheat_JLong_Binding.Register(app);

            ILRuntime.CLR.TypeSystem.CLRType __clrType = null;
        }

        /// <summary>
        /// Release the CLR binding, please invoke this BEFORE ILRuntime Appdomain destroy
        /// </summary>
        public static void Shutdown(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
        }
    }
}
