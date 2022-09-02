#if UNITY_2017_1_OR_NEWER
using UnityEngine;
#else
using System;
#endif

namespace Nino.Shared.Util
{
    public static class Logger
    {
        public static void D(object msg)
        {
#if UNITY_2017_1_OR_NEWER
            Debug.Log(msg);
#else
            Console.WriteLine(msg);
#endif
        }

        public static void W(object msg)
        {
#if UNITY_2017_1_OR_NEWER
            Debug.LogWarning(msg);
#else
            Console.WriteLine(msg);
#endif
        }

        public static void E(object msg)
        {
#if UNITY_2017_1_OR_NEWER
            Debug.LogError(msg);
#else
            Console.WriteLine(msg);
#endif
        }

        public static void LA(object msg)
        {
#if UNITY_2017_1_OR_NEWER
            Debug.LogAssertion(msg);
#else
            Console.WriteLine(msg);
#endif
        }

        public static void Assert(bool cond)
        {
#if UNITY_2017_1_OR_NEWER
            Debug.Assert(cond);
#else
            Console.WriteLine(cond);
#endif
        }

        public static void D(string tag, object msg)
        {
            D($"[{tag}] {msg}");
        }

        public static void W(string tag, object msg)
        {
            W($"[{tag}] {msg}");
        }

        public static void E(string tag, object msg)
        {
            E($"[{tag}] {msg}");
        }

        public static void LA(string tag, object msg)
        {
            LA($"[{tag}] {msg}");
        }
    }
}