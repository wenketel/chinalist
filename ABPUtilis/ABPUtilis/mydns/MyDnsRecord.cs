using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MyDnsPackage.ResourceRecord;
namespace MyDnsPackage
{
  public   class MyDnsRecord
    {
        // NAME    资源记录包含的域名
        //TYPE    2个字节表示资源记录的类型，指出RDATA数据的含义
        //CLASS   2个字节表示RDATA的类
        //TTL     4字节无符号整数表示资源记录可以缓存的时间。0代表只能被传输，但是不能被缓存。
        //RDLENGTH        2个字节无符号整数表示RDATA的长度
        //RDATA   不定长字符串来表示记录，格式根TYPE和CLASS有关。比如，TYPE是A，CLASS 是 IN，那么RDATA就是一个4个字节的ARPA网络地址。
        #region Field
        public string Name
        {
            get;
            set;
        }
        //byte[] _name;
        public QueryType QType
        {
            get;
            set;
        }
        public QueryClass QClass
        {
            get;
            set;
        }
        public int TTL
        {
            get;
            set;
        }
        public int RDLength
        {
            get;
            set;
        }
        public object RDDate
        {
            get;
            set;

        }
        //byte[] _RDDate;
        #endregion

        /// <summary>
        /// 资源集合
        /// </summary>
        public List<MyDnsRecord> Records = new List<MyDnsRecord>();

        public void Parse(byte[] data, int offset)
        {
            while (offset < data.Length)
            {
                int labelLen;
                MyDnsRecord RecordItem = new MyDnsRecord();
                RecordItem.Name = MyDns.GetLabelName(data, offset, out labelLen);
                offset += labelLen;
                //
                offset ++;
                RecordItem.QType = (QueryType)data[++offset];
                //
                offset++;
                RecordItem.QClass = (QueryClass)data[++offset];
                //
                offset++;
                RecordItem.TTL = data[offset++] * 256 * 256 * 256 + data[offset++] * 256 * 256 + data[offset++] * 256 + data[offset++];
                //
               RecordItem.RDLength = data[offset++] * 256 + data[offset++];
                //
               switch (RecordItem.QType)
               {
                   case QueryType.A:
                       RecordItem.RDDate = new A_RR(data, offset, RecordItem.RDLength);
                       break;
                   case QueryType.CNAME :
                       RecordItem.RDDate = new CNAME_RR(data, offset, RecordItem.RDLength);
                       break;
                   case QueryType.MX :
                       RecordItem.RDDate = new MX_RR(data, offset, RecordItem.RDLength);
                       break;
                   case QueryType.NS:
                       RecordItem.RDDate = new NS_RR(data, offset, RecordItem.RDLength);
                       break;
                   case QueryType.SOA:
                       RecordItem.RDDate = new SOA_RR(data, offset, RecordItem.RDLength);
                       break;
                   case QueryType.TXT:
                       RecordItem.RDDate =new TXT_RR (data, offset, RecordItem.RDLength);
                       break;
               }
               Records.Add(RecordItem);
               offset += RecordItem.RDLength;

            }
        }

    }
}
