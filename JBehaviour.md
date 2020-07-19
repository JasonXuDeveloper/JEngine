## JBehaviour

> JEngine now contains a **new behaviour** base on MonoBehaviour but **runs better**
>
> Why choose JBehaviour?
>
> - Simple lifecycle
> - Less codes to implement loops
> - Uses coroutine rather than methods to do updates



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
   using UnityEngine;
   using UnityEngine.UI;
   using JEngine.LifeCycle;
   
   namespace HotUpdateScripts
   {
       public class Sample : JBehaviour
       {
           public Text HelloText;
   
           public int times;
   
           public override void Init()
           {
               HelloText = GameObject.Find("Canvas/Text").GetComponent<Text>();
               times = 0;
           }
   
           public override void Run()
           {
               //Here in run method, we set up the frequency and mode of loop.
   
               frame = false;// Not loop in frame, but in milliseconds
               frequency = 1000;//Loop in 1000ms => 1 second
   
               /* OR:
                * frame = true;// Loop in frame
                * frequency = 10;//Loop in every 10 frames
                */
           }
   
           public override void Loop()
           {
               HelloText.text = "HELLO JEngine * " + times + " times";
               times++;
           }
       }
   }
   ```

5. As you might see, in **Run** method, there is an assignment of **frame and frequency** variable, these variables controls **loop** method.

   - frame: **bool**, when it is true, loop runs in **frames**; or loop runs in **milliseconds** 
   - frequency: **int**, it holds the **interval of frames or milliseconds** which calls loop method