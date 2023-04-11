using System.Collections;
using System.Collections.Generic;
using JEngine.Core;
using UnityEngine;

public class GenericExample<T> : MonoBehaviour
{
    public void LogTest()
    {
        Log.Print($"GenericExample::LogTest, and the type of T is {typeof(T).FullName}");
    }
}
