using JEngine.Core;
using UnityEngine;

public class ExampleAPI :MonoBehaviour
{ 
    public virtual void ExampleMethod()
    {
        Log.Print("ExampleAPI::ExampleMethod");
    }
}