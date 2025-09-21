using System;
using System.Collections;
using System.Reflection;
using System.Runtime.Serialization;
using ILRuntime.CLR.Method;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.Runtime.Intepreter;
using AppDomain = ILRuntime.Runtime.Enviorment.AppDomain;

public class ExceptionAdapter : CrossBindingAdaptor
{
    public override Type BaseCLRType
    {
        get
        {
            return typeof(Exception); //这是你想继承的那个类
        }
    }

    public override Type AdaptorType
    {
        get
        {
            return typeof(Adaptor); //这是实际的适配器类
        }
    }

    public override object CreateCLRInstance(AppDomain appdomain, ILTypeInstance instance)
    {
        return new Adaptor(appdomain, instance); //创建一个新的实例
    }

//实际的适配器类需要继承你想继承的那个类，并且实现CrossBindingAdaptorType接口
    public class Adaptor : Exception, CrossBindingAdaptorType
    {
        ILTypeInstance instance;

        AppDomain appdomain;

        //缓存这个数组来避免调用时的GC Alloc
        object[] param1 = new object[1];

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
        }

        bool m_bget_MessageGot;
        IMethod m_get_Message;

        public String get_Message()
        {
            if (!m_bget_MessageGot)
            {
                m_get_Message = instance.Type.GetMethod("get_Message", 0);
                m_bget_MessageGot = true;
            }

            if (m_get_Message != null)
            {
                return (String) appdomain.Invoke(m_get_Message, instance, null);
            }

            return null;
        }

        bool m_bget_DataGot;
        IMethod m_get_Data;

        public IDictionary get_Data()
        {
            if (!m_bget_DataGot)
            {
                m_get_Data = instance.Type.GetMethod("get_Data", 0);
                m_bget_DataGot = true;
            }

            if (m_get_Data != null)
            {
                return (IDictionary) appdomain.Invoke(m_get_Data, instance, null);
            }

            return null;
        }

        bool m_bGetBaseExceptionGot;
        IMethod m_GetBaseException;

        public override Exception GetBaseException()
        {
            if (!m_bGetBaseExceptionGot)
            {
                m_GetBaseException = instance.Type.GetMethod("GetBaseException", 0);
                m_bGetBaseExceptionGot = true;
            }

            if (m_GetBaseException != null)
            {
                return (Exception) appdomain.Invoke(m_GetBaseException, instance, null);
            }

            return null;
        }

        bool m_bget_InnerExceptionGot;
        IMethod m_get_InnerException;

        public Exception get_InnerException()
        {
            if (!m_bget_InnerExceptionGot)
            {
                m_get_InnerException = instance.Type.GetMethod("get_InnerException", 0);
                m_bget_InnerExceptionGot = true;
            }

            if (m_get_InnerException != null)
            {
                return (Exception) appdomain.Invoke(m_get_InnerException, instance, null);
            }

            return null;
        }

        bool m_bget_TargetSiteGot;
        IMethod m_get_TargetSite;

        public MethodBase get_TargetSite()
        {
            if (!m_bget_TargetSiteGot)
            {
                m_get_TargetSite = instance.Type.GetMethod("get_TargetSite", 0);
                m_bget_TargetSiteGot = true;
            }

            if (m_get_TargetSite != null)
            {
                return (MethodBase) appdomain.Invoke(m_get_TargetSite, instance, null);
            }

            return null;
        }

        bool m_bget_StackTraceGot;
        IMethod m_get_StackTrace;

        public String get_StackTrace()
        {
            if (!m_bget_StackTraceGot)
            {
                m_get_StackTrace = instance.Type.GetMethod("get_StackTrace", 0);
                m_bget_StackTraceGot = true;
            }

            if (m_get_StackTrace != null)
            {
                return (String) appdomain.Invoke(m_get_StackTrace, instance, null);
            }

            return null;
        }

        bool m_bget_HelpLinkGot;
        IMethod m_get_HelpLink;

        public String get_HelpLink()
        {
            if (!m_bget_HelpLinkGot)
            {
                m_get_HelpLink = instance.Type.GetMethod("get_HelpLink", 0);
                m_bget_HelpLinkGot = true;
            }

            if (m_get_HelpLink != null)
            {
                return (String) appdomain.Invoke(m_get_HelpLink, instance, null);
            }

            return null;
        }

        bool m_bset_HelpLinkGot;
        IMethod m_set_HelpLink;

        public void set_HelpLink(String arg0)
        {
            if (!m_bset_HelpLinkGot)
            {
                m_set_HelpLink = instance.Type.GetMethod("set_HelpLink", 1);
                m_bset_HelpLinkGot = true;
            }

            if (m_set_HelpLink != null)
            {
                appdomain.Invoke(m_set_HelpLink, instance, arg0);
            }
        }

        bool m_bget_SourceGot;
        IMethod m_get_Source;

        public String get_Source()
        {
            if (!m_bget_SourceGot)
            {
                m_get_Source = instance.Type.GetMethod("get_Source", 0);
                m_bget_SourceGot = true;
            }

            if (m_get_Source != null)
            {
                return (String) appdomain.Invoke(m_get_Source, instance, null);
            }

            return null;
        }

        bool m_bset_SourceGot;
        IMethod m_set_Source;

        public void set_Source(String arg0)
        {
            if (!m_bset_SourceGot)
            {
                m_set_Source = instance.Type.GetMethod("set_Source", 1);
                m_bset_SourceGot = true;
            }

            if (m_set_Source != null)
            {
                appdomain.Invoke(m_set_Source, instance, arg0);
            }
        }

        bool m_bToStringGot;
        IMethod m_ToString;

        public override String ToString()
        {
            if (!m_bToStringGot)
            {
                m_ToString = instance.Type.GetMethod("ToString", 0);
                m_bToStringGot = true;
            }

            if (m_ToString != null)
            {
                return (String) appdomain.Invoke(m_ToString, instance, null);
            }

            return null;
        }

        bool m_bGetObjectDataGot;
        IMethod m_GetObjectData;

        public override void GetObjectData(SerializationInfo arg0,
            StreamingContext arg1)
        {
            if (!m_bGetObjectDataGot)
            {
                m_GetObjectData = instance.Type.GetMethod("GetObjectData", 2);
                m_bGetObjectDataGot = true;
            }

            if (m_GetObjectData != null)
            {
                appdomain.Invoke(m_GetObjectData, instance, arg0, arg1);
            }
        }

        bool m_bGetTypeGot;
        IMethod m_GetType;

        public new Type GetType()
        {
            if (!m_bGetTypeGot)
            {
                m_GetType = instance.Type.GetMethod("GetType", 0);
                m_bGetTypeGot = true;
            }

            if (m_GetType != null)
            {
                return (Type) appdomain.Invoke(m_GetType, instance, null);
            }

            return null;
        }
    }
}