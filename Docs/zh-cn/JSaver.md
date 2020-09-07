## JSaver

> JEngine现已支持JSaver，是数据持久化的工具
>
> JSaver能干什么？
>
> - 将数据转字符串/JSON字符串
> - AES加密
> - 存储本地
> - 从本地加载（支持泛型）
> - 获取本地是否有Key
> - 删除本地Key


### APIs

- ```c#
  SaveAsString<T>(T val, string dataName, string encryptKey)
  ```

- ```c#
  SaveAsJSON<T>(T val, string dataName, string encryptKey)
  ```

- ```c#
  GetString(string dataName, string encryptKey)
  ```

- ```c#
  GetInt(string dataName, string encryptKey)
  ```

- ```c#
  GetShort(string dataName, string encryptKey)
  ```

- ```c#
  GetLong(string dataName, string encryptKey)
  ```

- ```c#
  GetDecimal(string dataName, string encryptKey)
  ```

- ```c#
  GetDouble(string dataName, string encryptKey)
  ```

- ```c#
  GetFloat(string dataName, string encryptKey)
  ```

- ```c#
  GetBool(string dataName, string encryptKey)
  ```

- ```c#
  GetObject<T>(string dataName, string encryptKey)
  ```

- ```c#
  HasData(string dataName)
  ```

- ```c#
  DeleteData(string dataName)
  ```

- ```c#
  DeleteAll()
  ```



## 如何使用

1. 在您的热更工程里，引入以下命名空间

   ```c#
   using JEngine.Core;
   ```

2. 保存字符串，配置加密密码和数据名称

   ```c#
   JSaver.SaveAsString("data to save", "dataName", "1234567890987654");//Set a data to local storage
   ```

3. 可获取返回的加密值

   ```c#
   var encryptStr = JSaver.SaveAsString("data to save", "dataName", "1234567890987654");//set and get the encrypted data string
   ```

4. 获取数据:

   ```c#
   var decryptStr = JSaver.GetString("dataName", "1234567890987654");
   ```

5. **恭喜！成功使用！**

##### 扩展

1. Demo示例（包含90%的API使用）：

   ```c#
   public class DataClass
   {
     public int id;
     public string name;
     public long money;
     public long diamond;
     public bool gm;
   }
   public static class Program
   {
     public static void RunGame()
     {
       	/*
   			* ====================================
         *           JSaver EXAMPLE
         * ====================================
         */
       JSaver.SaveAsString("data to save", "dataName", "1234567890987654");//Set a data to local storage
       var encryptStr = JSaver.SaveAsString("data to save", "dataName", "1234567890987654");//set and get the encrypted data string
       Log.Print($"[JSaver] Str Encrypted result: {encryptStr}");
       var decryptStr = JSaver.GetString("dataName", "1234567890987654");
       Log.Print($"[JSaver] Str Decrypted result: {decryptStr}");
   
       //save custom class
       DataClass data = new DataClass
       {
         id = 666,
         name = "JEngine牛逼",
         money = 999999,
         diamond = 999999,
         gm = true
       };
       encryptStr = JSaver.SaveAsJSON(data, "playerData", "password_is_this");
       Log.Print($"[JSaver] Custom Class Encrypted result: {encryptStr}");
       decryptStr = JSaver.GetString("playerData", "password_is_this");//Can convert to string
       Log.Print($"[JSaver] Str Decrypted result: {decryptStr}");
   
       DataClass newData = JSaver.GetObject<DataClass>("playerData", "password_is_this");//Can covert to class
   
     }
   }
   ```
