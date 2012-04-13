using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyDnsPackage.ResourceRecord
{
  public   class TXT_RR
    {
      public string text { get; set; }
      public override string ToString()
      {
          return text;
      }
      public TXT_RR(byte[] data, int offset, int len)
      {
          //由于txt的字段有可能大于63，超出一般GetLabelName的字符串长度。
          StringBuilder build = new StringBuilder(len);
          for (; len > 0; len--)
          {
              build.Append((char)data[offset++]);
          }
          text = build.ToString();
      }
    }
}
