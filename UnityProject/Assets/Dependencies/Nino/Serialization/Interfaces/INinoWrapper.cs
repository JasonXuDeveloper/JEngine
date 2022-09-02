namespace Nino.Serialization
{
    public interface INinoWrapper<T>
    {
        void Serialize(T val, Writer writer);
        T Deserialize(Reader reader);
    }

    public interface INinoWrapper
    {
        void Serialize(object val, Writer writer);
        object Deserialize(Reader reader);
    }
}