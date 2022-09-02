namespace Nino.Serialization
{
    /// <summary>
    /// Compress option
    /// 压缩类型
    /// </summary>
    public enum CompressOption : byte
    {
        /// <summary>
        /// zlib (gzip/deflate) compression (high compression ratio but low performance)
        /// zlib (gzip/deflate) 压缩模式 (高压缩率低性能)
        /// </summary>
        Zlib = 0,

        /// <summary>
        /// lz4 compression (average compression ration but high performance)
        /// lz4 压缩模式 (平均压缩率高性能)
        /// </summary>
        Lz4 = 1,

        /// <summary>
        /// no compression (very high performance but huge size)
        /// 无压缩 (高性能但体积很大)
        /// </summary>
        NoCompression = 2
    }
}