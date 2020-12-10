//
// JWebSocket.cs
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
using System.Threading.Tasks;
using JEngine.Core;
using UnityEngine;
using WebSocketSharp;

namespace JEngine.Net
{
    public class JWebSocket
    {
        private static GameObject mgr;

        private SocketIOComponent socket;

        public bool Connected => socket.IsConnected;

        private bool hasConnected = false;
        private Action onReconnect;

        /// <summary>
        /// 连接websokcet服务器，如果使用的socket-io，请给isSocketIO参数设置为true（socketio是nodejs服务器使用的一个websocket插件）
        /// </summary>
        /// <param name="url"></param>
        /// <param name="config"></param>
        /// <param name="isSocketIO"></param>
        public JWebSocket(string url,JSocketConfig config = null,Action<object,MessageEventArgs> OnMessage = null)
        {
            if (mgr == null)
            {
                mgr = new GameObject("JEngine.Net");
                UnityEngine.Object.DontDestroyOnLoad(mgr);
            }

            socket = mgr.AddComponent<SocketIOComponent>();

            socket.Init(url, config, OnMessage);
        }

        /// <summary>
        /// Connect to server, this method doesn't block the context
        /// 连接服务器，此方法不阻塞
        /// </summary>
        public void Connect()
        {
            socket.Connect();
        }

        public void OnOpen(Action<SocketIOEvent> socketIOEvent)
        {
            socket.On(socket.config.eventOpenName, socketIOEvent);
        }

        public void OnConnect(Action<SocketIOEvent> socketIOEvent)
        {
            Action<SocketIOEvent> todo = (e) =>
            {
                socketIOEvent(e);
                if (!hasConnected)
                {
                    hasConnected = true;
                }
                else
                {
                    onReconnect?.Invoke();
                }
            };
            socket.On(socket.config.eventConnectName, todo);
        }

        public void OnReconnect(Action socketIOEvent)
        {
            onReconnect = socketIOEvent;
        }

        public void OnDisconnect(Action<SocketIOEvent> socketIOEvent)
        {
            socket.On(socket.config.eventDisconnectName, socketIOEvent);
        }

        public void OnClose(Action<SocketIOEvent> socketIOEvent)
        {
            socket.On(socket.config.eventCloseName, socketIOEvent);
        }

        public void OnError(Action<SocketIOEvent> socketIOEvent)
        {
            socket.On(socket.config.eventErrorName, socketIOEvent);
        }

        public void SocketIO_On(string eventName, Action<SocketIOEvent> socketIOEvent)
        {
            socket.On(eventName, socketIOEvent);
        }

        public void SocketIO_Off(string eventName, Action<SocketIOEvent> socketIOEvent)
        {
            socket.Off(eventName, socketIOEvent);
        }

        public void SendToServer(string Data)
        {
            socket.socket.Send(Data);
        }

        public void SendToServerAsJson<T>(T Data)
        {
            socket.socket.Send(StringifyHelper.JSONSerliaze(Data));
        }

        public void SendToServerAsProtobuf<T>(T Data) where T : class
        {
            socket.socket.Send(StringifyHelper.ProtoSerialize(Data));
        }

        public void SendToServer(byte[] Data)
        {
            socket.socket.Send(Data);
        }

        #region EMIT TO SOCKET IO

        /// <summary>
        /// Call event on server (only for socket-io servers, eg. nodeJS servers that uses socket-io)
        /// 调用服务端事件（只有使用socket-io搭建的服务器才能使用该接口，比如nodeJS使用socket-io的服务器）
        /// </summary>
        /// <param name="ev"></param>
        public void EmitToSocketIOServer(string ev)
        {
            socket.Emit(ev);
        }

        /// <summary>
        /// Call event on server (only for socket-io servers, eg. nodeJS servers that uses socket-io)
        /// 调用服务端事件（只有使用socket-io搭建的服务器才能使用该接口，比如nodeJS使用socket-io的服务器）
        /// </summary>
        /// <param name="ev"></param>
        /// <param name="onComplete">异步并行回调</param>
        public void EmitToSocketIOServerAsync(string ev, Action<bool> onComplete)
        {
            socket.EmitAsync(ev, onComplete);
        }

        /// <summary>
        /// Call event on server (only for socket-io servers, eg. nodeJS servers that uses socket-io)
        /// 调用服务端事件（只有使用socket-io搭建的服务器才能使用该接口，比如nodeJS使用socket-io的服务器）
        /// </summary>
        /// <param name="ev"></param>
        /// /// <returns>发送结果 emit result</returns>
        public async Task<bool> EmitToSocketIOServerAsync(string ev)
        {
            return await socket.EmitAsync(ev);
        }

        /// <summary>
        /// Call event on server with a call back (only for socket-io servers, eg. nodeJS servers that uses socket-io)
        /// 调用服务端事件，并包含一个回调（只有使用socket-io搭建的服务器才能使用该接口，比如nodeJS使用socket-io的服务器）
        /// </summary>
        /// <param name="ev"></param>
        /// <param name="action">回调 callback</param>
        public void EmitToSocketIOServer(string ev, Action<JSONObject> action)
        {
            socket.Emit(ev, action);
        }

        /// <summary>
        /// Call event on server with a call back (only for socket-io servers, eg. nodeJS servers that uses socket-io)
        /// 调用服务端事件，并包含一个回调（只有使用socket-io搭建的服务器才能使用该接口，比如nodeJS使用socket-io的服务器）
        /// </summary>
        /// <param name="ev"></param>
        /// <param name="action">回调 callback</param>
        /// <param name="onComplete">异步并行回调</param>
        public void EmitToSocketIOServerAsync(string ev, Action<JSONObject> action, Action<bool> onComplete)
        {
            socket.EmitAsync(ev, action, onComplete);
        }

        /// <summary>
        /// Call event on server with a call back (only for socket-io servers, eg. nodeJS servers that uses socket-io)
        /// 调用服务端事件，并包含一个回调（只有使用socket-io搭建的服务器才能使用该接口，比如nodeJS使用socket-io的服务器）
        /// </summary>
        /// <param name="ev"></param>
        /// <param name="action">回调 callback</param>
        /// <returns>发送结果 emit result</returns>
        public async Task<bool> EmitToSocketIOServerAsync(string ev, Action<JSONObject> action)
        {
            return await socket.EmitAsync(ev, action);
        }

        /// <summary>
        /// Call event on server with a string (only for socket-io servers, eg. nodeJS servers that uses socket-io)
        /// 调用服务端事件，并发送个字符串（只有使用socket-io搭建的服务器才能使用该接口，比如nodeJS使用socket-io的服务器）
        /// </summary>
        /// <param name="ev">事件名字 event name</param>
        /// <param name="str">字符串 string</param>
        public void EmitToSocketIOServer(string ev, string str)
        {
            socket.Emit(ev, str);
        }

        /// <summary>
        /// Call event on server (only for socket-io servers, eg. nodeJS servers that uses socket-io)
        /// 调用服务端事件，并发送个字符串（只有使用socket-io搭建的服务器才能使用该接口，比如nodeJS使用socket-io的服务器）
        /// </summary>
        /// <param name="ev">事件名字 event name</param>
        /// /// <param name="str">字符串 string</param>
        /// <param name="onComplete">异步并行回调</param>
        public void EmitToSocketIOServerAsync(string ev, string str, Action<bool> onComplete)
        {
            socket.EmitAsync(ev,str, onComplete);
        }

        /// <summary>
        /// Call event on server (only for socket-io servers, eg. nodeJS servers that uses socket-io)
        /// 调用服务端事件，并发送个字符串（只有使用socket-io搭建的服务器才能使用该接口，比如nodeJS使用socket-io的服务器）
        /// </summary>
        /// <param name="ev">事件名字 event name</param>
        /// /// <param name="str">字符串 string</param>
        /// <returns>发送结果 emit result</returns>
        public async Task<bool> EmitToSocketIOServerAsync(string ev, string str)
        {
            return await socket.EmitAsync(ev,str);
        }

        /// <summary>
        /// Call event on server with a json data (only for socket-io servers, eg. nodeJS servers that uses socket-io)
        /// 调用服务端事件，并发送个JSON数据（只有使用socket-io搭建的服务器才能使用该接口，比如nodeJS使用socket-io的服务器）
        /// </summary>
        /// <param name="ev">事件名字 event name</param>
        /// <param name="data">json数据 json data</param>
        public void EmitToSocketIOServer(string ev, JSONObject data)
        {
            socket.Emit(ev, data);
        }

        /// <summary>
        /// Call event on server (only for socket-io servers, eg. nodeJS servers that uses socket-io)
        /// 调用服务端事件，并发送个JSON数据（只有使用socket-io搭建的服务器才能使用该接口，比如nodeJS使用socket-io的服务器）
        /// </summary>
        /// <param name="ev">事件名字 event name</param>
        /// <param name="data">json数据 json data</param>
        /// <param name="onComplete">异步并行回调</param>
        public void EmitToSocketIOServerAsync(string ev, JSONObject data, Action<bool> onComplete)
        {
            socket.EmitAsync(ev, data, onComplete);
        }

        /// <summary>
        /// Call event on server (only for socket-io servers, eg. nodeJS servers that uses socket-io)
        /// 调用服务端事件，并发送个JSON数据（只有使用socket-io搭建的服务器才能使用该接口，比如nodeJS使用socket-io的服务器）
        /// </summary>
        /// <param name="ev">事件名字 event name</param>
        /// <param name="data">json数据 json data</param>
        /// <param name="action">回调 callback</param>
        /// <returns>发送结果 emit result</returns>
        public async Task<bool> EmitToSocketIOServerAsync(string ev, JSONObject data)
        {
            return await socket.EmitAsync(ev, data);
        }

        /// <summary>
        /// Call event on server with a json data and a callback(only for socket-io servers, eg. nodeJS servers that uses socket-io)
        /// 调用服务端事件，发送JSON数据且包含回调（只有使用socket-io搭建的服务器才能使用该接口，比如nodeJS使用socket-io的服务器）
        /// </summary>
        /// <param name="ev">事件名字 event name</param>
        /// <param name="data">json数据 json data</param>
        /// <param name="action">回调 callback</param>
        public void EmitToSocketIOServer(string ev, JSONObject data, Action<JSONObject> action)
        {
            socket.Emit(ev, data, action);
        }

        /// <summary>
        /// Call event on server (only for socket-io servers, eg. nodeJS servers that uses socket-io)
        /// 调用服务端事件，发送JSON数据且包含回调（只有使用socket-io搭建的服务器才能使用该接口，比如nodeJS使用socket-io的服务器）
        /// </summary>
        /// <param name="ev">事件名字 event name</param>
        /// <param name="data">json数据 json data</param>
        /// <param name="action">回调 callback</param>
        /// <param name="onComplete">异步并行回调</param>
        public void EmitToSocketIOServerAsync(string ev, JSONObject data, Action<JSONObject> action, Action<bool> onComplete)
        {
            socket.EmitAsync(ev,data,action, onComplete);
        }

        /// <summary>
        /// Call event on server (only for socket-io servers, eg. nodeJS servers that uses socket-io)
        /// 调用服务端事件，发送JSON数据且包含回调（只有使用socket-io搭建的服务器才能使用该接口，比如nodeJS使用socket-io的服务器）
        /// </summary>
        /// <param name="ev">事件名字 event name</param>
        /// <param name="data">json数据 json data</param>
        /// <param name="action">回调 callback</param>
        /// <returns>发送结果 emit result</returns>
        public async Task<bool> EmitToSocketIOServerAsync(string ev, JSONObject data, Action<JSONObject> action)
        {
            return await socket.EmitAsync(ev,data,action);
        }

        #endregion
    }
}