using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using ILRuntime.CLR.TypeSystem;
using ILRuntime.CLR.Method;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.Runtime.Intepreter;
//下面这行为了取消使用WWW的警告，Unity2018以后推荐使用UnityWebRequest，处于兼容性考虑Demo依然使用WWW
#pragma warning disable CS0618

public delegate void TestDelegateMethod(int a);
public delegate string TestDelegateFunction(int a);


public class DelegateDemo : MonoBehaviour
{
    public static TestDelegateMethod TestMethodDelegate;
    public static TestDelegateFunction TestFunctionDelegate;
    public static System.Action<string> TestActionDelegate;

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
        //这里做一些ILRuntime的注册
        //TestDelegateMethod, 这个委托类型为有个参数为int的方法，注册仅需要注册不同的参数搭配即可
        appdomain.DelegateManager.RegisterMethodDelegate<int>();
        //带返回值的委托的话需要用RegisterFunctionDelegate，返回类型为最后一个
        appdomain.DelegateManager.RegisterFunctionDelegate<int, string>();
        //Action<string> 的参数为一个string
        appdomain.DelegateManager.RegisterMethodDelegate<string>();
        
        //ILRuntime内部是用Action和Func这两个系统内置的委托类型来创建实例的，所以其他的委托类型都需要写转换器
        //将Action或者Func转换成目标委托类型

        appdomain.DelegateManager.RegisterDelegateConvertor<TestDelegateMethod>((action) =>
        {
            //转换器的目的是把Action或者Func转换成正确的类型，这里则是把Action<int>转换成TestDelegateMethod
            return new TestDelegateMethod((a) =>
            {
                //调用委托实例
                ((System.Action<int>)action)(a);
            });
        });
        //对于TestDelegateFunction同理，只是是将Func<int, string>转换成TestDelegateFunction
        appdomain.DelegateManager.RegisterDelegateConvertor<TestDelegateFunction>((action) =>
        {
            return new TestDelegateFunction((a) =>
            {
                return ((System.Func<int, string>)action)(a);
            });
        });

        //下面再举一个这个Demo中没有用到，但是UGUI经常遇到的一个委托，例如UnityAction<float>
        appdomain.DelegateManager.RegisterDelegateConvertor<UnityEngine.Events.UnityAction<float>>((action) =>
        {
            return new UnityEngine.Events.UnityAction<float>((a) =>
            {
                ((System.Action<float>)action)(a);
            });
        });
    }

    void OnHotFixLoaded()
    {
        Debug.Log("完全在热更DLL内部使用的委托，直接可用，不需要做任何处理");
        
        Debug.Log("如果需要跨域调用委托（将热更DLL里面的委托实例传到Unity主工程用）, 就需要注册适配器");
        Debug.Log("这是因为iOS的IL2CPP模式下，不能动态生成类型，为了避免出现不可预知的问题，我们没有通过反射的方式创建委托实例，因此需要手动进行一些注册");
        Debug.Log("如果没有注册委托适配器，运行时会报错并提示需要的注册代码，直接复制粘贴到ILRuntime初始化的地方");
        appdomain.Invoke("HotFix_Project.TestDelegate", "Initialize2", null, null);
        appdomain.Invoke("HotFix_Project.TestDelegate", "RunTest2", null, null);
        Debug.Log("运行成功，我们可以看见，用Action或者Func当作委托类型的话，可以避免写转换器，所以项目中在不必要的情况下尽量只用Action和Func");
        Debug.Log("另外应该尽量减少不必要的跨域委托调用，如果委托只在热更DLL中用，是不需要进行任何注册的");
        Debug.Log("---------");
        Debug.Log("我们再来在Unity主工程中调用一下刚刚的委托试试");
        TestMethodDelegate(789);
        var str = TestFunctionDelegate(098);
        Debug.Log("!! OnHotFixLoaded str = " + str);
        TestActionDelegate("Hello From Unity Main Project");

    }

    void Update()
    {

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
