using System;
using System.Security.Cryptography;
using System.Text;

namespace ABPUtils
{
    static class Md5Helper
    {
        public static string GetMD5Hash(string input)
        {
            string result = string.Empty;
            using (MD5CryptoServiceProvider x = new MD5CryptoServiceProvider())
            {
                byte[] md5Hash = Encoding.UTF8.GetBytes(input);
                byte[] hashResult = x.ComputeHash(md5Hash);
                result = Convert.ToBase64String(hashResult);
                //remove trailing = characters if any
                result = result.TrimEnd('=');
            }

            return result;
        }
    }
}
