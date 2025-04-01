using System.IO;
using System.Security.Cryptography;

namespace FileHashManager
{
    public class HashCalculator
    {
        public static async ValueTask<byte[]> ComputeMd5Async(Stream stream)
        {
            using var md5 = MD5.Create();
            return await md5.ComputeHashAsync(stream); // .NET 5+ 提供的异步方法
        }
        public static async Task<byte[]> ComputeFileMd5Async(string filePath)
        {
            // 模拟处理大文件的随机耗时操作
            //Random ran = new();
            //int n = ran.Next(100, 2000);
            //Task.Delay(n).Wait(); 

            await using var stream = File.OpenRead(filePath); // 异步打开文件
            return await ComputeMd5Async(stream); // 调用底层方法
        }
    }
}
