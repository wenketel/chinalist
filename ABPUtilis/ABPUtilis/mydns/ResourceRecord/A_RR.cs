using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyDnsPackage.ResourceRecord
{
 public    class A_RR
    {
     public string address { get; set; }
     public override string ToString()
     {
         return address;
     }
     public A_RR(byte[] data, int offset, int len)
     {
         for (int i = 0; i < 4; i++)
         {
             address += data[offset++].ToString() + ".";
         }
         address = address.TrimEnd('.');
     }
    }
}
