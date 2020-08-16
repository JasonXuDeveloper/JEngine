using System;
using System.Collections.Generic;

namespace HotFix_Project
{
    public class TestDelegate
    {
        static TestDelegateMethod delegateMethod;
        static TestDelegateFunction delegateFunc;
        static Action<string> delegateAction;

        public static void Initialize()
        {
            delegateMethod = Method;
            delegateFunc = Function;
            delegateAction = Action;
        }

        public static void RunTest()
        {
            delegateMethod(123);
            var res = delegateFunc(456);
            UnityEngine.Debug.Log("!! TestDelegate.RunTest res = " + res);
            delegateAction("rrr");
        }

        public static void Initialize2()
        {
            DelegateDemo.TestMethodDelegate = Method;
            DelegateDemo.TestFunctionDelegate = Function;
            DelegateDemo.TestActionDelegate = Action;
        }

        public static void RunTest2()
        {
            DelegateDemo.TestMethodDelegate(123);
            var res = DelegateDemo.TestFunctionDelegate(456);
            UnityEngine.Debug.Log("!! TestDelegate.RunTest2 res = " + res);
            DelegateDemo.TestActionDelegate("rrr");
        }

        static void Method(int a)
        {
            UnityEngine.Debug.Log("!! TestDelegate.Method, a = " + a);
        }

        static string Function(int a)
        {
            return a.ToString();
        }

        static void Action(string a)
        {
            UnityEngine.Debug.Log("!! TestDelegate.Action, a = " + a);
        }
    }
}
