using System;
using System.ComponentModel;

namespace JEngine.Core
{
    public static partial class Tools
    {
        /// <summary>
        /// 将对象转换为特定类型
        /// </summary>
        /// <param name="value"></param>
        /// <param name="destinationType"></param>
        /// <returns></returns>
        public static object ConvertSimpleType(this object value, Type destinationType)
        {
            object returnValue;
            if (value == null || destinationType.IsInstanceOfType(value))
            {
                return value;
            }

            if (value is string str && str.Length == 0)
            {
                return destinationType.IsValueType ? Activator.CreateInstance(destinationType) : null;
            }

            TypeConverter converter = TypeDescriptor.GetConverter(destinationType);
            bool flag = converter.CanConvertFrom(value.GetType());
            if (!flag)
            {
                converter = TypeDescriptor.GetConverter(value.GetType());
            }

            if (!flag && !converter.CanConvertTo(destinationType))
            {
                Log.PrintError("无法转换成类型：'" + value + "' ==> " + destinationType);
            }

            try
            {
                returnValue = flag
                    ? converter.ConvertFrom(null, null, value)
                    : converter.ConvertTo(null, null, value, destinationType);
            }
            catch (Exception e)
            {
                Log.PrintError("类型转换出错：'" + value + "' ==> " + destinationType + "\n" + e.Message);
                returnValue = destinationType.IsValueType ? Activator.CreateInstance(destinationType) : null;
            }

            return returnValue;
        }
    }
}