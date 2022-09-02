using Nino.Shared.IO;

namespace Nino.Serialization
{
    public abstract class NinoWrapperBase<T> : INinoWrapper<T>, INinoWrapper
    {
        public abstract void Serialize(T val, Writer writer);
        public abstract T Deserialize(Reader reader);

        public void Serialize(object val, Writer writer)
        {
            Serialize((T)val, writer);
        }
        
        object INinoWrapper.Deserialize(Reader reader)
        {
            return Deserialize(reader);
        }
    }
}