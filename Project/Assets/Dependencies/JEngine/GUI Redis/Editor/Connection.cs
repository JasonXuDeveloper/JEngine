//
// Connection.cs
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
using UnityEngine;
using Renci.SshNet;
using System;
using Renci.SshNet.Common;
using ServiceStack.Redis;
using JEngine.Core;

namespace JEngine.Redis
{
    public class Connection
    {
        public string SQL_IP = "127.0.0.1";//Redis 数据库IP
        public uint SQL_Port = 6379;//Redis 数据库端口
        public string SQL_Password = "your_db_password";//Redis 数据库密码
        public int SQL_DB = 0;//数据库

        public bool Debug;//调试模式

        #region SSH连接部分
        public bool ConnectThroughSSH = true;
        public string SSH_Host = "127.0.0.1";   //SSH ip 地址
        public int SSH_Port = 22;    //SSH 端口（一般情况下都是22端口）
        public string SSH_User = "root";    //SSH 用户
        public string SSH_Password = "your_password";    //SSH 密码
        #endregion

        /// <summary>
        /// 连接Redis，执行success，错误回调error
        /// </summary>
        /// <param name="success"></param>
        /// <param name="error"></param>
        public void Connect(Action<RedisClient> success = null, Action error = null)
        {
            //如果通过SSH连接Redis
            if (ConnectThroughSSH)
            {
                SSH_Connect(success, error);
            }
            else
            {
                Direct_Connect();
            }
        }

        /// <summary>
        /// 直连
        /// </summary>
        private void Direct_Connect(Action<RedisClient> success = null, Action error = null)
        {
            //开启计时器
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();
            var redis = new RedisClient(SQL_IP, (int)SQL_Port, SQL_Password, SQL_DB);
            try
            {
                redis.GetRandomKey();
                stopwatch.Stop();

                if (Debug)
                {
                    if (RedisWindow.Language == Language.中文)
                    {
                        Log.Print("Redis连接成功 (耗时" + stopwatch.ElapsedMilliseconds + "ms)");
                    }
                    else
                    {
                        Log.Print("Connected Redis Successfully (Spent " + stopwatch.ElapsedMilliseconds + "ms)");
                    }
                }


                //执行
                if (success != null)
                {
                    stopwatch.Reset();
                    stopwatch.Start();
                    success(redis);
                    stopwatch.Stop();
                    if (Debug)
                    {
                        if (RedisWindow.Language == Language.中文)
                        {
                            Log.PrintWarning("任务完成(耗时" + stopwatch.ElapsedMilliseconds + "ms)");
                        }
                        else
                        {
                            Log.PrintWarning("Task Completed (Spent " + stopwatch.ElapsedMilliseconds + "ms)");
                        }
                    }
                }

            }
            catch (Exception e)
            {
                if (RedisWindow.Language == Language.中文)
                {
                    Log.PrintError("无法连接Redis: " + e.Message);
                }
                else
                {
                    Log.PrintError("Unable to connect to Redis: " + e.Message);
                }
                error?.Invoke();
            }
        }


        /// <summary>
        /// SSH 连接（推荐）
        /// </summary>
        private void SSH_Connect(Action<RedisClient> success = null, Action error = null)
        {
            //开启计时器
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();

            //创建SSH连接
            using (var client = new SshClient(SSH_Host, SSH_Port, SSH_User, SSH_Password))
            {
                try
                {
                    client.HostKeyReceived += delegate (object sender, HostKeyEventArgs e) { e.CanTrust = true; };
                    client.Connect();
                }
                catch (Exception e)
                {
                    if (RedisWindow.Language == Language.中文)
                    {
                        Log.PrintWarning("无法通过SSH连接服务器: " + e.Message);
                    }
                    else
                    {
                        Log.PrintWarning("Unable to connect to SSH: " + e.Message);
                    }
                    error?.Invoke();
                    return;
                }

                if (!client.IsConnected)
                {
                    if (RedisWindow.Language == Language.中文)
                    {
                        Log.PrintWarning("无法通过SSH连接服务器，请确保端口开放");
                    }
                    else
                    {
                        Log.PrintWarning("Unable to connect to SSH, make sure the port is avaliable");
                    }
                    error?.Invoke();
                    return;
                }

                stopwatch.Stop();
                if (Debug)
                {
                    if (RedisWindow.Language == Language.中文)
                    {
                        Log.Print("SSH连接成功 (耗时" + stopwatch.ElapsedMilliseconds + "ms)");
                    }
                    else
                    {
                        Log.Print("Successfully connected SSH (Spent " + stopwatch.ElapsedMilliseconds + "ms)");
                    }
                }

                var port = new ForwardedPortLocal("127.0.0.1", SQL_Port, "127.0.0.1", SQL_Port);
                client.AddForwardedPort(port);
                port.Start();

                stopwatch.Reset();
                stopwatch.Start();
                var redis = new RedisClient(SQL_IP, (int)SQL_Port, SQL_Password, SQL_DB);
                try
                {
                    redis.GetRandomKey();
                    stopwatch.Stop();

                    if (Debug)
                    {
                        if (RedisWindow.Language == Language.中文)
                        {
                            Log.Print("Redis连接成功 (耗时" + stopwatch.ElapsedMilliseconds + "ms)");
                        }
                        else
                        {
                            Log.Print("Connected Redis Successfully (Spent " + stopwatch.ElapsedMilliseconds + "ms)");
                        }
                    }


                    //执行
                    if (success != null)
                    {
                        stopwatch.Reset();
                        stopwatch.Start();
                        success(redis);
                        stopwatch.Stop();
                        if (Debug)
                        {
                            if (RedisWindow.Language == Language.中文)
                            {
                                Log.PrintWarning("任务完成(耗时" + stopwatch.ElapsedMilliseconds + "ms)");
                            }
                            else
                            {
                                Log.PrintWarning("Task Completed (Spent " + stopwatch.ElapsedMilliseconds + "ms)");
                            }
                        }
                    }

                }
                catch (Exception e)
                {
                    if (RedisWindow.Language == Language.中文)
                    {
                        Log.PrintError("无法连接Redis: " + e.Message);
                    }
                    else
                    {
                        Log.PrintError("Unable to connect to Redis: " + e.Message);
                    }
                    error?.Invoke();
                }
            }
        }
    }
}