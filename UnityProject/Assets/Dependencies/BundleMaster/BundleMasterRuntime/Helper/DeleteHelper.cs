using System;
using System.IO;

namespace BM
{
    public static class DeleteHelper
    {
        public static void DeleteDir(string srcPath)
        {
            try
            {
                DirectoryInfo buildBundlePath = new DirectoryInfo(srcPath);
                FileSystemInfo[] fileSystemInfos = buildBundlePath.GetFileSystemInfos();
                foreach (FileSystemInfo fileSystemInfo in fileSystemInfos)
                {
                    if (fileSystemInfo is DirectoryInfo)
                    {
                        DirectoryInfo subDir = new DirectoryInfo(fileSystemInfo.FullName);
                        subDir.Delete(true);
                    }
                    else
                    {
                        File.Delete(fileSystemInfo.FullName);
                    }
                }
            }
            catch (Exception e)
            {
                AssetLogHelper.LogError(e.ToString());
                throw;
            }
        }
    }
}