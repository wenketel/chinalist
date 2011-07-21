using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ABPUtils
{
    class Program
    {
        static void Main(string[] args)
        {
            if (null == args || args.Length == 0)
            {
                Console.WriteLine("wrong input argument.");
                return;
            }

            switch (args[0].ToLower())
            {
                case "update":
                    TextHelper.Update(args[1]);
                    Validate(args[1]);
                    break;
                case "validate":
                    Validate(args[1]);
                    break;
                case "merge":
                    List<string> argsList = new List<string>();
                    argsList.AddRange(args);
                    WebProxy proxy = null;
                    string p = "proxy";
                    if (argsList.Contains(p))
                    {
                        try
                        {
                            int index = argsList.IndexOf(p);
                            string[] temp = argsList[index + 1].Split(':');
                            proxy = new WebProxy(temp[0], int.Parse(temp[1]));
                            proxy.BypassProxyOnLocal = true;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                    }

                    Merge(args[1], proxy, argsList.Contains("patch"));
                    break;
                case "check":
                    if (args.Length == 2)
                        CheckUrls(args[1]);
                    else
                        CheckUrls(args[1], args[2]);
                    break;
                default:
                    break;
            }

            //Console.WriteLine("Press any key to continue...");
            //Console.ReadKey();
        }

        const string PATCH_FILE = "patch.xml";
        const string EASYLIST = "easylist_noadult.txt";
        const string EASYLIST_URL = "https://easylist-downloads.adblockplus.org/easylist_noadult.txt";
        const string EASYPRIVACY = "easyprivacy.txt";
        const string EASYPRIVACY_URL = "https://easylist-downloads.adblockplus.org/easyprivacy.txt";
        const string CHINALIST_END_MARK = "!------------------------End of List-------------------------";

        static int Validate(string fileName)
        {
            string checkSum = TextHelper.FindCheckSum(fileName);
            if (string.IsNullOrEmpty(checkSum))
            {
                Console.WriteLine("Couldn't find a checksum in the file " + fileName);
                return -1;
            }
            string content = TextHelper.GetContentForValidate(fileName);
            string contentForHash = TextHelper.GetContentForHash(content);
            string genearteCheckSum = Md5Helper.GetMD5Hash(contentForHash);

            if (checkSum.Equals(genearteCheckSum))
            {
                Console.WriteLine(fileName + " 's checksum is valid.");
                return 1;
            }
            else
            {
                Console.WriteLine(string.Format("Wrong checksum [{0}] found in the file {1}, expected is [{2}]", checkSum, fileName, genearteCheckSum));
                return 0;
            }
        }

        static void Merge(string chinaList, WebProxy proxy, bool patch, string lazyChinaList = "adblock-lazy.txt")
        {
            using (WebClient webClient = new WebClient())
            {
                if (proxy != null)
                {
                    webClient.Proxy = proxy;
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("use proxy: {0}", proxy.Address.Authority.ToString());
                    Console.ResetColor();
                }

                Dictionary<string, string> lists = new Dictionary<string, string>();
                lists.Add(EASYLIST, EASYLIST_URL);
                lists.Add(EASYPRIVACY, EASYPRIVACY_URL);
                Console.ForegroundColor = ConsoleColor.Magenta;
                foreach (var s in lists)
                {
                    if (IsFileExist(s.Key))
                    {
                        Console.WriteLine("{0} is the latest, skip over downloading.", s.Key);
                    }
                    else
                    {
                        Console.WriteLine("{0} is out of date, to start the update.", s.Key);
                        webClient.DownloadFile(s.Value, s.Key);
                        Console.WriteLine("update {0} completed.", s.Key);
                        if (!File.Exists(s.Key))
                        {
                            Console.WriteLine(string.Format("Can't download {0},pls try later.", s.Key));
                            return;
                        }
                    }
                }
                Console.ResetColor();
            }

            //merge
            string chinaListContent = string.Empty;
            StringBuilder sBuilder = new StringBuilder();
            using (StreamReader sr = new StreamReader(chinaList, Encoding.UTF8))
            {
                chinaListContent = sr.ReadToEnd();
                var index = chinaListContent.IndexOf(CHINALIST_END_MARK);
                chinaListContent = chinaListContent.Remove(index);
                sBuilder.Append(chinaListContent);
            }

            string easyListContent = ParseEasyList();
            sBuilder.AppendLine("!-----------------------EasyList----------------------------");
            sBuilder.AppendLine(easyListContent);

            string easyPrivacyContent = ParseEasyPrivacy();
            sBuilder.AppendLine("!-----------------------EasyPrivacy----------------------------");
            sBuilder.Append(easyPrivacyContent);

            //apply patch settings
            if (File.Exists(PATCH_FILE) && patch)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("use {0} to patch {1}", PATCH_FILE, lazyChinaList);
                using (StreamReader sr = new StreamReader(PATCH_FILE, Encoding.UTF8))
                {
                    string xml = sr.ReadToEnd();
                    PatchConfigurations patchconfig = SimpleSerializer.XmlDeserialize<PatchConfigurations>(xml);
                    Console.ForegroundColor = ConsoleColor.Red;
                    foreach (var item in patchconfig.RemovedItems)
                    {
                        sBuilder.Replace(item + "\n", string.Empty);
                        Console.WriteLine("remove filter {0}", item);
                    }

                    Console.ForegroundColor = ConsoleColor.Blue;
                    foreach (var item in patchconfig.ModifyItems)
                    {
                        sBuilder.Replace(item.OldItem, item.NewItem);
                        Console.WriteLine("replace filter {0} with {1}", item.OldItem, item.NewItem);
                    }

                    Console.ForegroundColor = ConsoleColor.Green;
                    foreach (var item in patchconfig.NewItems)
                    {
                        sBuilder.AppendLine(item);
                        Console.WriteLine("add filter {0}", item);
                    }
                }
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Patch file end.");
                Console.ResetColor();
            }

            sBuilder.AppendLine("");
            sBuilder.AppendLine(CHINALIST_END_MARK);

            Console.WriteLine(string.Format("Merge {0}, {1} and {2}.", chinaList, EASYLIST, EASYPRIVACY));
            TextHelper.Save(lazyChinaList, sBuilder.ToString());

            TextHelper.Update(chinaList);
            Validate(chinaList);

            TextHelper.Update(lazyChinaList);
            Validate(lazyChinaList);
            Console.WriteLine("End of merge and validate.");
        }

        /// <summary>
        /// Check urls
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="missurl"></param>
        static void CheckUrls(string fileName, string missurl = "invalidurls.txt")
        {
            List<string> urls = TextHelper.GetUrls(fileName);
            StringBuilder stringBuilder = new StringBuilder();
            List<string> urlList = new List<string>();

            Parallel.ForEach(urls, url =>
            {
                bool ret = false;
                for (int i = 1; i < 4; i++)
                {
                    if (PingUrl(url))
                    {
                        Console.WriteLine("Ping {0} successed.", url);
                        ret = true;
                        break;
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Ping {0} failed {1} time(s).", url, i);
                        Console.ResetColor();
                        ret = PingUrl(url);
                        if (i == 3)
                        {
                            if (IsUrlExists(url))
                            {
                                ret = true;
                                Console.WriteLine("{0} is validate by HttpWebRequest.", url);
                            }
                            else
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine("{0} is not exist.", url);
                                Console.ResetColor();
                            }
                        }
                    }
                }

                if (!ret && !urlList.Contains(url))
                    urlList.Add(url);

            });

            foreach (var u in urlList)
                stringBuilder.AppendLine(u);

            TextHelper.Save(missurl, stringBuilder.ToString());
        }

        /// <summary>
        /// Ping URL
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        static bool PingUrl(string url)
        {
            Ping pingSender = new Ping();
            PingOptions options = new PingOptions();
            options.DontFragment = true;

            string data = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
            byte[] buffer = Encoding.ASCII.GetBytes(data);
            int timeout = 120;

            int i = 0;
            try
            {
                PingReply reply = pingSender.Send(url, timeout, buffer, options);
                return (reply != null && reply.Status == IPStatus.Success);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Check url is valid
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        static bool IsUrlExists(string url)
        {
            try
            {
                HttpWebRequest request = WebRequest.Create(string.Format("http://www.{0}", url)) as HttpWebRequest;
                request.Method = "HEAD";
                HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                return (response.StatusCode == HttpStatusCode.OK);
            }
            catch
            {
                return false;
            }
        }

        static bool IsFileExist(string fileName)
        {
            DateTime dt = File.GetLastWriteTime(fileName);
            FileInfo fileInfo = new FileInfo(fileName);

            return (dt.ToString("yyyy-MM-dd").Equals(DateTime.Now.ToString("yyyy-MM-dd")) && fileInfo.Length > 0);
        }

        static string ParseEasyList()
        {
            StringBuilder sBuilder = new StringBuilder();
            using (StreamReader sr = new StreamReader(EASYLIST, Encoding.UTF8))
            {
                string easyListContent = sr.ReadToEnd();
                string[] t = Regex.Split(easyListContent, @"! \*\*\* ");

                for (int i = 1; i < t.Length; i++)
                {
                    if (i == 5 || i == 6 || i == 7)
                        continue;
                    var s = t[i];
                    var index = s.IndexOf("!-----------------");
                    if (index > 0)
                        s = s.Remove(index);

                    s = s.TrimEnd(new char[] { '\r', '\n' });
                    sBuilder.AppendLine("! *** " + s);
                }
            }

            return sBuilder.ToString();
        }

        static string ParseEasyPrivacy()
        {
            StringBuilder sBuilder = new StringBuilder();
            using (StreamReader sr = new StreamReader(EASYPRIVACY, Encoding.UTF8))
            {
                string easyPrivacyContent = sr.ReadToEnd();

                string[] t = Regex.Split(easyPrivacyContent, @"! \*\*\* ");

                for (int i = 1; i < t.Length; i++)
                {
                    if (i == 7 || i == 9 || i == 10)
                        continue;
                    var s = t[i];

                    if (i == 4 || i == 6 || i == 8)
                    {
                        int chinese = s.IndexOf("! Chinese");
                        int czech = s.IndexOf("! Czech");
                        if (czech < 0)
                            czech = s.IndexOf("! Danish");

                        int length = s.IndexOf(".txt ***");
                        s = s.Substring(0, length + 9) + s.Substring(chinese, czech - chinese);
                    }

                    var index = s.IndexOf("!-----------------");
                    if (index > 0)
                        s = s.Remove(index);

                    s = s.TrimEnd(new char[] { '\r', '\n' });
                    sBuilder.AppendLine("! *** " + s);
                }
            }

            return sBuilder.ToString();
        }
    }
}
