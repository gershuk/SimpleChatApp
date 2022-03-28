﻿using System.IO;
using System.Text;

namespace SimpleChatApp.CommonTypes
{
    public static class SHA256
    {
        private static readonly System.Security.Cryptography.SHA256 _mySHA256 = System.Security.Cryptography.SHA256.Create();

        private static Stream GenerateStreamFromString(string str)
        {
            return new MemoryStream(Encoding.Unicode.GetBytes(str));
        }

        public static string GetStringHash(string password)
        {
            using Stream? stream = GenerateStreamFromString(password);
            return Encoding.Unicode.GetString(_mySHA256.ComputeHash(stream));
        }
    }
}