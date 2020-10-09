using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace OMS.Core.Tools
{
    public class EncryptTools
    {
        //默认密钥向量
        private static byte[] _aesKey = { 0x12, 0x34, 0x56, 0x78, 0x90, 0xAB, 0xCD, 0xEF, 0x12, 0x34, 0x56, 0x78, 0x90, 0xAB, 0xCC, 0xEF };

        /// <summary>
        /// AES加密算法
        /// </summary>
        /// <param name="plainText">明文字符串</param>
        /// <returns>将加密后的密文转换为Base64编码，以便显示</returns>
        public static string AESEncrypt(string plainText, string Key)
        {
            //分组加密算法
            SymmetricAlgorithm des = Rijndael.Create();
            byte[] inputByteArray = Encoding.UTF8.GetBytes(plainText);//得到需要加密的字节数组
            //设置密钥及密钥向量
            des.Key = Encoding.UTF8.GetBytes(Key);
            des.IV = _aesKey;
            byte[] cipherBytes = null;
            using (MemoryStream ms = new MemoryStream())
            {
                using (CryptoStream cs = new CryptoStream(ms, des.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    cs.Write(inputByteArray, 0, inputByteArray.Length);
                    cs.FlushFinalBlock();
                    cipherBytes = ms.ToArray();//得到加密后的字节数组
                    cs.Close();
                    ms.Close();
                }
            }
            return Convert.ToBase64String(cipherBytes);
        }

        /// <summary>
        /// AES解密
        /// </summary>
        /// <param name="cipherText">密文字符串</param>
        /// <returns>返回解密后的明文字符串</returns>
        public static string AESDecrypt(string showText, string Key)
        {
            showText = showText.Replace("%3d", "=").Replace(" ", "+").Replace("%2b", "+");

            byte[] cipherText = Convert.FromBase64String(showText);
            SymmetricAlgorithm des = Rijndael.Create();
            des.Key = Encoding.UTF8.GetBytes(Key);
            des.IV = _aesKey;
            byte[] decryptBytes = new byte[cipherText.Length];
            using (MemoryStream ms = new MemoryStream(cipherText))
            {
                using (CryptoStream cs = new CryptoStream(ms, des.CreateDecryptor(), CryptoStreamMode.Read))
                {
                    cs.Read(decryptBytes, 0, decryptBytes.Length);
                    cs.Close();
                    ms.Close();
                }
            }
            return Encoding.UTF8.GetString(decryptBytes).Replace("\0", "");   ///将字符串后尾的'\0'去掉
        }

        /// <summary>
        /// md5摘要
        /// </summary>
        /// <param name="plain"></param>
        /// <param name="isMd5"></param>
        /// <returns></returns>
        public static string Md5Hash(string plain, string format = "X2")
        {
            MD5 md5 = MD5CryptoServiceProvider.Create();
            byte[] result = md5.ComputeHash(UTF8Encoding.UTF8.GetBytes(plain));
            StringBuilder sb = new StringBuilder();
            foreach (var i in result)
                sb.Append(i.ToString(format));
            return sb.ToString();
        }
    }
}
