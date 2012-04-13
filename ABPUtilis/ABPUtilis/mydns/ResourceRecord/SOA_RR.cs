using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyDnsPackage.ResourceRecord
{
  public   class SOA_RR
    {
      public string NameServer { get; set; }
      public string Mail { get; set; }
      public int Serial { get; set; }
      public int Refresh { get; set; }
      public int Retry { get; set; }
      public int Expire { get; set; }
      public int TTL { get; set; }
      public override string ToString()
      {
          return string.Format("nameServer={0} | mail={1} | serial={2} | refresh={3} | ...",NameServer ,Mail,Serial ,Refresh );
      }
      public SOA_RR(byte[] data, int offset, int len)
      {
          int endOffset = offset + len;
          int labelLen;
          NameServer = MyDns.GetLabelName(data, offset, out labelLen);
          offset += labelLen;
          Mail = MyDns.GetLabelName(data, ++offset, out labelLen);
          offset += labelLen;
          offset++;
          Serial = data[offset++] * 256 * 256 * 256 + data[offset++] * 256 * 256 + data[offset++] * 256 + data[offset++];
          Refresh =data[offset++] * 256 * 256 * 256 + data[offset++] * 256 * 256 + data[offset++] * 256 + data[offset++];
          Retry = data[offset++] * 256 * 256 * 256 + data[offset++] * 256 * 256 + data[offset++] * 256 + data[offset++];
          Expire = data[offset++] * 256 * 256 * 256 + data[offset++] * 256 * 256 + data[offset++] * 256 + data[offset++];
          TTL = data[offset++] * 256 * 256 * 256 + data[offset++] * 256 * 256 + data[offset++] * 256 + data[offset++];
      }
    }
}
