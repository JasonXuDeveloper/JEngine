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
        public void* GetPtr<T>(T obj) => _objectToVoidPtr(obj);
        
        /// <summary>
        /// Get object from pointer
        /// </summary>
        /// <param name="ptr"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T FromPtr<T>(void* ptr) => (T)_voidPtrToObject(ptr);

        private delegate void* ObjectToVoidPtrDelegate(object obj);
        private delegate object VoidPtrToObjectDelegate(void* ptr);
        [FieldOffset(0)] private readonly ObjectToVoidPtrDelegate _objectToVoidPtr;
        [FieldOffset(0)] private readonly VoidPtrToObjectDelegate _voidPtrToObject;
        [FieldOffset(0)] Func<object, object> func;

        private UnsafeMgr()
        {
            _objectToVoidPtr = null;
            _voidPtrToObject = null;
            func = Out;
        }

        private static object Out(object o) => o;
    }
}