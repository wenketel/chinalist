using System.Collections.Generic;
using System.Text;

namespace ABPUtils
{
    class QueryResult
    {
        public string Domain
        {
            get;
            set;
        }

        public string DNS
        {
            get;
            set;
        }

        public int NSCount
        {
            get;
            set;
        }

        public List<string> NSList
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

        public QueryResult()
        {
            NSList = new List<string>();
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("Domain:\t{0}\n", Domain);
            sb.AppendFormat("DNS:\t{0}\n", DNS);
            sb.AppendFormat("Info:\t{0}\n", Info);
            sb.AppendFormat("Count:\t{0}\n", NSCount);
            foreach (var ns in NSList)
            {
                sb.AppendFormat("NS => {0}\n", ns);
            }
            sb.AppendFormat("Error:\t{0}\n", string.IsNullOrEmpty(Error) ? "None" : Error);
            sb.AppendLine("--------------------------------------------------------------");

            return sb.ToString();
        }
    }
}
