using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpTibiaProxy
{
    public static class Extensions
    {
        public static void Raise<T>(this EventHandler<T> handler, object sender, T e) where T : EventArgs
        {
            if (handler != null)
                handler(sender, e);
        }

        public static void Raise(this EventHandler handler, object sender, EventArgs e)
        {
            if (handler != null)
                handler(sender, e);
        }

        public static byte ToByte(this char value)
        {
            return (byte)value;
        }

        public static byte[] ToByteArray(this string s)
        {
            List<byte> value = new List<byte>();
            foreach (char c in s.ToCharArray())
                value.Add(c.ToByte());
            return value.ToArray();
        }

        public static string ToHexString(this byte[] data)
        {
            return ToHexString(data, 0, data.Length);
        }

        public static string ToHexString(this byte[] data, int start, int length)
        {
            StringBuilder sb = new StringBuilder(data.Length * 3);
            for (int i = start; i < start + length; i++)
                sb.Append(Convert.ToString(data[i], 16).PadLeft(2, '0').PadRight(3, ' '));
            return sb.ToString().ToUpper();
        }
    }
}
