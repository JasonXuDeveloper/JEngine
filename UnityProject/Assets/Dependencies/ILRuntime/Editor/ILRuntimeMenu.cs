#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System;
using System.Text;
using System.Collections.Generic;
[System.Reflection.Obfuscation(Exclude = true)]
public class ILRuntimeMenu
{

    [MenuItem("JEngine/ILRuntime/Open Chinese Document")]
    static void OpenDocumentation()
    {
        Application.OpenURL("https://ourpalm.github.io/ILRuntime/");
    }

    [MenuItem("JEngine/ILRuntime/Open on Github")]
    static void OpenGithub()
    {
        Application.OpenURL("https://github.com/Ourpalm/ILRuntime");
    }
}
#endif
