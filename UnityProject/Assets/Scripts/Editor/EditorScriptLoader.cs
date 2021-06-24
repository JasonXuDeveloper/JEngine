using System.IO;
using JEngine.Core;
using UnityEngine;
using VEngine.Editor;

public static class EditorScriptLoader
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Init()
    {
        if (Settings.GetDefaultSettings().scriptPlayMode == ScriptPlayMode.Simulation)
        {
            InitJEngine.CustomLoader = LoadByEditor;
        }
        else
        {
            InitJEngine.CustomLoader = null;
        }
    }

    private static byte[] LoadByEditor(InitJEngine engine)
    {
        byte[] buffer = null;
        if (File.Exists(InitJEngine.DLLPath)) //直接读DLL
        {
            buffer = DLLMgr.FileToByte(InitJEngine.DLLPath);

            //模拟加密
            buffer = CryptoHelper.AesEncrypt(buffer, engine.key);
        }
        else
        {
            Log.PrintError("DLL文件不存在");
        }

        //查看是否有PDB文件
        if (File.Exists(InitJEngine.PdbPath) && engine.usePdb &&
            (File.GetLastWriteTime(InitJEngine.DLLPath) - File.GetLastWriteTime(InitJEngine.PdbPath)).Seconds < 30)
        {
            engine.pdb = new MemoryStream(DLLMgr.FileToByte(InitJEngine.PdbPath));
        }
        return buffer;
    }
}