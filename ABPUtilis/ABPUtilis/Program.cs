using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace ABPUtils
{
    class Program
    {
        const string PATCH_FILE = "patch.xml";
        const string EASYLIST = "easylist.txt";
        const string EASYLIST_URL = "https://easylist-downloads.adblockplus.org/easylist.txt";
        const string EASYPRIVACY = "easyprivacy.txt";
        const string EASYPRIVACY_URL = "https://easylist-downloads.adblockplus.org/easyprivacy.txt";
        const string CHINALIST_END_MARK = "!------------------------End of List-------------------------";
        const int EASYLIST_EASYLIST_GENERAL_BLOCK = 1;
        const int EASYLIST_EASYLIST_GENERAL_HIDE = 2;
        const int EASYLIST_EASYLIST_ADSERVERS = 3;
        const int EASYLIST_ADULT_ADULT_ADSERVERS = 4;
        const int EASYLIST_EASYLIST_THIRDPARTY = 5;
        const int EASYLIST_ADULT_ADULT_THIRDPARTY = 6;
        const int EASYLIST_EASYLIST_SPECIFIC_BLOCK = 7;//ignore
        const int EASYLIST_ADULT_ADULT_SPECIFIC_BLOCK = 8;//ignore
        const int EASYLIST_EASYLIST_SPECIFIC_HIDE = 9;//ignore
        const int EASYLIST_ADULT_ADULT_SPECIFIC_HIDE = 10;//ignore
        const int EASYLIST_EASYLIST_WHITELIST = 11;//ignore
        const int EASYLIST_ADULT_ADULT_WHITELIST = 12;//ignore

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
                    ChinaList chinaList = new ChinaList(args[1]);
                    chinaList.Update();
                    chinaList.Validate();
                    break;
                case "validate":
                    chinaList = new ChinaList(args[1]);
                    chinaList.Validate();
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
                case "ns":
                    Console.WriteLine("DNS Query Tool, User Google Public DNS : 8.8.8.8 to query.");
                    IPAddress dnsServer = IPAddress.Parse("8.8.8.8");
                    Console.WriteLine("DNS Query Tool, type 'quit' to exit");

                    while (true)
                    {
                        Console.Write(">");
                        string domain = Console.ReadLine();

                        // break out on quit command
                        if (domain.ToLower() == "quit") break;

                        // Information
                        Console.WriteLine("Querying DNS records for domain: " + domain);

                        // query AName, MX, NS, SOA
                        Query(dnsServer, domain, DnsType.ANAME);
                        Query(dnsServer, domain, DnsType.MX);
                        Query(dnsServer, domain, DnsType.NS);
                        Query(dnsServer, domain, DnsType.SOA);
                    }
                    break;
                default:
                    break;
            }

            //Console.WriteLine("Press any key to continue...");
            //Console.ReadKey();
        }

        static void Merge(string chinaList, WebProxy proxy, bool patch, string lazyList = "adblock-lazy.txt")
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
                        ChinaList t = new ChinaList(s.Key);
                        if (t.Validate() != 1)
                        {
                            Console.WriteLine(string.Format("Download {0} error,pls try later.", s.Key));
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

            string easyListContent = TrimEasyList();
            sBuilder.AppendLine("!-----------------------EasyList----------------------------");
            sBuilder.AppendLine(easyListContent);

            string easyPrivacyContent = TrimEasyPrivacy();
            sBuilder.AppendLine("!-----------------------EasyPrivacy----------------------------");
            sBuilder.Append(easyPrivacyContent);

            //apply patch settings
            if (File.Exists(PATCH_FILE) && patch)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("use {0} to patch {1}", PATCH_FILE, lazyList);
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
                    if (patchconfig.NewItems.Count > 0)
                        sBuilder.AppendLine("!-----------------additional for ChinaList Lazy-------------");
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
            ChinaList.Save(lazyList, sBuilder.ToString());

            ChinaList cl = new ChinaList(chinaList);
            cl.Update();
            cl.Validate();
            cl = new ChinaList(lazyList);
            cl.Update();
            cl.Validate();

            Console.WriteLine("End of merge and validate.");
        }

        /// <summary>
        /// Check urls
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="missurl"></param>
        static void CheckUrls(string fileName, string missurl = "invalidurls.txt")
        {
            ChinaList cl = new ChinaList(fileName);
            List<string> urls = cl.GetUrls();
            StringBuilder stringBuilder = new StringBuilder();
            List<string> urlList = new List<string>();
            IPAddress dnsServer = IPAddress.Parse("8.8.8.8");

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
                                Console.WriteLine("{0} is validated by HttpWebRequest.", url);
                            }
                            else if (DNSValidate(dnsServer, url))
                            {
                                ret = true;
                                Console.WriteLine("{0} is validated by DNSValidate.", url);
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

            ChinaList.Save(missurl, stringBuilder.ToString());
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

        static string TrimEasyList()
        {
            StringBuilder sBuilder = new StringBuilder();
            using (StreamReader sr = new StreamReader(EASYLIST, Encoding.UTF8))
            {
                string easyListContent = sr.ReadToEnd();
                string[] t = Regex.Split(easyListContent, @"! \*\*\* ");

                for (int i = 1; i < t.Length; i++)
                {
                    if (i == EASYLIST_EASYLIST_SPECIFIC_BLOCK || i == EASYLIST_ADULT_ADULT_SPECIFIC_BLOCK
                            || i == EASYLIST_EASYLIST_SPECIFIC_HIDE || i == EASYLIST_ADULT_ADULT_SPECIFIC_HIDE
                            || i == EASYLIST_EASYLIST_WHITELIST || i == EASYLIST_ADULT_ADULT_WHITELIST)
                        continue;
                    var s = t[i];
                    var index = s.IndexOf("!-----------------");
                    if (index > 0)
                        s = s.Remove(index);

                    s = s.TrimEnd(new char[] { '\r', '\n' });
                    sBuilder.AppendLine("! *** " + s);
                }
            }

            return sBuilder.Replace("\r", string.Empty).ToString();
        }

        static string TrimEasyPrivacy()
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

            return sBuilder.Replace("\r", string.Empty).ToString();
        }

        static bool DNSValidate(IPAddress dnsServer, string domain)
        {
            bool ret = false;

            DnsType[] types = new DnsType[] { DnsType.ANAME, DnsType.MX, DnsType.NS, DnsType.SOA };
            foreach (var type in types)
            {
                try
                {
                    Request request = new Request();
                    request.AddQuestion(new Question(domain, type, DnsClass.IN));
                    Response response = Resolver.Lookup(request, dnsServer);
                    if (response != null)
                    {
                        ret = (response.Answers.Length > 0 || response.NameServers.Length > 0 || response.AdditionalRecords.Length > 0);
                    }
                    if (ret)
                        break;
                }
                catch
                {
                }
            }

            return ret;
        }

        static void Query(IPAddress dnsServer, string domain, DnsType type)
        {
            try
            {
                // create a DNS request
                Request request = new Request();

                // create a question for this domain and DNS CLASS
                request.AddQuestion(new Question(domain, type, DnsClass.IN));

                // send it to the DNS server and get the response
                Response response = Resolver.Lookup(request, dnsServer);

                // check we have a response
                if (response == null)
                {
                    Console.WriteLine("No answer");
                    return;

                }
                // display each RR returned
                Console.WriteLine("--------------------------------------------------------------");

                // display whether this is an authoritative answer or not
                if (response.AuthoritativeAnswer)
                {
                    Console.WriteLine("authoritative answer");
                }
                else
                {
                    Console.WriteLine("Non-authoritative answer");
                }

                // Dump all the records - answers/name servers/additional records
                foreach (Answer answer in response.Answers)
                {
                    Console.WriteLine("{0} ({1}) : {2}", answer.Type.ToString(), answer.Domain, answer.Record.ToString());
                }

                foreach (NameServer nameServer in response.NameServers)
                {
                    Console.WriteLine("{0} ({1}) : {2}", nameServer.Type.ToString(), nameServer.Domain, nameServer.Record.ToString());
                }

                foreach (AdditionalRecord additionalRecord in response.AdditionalRecords)
                {
                    Console.WriteLine("{0} ({1}) : {2}", additionalRecord.Type.ToString(), additionalRecord.Domain, additionalRecord.Record.ToString());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        static string GetWhoisInformation(string whoisServer, string url)
        {
            StringBuilder stringBuilderResult = new StringBuilder();
            TcpClient tcpClinetWhois = new TcpClient(whoisServer, 43);
            NetworkStream networkStreamWhois = tcpClinetWhois.GetStream();
            BufferedStream bufferedStreamWhois = new BufferedStream(networkStreamWhois);
            StreamWriter streamWriter = new StreamWriter(bufferedStreamWhois);

            streamWriter.WriteLine(url);
            streamWriter.Flush();

            StreamReader streamReaderReceive = new StreamReader(bufferedStreamWhois);

            while (!streamReaderReceive.EndOfStream)
                stringBuilderResult.AppendLine(streamReaderReceive.ReadLine());

            return stringBuilderResult.ToString();
        }
    }
}
