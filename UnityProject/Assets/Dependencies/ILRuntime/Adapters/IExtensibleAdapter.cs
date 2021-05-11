using System;
using ILRuntime.CLR.Method;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.Runtime.Intepreter;
using AppDomain = ILRuntime.Runtime.Enviorment.AppDomain;

namespace ProtoBuf
{
    public sealed class IExtensibleAdapter : CrossBindingAdaptor
    {
        public override Type BaseCLRType
        {
            get
            {
                return typeof(IExtensible);
            }
        }

        public override Type AdaptorType
        {
            get
            {
                return typeof(Adaptor);
            }
        }

        public override object CreateCLRInstance(AppDomain appdomain, ILTypeInstance instance)
        {
            return new Adaptor(appdomain, instance);
        }

        internal class Adaptor : IExtensible, CrossBindingAdaptorType
        {
            ILTypeInstance instance;
            AppDomain appdomain;

            public Adaptor()
            {

            }

            public Adaptor(AppDomain appdomain, ILTypeInstance instance)
            {
                this.appdomain = appdomain;
                this.instance = instance;
                Init();
            }

            public ILTypeInstance ILInstance { get { return instance; } }

            public IExtension GetExtensionObject(bool createIfMissing)
            {
                return appdomain.Invoke(mMethoGetExObject, instance, createIfMissing) as IExtension;
            }

            IMethod mMethoGetExObject;

            void Init()
            {
                mMethoGetExObject = instance.Type.GetMethod("GetExtensionObject",0);
            }
        }
    }
}