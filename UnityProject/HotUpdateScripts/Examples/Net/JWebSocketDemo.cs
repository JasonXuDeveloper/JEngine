//
// JWebSocketDemo.cs
//
// Author:
//       JasonXuDeveloper（傑） <jasonxudeveloper@gmail.com>
//
// Copyright (c) 2020 JEngine
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
using System;
using JEngine.Core;
using JEngine.Net;
using WebSocketSharp;

namespace JEngine.Examples
{
    public class JWebSocketDemo
    {
        /// <summary>
        /// 接收服务器消息的方法
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OnMessageFromNormalServer(object sender, MessageEventArgs e)
        {
            Log.Print("服务端发送了：" + e.Data);//二进制是e.RawData

            /*在这里给个具体思路
            * 例如目前封装的socket io的消息模型
            * socketio采用的是json封装，包含了event name，event type 和 event data（这个在socket io 的packer里）
            * event name 顾名思义是事件名称
            * event type 这个可以忽略，就是事件类型，普遍是message
            * event data 是数据的json格式
            * 会返回一个JSONObject，然后可以根据这个JSONObject做你的逻辑
            * 
            * 所以这里的思路是你也封装一个这样的模型，然后写个字典来存事件
            * 以socket io 为例，先解析e.Data，得到字符串：event name，JSONObject：event data
            * 然后直接在Dictionary<string,Action<JSONObject>>的字典里，获取event name这个值
            * 如果存在，Invoke(data)，通过数据去Invoke回调
            * 然后没然后了，别在群里问JWebSocket非socket io 服务器怎么事件派发无效了，
            * 因为你们要自己按项目定模型，自己写的，JEngine不是双端框架，不像ET那样直接限制前后端通信方式，
            * JEngine告诉你该怎么搞，工具给你写完，后面的就要靠你自己了！
            * 
            * 所以这里建议没接触过后端的，用nodeJS服务器，用里面的socketIO，当时我从没接触过后端到能撸双端游戏，这个东西功不可没
            * 
            * 如果一定要写的，可以研究下JEvent，专门搞一个类处理事件，
            * 里面的方法全写为void xxx(string event, JSONObject data)，然后类打上[Subscriber]标签
            * 然后写个JEvent来注册，在这里获取到event name 和event data 后直接JEvent.Post就能派发了，简单的很~
            */
        }

        /// <summary>
        /// 这个用于连接其他web socket服务器，需要自己实现on方法，在OnMessageFromNormalServer方法内根据自己的消息模型自己实现。
        /// 这个连接普通WebSocket服务器的方法监听，建议有服务器基础，且C#有经验的人去写监听，没经验的，也可以用这个做发送，但是监听这边会很麻烦
        /// </summary>
        public static void RunNormalServerDemo(string ip, int port)
        {
            /*
             * 这个地方也有个配套demo，到时候会上传到GitHub，估计叫做JEngine.Net WebSokcet Server Demo
             */

            //直接连接，使用默认配置(中间参数null)，需要配置一个接收服务器消息的方法
            JWebSocket socket = new JWebSocket($"ws://{ip}:{port}/demo", null, OnMessageFromNormalServer);

            //带配置连接，需要配置一个接收服务器消息的方法
            //JSocketConfig config = JSocketConfig.Default();
            //config.debug = true;
            //JSocket socket = new JSocket($"ws://{ip}:{port}/demo", config, OnMessageFromNormalServer);

            socket.OnConnect((e) =>
            {
                Log.Print("服务端连接成功");

                //发送hi到服务端
                socket.SendToServer("hi");//同步
            });

            socket.OnError(e =>
            {
                Log.PrintError(e);
            });

            socket.OnDisconnect((e) =>
            {
                Log.Print("服务端连接关闭");
            });

            //此方法不阻塞
            socket.Connect();

            //断线重连事件
            socket.OnReconnect(() =>
            {
                Log.Print("断开连接后重连成功");
            });

            //这个地方不能用SocketIO_On来监听事件，因为不同的人的消息模型不同，所以需要在OnMessageFromNormalServer内根据服务器发来的东西自己实现
        }

        /// <summary>
        /// 这个用于连接socket-io服务器
        /// </summary>
        public static void RunSocketIOServerDemo(string ip, int port)
        {
            /* 使用前需要建立node js 服务器，并npm按照socket io，参考socketio的官网
             * 将下面的js代码丢你nodejs服务器里并node运行
             
                let server = require('socket.io');//socket

                let io = new server(8001);//创建服务器于7999端口
                console.log('服务器已开始运行');
                //开始运行服务器
                io.on('connection', async function(socket) {
                    //连接信息
                    console.log("有连接了,sid: "+socket.id);

                    //监听事件
                    socket.on('hi',function(){
                        //发送事件
                        socket.emit("hi_back");
                        console.log("已发送hi_back");
                    });
    
    
                    //发送事件+数据
                    var player = new Object();
                    player.id = 666;
                    player.name = "傑";
                    socket.emit("simulate_auth",player);
                });

             */

            //直接连接，使用默认配置
            JWebSocket socket = new JWebSocket($"ws://{ip}:{port}/socket.io/?EIO=3&transport=websocket");

            //带配置连接
            //JSocketConfig config = JSocketConfig.Default();
            //config.debug = true;
            //JSocket socket = new JSocket($"ws://{ip}:{port}/socket.io/?EIO=3&transport=websocket", config);

            socket.OnConnect(async (e) =>
            {
                Log.Print("服务端连接成功");

                //发送hi到服务端
                socket.EmitToSocketIOServer("hi");//同步

                socket.EmitToSocketIOServerAsync("hi", (bool res) =>
                 {
                     Log.Print("异步并行发送" + (res ? "成功" : "失败"));
                 });//异步并行

                var result = await socket.EmitToSocketIOServerAsync("hi");
                Log.Print("纯异步发送" + (result ? "成功" : "失败"));//纯异步
            });

            socket.OnDisconnect((e) =>
            {
                Log.Print("服务端连接关闭");
            });

            //此方法不阻塞
            socket.Connect();

            //断线重连事件
            socket.OnReconnect(() =>
            {
                Log.Print("断开连接后重连成功");
            });

            //监听hi_back事件
            socket.SocketIO_On("hi_back", (e) =>
            {
                Log.Print("这里只会收到hi_back这个字符串，作为SocketIOEvent.Name，而这个事件的data为空");
                Log.Print($"{e.name}: {e.data}");
                Log.Print($"当前在线程：{System.Threading.Thread.CurrentThread.ManagedThreadId}");
            });

            //监听simulate_auth事件
            socket.SocketIO_On("simulate_auth", (e) =>
            {
                Log.Print("这里SocketIOEvent.Name是simulate_auth，而这个事件的data是js服务器发来的json数据");
                Log.Print($"{e.name}: {e.data.ToString()}");
                Log.Print("解析JSONData也很简单");
                var jsonData = e.data;
                Log.Print($"服务端发来的数据的json的keys是：{string.Join(",", jsonData.keys)}");
                Log.Print($"服务端发来的数据的id是：{jsonData["id"]}");
                Log.Print($"服务端发来的数据的name是：{jsonData["name"]}");
                Log.Print($"当前在线程：{System.Threading.Thread.CurrentThread.ManagedThreadId}");
            });
        }
    }
}