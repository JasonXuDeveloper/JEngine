namespace Nino.Serialization
{
    /// <summary>
    /// Compress type when serializing and deserializing
    /// 序列化或反序列化时的压缩类型
    /// </summary>
    public enum CompressType : byte
    {
        /// <summary>
        /// A number has a length of 0 to 255 (byte) 8 bit
        /// 一个在0到255之间的数字
        /// </summary>
        Byte = 10,

        /// <summary>
        /// A number has a length of -128 to 127 (sbyte) 8 bit
        /// 一个在-128到127之间的数字
        /// </summary>
        SByte = 11,

        /// <summary>
        /// A number has a length of -32,768 to 32,767 (short) 16 bit
        /// 一个在-32,768到32,767之间的数字
        /// </summary>
        Int16 = 12,

        /// <summary>
        /// A number has a length of 0 to 65,535 (ushort) 16 bit
        /// 一个在0到65536之间的数字
        /// </summary>
        UInt16 = 13,

        /// <summary>
        /// A number has a length of -2,147,483,648 to 2,147,483,647 (int) 32 bit
        /// 一个在-2,147,483,648到2,147,483,647之间的数字
        /// </summary>
        Int32 = 14,

        /// <summary>
        /// A number has a length of 0 to 4,294,967,295 (uint) 32 bit
        /// 一个在0到4,294,967,295之间的数字
        /// </summary>
        UInt32 = 15,

        /// <summary>
        /// A number has a length of -9,223,372,036,854,775,808 to 9,223,372,036,854,775,807 (long) 64 bit
        /// 一个在-9,223,372,036,854,775,808到9,223,372,036,854,775,807之间的数字
        /// </summary>
        Int64 = 16,

        /// <summary>
        /// A number has a length of 0 to 18,446,744,073,709,551,615 (ulong) 64 bit
        /// 一个在0到18,446,744,073,709,551,615之间的数字
        /// </summary>
        UInt64 = 17,
    }
}