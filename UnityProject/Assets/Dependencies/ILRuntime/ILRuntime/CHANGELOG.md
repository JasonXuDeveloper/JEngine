## 更新记录

### V1.6.7
- Appdomain.Prewarm接口加入递归预热的选项
- 优化ILRuntime遇到异常时输出的日志，可在Unity的Console中直接点击前往源码
- 增加CLRBinding在处理遇到同名类型时的容错
- 修正跨域继承适配器自动生成工具生成的代码部分情况无法正常编译的问题
- 完善Debug模式下部分异常的错误日志输出，方便定位问题
- 修正个别情况下，为值类型方法this赋值时导致栈损坏的问题
- 修正一个泛型方法匹配的Bug
- 修正将ILRuntime中的多播委托实例存入List等容器时，只有最后一个挂接的回调被执行的Bug
- 修正一个ref/out操作枚举变量时的Bug
- 修正在泛型方法中转换Action等泛型委托失败的Bug
- 增加CLRBindingUtils，使得应用可以同时兼容有CLR绑定和无CLR绑定的情况（直接删除CLR绑定自动生成代码也不会造成编译错误）
- 给Litjson加入JsonIgnore特性
- 修正FieldInfo，PropertyInfo等反射类型调用GetCustomAttributes时出错的问题
- 修正ILRuntime中获取Attributes无法获取部分参数值的Bug
- 修正一个导致async await中断的Bug
- 修正一个跨域继承访问基类protected变量出错的Bug
- 升级Mono.Cecil到0.11.3

### V1.6.6
- Appdomain.Prewarm接口加入预热指定方法的参数
- AppDomain新增GetCurrentStackTrace接口获取当前调用栈，仅支持从热更中调用
- 修改Profiler宏，以简化Unity平台上报Profiler数据
- 修复一个基类包含值类型字段的类型报错的Bug
- 修复热更中调用自定义Attribute后，自动分析绑定生成的代码编译不过的问题
- 修复几个CLR自动分析绑定生成的代码出错的问题
- 修复几个泛型方法匹配的Bug
- 修复在基类实现接口的虚方法上有不必要开销的问题
- 修复GetCustomAttributes接口当inherit为true时不包含基类Attribute的Bug
- 修复CLR自动分析绑定无法自动生成event调用的绑定代码的问题
- 修复在热更内调用Enum.CompareTo和Enum.ToObject的问题

### V1.6.5
- 提升了Release环境下（非Development Build）大约25%的执行效率
- 修复了一个泛型类型中泛型变量无法置null的Bug
- 修复一个default关键字作用于枚举或者值类型时报错的Bug
- 修复一个导致带条件的await语句失效的Bug
- 修复一个Enum.ToObject返回的Enum值不正确的Bug
- 修复一个泛型方法中out到一个值类型报错的Bug
- 修复一个由自动生成产生的跨域继承适配器，虚方法未被重载时不执行的Bug
- 修复Litjson集成反序列化枚举数组时崩溃的Bug

### V1.6.4
- 修正一个使用委托返回经过值类型绑定过的值类型时造成的栈损坏问题
- 修正一个自动生成跨域继承适配器代码时，无参数虚函数适配代码执行的报错
- 修正一个自动生成跨域继承适配器代码对接口方法生成后，某些情况下导致StackOverflow无限递归的Bug
- 修正Type.ToString结果和原生不一致的问题
- 修正泛型嵌套数组T[][]字段初始化时的报错问题
- 修正一个泛型方法匹配的Bug
- 修正一个泛型数组作为参数时方法匹配的错误
- 删掉一个C#7语法使用以提高兼容性

### V1.6.3
- 修复对泛型字段赋值导致栈损坏的Bug
- 修复由跨域继承适配器代码生成器自动生成的代码，导致运行时进入错误的类方法的Bug
- 修复一个在静态方法中实例化对象导致运行出错的Bug
- 修复一个某些情况下静态构造函数没有执行的Bug
- 修复一个加载多DLL，引用字段导致的运行时报错
- 修复值类型绑定Enum字段分配内存时被意外装箱的Bug
- 修复一个CLR自动分析绑定在某个特定情况下遗漏分析的Bug
- 移除所有编译警告
- 修复一个由于Unity的Bug导致的导入ILRuntime包时的报错

### V1.6.2
- 修正值类型部分情况下++, +=之类操作符失效的Bug
- 修正将值类型成员变量传入方法，并在该方法里对这个参数赋值报错的Bug
- 修复一个Async Await卡主的bug

### V1.6.1
- 去掉部分C#7的用法以增加unity2019以前版本的兼容性

### V1.6
- 增加了跨域继承适配器的工具类，简化适配器编写
- 增加了跨域继承适配器的自动生成模版，做到半自动生成
- 优化了热更存取主工程类字段的性能
- 优化了热更内方法调用的性能
- 累积Bug修复