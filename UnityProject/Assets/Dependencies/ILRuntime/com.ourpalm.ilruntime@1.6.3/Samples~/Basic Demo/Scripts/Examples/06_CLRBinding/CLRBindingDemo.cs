using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using ILRuntime.CLR.TypeSystem;
using ILRuntime.CLR.Method;
using ILRuntime.CLR.Utils;
using ILRuntime.Runtime.Intepreter;
using ILRuntime.Runtime.Stack;
using ILRuntime.Runtime.Enviorment;
#if UNITY_5_5_OR_NEWER
using UnityEngine.Profiling;
#endif
//下面这行为了取消使用WWW的警告，Unity2018以后推荐使用UnityWebRequest，处于兼容性考虑Demo依然使用WWW
#pragma warning disable CS0618

public class CLRBindingTestClass
{
    public static float DoSomeTest(int a, float b)
    {
        return a + b;
    }
}

public class CLRBindingDemo : MonoBehaviour
{
    //AppDomain是ILRuntime的入口，最好是在一个单例类中保存，整个游戏全局就一个，这里为了示例方便，每个例子里面都单独做了一个
    //大家在正式项目中请全局只创建一个AppDomain
    AppDomain appdomain;
    System.IO.MemoryStream fs;
    System.IO.MemoryStream p;

    void Start()
    {
        StartCoroutine(LoadHotFixAssembly());
    }

    IEnumerator LoadHotFixAssembly()
    {
        //首先实例化ILRuntime的AppDomain，AppDomain是一个应用程序域，每个AppDomain都是一个独立的沙盒
        appdomain = new ILRuntime.Runtime.Enviorment.AppDomain();
        //正常项目中应该是自行从其他地方下载dll，或者打包在AssetBundle中读取，平时开发以及为了演示方便直接从StreammingAssets中读取，
        //正式发布的时候需要大家自行从其他地方读取dll

        //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        //这个DLL文件是直接编译HotFix_Project.sln生成的，已经在项目中设置好输出目录为StreamingAssets，在VS里直接编译即可生成到对应目录，无需手动拷贝
        //工程目录在Assets\Samples\ILRuntime\1.6\Demo\HotFix_Project~
#if UNITY_ANDROID
        WWW www = new WWW(Application.streamingAssetsPath + "/HotFix_Project.dll");
#else
        WWW www = new WWW("file:///" + Application.streamingAssetsPath + "/HotFix_Project.dll");
#endif
        while (!www.isDone)
            yield return null;
        if (!string.IsNullOrEmpty(www.error))
            UnityEngine.Debug.LogError(www.error);
        byte[] dll = www.bytes;
        www.Dispose();

        //PDB文件是调试数据库，如需要在日志中显示报错的行号，则必须提供PDB文件，不过由于会额外耗用内存，正式发布时请将PDB去掉，下面LoadAssembly的时候pdb传null即可
#if UNITY_ANDROID
        www = new WWW(Application.streamingAssetsPath + "/HotFix_Project.pdb");
#else
        www = new WWW("file:///" + Application.streamingAssetsPath + "/HotFix_Project.pdb");
#endif
        while (!www.isDone)
            yield return null;
        if (!string.IsNullOrEmpty(www.error))
            UnityEngine.Debug.LogError(www.error);
        byte[] pdb = www.bytes;
        fs = new MemoryStream(dll);
        p = new MemoryStream(pdb);
        try
        {
            appdomain.LoadAssembly(fs, p, new ILRuntime.Mono.Cecil.Pdb.PdbReaderProvider());
        }
        catch
        {
            Debug.LogError("加载热更DLL失败，请确保已经通过VS打开Assets/Samples/ILRuntime/1.6/Demo/HotFix_Project/HotFix_Project.sln编译过热更DLL");
        }

        InitializeILRuntime();
        OnHotFixLoaded();
    }

    void InitializeILRuntime()
    {
#if DEBUG && (UNITY_EDITOR || UNITY_ANDROID || UNITY_IPHONE)
        //由于Unity的Profiler接口只允许在主线程使用，为了避免出异常，需要告诉ILRuntime主线程的线程ID才能正确将函数运行耗时报告给Profiler
        appdomain.UnityMainThreadID = System.Threading.Thread.CurrentThread.ManagedThreadId;
#endif
        //这里做一些ILRuntime的注册，如委托适配器，值类型绑定等等


        //初始化CLR绑定请放在初始化的最后一步！！
        //初始化CLR绑定请放在初始化的最后一步！！
        //初始化CLR绑定请放在初始化的最后一步！！

        //请在生成了绑定代码后解除下面这行的注释
        //请在生成了绑定代码后解除下面这行的注释
        //请在生成了绑定代码后解除下面这行的注释
        //ILRuntime.Runtime.Generated.CLRBindings.Initialize(appdomain);
    }

    unsafe void OnHotFixLoaded()
    {
        ilruntimeReady = true;
    }

    bool ilruntimeReady = false;
    bool executed = false;
    void Update()
    {
        if (ilruntimeReady && !executed && Time.realtimeSinceStartup > 3)
        {
            executed = true;
            //这里为了方便看Profiler，代码挪到Update中了
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            Debug.LogWarning("运行这个Demo前请先点击菜单ILRuntime->Generate来生成所需的绑定代码，并按照提示解除下面相关代码的注释");
            Debug.Log("默认情况下，从热更DLL里调用Unity主工程的方法，是通过反射的方式调用的，这个过程中会产生GC Alloc，并且执行效率会偏低");
            
            Debug.Log("请在Unity菜单里面的ILRuntime->Generate CLR Binding Code by Analysis来生成绑定代码");
            
            var type = appdomain.LoadedTypes["HotFix_Project.TestCLRBinding"];
            var m = type.GetMethod("RunTest", 0);
            Debug.Log("请解除InitializeILRuntime方法中的注释对比有无CLR绑定对运行耗时和GC开销的影响");
            sw.Reset();
            sw.Start();
            Profiler.BeginSample("RunTest2");
            appdomain.Invoke(m, null, null);
            Profiler.EndSample();
            sw.Stop();
            Debug.LogFormat("刚刚的方法执行了:{0} ms", sw.ElapsedMilliseconds);

            Debug.Log("可以看到运行时间和GC Alloc有大量的差别，RunTest2之所以有20字节的GC Alloc是因为Editor模式ILRuntime会有调试支持，正式发布（关闭Development Build）时这20字节也会随之消失");
        }
    }

    void RunTest()
    {
        appdomain.Invoke("HotFix_Project.TestCLRBinding", "RunTest", null, null);
    }

    void RunTest2(IMethod m)
    {
        appdomain.Invoke(m, null, null);
    }

    private void OnDestroy()
    {
        if (fs != null)
            fs.Close();
        if (p != null)
            p.Close();
        fs = null;
        p = null;
    }
}
