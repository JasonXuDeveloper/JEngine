#if UNITY_EDITOR
using System.Reflection;
using UnityEditor;
using UnityEngine;

[Obfuscation(Exclude = true)]
public class ILRuntimeMenu
{

    [MenuItem("JEngine/ILRuntime/Open Chinese Document",priority = 1003)]
    static void OpenDocumentation()
    {
        Application.OpenURL("https://ourpalm.github.io/ILRuntime/");
    }

    [MenuItem("JEngine/ILRuntime/Open on Github",priority = 1004)]
    static void OpenGithub()
    {
        Application.OpenURL("https://github.com/Ourpalm/ILRuntime");
    }
}
#endif
