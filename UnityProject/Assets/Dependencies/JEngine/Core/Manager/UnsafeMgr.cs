using System;
using System.Runtime.InteropServices;

namespace JEngine.Core
{
    [StructLayout(LayoutKind.Explicit)]
    public unsafe class UnsafeMgr
    {
        /// <summary>
        /// Unsafe Mgr instance
        /// </summary>
        public static UnsafeMgr Instance = new UnsafeMgr();

        /// <summary>
        /// Get pointer of object (will not pin)
        /// </summary>
        /// <param name="obj"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public void* GetPtr<T>(T obj) => objectToVoidPtr(obj);
        
        /// <summary>
        /// Get object from pointer
        /// </summary>
        /// <param name="ptr"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T FromPtr<T>(void* ptr) => (T)voidPtrToObject(ptr);

        private delegate void* ObjectToVoidPtr(object obj);

        [FieldOffset(0)] private ObjectToVoidPtr objectToVoidPtr;
        [FieldOffset(0)] Func<object, object> func;

        private delegate object VoidPtrToObject(void* ptr);

        [FieldOffset(0)] private VoidPtrToObject voidPtrToObject;
        [FieldOffset(0)] Func<object, object> func2;

        private UnsafeMgr()
        {
            func = Out;
            func2 = Out;
        }

        private object Out(object o)
        {
            return o;
        }
    }
}