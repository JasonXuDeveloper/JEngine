## Basics

> The basics tells you how to use this framework in basics

1. **Clone** this project

2. Open **Project Directory** via Unity

3. Go to **HotUpdateResources/DLL & HotUpdateResources/Scene**, make sure you have **HotUpdateScripts.dll & Game.unity**

   <img src="https://s1.ax1x.com/2020/07/14/Ut6vWR.png" alt="check1" style="width:50%;margin-left:25%" />

   <img src="https://s1.ax1x.com/2020/07/14/UtcFTe.png" alt="check2" style="width:50%;margin-left:25%" />

4. On top menu, select **JEngine/XAsset/Build Bundles**

   <img src="https://s1.ax1x.com/2020/07/14/Ut6qwF.png" alt="menu" style="width:75%;margin-left:12.5%" />

5. Once it has built, upload your the whole **DLC** directory into your server (If you don't know how to do it, google search how to build a web server)

6. Go to **Init scene** in Unity, choose **Updater** in hierarchy

   <img src="https://s1.ax1x.com/2020/07/14/UtcuOf.png" alt="hierarchy" style="width:50%;margin-left:25%" />

7. In Inspector, change **Base URL** to your web host address which can locates hot-update files

   > Format: http://your-ip:your-port/DLC/ or http://your-domain/DLC/
   >
   > <img src="https://s1.ax1x.com/2020/07/16/UBC5uD.png" alt="inspector" style="width:50%;margin-left:25%" />

8. Now, run your game, and you will feel how hot-update works!

   > Note that stuffs shown in Unity Demo is in Chinese and will supports English in the future

