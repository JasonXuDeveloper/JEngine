using System;

namespace JEngine.Editor
{
    [AttributeUsage(AttributeTargets.Method)]
    public class SerializeTypeMethod : Attribute
    {
        public int Priority;

        public SerializeTypeMethod(int priority)
        {
            Priority = priority;
        }
    }
}