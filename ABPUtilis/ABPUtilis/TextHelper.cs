using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Generic;

namespace ABPUtils
{
    public static class TextHelper
    {
        const string CHECKSUM_REGX = @"^\s*!\s*checksum[\s\-:]+([\w\+\/=]+).*\n";
        const string URL_REGX = @"([a-z0-9][a-z0-9\-]*?\.(?:com|edu|cn|net|org|gov|im|info|la|co|tv|biz|mobi)(?:\.(?:cn|tw))?)";

        /// <summary>
        /// Update time and checksum
        /// </summary>
        /// <param name="fileName"></param>
        public static void Update(string fileName)
        {
            string content = GetContent(fileName);
            string result = UpdateCheckSum(content);
            Save(fileName, result);
        }

        /// <summary>
        /// Get urls for validate
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static List<string> GetUrls(string fileName)
        {
            List<string> urls = new List<string>();

            string s = string.Empty;
            using (StreamReader sr = new StreamReader(fileName, Encoding.UTF8))
            {
                s = sr.ReadToEnd();
            }

            Regex regex = new Regex(URL_REGX, RegexOptions.Multiline | RegexOptions.IgnoreCase);
            MatchCollection matches = regex.Matches(s);
            foreach (Match match in matches)
            {
                string url = match.Value;
                if (urls.Contains(url) || url.EndsWith(".tw"))
                    continue;
                urls.Add(match.Value);
            }
            urls.Sort();

            return urls;
        }
        /// <summary>
        /// Find CheckSum
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static string FindCheckSum(string fileName)
        {
            Regex regex = new Regex(CHECKSUM_REGX, RegexOptions.Multiline | RegexOptions.IgnoreCase);

            string s = string.Empty;
            using (StreamReader sr = new StreamReader(fileName, Encoding.UTF8))
            {
                s = sr.ReadToEnd();
                s = s.Replace("\r", string.Empty);
            }

            Match match = regex.Match(s);
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
        /// Get validate content
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static string GetContentForValidate(string fileName)
        {
            string s = string.Empty;
            using (StreamReader sr = new StreamReader(fileName, Encoding.UTF8))
            {
                s = sr.ReadToEnd();
                s = s.Replace("\r", string.Empty);

                //remove checksum
                Regex regex = new Regex(CHECKSUM_REGX, RegexOptions.Multiline | RegexOptions.IgnoreCase);
                s = regex.Replace(s, string.Empty);
            }

            return s;
        }

        /// <summary>
        /// Get content and update time
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static string GetContent(string fileName)
        {
            string s;
            using (StreamReader sr = new StreamReader(fileName, Encoding.UTF8))
            {
                s = sr.ReadToEnd();
                s = Convertor.ToSimplified(s);
                s = s.Replace("\r", string.Empty);
                DateTime dt = DateTime.Now;
                //Wed, 22 Jul 2009 16:39:15 +0800
                string time = string.Format("Last Modified:  {0}", dt.ToString("r")).Replace("GMT", "+0800");
                Regex regex = new Regex(@"Last Modified:.*$", RegexOptions.Multiline);
                s = regex.Replace(s, time);

                //remove checksum
                regex = new Regex(@"^\s*!\s*Checksum[\s\-:]+([\w\+\/=]+).*\n", RegexOptions.Multiline);
                s = regex.Replace(s, string.Empty);
            }

            return s;
        }

        /// <summary>
        ///  remove white space
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public static string GetContentForHash(string content)
        {
            content = Regex.Replace(content, "\n+", "\n");
            return content;
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
        /// update checksum
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        static string UpdateCheckSum(string content)
        {
            int index = content.IndexOf("]");
            string checkSum = string.Format("\n!  Checksum: {0}", Md5Helper.GetMD5Hash(GetContentForHash(content)));
            content = content.Insert(index + 1, checkSum);

            return content;
        }
    }
}