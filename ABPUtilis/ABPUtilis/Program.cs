using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Bdev.Net.Dns;

namespace ABPUtils
{
    class Program
    {
        const string PATCH_FILE = "patch.xml";
        const string EASYLIST = "easylist.txt";
        const string CHINALIST_LAZY_HEADER = @"[Adblock Plus 1.2]
!  Adblock Plus List with Main Focus on Chinese Sites.
!  Last Modified:  
!  Homepage: http://adblock-chinalist.googlecode.com/
!
!  ChinaList Lazy = Part of EasyList + ChinaList + Part of EasyPrivacy
!  If you need to know the details,
!  please visit: https://code.google.com/p/adblock-chinalist/wiki/something_about_ChinaList_Lazy
!
!  If you need help or have any question,
!  please visit: http://adblock-chinalist.googlecode.com/
!
!  coding: utf-8, expires: 5 days
!--CC-BY-SA 3.0 + Licensed, NO WARRANTY but Best Wishes----
";
        const string EASYLIST_URL = "https://easylist-downloads.adblockplus.org/easylist.txt";
        const string EASYPRIVACY = "easyprivacy.txt";
        const string EASYPRIVACY_URL = "https://easylist-downloads.adblockplus.org/easyprivacy.txt";
        const string CHINALIST_LAZY_HEADER_MARK = "!----------------------------White List--------------------";
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
                        ValidateDomains(args[1]);
                    else
                        ValidateDomains(args[1], args[2]);
                    break;
                case "ns":
                    try
                    {
                        IPAddress dns = null;
                        if (args.Length == 2)
                            dns = IPAddress.Parse("8.8.8.8");
                        else
                            dns = IPAddress.Parse(args[2]);

                        QueryResult queryResult = DNSQuery(dns, args[1]);
                        Console.Write(queryResult.ToString());
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
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
                    Console.WriteLine("use proxy: {0}", proxy.Address.Authority.ToString());
                }

                Dictionary<string, string> lists = new Dictionary<string, string>();
                lists.Add(EASYLIST, EASYLIST_URL);
                lists.Add(EASYPRIVACY, EASYPRIVACY_URL);
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
            }

            //merge
            string chinaListContent = string.Empty;
            StringBuilder sBuilder = new StringBuilder();
            using (StreamReader sr = new StreamReader(chinaList, Encoding.UTF8))
            {
                chinaListContent = sr.ReadToEnd();
                //TODO:replace header
                var headerIndex = chinaListContent.IndexOf(CHINALIST_LAZY_HEADER_MARK);
                chinaListContent = chinaListContent.Substring(headerIndex).Insert(0, CHINALIST_LAZY_HEADER);
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
                Console.WriteLine("use {0} to patch {1}", PATCH_FILE, lazyList);
                using (StreamReader sr = new StreamReader(PATCH_FILE, Encoding.UTF8))
                {
                    string xml = sr.ReadToEnd();
                    PatchConfigurations patchconfig = SimpleSerializer.XmlDeserialize<PatchConfigurations>(xml);
                    foreach (var item in patchconfig.RemovedItems)
                    {
                        sBuilder.Replace(item + "\n", string.Empty);
                        Console.WriteLine("remove filter {0}", item);
                    }

                    foreach (var item in patchconfig.ModifyItems)
                    {
                        sBuilder.Replace(item.OldItem, item.NewItem);
                        Console.WriteLine("replace filter {0} with {1}", item.OldItem, item.NewItem);
                    }

                    if (patchconfig.NewItems.Count > 0)
                        sBuilder.AppendLine("!-----------------additional for ChinaList Lazy-------------");
                    foreach (var item in patchconfig.NewItems)
                    {
                        sBuilder.AppendLine(item);
                        Console.WriteLine("add filter {0}", item);
                    }
                }
                Console.WriteLine("Patch file end.");
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
        static void ValidateDomains(string fileName, string missurl = "invalid_domains.txt")
        {
            ChinaList cl = new ChinaList(fileName);
            List<string> domains = cl.GetDomains();
            //List<string> urls = cl.ParseURLs();
            StringBuilder results = new StringBuilder();
            //StringBuilder fullResult = new StringBuilder();

            IPAddress dns = IPAddress.Parse("8.8.8.8");

            Parallel.ForEach(domains, domain =>
            {
                Console.WriteLine("Querying DNS records for domain: {0}", domain);
                QueryResult queryResult = DNSQuery(dns, domain);
                Console.Write(queryResult.ToString());
                //fullResult.Append(queryResult.ToString());

                if (queryResult.NSCount == 0)
                {
                    results.Append(queryResult.ToString());
                }
            });

            ChinaList.Save(missurl, results.ToString());
            // ChinaList.Save("full_domains.txt", fullResult.ToString());
        }

        static QueryResult DNSQuery(IPAddress dnsServer, string domain)
        {
            QueryResult queryResult = new QueryResult()
            {
                Domain = domain,
                DNS = dnsServer.ToString()
            };

            try
            {
                // create a DNS request
                Request request = new Request();
                request.AddQuestion(new Question(domain, DnsType.NS, DnsClass.IN));
                Response response = Resolver.Lookup(request, dnsServer);

                if (response == null)
                {
                    queryResult.Info = "No answer";
                    return queryResult;
                }

                queryResult.Info = response.AuthoritativeAnswer ? "authoritative answer" : "Non-authoritative answer";

                queryResult.NSCount = response.Answers.Length + response.AdditionalRecords.Length + response.NameServers.Length;

                foreach (Answer answer in response.Answers)
                {
                    queryResult.NSList.Add(answer.Record.ToString());
                }

                foreach (AdditionalRecord additionalRecord in response.AdditionalRecords)
                {
                    queryResult.NSList.Add(additionalRecord.Record.ToString());
                }

                foreach (NameServer nameServer in response.NameServers)
                {
                    queryResult.NSList.Add(nameServer.Record.ToString());
                }
            }
            catch (Exception ex)
            {
                queryResult.Error = ex.Message;
            }

            return queryResult;
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
