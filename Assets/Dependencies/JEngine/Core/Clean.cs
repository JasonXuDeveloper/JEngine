using System.Diagnostics;
using UnityEditor;
using UnityEngine;
using System.IO;
using JEngine.Core;
#if UNITY_EDITOR
[InitializeOnLoad]
class Clean
{
    static Clean ()
    {
        isDone = true;
        EditorApplication.update += Update;
    }

    private static bool isDone;
    
    static void Update ()
    {
        if (!isDone)
        {
            return;
        }
        DirectoryInfo di = new DirectoryInfo("Assets/HotUpdateResources/Dll");
        var files = di.GetFiles();
        if (files.Length > 1)
        {
            isDone = false;
            int counts = 0;
            var watch = new Stopwatch();
            watch.Start();
            foreach (var file in files)
            {
                var name = file.Name;
                if (!file.Name.Contains("HotUpdateScripts"))
                {
                    File.Delete(file.FullName);
                    counts++;
                }
            }
            watch.Stop();
            if (counts > 0)
            {
                Log.Print("Cleaned: "+ counts+" files in: " + watch.ElapsedMilliseconds + " ms.");
            }
            isDone = true;
        }
    }
}
#endif