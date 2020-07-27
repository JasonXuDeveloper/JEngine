## JBehaviour

> JEngine now contains a **new behaviour** base on MonoBehaviour but **runs better**
>
> Why choose JBehaviour?
>
> - Simple lifecycle
> - Less codes to implement loops
> - Uses coroutine rather than methods to do updates

<img src="https://s1.ax1x.com/2020/07/19/URW5mn.png" alt="JBehaviour" style="zoom:50%;" />



1. In your Hot Update Scripts, and in your c# file:

   Add import at the top:

   ```c#
   using JEngine.LifeCycle;
   ```

2. Inherit **JBehaviour** in your class

   ```c#
   namespace HotUpdateScripts
   {
       public class Sample : JBehaviour
       {
       	//ToDo
       }
   }
   ```

3. There are four main methods in **JBehaviour**

   - Init => When this class has been added to an Unity GameObject
   - Run => This method will be called after Init
   - Loop => This method will loop in specific mode and specific frequency
   - End => Will be called when the GameObject with this class has been destoryed

4. Example Showcase:

   ```c#
   using System;
   using JEngine.Core;
   using JEngine.LifeCycle;
   
   namespace JEngine.Examples
   {
       public class JBehaviourExample : JBehaviour
       {
           private int i;
   
           public override void Init()
           {
               base.Init();
               Log.Print("JBehaviour has been created!");
           }
   
           public override void Run()
           {
               base.Run();
               Log.Print("JBehaviour is running!");
               //Change the frequency of loop
               FrameMode = false;//Don't loop in frame
               Frequency = 1000;//Run every 1000 milliseconds
   
               i = 1;
   
               Destroy(this.gameObject, 10);
           }
   
           public override void Loop()
           {
               Log.Print("Hello JBehaviour * " + i + " times!");
               i++;
           }
   
           public override void End()
           {
               Log.Print("I have been destroyed!");
           }
       }
   }
   
   ```

5. As you might see, in **Run** method, there is an assignment of **frame and frequency** variable, these variables controls **loop** method.

   - FrameMode: **bool**, when it is true, loop runs in **frames**; or loop runs in **milliseconds** 
   - Frequency: **int**, it holds the **interval of frames or milliseconds** which calls loop method