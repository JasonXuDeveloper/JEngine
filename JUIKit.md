### JUIKit

> JUIKit 是一套剥离MonoBehaviour的UGUI框架。其主要设计目的是让UI界面开发逻辑更为便捷。

* #### APanelBase 面板基类

> APanelBase 为所有面板的基，所有UI面板都继承于APanelBase ，负责面板自身的注册、打开、关闭、以及生命周期函数

**1、常用API**

``` csharp
//[API] 注册界面相关的一些其他资源，
public override void Refer()

//[API] 打开面板函数
public void ShowUI(string uiName)    
public void ShowUI(string uiName, Action<APanelBase> openCall, Action<APanelBase> closeCall)

//[API] 预加载界面函数
public virtual void PreLoadUI()
    
//[API] 清理界面函数
public virtual void Clear()    

//[API] 界面的销毁函数    
public virtual void Destroy()    

//[API] 界面的关闭函数
public virtual void CloseUI()

//[生命周期函数] 在界面实例化的时候调用一次，类似Awake,Start
public virtual void Init()
    
//[生命周期函数] 界面每次被打开时自动调用，也可以外部调用刷新
public virtual void Refresh()
    
//[生命周期函数] 在Refresh()后执行，用做清理还原面板上数据
public virtual void ResetUI()

//[生命周期函数] 界面的帧刷新函数
public virtual void Update()

//[生命周期函数] 界面的帧刷新函数
public virtual void LateUpdate()
```

**2、例子**

```csharp
public class JTestView : APanelBase
{
    //每个界面面板依赖于单例模式
    public static JTestView Instance { get { return Singleton<JTestView>.Instance; } }

    /// <summary>
    /// 声明存放位置，属于什么类型的面板
    /// </summary>
    public JTestView() : base()
    {
        isFilm = true;
        m_Type = UIPanelType.One;
    }
    
    /// <summary>
    /// 关联注册函数
    /// </summary>
    public override void Refer()
    {
        base.Refer();
        //在面板被注册的时候，会同时注册这个面板相关的资源。
        //在异步加载本面板的时候，会自动加载此处注册的资源
        //在打开面板时，当所有面板被异步加载完成后，再执行生命周期函数
        AddRefer(ResType.UI, "相关资源", AssetType.UIPrefab);
    }
    
    /// <summary>
    /// 初始化函数，相当于Awake
    /// </summary>
    public override void Init()
    {
        base.Init();
    }
    
    /// <summary>
    /// 帧函数
    /// </summary>
    public override void Update()
    {
        base.Update();
    }

    /// <summary>
    /// 周期函数，界面打开会自动执行一次
    /// </summary>
    public override void Refresh()
    {
        base.Refresh();
    }

    /// <summary>
    /// 周期函数，界面关闭会自动执行，
    /// 也可以外部调用，进行关闭面板
    /// </summary>
    public override void CloseUI()
    {
        base.CloseUI();
    }
}
```

* #### AItemBase 面板子部件的基类

> UI面板子部件。考虑用 APanelBase 管理面板所有信息有点太过于庞大，于是便有了 AItemBase ，
>
> 用做拆分成每个小的子部件，各自管理各自的部分，APanelBase 管理所有的 AItemBase 

```csharp
protected internal GameObject m_gameobj = null;
protected internal Transform Trans = null;
protected internal RectTransform RectTrans = null;

//[生命周期函数] 初始化子部件，只执行一次
public virtual void setObj(GameObject obj)

//[API] 刷新子部件，注意区分 Refresh()
public virtual void Refresh<T>(T data)
    
//[API] 刷新子部件，注意区分 Refresh<T>(T data)
public virtual void Refresh() { }

//[API] 帧刷新，非生命周期
public virtual void Update() { }

//[API] 清理
public virtual void Clear() { }

//[属性] 判断子部件是否打开
public virtual bool IsActive

```

**1、例子**

```csharp
public class JTestItem : AItemBase
{
    public override void setObj(GameObject obj)
    {
        base.setObj(obj);
        //持有 GameObject，Transform，RectTransform
    }
}
```

**2、绑定子部件**

```csharp
public class JTestView : APanelBase
{
    public static JTestView Instance { get { return Singleton<JTestView>.Instance; } }

    public JTestView() : base()
    {
        isFilm = true;
        m_Type = UIPanelType.One;
    }
    private JTestItem testItem;

    public override void Init()
    {
        base.Init();
        
        //绑定子部件。
        //子部件名字=层级游戏物体名字
        //绑定成功会自动执行子部件的 setObj(GameObject obj) 函数
        //并且持有 GameObject 和 Transform
        testItem = UIUtility.CreateItemNoClone<JTestItem>(Trans, "子部件名字");
    }
}
```

#### UIMgr 界面管理器

> UIMgr是所有UI界面的管理器，这里承载了UI面板的注册，打开，关闭，以及销毁。是框架的核心管理部分。

**1、界面注册**

* 声明、注册面板

```csharp
//声明UI面板，用做注册界面的Key
public static string JTestViewPath = "uiview_testView";

//UIMgr注册界面（需要在游戏初始化函数中对所有面板进行注册）
UIMgr.Instance.Register(
     (JTestViewPath, JTestView.Instance),
     (JBtnViewPath, JBtnView.Instance));
```

* 初始化时，注册面板的函数

```CSharp
/// <summary>
/// 注册界面
/// </summary>
/// <param name="panels">全部界面，string是路径</param>
public void Register(params (string,APanelBase)[] panels)
{
    #region 注册所有UI界面
    for(int i = 0; i < panels.Length; i++)
    {
    	Register(panels[i].Item1, panels[i].Item2);
    }
    #endregion
}

private void Register<T>(string name, T panel) where T : APanelBase
{
    if (string.IsNullOrEmpty(name) || panel == null) return;
    if (m_uIbaseDic.ContainsKey(name))
    {
    	Log.PrintError($"panel为[{panel.GetType()}]注册为{name}失败，{name}已定义为{m_uIbaseDic[name].GetType()}");
    }
    else
    {
        panel.m_strPanelViewName = name;
        panel.Refer();
        m_uIbaseDic.Add(name, panel);
    }
}
```

**2、常用API**

```csharp
//[API] 打开界面的统一入口
public void ShowUI(string uibaseName, bool isSaveShow = false, Action<APanelBase> closeCall = null, Action<APanelBase> openCall = null, bool isClearAll = true)
    
//[API] 设定打开或者关闭已开UI
public void SetShowAllOpenUI(bool isOpen)
    
//[API] 预加载某个面板
public void PreLoadUI(string uibaseName)
    
//[API] 获得某个面板
public APanelBase GetUI(string uibaseName)
    
//[API] 删除某个面板    
public void DestoryUI(string uibaseName)
    
//[API] 关闭并删除某个面板   
public void CloseAndDestoryUI(string uibaseName)

//[API] 判断面板是否打开
public bool IsOpen(string planeName)
    
//[API] 判断面板是否存在
public bool InHavePanel(string planeName)

//[API] 关闭某个面板
public void CloseUI(string uibaseName)
    
//[API] 关闭所有打开面板(排除设置为保持状态的)
public void CloseAllUI(bool isFilm)
    
//[API] 帧刷新
public void Update()
```

**3、例子**

```csharp
//执行打开面板
UIMgr.Instance.ShowUI(JTestViewPath);

//需要界面完成打开后回调时
UIMgr.Instance.ShowUI(JTestViewPath,
     openCall:(p)=> 
     { 
        //界面打开后的回调
     });

//可以按需注册打开后回调和关闭前回调
UIMgr.Instance.ShowUI(JTestViewPath,
     openCall:(p)=> 
     { 
         //界面打开后的回调
     },
     closeCall:(p)=> 
     {
         //界面被关闭前的回调
     });
```

#### UIUtility 其他依赖工具

> UIUtility 能快速查找某个层级，创建点击事件，创建子部件

**1、获取某个组件**

```csharp
//不需要关心层级具体位子，只保证Trans下面有该游戏物体即可
Image img = UIUtility.GetComponent<Image>(Trans, "组件层级名字");

//获取游戏物体，只保证 m_gameobj 下面有该游戏物体即可
GameObject obj = UIUtility.Control("游戏物体名", m_gameobj);

//获取变换组件
RectTransform transform = UIUtility.Control("rect", Trans);
```

**2、绑定事件**

```csharp
//绑定点击事件
GameObject button = UIUtility.BindClickEvent(Trans, "按钮或者图片名称", OnClick);

//绑定双击事件
GameObject button = UIUtility.BindDoubleClickEvent(Trans, "目标名称", OnClick);

//绑定开始拖拽事件
GameObject button = UIUtility.BindDragBeginEvent(Trans, "目标名称", OnClick);

//绑定拖拽事件
GameObject button = UIUtility.BindDragEvent(Trans, "目标名称", OnClick);

//绑定结束拖拽事件
GameObject button = UIUtility.BindDragEndEvent(Trans, "目标名称", OnClick);

//绑定按下事件
GameObject button = UIUtility.BindPressDownEvent(Trans, "目标名称", OnClick);

//绑定抬起事件
GameObject button = UIUtility.BindPressUpEvent(Trans, "目标名称", OnClick);

//支持自定义其他事件类型，详情参考 UIUtility.cs
......
```

**3、绑定子部件**

```csharp
//给层级绑定一个子部件脚本，子部件已存在，不需要实例化
JTestItem item = UIUtility.CreateItemNoClone<JTestItem>(Trans, "子部件名字");

//实例化子部件并且绑定子部件脚本
JTestItem item = UIUtility.CreateItem<JTestItem>(Trans, "子部件名字");
```

**4、例子**

* 主面板 JTestView.cs

```csharp
public class JTestView : APanelBase
{
    public static JTestView Instance { get { return Singleton<JTestView>.Instance; } }

    public JTestView() : base()
    {
        isFilm = true;
        m_Type = UIPanelType.One;
    }

    private GameObject JBtnButton;
    private JTestItem testItem;

    public override void Init()
    {
        base.Init();

        JBtnButton = UIUtility.BindClickEvent(Trans, "JBtnButton", OnClick);

        //注册子部件
        testItem = UIUtility.CreateItem<JTestItem>(Trans, "子部件名字");
    }

    public override void Refresh()
    {
        base.Refresh();
        
        //刷新子部件
        testItem.Refresh("传递给子部件的内容");
        
        Log.Print("面板被打开会自动调用一次，也可以外部调用此函数进行刷新");
        //外部调用方式
        //JTestView.Instance.Refresh();
    }

    private void OnOpenJBtnExample()
    {
        UIMgr.Instance.ShowUI(JumpToUIDemo.JBtnViewPath,
                              openCall: (p) =>
                              {
                                  CloseUI();
                              },
                              closeCall: (p) =>
                              {
                                  UIMgr.Instance.ShowUI(m_strPanelViewName);
                              });
    }

    public override void OnClick(GameObject obj, PointerEventData eventData)
    {
        base.OnClick(obj, eventData);

        if (obj.Equals(JBtnButton))
        {
            OnOpenJBtnExample();
        }
    }
}
```

* 子部件 JTestItem.cs

```csharp
public class JTestItem : AItemBase
{
    private Text label;

    public override void setObj(GameObject obj)
    {
        base.setObj(obj);

        label = UIUtility.GetComponent<Text>(RectTrans, "名字要唯一");
    }

    public override void Refresh<T>(T data)
    {
        base.Refresh(data);		
        //给Text赋值，其他小功能可以查看UIUtility.cs源码
        UIUtility.Safe_UGUI(ref label, data as object);
    }
}
```



