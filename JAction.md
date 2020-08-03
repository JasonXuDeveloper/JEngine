## JAction

> JEngine now supports JAction, which is an extension for Action
>
> Why use JAction?
>
> - Less code, does more things
> - Method-Chaining style
> - Able to wait for a condition
> - Able to delay actions



### APIs

- Do(Action action)
- Delay(float time)
- Until(Func<bool> condition, float frequency = 25, float timeout = -1)
- RepeatWhen(Action action, Func<bool> condition, float frequency = 25, float timeout = -1)
- RepeatUntil(Action action, Func<bool> condition, float frequency = 25, float timeout = -1)
- Repeat(Action action, int counts, float duration = 0)
- JAction Excute()



## How to use it

1. In your Hot Update Scripts, and in your c# file, add the import at the top:

   ```c#
   using JEngine.Core;
   ```

2. Create new **JAction**

   ```c#
   JAction j = new JAction();
   ```

3. Make JAction do something

   ```c#
   j.Do(() => Log.Print("Hello from JAction!"))
     .Do(() => Log.Print("Bye from JAction"))
   ```

4. **To Excute JAction (IMPORTANT)**:

   ```c#
   j.Excute();
   ```

5. **All done!** (Remember to call **Excute** method when if you want to excute your action)

##### Extension

1. Method Chaining:

   ```c#
   JAction j = new JAction();
   j.Do(xxx)
     .Repeat(xxx,times,frequecny)
     .Delay(duration)
     .Do(xxx)
     .Excute();
   ```

2. Example:

   ```c#
   public class Example : MonoBehaviour
       {
           public void Start()
           {
               int num = 0;
               int repeatCounts = 3;
               float repeatDuration = 0.5f;
               float timeout = 10f;
   
               JAction j = new JAction();
   
               j.Do(() => Log.Print("Hello from JAction!"))
               .Repeat(() =>
               {
                   num++;
                   Log.Print($"num is: {num}");
               }, repeatCounts, repeatDuration)
               .Do(() => Log.Print($"num has increased {repeatCounts} times"))
               .RepeatWhen(() =>
               {
                   Log.Print($"num is more than 0, num--");
                   num--;
               },
               () => num > 0, repeatDuration, timeout)
               .Do(() => Log.Print("JAction will do something else in 3 seconds"))
               .Delay(3.0f)
               .Do(() => Log.Print("Bye from JAction"))
               .Excute();
           }
       }
   ```

