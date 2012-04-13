using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyDnsPackage.ResourceRecord
{
   public  class CNAME_RR
    {
        public string name { get; set; }
        public override string ToString()
        {
            return name;
        }
        public CNAME_RR(byte[] data, int offset, int len)
        {
            int labelLen;
            name += MyDns.GetLabelName(data, offset, out  labelLen);

        }
    }
}
