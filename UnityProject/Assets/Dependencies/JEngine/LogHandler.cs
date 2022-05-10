using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace JEngine.Core
{
    internal class Logger : ILogger
    {
        private const string kNoTagFormat = "{0}";
        private const string kTagFormat = "{0}: {1}";

        private Logger()
        {}

        public Logger(ILogHandler logHandler)
        {
            this.logHandler = logHandler;
            this.logEnabled = true;
            this.filterLogType = LogType.Log;
        }

        public ILogHandler logHandler { get; set; }

        public bool logEnabled { get; set; }

        public LogType filterLogType { get; set; }

        public bool IsLogTypeAllowed(LogType logType)
        {
            if (logEnabled)
            {
                if (logType == LogType.Exception)
                    return true;

                if (filterLogType != LogType.Exception)
                    return (logType <= filterLogType);
            }
            return false;
        }

        private static string GetString(object message)
        {
            if (message == null)
            {
                return "Null";
            }
            var formattable = message as IFormattable;
            if (formattable != null)
            {
                return formattable.ToString(null, CultureInfo.InvariantCulture);
            }
            else
            {
                return message.ToString();
            }
        }

        public void Log(LogType logType, object message)
        {
            if (IsLogTypeAllowed(logType))
                logHandler.LogFormat(logType, null, kNoTagFormat, new object[] {GetString(message)});
        }

        public void Log(LogType logType, object message, Object context)
        {
            if (IsLogTypeAllowed(logType))
                logHandler.LogFormat(logType, context, kNoTagFormat, new object[] {GetString(message)});
        }

        public void Log(LogType logType, string tag, object message)
        {
            if (IsLogTypeAllowed(logType))
                logHandler.LogFormat(logType, null, kTagFormat, new object[] {tag, GetString(message)});
        }

        public void Log(LogType logType, string tag, object message, Object context)
        {
            if (IsLogTypeAllowed(logType))
                logHandler.LogFormat(logType, context, kTagFormat, new object[] {tag, GetString(message)});
        }

        public void Log(object message)
        {
            if (IsLogTypeAllowed(LogType.Log))
                logHandler.LogFormat(LogType.Log, null, kNoTagFormat, new object[] {GetString(message)});
        }

        public void Log(string tag, object message)
        {
            if (IsLogTypeAllowed(LogType.Log))
                logHandler.LogFormat(LogType.Log, null, kTagFormat, new object[] {tag, GetString(message)});
        }

        public void Log(string tag, object message, Object context)
        {
            if (IsLogTypeAllowed(LogType.Log))
                logHandler.LogFormat(LogType.Log, context, kTagFormat, new object[] {tag, GetString(message)});
        }

        public void LogWarning(string tag, object message)
        {
            if (IsLogTypeAllowed(LogType.Warning))
                logHandler.LogFormat(LogType.Warning, null, kTagFormat, new object[] {tag, GetString(message)});
        }

        public void LogWarning(string tag, object message, Object context)
        {
            if (IsLogTypeAllowed(LogType.Warning))
                logHandler.LogFormat(LogType.Warning, context, kTagFormat, new object[] {tag, GetString(message)});
        }

        public void LogError(string tag, object message)
        {
            if (IsLogTypeAllowed(LogType.Error))
                logHandler.LogFormat(LogType.Error, null, kTagFormat, new object[] {tag, GetString(message)});
        }

        public void LogError(string tag, object message, Object context)
        {
            if (IsLogTypeAllowed(LogType.Error))
                logHandler.LogFormat(LogType.Error, context, kTagFormat, new object[] {tag, GetString(message)});
        }

        public void LogException(Exception exception)
        {
            if (logEnabled)
            {
                exception = exception.Demystify();
                var d = exception.Data["StackTrace"];
                if (d != null)
                {
                    string s = GetAllExceptionStackTrace(exception);
                    //能反射就反射
                    if (_stackTraceString != null)
                    {
                        SetStackTracesString(exception,
                            $"==========ILRuntime StackTrace==========\n{s}\n\n==========Normal StackTrace=========\n{exception.StackTrace}");
                    }
                    //不能反射就额外打个Log
                    else
                    {
                        Debug.LogError($"下面的报错的额外信息：\n==========ILRuntime StackTrace==========\n{s}");
                    }
                }
                logHandler.LogException(exception, null);
            }
        }

        public void LogException(Exception exception, Object context)
        {
            if (logEnabled)
            {
                exception = exception.Demystify();
                var d = exception.Data["StackTrace"];
                if (d != null)
                {
                    string s = GetAllExceptionStackTrace(exception);
                    //能反射就反射
                    if (_stackTraceString != null)
                    {
                        SetStackTracesString(exception,
                            $"==========ILRuntime StackTrace==========\n{s}\n\n==========Normal StackTrace=========\n{exception.StackTrace}");
                    }
                    //不能反射就额外打个Log
                    else
                    {
                        Debug.LogError($"下面的报错的额外信息：\n==========ILRuntime StackTrace==========\n{s}");
                    }
                }
                logHandler.LogException(exception, context);
            }
        }

        /// <summary>
        /// 获取全部堆栈信息
        /// </summary>
        /// <param name="exception"></param>
        /// <returns></returns>
        private string GetAllExceptionStackTrace(Exception exception)
        {
            Exception temp = exception;
            List<Exception> all = new List<Exception>();
            int depth = 20;//深度20层
            while (depth-- > 0 && temp != null && temp.Data["StackTrace"] != null)
            {
                all.Add(temp);
                temp = temp != exception.InnerException ? exception.InnerException : null;//inner是自己就好退出了
            }
            //把最底层的放最外面
            all.Reverse();
            return string.Join("\n\n", all.Select(e => e.Data["StackTrace"]).ToList().FindAll(s => s != null));
        }

        public void LogFormat(LogType logType, string format, params object[] args)
        {
            if (IsLogTypeAllowed(logType))
                logHandler.LogFormat(logType, null, format, args);
        }

        public void LogFormat(LogType logType, Object context, string format, params object[] args)
        {
            if (IsLogTypeAllowed(logType))
                logHandler.LogFormat(logType, context, format, args);
        }
        
        private readonly FieldInfo _stackTraceString = typeof (Exception).GetField("_stackTraceString", BindingFlags.Instance | BindingFlags.NonPublic);

        private void SetStackTracesString(Exception exception, string value)
        {
            if (_stackTraceString != null)
            {
                _stackTraceString.SetValue((object)exception, (object)value);
            }
        }
    }
}