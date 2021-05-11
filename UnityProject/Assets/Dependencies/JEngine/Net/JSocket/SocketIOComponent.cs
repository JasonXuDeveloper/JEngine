//
// SocketIOComponent.cs
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

#define SOCKET_IO_DEBUG			// Uncomment this for debug
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JEngine.Core;
using UnityEngine;
using WebSocketSharp;

namespace JEngine.Net
{
	public class JSocketConfig
	{
		public bool debug = false;
		public bool autoConnect = false;
		public int reconnectDelay = 5;
		public float ackExpirationTime = 30f;
		public float pingInterval = 25f;
		public float pingTimeout = 60f;
		public string eventOpenName = "open";
		public string eventConnectName = "connect";
		public string eventDisconnectName = "disconnect";
		public string eventErrorName = "error";
		public string eventCloseName = "close";

		public static JSocketConfig Default()
		{
			return new JSocketConfig();
		}
	}

	public class SocketIOComponent:MonoBehaviour
	{
		#region Public Properties

		public string url; //ws://127.0.0.1:4567/socket.io/?EIO=3&transport=websocket
		public JSocketConfig config;
		public EventHandler<MessageEventArgs> OnMessage;
		
		public WebSocket socket
		{
			get { return ws; }
		}

		public string sid { get; set; }

		public bool IsConnected
		{
			get { return connected; }
		}

		#endregion

		#region Private Properties

		private volatile bool connected;
		private volatile bool thPinging;
		private volatile bool thPong;
		private volatile bool wsConnected;

		private Thread socketThread;
		private Thread pingThread;
		private WebSocket ws;

		private Encoder encoder;
		public Decoder decoder;
		private Parser parser;

		private Dictionary<string, List<Action<SocketIOEvent>>> handlers;
		private List<Ack> ackList;

		private int packetId;

		private object eventQueueLock;
		private Queue<SocketIOEvent> eventQueue;

		private object ackQueueLock;
		private Queue<Packet> ackQueue;


		#endregion

#if SOCKET_IO_DEBUG
		public Action<string> debugMethod;
#endif

		#region Unity interface
		

		public void Init(string url,JSocketConfig config = null,Action<object,MessageEventArgs> onMessage=null)
		{
			if (config == null)
			{
				config = JSocketConfig.Default();
			}

			if (onMessage == null)
			{
				OnMessage = _OnMessage;
			}
			else
			{
				OnMessage = (sender, e) => onMessage(sender, e);
			}
			
			this.url = url;
			this.config = config;

			encoder = new Encoder();
			decoder = new Decoder();
			parser = new Parser();
			handlers = new Dictionary<string, List<Action<SocketIOEvent>>>();
			ackList = new List<Ack>();
			sid = null;
			packetId = 0;

			eventQueueLock = new object();
			eventQueue = new Queue<SocketIOEvent>();

			ackQueueLock = new object();
			ackQueue = new Queue<Packet>();

			connected = false;

#if SOCKET_IO_DEBUG
			if (debugMethod == null)
			{
				debugMethod = delegate(string s)
				{
					if (config.debug)
					{
						Log.Print(s);
					}
				};
			}
#endif

			if (config.autoConnect)
			{
				Connect();
			}
		}

		public void Update()
		{
			lock (eventQueueLock)
			{
				while (eventQueue.Count > 0)
				{
					EmitEvent(eventQueue.Dequeue());
				}
			}

			lock (ackQueueLock)
			{
				while (ackQueue.Count > 0)
				{
					InvokeAck(ackQueue.Dequeue());
				}
			}

			if (wsConnected != ws.IsConnected)
			{
				wsConnected = ws.IsConnected;
				if (wsConnected)
				{
					EmitEvent(config.eventConnectName);
				}
				else
				{
					EmitEvent(config.eventDisconnectName);
				}
			}

			// GC expired acks
			if (ackList.Count == 0)
			{
				return;
			}

			if (DateTime.Now.Subtract(ackList[0].time).TotalSeconds < config.ackExpirationTime)
			{
				return;
			}

			ackList.RemoveAt(0);
		}

		public void OnDestroy()
		{
			if (socketThread != null)
			{
				socketThread.Abort();
			}

			if (pingThread != null)
			{
				pingThread.Abort();
			}
		}

		public void OnApplicationQuit()
		{
			Close();
		}

		#endregion

		#region Public Interface

		public void SetHeader(string header, string value)
		{
			ws.SetHeader(header, value);
		}

		public void Connect()
		{
			ws = new WebSocket(url);
			ws.OnOpen += OnOpen;
			ws.OnError += OnError;
			ws.OnClose += OnClose;
			ws.OnMessage += OnMessage;
			wsConnected = false;

			connected = true;

			socketThread = new Thread(RunSocketThread);
			socketThread.Start(ws);

			pingThread = new Thread(RunPingThread);
			pingThread.Start(ws);
		}

		private void _OnMessage(object sender, MessageEventArgs e)
		{
			debugMethod.Invoke("[SocketIO] Raw message: " + e.Data);

			Packet packet = decoder.Decode(e);

			switch (packet.enginePacketType)
			{
				case EnginePacketType.OPEN:
					HandleOpen(packet);
					break;
				case EnginePacketType.CLOSE:
					EmitEvent(config.eventCloseName);
					break;
				case EnginePacketType.PING:
					HandlePing();
					break;
				case EnginePacketType.PONG:
					HandlePong();
					break;
				case EnginePacketType.MESSAGE:
					HandleMessage(packet);
					break;
			}
		}
		
		public void Close()
		{
			EmitClose();
			connected = false;
		}

		public void On(string ev, Action<SocketIOEvent> callback)
		{
			if (!handlers.ContainsKey(ev))
			{
				handlers[ev] = new List<Action<SocketIOEvent>>();
			}

			handlers[ev].Add(callback);
		}

		public void Off(string ev, Action<SocketIOEvent> callback)
		{
			if (!handlers.ContainsKey(ev))
			{
#if SOCKET_IO_DEBUG
				debugMethod.Invoke("[SocketIO] No callbacks registered for event: " + ev);
#endif
				return;
			}

			List<Action<SocketIOEvent>> l = handlers[ev];
			if (!l.Contains(callback))
			{
#if SOCKET_IO_DEBUG
				debugMethod.Invoke("[SocketIO] Couldn't remove callback action for event: " + ev);
#endif
				return;
			}

			l.Remove(callback);
			if (l.Count == 0)
			{
				handlers.Remove(ev);
			}
		}

		public void Emit(string ev)
		{
			EmitMessage(-1, string.Format("[\"{0}\"]", ev));
		}
		
		public void EmitAsync(string ev,Action<bool> onComplete)
		{
			EmitMessageAsync(-1, string.Format("[\"{0}\"]", ev),onComplete);
		}
		
		public async Task<bool> EmitAsync(string ev)
		{
			var result = await EmitMessageAsync(-1, string.Format("[\"{0}\"]", ev));
			return result;
		}

		public void Emit(string ev, Action<JSONObject> action)
		{
			EmitMessage(++packetId, string.Format("[\"{0}\"]", ev));
			ackList.Add(new Ack(packetId, action));
		}
		
		public void EmitAsync(string ev, Action<JSONObject> action,Action<bool> onComplete)
		{
			EmitMessageAsync(-1, string.Format("[\"{0}\"]", ev),onComplete);
		}
		
		public async Task<bool> EmitAsync(string ev, Action<JSONObject> action)
		{
			var result = await EmitMessageAsync(-1, string.Format("[\"{0}\"]", ev));
			return result;
		}

		public void Emit(string ev, string str)
		{
			EmitMessage(-1, string.Format("[\"{0}\",\"{1}\"]", ev, str));
		}
		
		public void EmitAsync(string ev, string str,Action<bool> onComplete)
		{
			EmitMessageAsync(-1, string.Format("[\"{0}\"]", ev),onComplete);
		}
		
		public async Task<bool> EmitAsync(string ev, string str)
		{
			var result = await EmitMessageAsync(-1, string.Format("[\"{0}\"]", ev));
			return result;
		}


		public void Emit(string ev, JSONObject data)
		{
			EmitMessage(-1, string.Format("[\"{0}\",{1}]", ev, data));
		}

		public void EmitAsync(string ev, JSONObject data,Action<bool> onComplete)
		{
			EmitMessageAsync(-1, string.Format("[\"{0}\"]", ev),onComplete);
		}
		
		public async Task<bool> EmitAsync(string ev, JSONObject data)
		{
			var result = await EmitMessageAsync(-1, string.Format("[\"{0}\"]", ev));
			return result;
		}

		
		public void Emit(string ev, JSONObject data, Action<JSONObject> action)
		{
			EmitMessage(++packetId, string.Format("[\"{0}\",{1}]", ev, data));
			ackList.Add(new Ack(packetId, action));
		}
		
		public void EmitAsync(string ev, JSONObject data, Action<JSONObject> action,Action<bool> onComplete)
		{
			EmitMessageAsync(-1, string.Format("[\"{0}\"]", ev),onComplete);
		}
		
		public async Task<bool> EmitAsync(string ev, JSONObject data, Action<JSONObject> action)
		{
			var result = await EmitMessageAsync(-1, string.Format("[\"{0}\"]", ev));
			return result;
		}


		#endregion

		#region Private Methods

		private async void RunSocketThread(object obj)
		{
			WebSocket webSocket = (WebSocket) obj;
			while (connected)
			{
				if (webSocket.IsConnected)
				{
					await Task.Delay(config.reconnectDelay);
				}
				else
				{
					webSocket.Connect();
				}
			}

			webSocket.Close();
		}

		private async void RunPingThread(object obj)
		{
			WebSocket webSocket = (WebSocket) obj;

			int timeoutMilis = Mathf.FloorToInt(config.pingTimeout * 1000);
			int intervalMilis = Mathf.FloorToInt(config.pingInterval * 1000);

			DateTime pingStart;

			while (connected)
			{
				if (!wsConnected)
				{
					await Task.Delay(
						config.reconnectDelay);
				}
				else
				{
					thPinging = true;
					thPong = false;

					EmitPacket(new Packet(EnginePacketType.PING));
					pingStart = DateTime.Now;

					while (webSocket.IsConnected && thPinging &&
					       (DateTime.Now.Subtract(pingStart).TotalSeconds < timeoutMilis))
					{
						await Task.Delay(200);
					}

					if (!thPong)
					{
						webSocket.Close();
					}

					await Task.Delay(intervalMilis);
				}
			}
		}

		private void EmitMessage(int id, string raw)
		{
			EmitPacket(new Packet(EnginePacketType.MESSAGE, SocketPacketType.EVENT, 0, "/", id, new JSONObject(raw)));
		}
		
		private void EmitMessageAsync(int id, string raw,Action<bool> onComplete)
		{
			EmitPacketAsync(
				new Packet(EnginePacketType.MESSAGE, SocketPacketType.EVENT, 0, "/", id, new JSONObject(raw)),
				onComplete);
		}
		
		private async Task<bool> EmitMessageAsync(int id, string raw)
		{
			var result =  await EmitPacketAsync(new Packet(EnginePacketType.MESSAGE, SocketPacketType.EVENT, 0, "/", id, new JSONObject(raw)));
			return result;
		}

		private void EmitClose()
		{
			EmitPacket(
				new Packet(EnginePacketType.MESSAGE, SocketPacketType.DISCONNECT, 0, "/", -1, new JSONObject("")));
			EmitPacket(new Packet(EnginePacketType.CLOSE));
		}

		private void EmitPacket(Packet packet)
		{
#if SOCKET_IO_DEBUG
			debugMethod.Invoke("[SocketIO] " + packet);
#endif

			try
			{
				ws.Send(encoder.Encode(packet));
			}
			catch (SocketIOException ex)
			{
#if SOCKET_IO_DEBUG
				debugMethod.Invoke(ex.ToString());
#endif
			}
		}
		
		private void EmitPacketAsync(Packet packet,Action<bool> onComplete)
		{
#if SOCKET_IO_DEBUG
			debugMethod.Invoke("[SocketIO] " + packet);
#endif

			try
			{
				ws.SendAsync(encoder.Encode(packet),onComplete);
			}
			catch (SocketIOException ex)
			{
#if SOCKET_IO_DEBUG
				debugMethod.Invoke(ex.ToString());
#endif
			}
		}
		
		private async Task<bool> EmitPacketAsync(Packet packet)
		{
#if SOCKET_IO_DEBUG
			debugMethod.Invoke("[SocketIO] " + packet);
#endif

			bool complete = false;
			bool result = false;
			Action<bool> onComplete = b =>
			{
				complete = true;
				result = b;
			};
			
			try
			{
				ws.SendAsync(encoder.Encode(packet),onComplete);
			}
			catch (SocketIOException ex)
			{
#if SOCKET_IO_DEBUG
				debugMethod.Invoke(ex.ToString());
#endif
			}

			while (wsConnected && !complete)
			{
				await Task.Delay(10);
			}

			return result;
		}

		private void OnOpen(object sender, EventArgs e)
		{
			EmitEvent(config.eventOpenName);
		}

		

		public void HandleOpen(Packet packet)
		{
#if SOCKET_IO_DEBUG
			debugMethod.Invoke("[SocketIO] Socket.IO sid: " + packet.json["sid"].str);
#endif
			sid = packet.json["sid"].str;
			EmitEvent(config.eventOpenName);
		}

		public void HandlePing()
		{
			EmitPacket(new Packet(EnginePacketType.PONG));
		}

		public void HandlePong()
		{
			thPong = true;
			thPinging = false;
		}

		public void HandleMessage(Packet packet)
		{
			if (packet.json == null)
			{
				return;
			}

			if (packet.socketPacketType == SocketPacketType.ACK)
			{
				for (int i = 0; i < ackList.Count; i++)
				{
					if (ackList[i].packetId != packet.id)
					{
						continue;
					}

					lock (ackQueueLock)
					{
						ackQueue.Enqueue(packet);
					}

					return;
				}

#if SOCKET_IO_DEBUG
				debugMethod.Invoke("[SocketIO] Ack received for invalid Action: " + packet.id);
#endif
			}

			if (packet.socketPacketType == SocketPacketType.EVENT)
			{
				SocketIOEvent e = parser.Parse(packet.json);
				lock (eventQueueLock)
				{
					eventQueue.Enqueue(e);
				}
			}
		}

		private void OnError(object sender, ErrorEventArgs e)
		{
			if(Application.isPlaying)
				EmitEvent(new SocketIOEvent(config.eventErrorName, JSONObject.CreateStringObject(e.Message)));
		}

		private void OnClose(object sender, CloseEventArgs e)
		{
			EmitEvent(config.eventCloseName);
		}

		public void EmitEvent(string type)
		{
			EmitEvent(new SocketIOEvent(type));
		}

		private void EmitEvent(SocketIOEvent ev)
		{
			if (!handlers.ContainsKey(ev.name))
			{
				return;
			}

			foreach (Action<SocketIOEvent> handler in handlers[ev.name])
			{
				try
				{
					handler(ev);
				}
				catch (Exception ex)
				{
#if SOCKET_IO_DEBUG
					debugMethod.Invoke(ex.ToString());
#endif
				}
			}
		}

		private void InvokeAck(Packet packet)
		{
			Ack ack;
			for (int i = 0; i < ackList.Count; i++)
			{
				if (ackList[i].packetId != packet.id)
				{
					continue;
				}

				ack = ackList[i];
				ackList.RemoveAt(i);
				ack.Invoke(packet.json);
				return;
			}
		}

		#endregion
	}
}