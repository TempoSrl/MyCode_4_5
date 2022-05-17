using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace mdl_utils {
    public class CryptDecrypt {


        /// <summary>
        /// Crypts a string with 3-des
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static byte[] CryptString(string key) {
            if (key == null) return null;
            byte[] A = Encoding.Default.GetBytes(key);

            var MS = new MemoryStream(1000);
            var CryptoS = new CryptoStream(MS,
                new TripleDESCryptoServiceProvider().CreateEncryptor(
                new byte[] { 75, 12, 0, 215, 93, 89, 45, 11, 171, 96, 4, 64, 13, 158, 36, 190 },
                new byte[] { 61, 12, 99, 78, 149, 123, 147, 48, 81, 20, 238, 57, 125, 38, 13, 4 }
                ), CryptoStreamMode.Write);
            CryptoS.Write(A, 0, A.Length);
            CryptoS.FlushFinalBlock();
            byte[] B = MS.ToArray();
            return B;
        }


        /// <summary>
        /// Decrypts a string with 3-des
        /// </summary>
        /// <param name="B"></param>
        /// <returns></returns>
		public static string DecryptString(byte[] B) {
            if (B == null) return null;
            var MS = new MemoryStream();
            var CryptoS = new CryptoStream(MS,
                new TripleDESCryptoServiceProvider().CreateDecryptor(
                new byte[] { 75, 12, 0, 215, 93, 89, 45, 11, 171, 96, 4, 64, 13, 158, 36, 190 },
                new byte[] { 61, 12, 99, 78, 149, 123, 147, 48, 81, 20, 238, 57, 125, 38, 13, 4 }
                ), CryptoStreamMode.Write);
            CryptoS.Write(B, 0, B.Length);
            CryptoS.FlushFinalBlock();
            string key = Encoding.Default.GetString(MS.ToArray()).TrimEnd();
            return key;
        }

        /// <summary>
        /// Crypts an array of bytes  with 3-des
        /// </summary>
        /// <param name="A"></param>
        /// <returns></returns>
        public static byte[] CryptBytes(byte[] A) {
            var MS = new MemoryStream(1000);
            var CryptoS = new CryptoStream(MS,
                new TripleDESCryptoServiceProvider().CreateEncryptor(
                new byte[] { 75, 12, 0, 215, 93, 89, 45, 11, 171, 96, 4, 64, 13, 158, 36, 190 },
                new byte[] { 61, 12, 99, 78, 149, 123, 147, 48, 81, 20, 238, 57, 125, 38, 13, 4 }
                ), CryptoStreamMode.Write);
            CryptoS.Write(A, 0, A.Length);
            CryptoS.FlushFinalBlock();
            byte[] B = MS.ToArray();
            return B;
        }


        /// <summary>
        /// Decryps an array of bytes   with 3-des
        /// </summary>
        /// <param name="B"></param>
        /// <returns></returns>
		public static byte[] DecryptBytes(byte[] B) {
            if (B == null) return null;
            var MS = new MemoryStream();
            var CryptoS = new CryptoStream(MS,
                new TripleDESCryptoServiceProvider().CreateDecryptor(
                new byte[] { 75, 12, 0, 215, 93, 89, 45, 11, 171, 96, 4, 64, 13, 158, 36, 190 },
                new byte[] { 61, 12, 99, 78, 149, 123, 147, 48, 81, 20, 238, 57, 125, 38, 13, 4 }
                ), CryptoStreamMode.Write);
            CryptoS.Write(B, 0, B.Length);
            CryptoS.FlushFinalBlock();
            return MS.ToArray();
        }

    }
}
