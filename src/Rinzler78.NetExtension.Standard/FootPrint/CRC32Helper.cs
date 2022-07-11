using System;
using System.Linq;
using ICSharpCode.SharpZipLib.Checksum;
using Rinzler78.NetExtension.Array;

namespace Rinzler78.NetExtension.FootPrint
{
    public static class CRC32Helper
    {
        public static uint GenerateCRC32(this ulong[] data)
        {
            var tool = new Crc32();

            tool.Reset();
            tool.Update(data.GetBytes());

            return (uint)tool.Value;
        }

        public static uint GenerateCRC32(this uint[] data)
        {
            var tool = new Crc32();

            tool.Reset();
            tool.Update(data.GetBytes());

            return (uint)tool.Value;
        }

        public static uint GenerateCRC32(this string[] data)
        {
            var tool = new Crc32();

            tool.Reset();
            tool.Update(data.GetBytes());

            return (uint)tool.Value;
        }

        public static uint GenerateCRC32(this byte[] data)
        {
            var tool = new Crc32();

            tool.Reset();
            tool.Update(data);

            return (uint)tool.Value;
        }

        public static uint GenerateCRC32(this string str)
        {
            try
            {
                return System.Text.Encoding.ASCII.GetBytes(str).GenerateCRC32();
            }
            catch
            {
            }

            return 0;
        }
    }
}

