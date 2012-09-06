using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.VisualBasic;

namespace ABPUtils
{
    class ChinaList
    {
        private const string CHECKSUM_REGX = @"^\s*!\s*checksum[\s\-:]+([\w\+\/=]+).*\n";
        private const string URL_REGX = @"([a-z0-9][a-z0-9\-]*?\.(?:com|edu|cn|net|org|gov|im|info|la|co|tv|biz|mobi)(?:\.(?:cn|tw))?)";

        public String FileName
        {
            get;
            private set;
        }

        public ChinaList(string fileName)
        {
            FileName = fileName;
        }

        /// <summary>
        /// update list
        /// </summary>
        public void Update()
        {
            string content = ReadList();
            content = UpdateTime(content);
            content = RemoveChecksum(content);

            string result = UpdateCheckSum(content);
            Save(FileName, result);
        }

        /// <summary>
        /// validate list
        /// </summary>
        /// <returns></returns>
        public int Validate()
        {
            string content = ReadListToEnd();
            string checkSum = FindCheckSum(content);
            if (string.IsNullOrEmpty(checkSum))
            {
                Console.WriteLine("Couldn't find a checksum in the file {0}", FileName);
                return -1;
            }

            content = RemoveChecksum(content);
            string genearteCheckSum = CalculateMD5Hash(RemoveEmptyLines(content));

            if (checkSum.Equals(genearteCheckSum))
            {
                Console.WriteLine("Checksum in the file {0} is valid.", FileName);
                return 1;
            }
            else
            {
                Console.WriteLine("Wrong checksum [{0}] found in the file {1}, expected is [{2}]", checkSum, FileName, genearteCheckSum);
                return 0;
            }
        }

        /// <summary>
        /// get domains from list
        /// </summary>
        /// <returns></returns>
        public List<string> GetDomains()
        {
            List<string> urls = new List<string>();

            string s = string.Empty;
            using (StreamReader sr = new StreamReader(FileName, Encoding.UTF8))
            {
                s = sr.ReadToEnd();
            }

            Regex regex = new Regex(URL_REGX, RegexOptions.Multiline | RegexOptions.IgnoreCase);
            MatchCollection matches = regex.Matches(s);
            foreach (Match match in matches)
            {
                string url = match.Value;
                if (urls.Contains(url))
                    continue;
                urls.Add(match.Value);
            }
            urls.Sort();

            return urls;
        }

        /// <summary>
        /// save file
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="content"></param>
        public static void Save(string fileName, string content)
        {
            using (StreamWriter sw = new StreamWriter(fileName, false))
            {
                sw.Write(content);
                sw.Flush();
            }
        }

        /// <summary>
        /// Read list content and convert to Simplified Chinese
        /// </summary>
        /// <returns></returns>
        private string ReadList()
        {
            var line = string.Empty;
            var content = string.Empty;
            var sBuilder = new StringBuilder();
            var list = new List<string>();

            using (StreamReader sr = new StreamReader(FileName, Encoding.UTF8))
            {
                // filter duplicate line
                while ((line = sr.ReadLine()) != null)
                {
                    if (!string.IsNullOrEmpty(line))
                    {
                        if (list.Contains(line) && !line.StartsWith("!"))
                            continue;

                        list.Add(line);
                    }

                    sBuilder.AppendLine(line);
                }
            }

            content = sBuilder.ToString();
            content = ToSimplified(content);
            content = content.Replace("\r", string.Empty);

            return content;
        }

        /// <summary>
        /// Read list content one time
        /// </summary>
        /// <returns></returns>
        private string ReadListToEnd()
        {
            var content = string.Empty;

            using (StreamReader sr = new StreamReader(FileName, Encoding.UTF8))
            {
                content = sr.ReadToEnd();
                content = content.Replace("\r", string.Empty);
            }

            return content;
        }

        /// <summary>
        /// change list update time
        /// </summary>
        /// <returns></returns>
        private string UpdateTime(string content)
        {
            DateTime dt = DateTime.Now;
            //Wed, 22 Jul 2009 16:39:15 +0800
            string time = string.Format("Last Modified:  {0}", dt.ToString("r")).Replace("GMT", "+0800");
            Regex regex = new Regex(@"Last Modified:.*$", RegexOptions.Multiline);
            content = regex.Replace(content, time);

            return content;
        }

        /// <summary>
        /// Remove empty lines
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        private string RemoveEmptyLines(string content)
        {
            content = Regex.Replace(content, "\n+", "\n");
            return content;
        }

        /// <summary>
        /// remove checksum
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        private string RemoveChecksum(string content)
        {
            Regex regex = new Regex(CHECKSUM_REGX, RegexOptions.Multiline | RegexOptions.IgnoreCase);
            content = regex.Replace(content, string.Empty);

            return content;
        }

        /// <summary>
        /// Get checksum for list
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        private string FindCheckSum(string content)
        {
            Regex regex = new Regex(CHECKSUM_REGX, RegexOptions.Multiline | RegexOptions.IgnoreCase);

            Match match = regex.Match(content);
            string value = match.Value;

            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }
            else
            {
                string[] temp = match.Value.Split(':');
                return temp[1].Trim();
            }
        }

        /// <summary>
        /// update checksum
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        private string UpdateCheckSum(string content)
        {
            int index = content.IndexOf("]");
            string checkSum = string.Format("\n!  Checksum: {0}", CalculateMD5Hash(RemoveEmptyLines(content)));
            content = content.Insert(index + 1, checkSum);

            return content;
        }

        /// <summary>
        /// Convert string to Simplified Chinese
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        private string ToSimplified(string source)
        {
            System.Globalization.CultureInfo cl = new System.Globalization.CultureInfo("zh-CN", false);

            return Strings.StrConv(source, VbStrConv.SimplifiedChinese, cl.LCID);
        }

        /// <summary>
        /// Calculate md5 hash of the string content
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        private string CalculateMD5Hash(string content)
        {
            string result = string.Empty;
            using (MD5CryptoServiceProvider x = new MD5CryptoServiceProvider())
            {
                byte[] md5Hash = Encoding.UTF8.GetBytes(content);
                byte[] hashResult = x.ComputeHash(md5Hash);
                result = Convert.ToBase64String(hashResult);
                //remove trailing = characters if any
                result = result.TrimEnd('=');
            }

            return result;
        }
    }
}
