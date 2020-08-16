using System;
using System.Collections.Generic;
using UnityEngine;

namespace HotFix_Project
{
    class SomeMonoBehaviour : MonoBehaviour
    {
        float time;
        void Awake()
        {
            Debug.Log("!! SomeMonoBehaviour.Awake");
        }

        void Start()
        {
            Debug.Log("!! SomeMonoBehaviour.Start");
        }

        void Update()
        {
            if(Time.time - time > 1)
            {
                Debug.Log("!! SomeMonoBehaviour.Update, t=" + Time.time);
                time = Time.time;
            }
        }

        public void Test()
        {
            Debug.Log("SomeMonoBehaviour");
        }
    }

    class SomeMonoBehaviour2 : MonoBehaviour
    {
        public GameObject TargetGO;
        public Texture2D Texture;
        public void Test2()
        {
            Debug.Log("!!! SomeMonoBehaviour2.Test2");
        }
    }

    public class TestMonoBehaviour
    {
        public static void RunTest(GameObject go)
        {
            go.AddComponent<SomeMonoBehaviour>();
        }

        public static void RunTest2(GameObject go)
        {
            go.AddComponent<SomeMonoBehaviour2>();
            var mb = go.GetComponent<SomeMonoBehaviour2>();
            Debug.Log("!!!TestMonoBehaviour.RunTest2 mb= " + mb);
            mb.Test2();
        }
    }
}
