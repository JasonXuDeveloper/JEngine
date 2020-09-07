## Auto Bind Guide

> Sometimes, a GameObject may include heaps of hot-update scripts, and we have to add them one by one by using AddComponent, this is a waste of time, so here we come auto-bind tool

1. **Add Class Bind Script** on a GameObject
2. **Change the size of Classes to whatever numbers of scripts you wanna auto add**
3. **Namespace section put the namespace of hot update script (default is HotUpdateScripts, which is the hot-update namespace), Class section put the script name**
4. Will **auto bind ** **when running**, can check Active After to active automatically
5. **MonoBehaviour**'s scripts，**requires set enabled = true and call Awake()** to activate the script
6. **JBehabviour**'s scripts，just call **Activate()** to activate



### Params

- ScriptsToBind

  - **Size is how many scripts** from hot update dll you wanna add into this gameObject/ prefab

- Namespace

  - **Namespace** of the script, **default HotUpdateScripts**

- Class

  - **Class name** of the script

- Active After

  - **When enabled**, will **auto activate** after binding/ binding + setting values
  - **When not enabled**, please follow Step 5 & 6 from guide above to activate

- Require Bind Fields

  - **When enabled**, **will auto set values from Fields part**
  - **When not enabled**，**won't set any values even Fields part are set**

- Fields

  - **Auto set** how many **values** on this script

- Field Type

  - **Type of this value** (Support **number, bool, string, GameoObject and Unity Components which are not hot updatable**)

- Field Name

  - Name stands for this value

    ![name](https://s1.ax1x.com/2020/09/05/wEyk9K.png)

- Value

  - Value of this value
    - If it is a number or a string, just write them in
    - If it is a bool value, write true or false
    - If it is a **GameObject**，please write **full path**，**if it is a child GameObject of a parent GameObject, parent needs to be Active**; **if it is not a child GameObject, itself needs to be Active**, EG:**Path,Canvas/Text，Text can be inactive, but Canvas must be Active; Path,Demo, Demo GameObject must be Active**, can link to this gameObjecet, use **${this}** for value
    - If it is a **Unity Component**, please write **full path, referenced by above GameObject**, and then add a **".", at the end put the name of script**,**EG: Canvas/Text.Outline，Which means get Outline script From Canvas/Text**, can link to script on this gameObject, use **${this}.xxx** for value, xxx is name of component
  
  

![show](https://s1.ax1x.com/2020/09/06/wenolT.png)

