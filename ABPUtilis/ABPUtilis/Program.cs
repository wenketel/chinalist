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
    class Program
    {
        static void Main(string[] args)
        {
            if (null == args || args.Length == 0)
            {
                Console.WriteLine("wrong input argument.");
                return;
            }

            var arguments = new Arguments(args);
            DispatcherTask(arguments);

            //Console.WriteLine("Press any key to continue...");
            //Console.ReadKey();
        }

        static void DispatcherTask(Arguments args)
        {
            if (args.IsTrue("help") || args.IsTrue("h"))
            {
                Console.WriteLine(ConstString.HELP_INFO);
            }
            else if (args.IsTrue("version"))
            {
                Console.WriteLine("ABPUtils version: {0}", GetVersion());
            }
            else if (args.IsTrue("ns") || args.IsTrue("nslookup"))
            {
                var domain = args.Single("d");

                if (string.IsNullOrEmpty(domain))
                    domain = args.Single("domain");

                if (string.IsNullOrEmpty(domain))
                {
                    Console.WriteLine("wrong input domain.");
                    return;
                }

                QueryResult result = null;
                if (string.IsNullOrEmpty(args.Single("dns")))
                    result = DNSQuery(null, domain);
                else
                    result = DNSQuery(IPAddress.Parse(args.Single("dns")), domain);

                if (result == null)
                {
                    Console.WriteLine("Query result is null.");
                }
                else
                {
                    Console.Write(result.ToString());
                }
            }
            else if (args.IsTrue("v") || args.IsTrue("validate"))
            {
                var input = args.Single("i");
                if (string.IsNullOrEmpty(input))
                    input = args.Single("input");

                if (string.IsNullOrEmpty(input))
                {
                    Console.WriteLine("wrong input file.");
                    return;
                }

                var chinaList = new ChinaList(input);
                chinaList.Validate();
            }
            else if (args.IsTrue("u") || args.IsTrue("update"))
            {
                var input = args.Single("i");
                if (string.IsNullOrEmpty(input))
                    input = args.Single("input");

                if (string.IsNullOrEmpty(input))
                {
                    Console.WriteLine("wrong input file.");
                    return;
                }

                var chinaList = new ChinaList(input);
                chinaList.Update();
                chinaList.Validate();
            }
            else if (args.IsTrue("c") || args.IsTrue("check"))
            {
                var input = args.Single("i");
                if (string.IsNullOrEmpty(input))
                    input = args.Single("input");

                if (string.IsNullOrEmpty(input))
                {
                    Console.WriteLine("wrong input file.");
                    return;
                }

                var output = args.Single("o");

                if (string.IsNullOrEmpty(output))
                    output = args.Single("output");

                var dns = args.Single("dns");
                if (string.IsNullOrEmpty(dns))
                {
                    ValidateDomains(null, input, output);
                }
                else
                {
                    ValidateDomains(IPAddress.Parse(args.Single("dns")), input, output);
                }
            }
            else if (args.IsTrue("m") || args.IsTrue("merge"))
            {
                var input = string.Empty;
                input = args.Single("i");

                if (string.IsNullOrEmpty(input))
                    input = args.Single("input");

                if (string.IsNullOrEmpty(input))
                {
                    Console.WriteLine("wrong input file.");
                    return;
                }

                WebProxy proxy = null;
                var p = args.Single("p");
                if (string.IsNullOrEmpty(p))
                    p = args.Single("proxy");

                if (!string.IsNullOrEmpty(p))
                {
                    var temp = p.Split(':');
                    proxy = new WebProxy(temp[0], int.Parse(temp[1]));
                    proxy.BypassProxyOnLocal = true;
                }

                var output = args.Single("o");
                if (string.IsNullOrEmpty(output))
                    output = args.Single("output");

                Merge(input, proxy, args.IsTrue("patch"), output);
            }
        }

        /// <summary>
        /// Get assembly version
        /// </summary>
        /// <returns></returns>
        static string GetVersion()
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
                lists.Add(ConstString.EASYLIST, ConstString.EASYLIST_URL);
                lists.Add(ConstString.EASYPRIVACY, ConstString.EASYPRIVACY_URL);
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
                var headerIndex = chinaListContent.IndexOf(ConstString.CHINALIST_LAZY_HEADER_MARK);
                chinaListContent = chinaListContent.Substring(headerIndex).Insert(0, ConstString.CHINALIST_LAZY_HEADER);
                var index = chinaListContent.IndexOf(ConstString.CHINALIST_END_MARK);
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
            if (File.Exists(ConstString.PATCH_FILE) && patch)
            {
                Console.WriteLine("use {0} to patch {1}", ConstString.PATCH_FILE, lazyList);
                using (StreamReader sr = new StreamReader(ConstString.PATCH_FILE, Encoding.UTF8))
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
            sBuilder.AppendLine(ConstString.CHINALIST_END_MARK);

            Console.WriteLine(string.Format("Merge {0}, {1} and {2}.", chinaList, ConstString.EASYLIST, ConstString.EASYPRIVACY));
            ChinaList.Save(lazyList, sBuilder.ToString());

            cl = new ChinaList(lazyList);
            cl.Update();
            cl.Validate();

            Console.WriteLine("End of merge and validate.");
        }

        /// <summary>
        /// validate domain by nslookup
        /// </summary>
        /// <param name="dns"></param>
        /// <param name="fileName"></param>
        /// <param name="invalidDomains"></param>
        static void ValidateDomains(IPAddress dns, string fileName, string invalidDomains = "invalid_domains.txt")
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

            ChinaList.Save(invalidDomains, results.ToString());
            // ChinaList.Save("full_domains.txt", fullResult.ToString());
        }

        static QueryResult DNSQuery(IPAddress dnsServer, string domain)
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

        static bool IsFileExist(string fileName)
        {
            DateTime dt = File.GetLastWriteTime(fileName);
            FileInfo fileInfo = new FileInfo(fileName);

            return (dt.ToString("yyyy-MM-dd").Equals(DateTime.Now.ToString("yyyy-MM-dd")) && fileInfo.Length > 0);
        }

        static string TrimEasyList()
        {
            StringBuilder sBuilder = new StringBuilder();
            using (StreamReader sr = new StreamReader(ConstString.EASYLIST, Encoding.UTF8))
            {
                string easyListContent = sr.ReadToEnd();
                string[] t = Regex.Split(easyListContent, @"! \*\*\* ");

                for (int i = 1; i < t.Length; i++)
                {
                    if (i == ConstString.EASYLIST_EASYLIST_SPECIFIC_BLOCK || i == ConstString.EASYLIST_ADULT_ADULT_SPECIFIC_BLOCK
                            || i == ConstString.EASYLIST_EASYLIST_SPECIFIC_HIDE || i == ConstString.EASYLIST_ADULT_ADULT_SPECIFIC_HIDE
                            || i == ConstString.EASYLIST_EASYLIST_WHITELIST || i == ConstString.EASYLIST_ADULT_ADULT_WHITELIST)
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
            using (StreamReader sr = new StreamReader(ConstString.EASYPRIVACY, Encoding.UTF8))
            {
                string easyPrivacyContent = sr.ReadToEnd();

                string[] t = Regex.Split(easyPrivacyContent, @"! \*\*\* ");

                for (int i = 1; i < t.Length; i++)
                {
                    if (i == ConstString.EASYPRIVACY_WHITELIST || i == ConstString.EASYPRIVACY_WHITELIST_INTERNATIONAL)
                        continue;
                    var s = t[i];

                    if (i == ConstString.EASYPRIVACY_TRACKINGSERVERS_INTERNATIONAL || i == ConstString.EASYPRIVACY_THIRDPARTY_INTERNATIONAL
                        || i == ConstString.EASYPRIVACY_SPECIFIC_INTERNATIONAL)
                    {
                        int chinese = s.IndexOf("! Chinese");
                        if (chinese < 0)
                            continue;

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

        static string ParseNameServer(string ns)
        {
            string temp = string.Empty;
            temp = ns.Split('=')[1].Trim();
            temp = temp.Split('\n')[0].Trim();

            return temp;
        }

        static StringBuilder RemoveDuplicateFilter(StringBuilder sBuilder)
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
    }
}
