using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Bdev.Net.Dns;

namespace ABPUtils
{
    public class ChinaLists
    {
        private ChinaLists() { }

        /// <summary>
        /// Get assembly version
        /// </summary>
        /// <returns></returns>
        public static string GetVersion()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            return fvi.ProductVersion;
        }

        /// <summary>
        /// Merge input list with part of EasyList and EasyPrivacy
        /// </summary>
        /// <param name="chinaList"></param>
        /// <param name="proxy"></param>
        /// <param name="patch"></param>
        /// <param name="lazyList"></param>
        public static void Merge(string chinaList, WebProxy proxy, bool patch, string lazyList = "adblock-lazy.txt")
        {
            if (!DownloadEasyList(proxy))
                return;

            if (string.IsNullOrEmpty(lazyList))
                lazyList = "adblock-lazy.txt";

            // validate ChinaList to merge
            ChinaList cl = new ChinaList(chinaList);
            cl.Update();

            if (cl.Validate() != 1)
                return;

            // load ChinaList content
            string chinaListContent = string.Empty;
            StringBuilder sBuilder = new StringBuilder();
            using (StreamReader sr = new StreamReader(chinaList, Encoding.UTF8))
            {
                chinaListContent = sr.ReadToEnd();
                var headerIndex = chinaListContent.IndexOf(ChinaListConst.CHINALIST_LAZY_HEADER_MARK);
                chinaListContent = chinaListContent.Substring(headerIndex).Insert(0, ChinaListConst.CHINALIST_LAZY_HEADER);
                var index = chinaListContent.IndexOf(ChinaListConst.CHINALIST_END_MARK);
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
            if (File.Exists(ChinaListConst.PATCH_FILE) && patch)
            {
                Console.WriteLine("use {0} to patch {1}", ChinaListConst.PATCH_FILE, lazyList);

                Configurations pConfig = GetConfigurations();

                if (pConfig != null)
                {
                    foreach (var item in pConfig.RemovedItems)
                    {
                        sBuilder.Replace(item + "\n", string.Empty);
                        Console.WriteLine("remove filter {0}", item);
                    }

                    foreach (var item in pConfig.ModifyItems)
                    {
                        sBuilder.Replace(item.OldItem, item.NewItem);
                        Console.WriteLine("replace filter {0} with {1}", item.OldItem, item.NewItem);
                    }

                    if (pConfig.NewItems.Count > 0)
                        sBuilder.AppendLine("!-----------------additional for ChinaList Lazy-------------");

                    foreach (var item in pConfig.NewItems)
                    {
                        sBuilder.AppendLine(item);
                        Console.WriteLine("add filter {0}", item);
                    }

                    // Merge ChinaList Privacy
                    if (!string.IsNullOrEmpty(pConfig.Privacy))
                    {
                        sBuilder.AppendLine("! *** adblock-privacy.txt");
                        var line = string.Empty;
                        using (StreamReader sr = new StreamReader(pConfig.Privacy, Encoding.UTF8))
                        {
                            while ((line = sr.ReadLine()) != null)
                            {
                                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("!") || line.StartsWith("["))
                                    continue;
                                sBuilder.AppendLine(line);
                            }
                        }
                    }
                }

                Console.WriteLine("Patch file end.");
            }

            sBuilder.AppendLine(string.Empty);
            sBuilder.AppendLine(ChinaListConst.CHINALIST_END_MARK);

            Console.WriteLine(string.Format("Merge {0}, {1} and {2}.", chinaList, ChinaListConst.EASYLIST, ChinaListConst.EASYPRIVACY));
            Save(lazyList, sBuilder.ToString());

            cl = new ChinaList(lazyList);
            cl.Update();
            cl.Validate();

            Console.WriteLine("End of merge and validate.");
        }

        /// <summary>
        /// Clean Patch file
        /// </summary>
        /// <param name="chinaList"></param>
        /// <param name="proxy"></param>
        public static void CleanConfigurations(string chinaList, WebProxy proxy)
        {
            if (DownloadEasyList(null))
            {
                Configurations patchConfig = ChinaLists.GetConfigurations();
                if (patchConfig == null)
                {
                    Console.WriteLine("wrong Patch Confguration file.");
                    return;
                }

                StringBuilder sBuilder = new StringBuilder();
                using (StreamReader sr = new StreamReader(chinaList, Encoding.UTF8))
                {
                    sBuilder.Append(sr.ReadToEnd());
                }

                using (StreamReader sr = new StreamReader(ChinaListConst.EASYLIST, Encoding.UTF8))
                {
                    sBuilder.Append(sr.ReadToEnd());
                }

                using (StreamReader sr = new StreamReader(ChinaListConst.EASYPRIVACY, Encoding.UTF8))
                {
                    sBuilder.Append(sr.ReadToEnd());
                }

                string s = sBuilder.ToString();

                List<string> removedItems = new List<string>(patchConfig.RemovedItems);
                foreach (var item in patchConfig.RemovedItems)
                {
                    if (s.IndexOf(item) > -1)
                        continue;

                    removedItems.Remove(item);
                }

                List<ModifyItem> modifyItems = new List<ModifyItem>(patchConfig.ModifyItems);
                foreach (var item in patchConfig.ModifyItems)
                {
                    if (s.IndexOf(item.OldItem) > -1)
                        continue;

                    modifyItems.Remove(item);
                }

                patchConfig.ModifyItems = modifyItems;
                patchConfig.RemovedItems = removedItems;
                string xml = SimpleSerializer.XmlSerialize<Configurations>(patchConfig);
                Save(ChinaListConst.PATCH_FILE, xml);
            }
        }

        /// <summary>
        /// validate domain by nslookup
        /// </summary>
        /// <param name="dns"></param>
        /// <param name="fileName"></param>
        /// <param name="invalidDomains"></param>
        public static void ValidateDomains(IPAddress dns, string fileName, string invalidDomains = "invalid_domains.txt")
        {
            if (dns == null)
                dns = IPAddress.Parse("8.8.8.8");

            if (string.IsNullOrEmpty(invalidDomains))
                invalidDomains = "invalid_domains.txt";

            ChinaList cl = new ChinaList(fileName);
            List<string> domains = cl.GetDomains();
            //List<string> urls = cl.ParseURLs();
            StringBuilder results = new StringBuilder();
            //StringBuilder fullResult = new StringBuilder();
            List<string> whiteList = new List<string>();
            whiteList.Add("ns1.dnsv2.com");

            Parallel.ForEach(domains, domain =>
            {
                Console.WriteLine("Querying DNS records for domain: {0}", domain);
                QueryResult queryResult = DNSQuery(dns, domain);
                Console.Write(queryResult.ToString());
                //fullResult.Append(queryResult.ToString());
                bool ret = false;

                if (queryResult.NSCount < 1)
                {
                    results.Append(queryResult.ToString());
                    return;
                }

                foreach (var ns in queryResult.NSList)
                {
                    var t = ns;
                    if (ns.Contains("="))
                        t = ParseNameServer(ns);

                    try
                    {
                        IPHostEntry ip = Dns.GetHostEntry(t);
                        QueryResult temp = DNSQuery(ip.AddressList[0], domain);
                        if (temp.NSCount > 0 || whiteList.Contains(t))
                        {
                            ret = true;
                            break;
                        }
                        else
                        {
                            queryResult.Error += string.Format("\n[V]: ns->{0}, Count->{1}", t, temp.NSCount);
                        }
                    }
                    catch (Exception ex)
                    {
                        queryResult.Error += string.Format("\n[V]: ns->{0}, Error->{1}", t, ex.Message);
                        Console.WriteLine("Validate domain: {0}, ns: {1} Error: {2}", domain, t, ex.Message);
                    }
                }

                if (!ret)
                {
                    queryResult.Error += "\n[V]: validate domian fail.";
                    results.Append(queryResult.ToString());
                }
            });

            Save(invalidDomains, results.ToString());
        }

        public static QueryResult DNSQuery(IPAddress dnsServer, string domain)
        {
            if (dnsServer == null)
                dnsServer = IPAddress.Parse("8.8.8.8");

            QueryResult queryResult = new QueryResult()
            {
                Domain = domain,
                DNS = dnsServer.ToString(),
                NSCount = -1
            };

            Response response = null;
            try
            {
                // create a DNS request
                Request request = new Request();
                request.AddQuestion(new Question(domain, DnsType.NS, DnsClass.IN));

                response = Resolver.Lookup(request, dnsServer);
            }
            catch (Exception ex)
            {
                queryResult.Error = ex.Message;
            }

            if (response == null)
            {
                queryResult.Info = "No answer";
                return queryResult;
            }

            queryResult.Info = response.AuthoritativeAnswer ? "authoritative answer" : "Non-authoritative answer";

            // queryResult.NSCount = response.Answers.Length + response.AdditionalRecords.Length + response.NameServers.Length;

            foreach (Answer answer in response.Answers)
            {
                if (answer.Record != null)
                    queryResult.NSList.Add(answer.Record.ToString());
            }

            foreach (AdditionalRecord additionalRecord in response.AdditionalRecords)
            {
                if (additionalRecord.Record != null)
                    queryResult.NSList.Add(additionalRecord.Record.ToString());
            }

            foreach (NameServer nameServer in response.NameServers)
            {
                if (nameServer.Record != null)
                    queryResult.NSList.Add(nameServer.Record.ToString());
            }

            queryResult.NSCount = queryResult.NSList.Count;

            return queryResult;
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

        private static bool IsFileExist(string fileName)
        {
            DateTime dt = File.GetLastWriteTime(fileName);
            FileInfo fileInfo = new FileInfo(fileName);

            return (dt.ToString("yyyy-MM-dd").Equals(DateTime.Now.ToString("yyyy-MM-dd")) && fileInfo.Length > 0);
        }

        private static string TrimEasyList()
        {
            StringBuilder sBuilder = new StringBuilder();
            using (StreamReader sr = new StreamReader(ChinaListConst.EASYLIST, Encoding.UTF8))
            {
                string easyListContent = sr.ReadToEnd();
                string[] lists = Regex.Split(easyListContent, @"! \*\*\* ");

                foreach (var list in lists)
                {
                    var t = list.Trim();

                    if (IsEasyListItemOn(t))
                    {
                        var index = t.IndexOf("!-----------------");
                        if (index > 0)
                            t = t.Remove(index);

                        t = t.TrimEnd(new char[] { '\r', '\n' });
                        sBuilder.AppendLine("! *** " + t);
                    }
                }
            }

            return sBuilder.Replace("\r", string.Empty).ToString();
        }

        private static string TrimEasyPrivacy()
        {
            StringBuilder sBuilder = new StringBuilder();
            using (StreamReader sr = new StreamReader(ChinaListConst.EASYPRIVACY, Encoding.UTF8))
            {
                string easyPrivacyContent = sr.ReadToEnd();

                string[] lists = Regex.Split(easyPrivacyContent, @"! \*\*\* ");

                foreach (var list in lists)
                {
                    var t = list.Trim();

                    if (t.StartsWith(ChinaListConst.HEAD)
                        || IsEasyPrivacyOff(t))
                        continue;

                    if (t.IndexOf("_international.txt") > -1)
                    {
                        int chinese = t.IndexOf("! Chinese");
                        if (chinese < 0)
                            continue;

                        int czech = t.IndexOf("! Czech");
                        if (czech < 0)
                            czech = t.IndexOf("! Danish");

                        int length = t.IndexOf(".txt ***");
                        t = t.Substring(0, length + 9) + t.Substring(chinese, czech - chinese);
                    }

                    var index = t.IndexOf("!-----------------");
                    if (index > 0)
                        t = t.Remove(index);

                    t = t.TrimEnd(new char[] { '\r', '\n' });
                    sBuilder.AppendLine("! *** " + t);
                }
            }

            return sBuilder.Replace("\r", string.Empty).ToString();
        }

        private static string ParseNameServer(string ns)
        {
            string temp = string.Empty;
            temp = ns.Split('=')[1].Trim();
            temp = temp.Split('\n')[0].Trim();

            return temp;
        }

        private static StringBuilder RemoveDuplicateFilter(StringBuilder sBuilder)
        {
            var list = new List<string>(sBuilder.ToString().Split('\n')).Distinct<string>();
            var t = new List<string>();

            sBuilder.Clear();

            foreach (var f in list)
            {
                if (t.Contains(f))
                    continue;

                t.Add(f);
                sBuilder.AppendLine(f);
            }

            return sBuilder;
        }

        private static Configurations GetConfigurations()
        {
            if (File.Exists(ChinaListConst.PATCH_FILE))
            {
                using (StreamReader sr = new StreamReader(ChinaListConst.PATCH_FILE, Encoding.UTF8))
                {
                    string xml = sr.ReadToEnd();
                    return SimpleSerializer.XmlDeserialize<Configurations>(xml);
                }
            }

            return null;
        }



        private static bool IsEasyListItemOn(string value)
        {
            Configurations patchconfig = GetConfigurations();
            List<string> easyList = null;

            if (patchconfig == null || patchconfig.EasyListFlag == null)
            {
                easyList = new List<string>(new string[] {ChinaListConst.EASYLIST_EASYLIST_GENERAL_BLOCK,
                                        ChinaListConst.EASYLIST_EASYLIST_GENERAL_HIDE,
                                        ChinaListConst.EASYLIST_EASYLIST_GENERAL_POPUP,
                                        ChinaListConst.EASYLIST_GENERAL_BLOCK_DIMENSIONS,
                                        ChinaListConst.EASYLIST_EASYLIST_ADSERVERS,
                                        ChinaListConst.EASYLIST_ADSERVERS_POPUP,
                                        ChinaListConst.EASYLIST_EASYLIST_THIRDPARTY,
                                        ChinaListConst.EASYLIST_THIRDPARTY_POPUP});
            }
            else
            {
                easyList = patchconfig.EasyListFlag;
            }

            foreach (var s in easyList)
            {
                if (value.IndexOf(s) > -1)
                    return true;
            }

            return false;
        }

        private static bool IsEasyPrivacyOff(string value)
        {
            List<string> easyPrivacy = null;
            Configurations patchconfig = GetConfigurations();

            if (patchconfig == null || patchconfig.EasyPrivacyFlag == null)
            {
                easyPrivacy = new List<string>(
                 new string[] {
                                ChinaListConst.EASYPRIVACY_WHITELIST,
                                ChinaListConst.EASYPRIVACY_WHITELIST_INTERNATIONAL
                            });
            }
            else
            {
                easyPrivacy = patchconfig.EasyPrivacyFlag;
            }

            foreach (var s in easyPrivacy)
            {
                if (value.IndexOf(s) > -1)
                    return true;
            }

            return false;
        }

        private static bool DownloadEasyList(WebProxy proxy)
        {
            using (WebClient webClient = new WebClient())
            {
                if (proxy != null)
                {
                    webClient.Proxy = proxy;
                    Console.WriteLine("use proxy: {0}", proxy.Address.Authority.ToString());
                }

                Dictionary<string, string> lists = new Dictionary<string, string>();
                lists.Add(ChinaListConst.EASYLIST, ChinaListConst.EASYLIST_URL);
                lists.Add(ChinaListConst.EASYPRIVACY, ChinaListConst.EASYPRIVACY_URL);

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
                            return false;
                        }
                    }
                }
            }

            return true;
        }
    }
}
