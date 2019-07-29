using System;
using System.IO;

namespace DataEncrypter.IO
{
    /// <summary>
    /// Contains misc helper functions
    /// </summary>
    public static class BinaryTools
    {
        /// <summary>
        /// Converts a string to a byte array.
        /// </summary>
        /// <param name="str">String to be converted</param>
        public static byte[] StringToBytes(string str)
        {
            byte[] bytes = new byte[str.Length];
            for (int i = 0; i < str.Length; i++)
            {
                bytes[i] = (byte)str[i];
            }

            return bytes;
        }

        /// <summary>
        /// Converts a byte array to a string.
        /// </summary>
        /// <param name="str">Bytes to be converted</param>
        public static string BytesToString(byte[] bytes, int startIndex = 0)
        {
            string str = "";
            for (int i = startIndex; i < bytes.Length; i++)
            {
                str += (char)bytes[i];
            }

            return str;
        }

        /// <summary>
        /// Converts a string to a byte array with fixed size. Is null-terminated
        /// </summary>
        /// <param name="str">String to be converted</param>
        /// <param name="length">Length of the array</param>
        public static byte[] StringToFixSizedByte(string str, int length)
        {
            byte[] bytes = new byte[length];

            for (int i = 0; i < Math.Min(str.Length, length); i++)
            {
                bytes[i] = (byte)str[i];
            }
            bytes[length - 1] = 0;

            return bytes;
        }

        /// <summary>
        /// Converts a byte array with fixed size to a string. Is null-terminated
        /// </summary>
        /// <param name="bytes">Bytes to be converted</param>
        /// <param name="startIndex"></param>
        /// <returns></returns>
        public static string StringFromFixSizedByte(byte[] bytes, int startIndex)
        {
            string str = "";
            for (int i = startIndex; i < bytes.Length; i++)
            {
                if (bytes[i] != 0)
                {
                    str += (char)bytes[i];
                }
                else
                {
                    break;
                }
            }

            return str;
        }

        public static byte[] SerializeObject<T>(T objectToWrite)
        {
            using (var stream = new MemoryStream())
            {
                var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                binaryFormatter.Serialize(stream, objectToWrite);
                return stream.ToArray();
            }
        }

        public static T DeserializeObject<T>(byte[] objectToRead)
        {
            using (var stream = new MemoryStream(objectToRead))
            {
                var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                return (T)binaryFormatter.Deserialize(stream);
            }
        }
    }
}
