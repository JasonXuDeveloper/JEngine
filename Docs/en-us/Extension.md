## Extensions

> The extensions will show you how to hot-update your game in coding levels

1. Open **ClonedProject/HotUpdateScripts/HotUpdateScripts.sln**
2. Find **Program.cs** and open it
3. Change something but **DO NOT DELETE** *RunGame()* Method
4. **Build Solution with RELEASE**
5. Go back to Unity, **redo step 4 & 5 in the Basics**
6. Run Game, and you will see the differences which you made



### Auto-bind code guide

> Sometimes, a GameObject may include heaps of hot-update scripts, and we have to add them one by one by using AddComponent, this is a waste of time, so here we come auto-bind tool

1. **Add Class Bind Script** on a GameObject
2. **Change the size of Classes to whatever numbers of scripts you wanna auto add**
3. **Namespace section put the namespace of hot update script (default is HotUpdateScripts, which is the hot-update namespace), Class section put the script name**
4. Will **auto bind ** **when running**，need to be aware that**those scripts been auto bind has no params, you need to set properties and params in code**
5. **MonoBehaviour**'s scripts，**requires set enabled = true and call Awake()** to activate the script
6. **JBehabviour**'s scripts，just call **Activate()** to activate
7. When necessary, can call **InitILrt.BindAllScripts();** to request auto-bind (It has been called in Program.cs, please do not delete it)

![autobind](https://s1.ax1x.com/2020/09/04/wkGjqe.jpg)

