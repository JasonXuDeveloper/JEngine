#if UNITY_EDITOR
using System.IO;
using System.Reflection;
using ILRuntime.Runtime.CLRBinding;
using ILRuntime.Runtime.Enviorment;
using JEngine.Helper;
using UnityEditor;

[Obfuscation(Exclude = true)]
public class ILRuntimeCLRBinding
{
    [MenuItem("JEngine/ILRuntime/Generate/CLR Binding #B",priority = 1002)]
    static void GenerateCLRBindingByAnalysis()
    {
        //用新的分析热更dll调用引用来生成绑定代码
        AppDomain domain = new AppDomain();
        using (FileStream fs = new FileStream("Assets/HotUpdateResources/Dll/Hidden~/HotUpdateScripts.dll", FileMode.Open, FileAccess.Read))
        {
            domain.LoadAssembly(fs);

            //Crossbind Adapter is needed to generate the correct binding code
            InitILRuntime(domain);
            BindingCodeGenerator.GenerateBindingCode(domain, "Assets/Dependencies/ILRuntime/Generated");
        }

        AssetDatabase.Refresh();
    }

    static void InitILRuntime(AppDomain domain)
    {
        //这里需要注册所有热更DLL中用到的跨域继承Adapter，否则无法正确抓取引用
        RegisterCrossBindingAdaptorHelper.HelperRegister(domain);
    }
}
#endif