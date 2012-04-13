using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyDnsPackage.ResourceRecord
{
   public  class NS_RR
    {
        public string NameServer { get; set; }
        public override string ToString()
        {
            return NameServer;
        }
        public NS_RR(byte[] data, int offset, int len)
        {
            int labelLen;
            NameServer += MyDns.GetLabelName(data, offset, out  labelLen);
          
        }
    }
}
