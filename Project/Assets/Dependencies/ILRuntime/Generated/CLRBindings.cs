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
            System_Type_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_Type_UIBehaviour_Binding.Register(app);
            UnityEngine_Component_Binding.Register(app);
            UnityEngine_Object_Binding.Register(app);
            UnityEngine_GameObject_Binding.Register(app);
            System_Action_1_MonoBehaviourAdapter_Binding_Adaptor_Binding.Register(app);
            System_Runtime_CompilerServices_AsyncVoidMethodBuilder_Binding.Register(app);
            System_Threading_Tasks_Task_Binding.Register(app);
            System_Runtime_CompilerServices_TaskAwaiter_Binding.Register(app);
            UnityEngine_Application_Binding.Register(app);
            System_Collections_Generic_List_1_GameObject_Binding.Register(app);
            System_Collections_Generic_List_1_Action_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_Int32_Single_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_Int32_Func_1_Boolean_Binding.Register(app);
            System_Collections_Generic_Dictionary_2_Int32_Action_Binding.Register(app);
            System_Runtime_CompilerServices_AsyncTaskMethodBuilder_1_ILTypeInstance_Binding.Register(app);
            JEngine_Core_Log_Binding.Register(app);
            System_Action_Binding.Register(app);
            System_Func_1_Boolean_Binding.Register(app);
            System_Threading_Tasks_Task_1_ILTypeInstance_Binding.Register(app);
            System_Runtime_CompilerServices_TaskAwaiter_1_ILTypeInstance_Binding.Register(app);
            System_Runtime_CompilerServices_AsyncTaskMethodBuilder_Binding.Register(app);
            System_Collections_Generic_List_1_Action_Binding_Enumerator_Binding.Register(app);
            System_TimeoutException_Binding.Register(app);
            System_IDisposable_Binding.Register(app);
            UnityEngine_Time_Binding.Register(app);
            System_String_Binding.Register(app);
            System_Int32_Binding.Register(app);
            UnityEngine_UI_Text_Binding.Register(app);
            UnityEngine_Transform_Binding.Register(app);
            UnityEngine_Resources_Binding.Register(app);
            UnityEngine_Color_Binding.Register(app);
            UnityEngine_UI_Graphic_Binding.Register(app);
            UnityEngine_Vector2_Binding.Register(app);
            UnityEngine_RectTransform_Binding.Register(app);
            UnityEngine_UI_Button_Binding.Register(app);
            UnityEngine_Events_UnityEvent_Binding.Register(app);
            UnityEngine_Screen_Binding.Register(app);
            System_Object_Binding.Register(app);

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
