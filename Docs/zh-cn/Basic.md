## 基础指南

> 该文章将告诉您如何初步使用JEngine

1. **Clone该项目**

2. 将**Project目录用Unity打开**

3. 找到**HotUpdateResources/DLL**以及**HotUpdateResources/Scene**, 确保你能找到**HotUpdateScripts.dll以及Game.unity**

   <img src="https://s1.ax1x.com/2020/07/14/Ut6vWR.png" alt="check1" style="width:50%;margin-left:25%" />

   <img src="https://s1.ax1x.com/2020/07/14/UtcFTe.png" alt="check2" style="width:50%;margin-left:25%" />

4. 找到并点击顶部导航栏中**JEngine/XAsset/Build Bundles**选项

   <img src="https://s1.ax1x.com/2020/07/14/Ut6qwF.png" alt="menu" style="width:75%;margin-left:12.5%" />

5. 根据弹窗，**输入DLL加密密码**，点击**Build**

6. 生成成功后，资源将进入**DLC目录**，将该目录传入您的资源服务器（如果不知道如何操作，请百度搜索如何搭建资源服务器）

7. 进入**Init场景**，在阶级中选择**Updater**

   <img src="https://s1.ax1x.com/2020/07/14/UtcuOf.png" alt="hierarchy" style="width:50%;margin-left:25%" />

8. 在**Inspector**中，将**BaseURL**换为您**存放资源的地址**

   > 格式: http://your-ip:your-port/DLC/ or http://your-domain/DLC/
   >
   > <img src="https://s1.ax1x.com/2020/07/16/UBC5uD.png" alt="inspector" style="width:50%;margin-left:25%" />

9. **现在，运行游戏，即可体验热更功能！**

   > 到这里，您已经完成了热更游戏的第一步，恭喜！

