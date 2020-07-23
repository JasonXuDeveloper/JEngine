## JEngine.UI (JUI)

> JEngine now contains a new class which enhance the productivity of your UI (Supports any UGUI components, eg. Button, Text, Slider,etc.)
>
> Why choose JEngine.UI?
>
> - Method-Chaning coding
> - Bind datas and update UI when datas are changed
> - Update UI in specific frequency
> - Simple but powerful



1. In your Hot Update Scripts, and in your c# file, add the import at the top:

   ```c#
   using JEngine.UI;
   ```

2. Create a JUI with **AddComponent<T>** Method:

   ```c#
   //Here it is an example of adding JUI
   JUI t = GameObject.Find("Canvas/AnyGameObject").AddComponent<JUI>();
   ```

3. You can choose whether to give this JUI actions:

   ```c#
   //NORMAL BASIC ACTIONS
   //To Init it
    t.onInit(t =>
             {
               
             });
   
   //To Run it
    t.onRun(t =>
             {
               
             });
   //When it has ended
    t.onEnd(t =>
             {
               
             });
   
   
   /*
    * To Loop it
    */
   //USE IT IF YOU WANT TO UPDATE AN UI IN SPECIFIC FREQUENCY 
    t.onLoop(t =>
             {
               
             });
   
   /*
    * To Bind a Data
    */
   BindableProperty<int> i = new BindableProperty<int>(0);
   t.Bind(i);//Bind JUI to an data
   //What to do when data has updated
   t.onMessage(t =>
               {
                 
               });
   ```

4. **To activate the JUI (IMPORTANT)**:

   ```c#
   t.Activate();
   ```

5. **All done!** (Remember to call **Activate** method when if you want to activate a JUI)

##### Extension

1. Method Chaining:

   ```
   JUIText t = GameObject.Find("Canvas/Text").AddComponent<JUI>()
                   .onInit(t1 =>
                   {
                   
                   })
                   .onRun(t2 =>
                   {
                   })
                   .onLoop(t3 =>
                   {
                   })
                   .onEnd(t4 =>
                   {
                   })
                   .Activate();
   ```

2. Example:

   - Countdown example using JUI loop mode

   ```c#
   public class Example
       {
           public void Start()
           {
               /*
                * ========================================================================
                * 10 seconds countdown demo
                * 10秒倒计时例子
                * ========================================================================
                */
   
               int i = 10;
   
               JUI t = GameObject.Find("Canvas/Text").AddComponent<JUI>()//给一个GameObject绑定JUI，该GameObject可以不包含任何UI控件
                   .onInit(t1 =>
                   {
                       var text = t1.Element<Text>();
                       text.text = "I have been Inited!";
                       Debug.Log(text.text);
                   })
                   .onRun(t2 =>
                   {
                       var text = t2.Element<Text>();
                       text.text = "I am Running!";
                       Debug.Log(text.text);
   
                       //Set the loop mode and frequency
                       t2.frame = false;//Run in milliseconds
                       t2.frequency = 1000;//Run in every 1000 ms (1 second)
   
                       UnityEngine.Object.Destroy(t2.gameObject, 10);
                   })
                   .onLoop(t3 =>
                   {
                       i--;
                       var text = t3.Element<Text>();
                       text.text = "I will be destroyed in " + i +" seconds!";
                   })
                   .onEnd(t4 =>
                   {
                       Debug.Log("My lifecycle has been ended!");
                   })
                   .Activate();
           }
       }
   ```

   - Data binding example

   ```c#
    [Serializable]
       public class Data
       {
           public int a = 0;
           public BindableProperty<int> b = new BindableProperty<int>(0);
       }
   
       /// <summary>
       /// This showcase shows how JUI works if an UI needs to update frequently
       /// </summary>
       public class Demo :MonoBehaviour
       {
           public static Demo Instance;
   
           public Data data;
   
           public void Awake()
           {
               Instance = this;
               data = new Data();//Create data
           }
   
           //You need Start here in ILRuntime so that it leads to update
           //If you dont have Start method when you inherit MonoBehaviour, ILRuntime will not call Update (It is an unknown bug and i will fix it soon as possible)
           public void Start()
           {
               
           }
   
           float seconds = 0;
           //pretends to modify data every second
           public void Update()
           {
               seconds += Time.deltaTime;
   
               if (seconds >= 1)
               {
                   data.a++;//Pretending modifing data
                   data.b.Value++;//Pretending modifing data
                   seconds -= 1;
               }
           }
       }
   
   
   
       public class JUIShowcase : MonoBehaviour
       {
           GameObject a;
           GameObject b;
   
           #region NORMAL WAY TO UPDATE UI
           public void Awake()
           {
               //Add showcase data
               new GameObject("BindShowcase").AddComponent<Demo>();
               a = GameObject.Find("Canvas/A");//Bind the gameobject which has the UI element
           }
           //In normal way you need to update your UI in every frame so that you can make your text acurately present your data
           int times = 0;
           public void Update()
           {
               a.GetComponent<Text>().text = "(Without JUI)a="+Demo.Instance.data.a.ToString()+"\n<size=20>I have been run for "+times+" times</size>";//Update UI
               times++;
           }
           #endregion
   
           #region USE JUI TO UPDATE UI(With Bind)
           /*
            * ========================================================================
            * JUI bind demo
            * JUI绑定数据例子
            * ========================================================================
           */
           public void Start()
           {
               b = GameObject.Find("Canvas/B");//Bind gameobject to show data
   
               //In JUI it is easy to bind data with text
               int times2 = 0;
               var JUI = b.AddComponent<JUI>()//Add JUI to an gameobject
               .Bind(Demo.Instance.data.b)//Bind data.b to this gameobject
               .onMessage(t1 =>//Tells JUI what to do when the binded data has updated
               {
                   //EG. we have update UI here
                   t1.Element<Text>().text = "(With JUI)b=" + ((int)Demo.Instance.data.b).ToString() + "\n<size=20>I have been run for " + times2 + " times</size>";
                   //You can convert bindable properties easily and get their values
                   times2++;
               })
               .Activate();//Activate the UI
           }
           #endregion
   ```
   
   
   
3. Controls the frequency of a loop: *(Only if you want to make your UI do something in loop)*

   **JUI** inherits from **JUIBehaviour**, which can manage the mode and frequency of loop.

   - **frame** is a bool value which holds whether the loop runs in frames or in milliseconds
   - **frequency** is a int value which holds the interval of loops (in frame counts when **frame** is true or in milliseconds)

4. How to bind my data to JUI

   ```c#
   public class MyData
   {
     public int a;//Normal data
     public BindableProperty<int> b = new BindableProperty<int>(0);//Bindable data
   }
   ```

5. How to get a BindableProperty's value and how to change it

   ```c#
   void MyMethod()
   {
     //To get a BindableProperty's value:
     int newB = b;//Automatically convert from BindableProperty
     
     //To change a BindableProperty's value:
     b.Value = 10;//Use fieldName.Value to change a value
   }
   ```

6. How to Bind data into a JUI

   ```c#
   void MyJUIExample()
   {
     MyData data;//Create a data
     JUI jui = GameObject.Find("Canvas/MyUIElement").AddComponent<JUI>()//Add JUI to an UI element
       .Bind(data.b)//Bind a data
       .onMessage(t=>		//Tell what to do when data has changed
       {
         Debug.Log("b has changed!");
       })
       .Activate();	//Activate JUI
   }
   ```

7. How to get UI components on JUI object

   ```c#
   JUI jui = GameObject.Find("Canvas/MyUIElement").AddComponent<JUI>();//Add JUI to an UI element
   Button btn = jui.Element<Button>();//It is a generic method to get an UI element
   //If you dont have this component on the element, JUI will automatically add one
   ```

8. **IMPORTANT: IF YOU BIND A DATA TO JUI, LOOP WILL NOT BE CALLED; IF YOU CHOOSE TO LOOP JUI, BIND DATA WILL NOT BE CALLED**
   

