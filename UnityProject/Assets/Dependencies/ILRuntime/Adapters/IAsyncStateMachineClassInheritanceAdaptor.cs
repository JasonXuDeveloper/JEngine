using System;
using System.Runtime.CompilerServices;
using ILRuntime.CLR.Method;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.Runtime.Intepreter;
using AppDomain = ILRuntime.Runtime.Enviorment.AppDomain;

/// <summary>
/// 用于async await适配
/// </summary>
public class IAsyncStateMachineClassInheritanceAdaptor : CrossBindingAdaptor
{
	public override Type BaseCLRType
	{
		get
		{
			return typeof(IAsyncStateMachine);
		}
	}

	public override Type AdaptorType
	{
		get
		{
			return typeof(IAsyncStateMachineAdaptor);
		}
	}

	public override object CreateCLRInstance(AppDomain appdomain, ILTypeInstance instance)
	{
		return new IAsyncStateMachineAdaptor(appdomain, instance);
	}

	public class IAsyncStateMachineAdaptor : IAsyncStateMachine, CrossBindingAdaptorType
	{
		private ILTypeInstance instance;
		private AppDomain appDomain;

		private IMethod mMoveNext;
		private IMethod mSetStateMachine;
		private readonly object[] param1 = new object[1];

		public IAsyncStateMachineAdaptor()
		{
		}

		public IAsyncStateMachineAdaptor(AppDomain appDomain, ILTypeInstance instance)
		{
			this.appDomain = appDomain;
			this.instance = instance;
		}

		public ILTypeInstance ILInstance
		{
			get
			{
				return instance;
			}
		}

		public void MoveNext()
		{
			if (mMoveNext == null)
			{
				mMoveNext = instance.Type.GetMethod("MoveNext", 0);
			}
			appDomain.Invoke(mMoveNext, instance, null);
		}

		public void SetStateMachine(IAsyncStateMachine stateMachine)
		{
			if (mSetStateMachine == null)
			{
				mSetStateMachine = instance.Type.GetMethod("SetStateMachine");
			}
			appDomain.Invoke(mSetStateMachine, instance, stateMachine);
		}

		public override string ToString()
		{
			IMethod m = appDomain.ObjectType.GetMethod("ToString", 0);
			m = instance.Type.GetVirtualMethod(m);
			if (m == null || m is ILMethod)
			{
				return instance.ToString();
			}

			return instance.Type.FullName;
		}
	}
}