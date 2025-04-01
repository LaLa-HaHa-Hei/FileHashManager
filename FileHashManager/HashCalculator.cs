using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Windows;
using System.Windows.Shapes;

namespace FileHashManager
{
    public class HashCalculator
    {
        public static async ValueTask<byte[]> ComputeMd5Async(Stream stream)
        {
            using var md5 = MD5.Create();
            return await md5.ComputeHashAsync(stream);
        }

        public static async Task<byte[]> ComputeFileMd5Async(string filePath)
        {
            await using var stream = File.OpenRead(filePath);
            return await ComputeMd5Async(stream);
        }

        //public static async Task<byte[]> ComputeFileMd5Async2(string filePath)
        //{
        //    // 并没有快多少
        //    var startInfo = new ProcessStartInfo
        //    {
        //        FileName = "certutil",
        //        Arguments = $"-hashfile \"{filePath}\" MD5",
        //        RedirectStandardOutput = true,
        //        UseShellExecute = false,
        //        CreateNoWindow = true
        //    };

        //    using var process = new Process { StartInfo = startInfo };
        //    process.Start();

        //    string output = await process.StandardOutput.ReadToEndAsync();
        //    await process.WaitForExitAsync();

        //    // 解析输出，获取第二行的MD5值
        //    var lines = output.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
        //    if (lines.Length < 2)
        //    {
        //        throw new InvalidOperationException("无法解析certutil输出");
        //    }

        //    string md5Hex = lines[1].Replace(" ", string.Empty);
        //    return Convert.FromHexString(md5Hex);
        //}
    }
}
