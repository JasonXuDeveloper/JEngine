namespace BM
{
    public static class HandlerIdHelper
    {
        private static uint _handlerIdCounter = 0;

        /// <summary>
        /// 获取唯一ID
        /// </summary>
        /// <returns></returns>
        public static uint GetUniqueId()
        {
            _handlerIdCounter++;
            return _handlerIdCounter;
        }
        
    }
}