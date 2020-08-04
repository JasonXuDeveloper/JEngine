## JAction

> JEngine now supports JAction, which is an extension for Action
>
> Why use JAction?
>
> - Less code, does more things
> - Method-Chaining style
> - Able to wait for a condition
> - Able to delay actions
> - Able to make loops
> - Able to make loops in condition
> - Able to cancle at anytime
> - Able to run sync, async and  async  parallely 



### APIs

- ```c#
  Do(Action action)
  ```

- ```c#
  Delay(float time)
  ```

- ```c#
  Until(Func<bool> condition, float frequency = 25, float timeout = -1)
  ```

- ```c#
  Repeat(Action action, int counts, float duration = 0)
  ```

- ```c#
  RepeatUntil(Action action, Func<bool> condition, float frequency = 25, float timeout = -1)
  ```

- ```c#
  RepeatWhen(Action action, Func<bool> condition, float frequency = 25, float timeout = -1)
  ```

- ```c#
  Excute()
  ```

- ```c#
  ExcuteAsync()
  ```

- ```c#
  ExcuteAsyncParallel()
  ```

- ```c#
  Cancel()
  ```



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
       /*
       * ====================================
       *           JAction EXAMPLE
       * ====================================
       */
       int num = 0;
       int repeatCounts = 3;
       float repeatDuration = 0.5f;
       float timeout = 10f;
   
       //Simple use
       JAction j = new JAction();
       j.Do(() => Log.Print("[j] Hello from JAction!"))
         .Execute();
   
       //Until
       JAction j1 = new JAction();
       j1.Until(() => true)
         .Do(()=>Log.Print("[j1] until condition has done"))
         .Execute();
   
       //Repeat
       JAction j2 = new JAction();
       j2.Repeat(() =>
                 {
                   num++;
                   Log.Print($"[j2] num is: {num}");
                 }, repeatCounts, repeatDuration)
         .Execute();
   
       //Repeat when
       JAction j3 = new JAction();
       j3.RepeatWhen(() =>
                     {
                       Log.Print($"[j3] num is more than 0, num--");
                       num--;
                     },
                     () => num > 0, repeatDuration, timeout)
         .Execute();
   
       //Repeat until
       JAction j4 = new JAction();
       j4.RepeatUntil(() =>
                      {
                        Log.Print($"[j4] num is less than 3, num++");
                        num++;
                      }, () => num < 3, repeatDuration, timeout)
         .Execute();
   
       //Delay
       JAction j5 = new JAction();
       j5.Do(() => Log.Print("[j5] JAction will do something else in 3 seconds"))
         .Delay(3.0f)
         .Do(() => Log.Print("[j5] Bye from JAction"))
         .Execute();
   
       //Execute Async
       JAction j6 = new JAction();
       _ = j6.Do(() => Log.Print("[j6] This is an async JAction"))
         .ExecuteAsync();
   
       //Execute Async Parallel
       JAction j7 = new JAction();
       j7.Do(() => Log.Print("[j7] This is an async JAction but runs parallel, callback will be called after it has done"))
         .ExecuteAsyncParallel(() => Log.Print("[j7] Done"));
   
       //Cancel a JAction
       JAction j8 = new JAction();
       _ = j8.RepeatWhen(() => Log.Print("[j8] I am repeating!!!"), () => true, repeatDuration, timeout)
         .ExecuteAsync();
       JAction j9 = new JAction();
       j9.Delay(5)
         .Do(() =>
             {
               j8.Cancel();
               Log.Print("[j9] cancelled j8");
             })
         .Execute();
     }
   }
   ```

