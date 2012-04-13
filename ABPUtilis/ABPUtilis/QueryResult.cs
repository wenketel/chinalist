using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ABPUtils
{
    class QueryResult
    {
        public string Url
        {
            get;
            set;
        }

        public string DNS
        {
            get;
            set;
        }

        public int NS
        {
            get;
            set;
        }

        public string Info
        {
            get;
            set;
        }

        public string Error
        {
            get;
            set;
        }

        public override string ToString()
        {

            return string.Format("Url:{0}\nDNS:{1}\nNS:{2}\nInfo:{3}\nError:{4}",
                    Url, DNS, NS, Info, string.IsNullOrEmpty(Error) ? "None." : Error
                );
        }
    }
}
