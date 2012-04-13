using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyDnsPackage.ResourceRecord
{
    public class MX_RR
    {
        public int Preference { get; set; }
        public string Mail { get; set; }
        public override string ToString()
        {
            return string.Format("Preference={0} | Mail={1}", Preference, Mail);
        }
        public MX_RR(byte[] data, int offset, int len)
        {
            Preference = data[offset++] * 256 + data[offset++];
            int labelLen;
            Mail = MyDns.GetLabelName(data, offset, out  labelLen);

        }
    }
}
