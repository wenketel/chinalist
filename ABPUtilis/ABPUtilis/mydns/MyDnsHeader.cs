using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyDnsPackage
{
  public   class MyDnsHeader
    {
        //头部结构为固定长度12字节
        //ID：16Bit，报文编号，由客户端指定，服务端回复的时候返回相同
        //QR： 1Bit，0代表查询，1代表回复
        //Opcode：4Bit，查询代码，0：标准查询；1：反向查询；2：服务器状态查询
        //AA：1Bit，是否权威回复
        //TC：1Bit，是否截断超过512字节部分
        //RD：1Bit，1：递归查询；0：否
        //RA：1Bit，服务器回复时指定，1：支持递归查询
        //Z：3Bit，保留，值为0
        //RCode：4Bit，服务器回复时指定，0:无差错；1:格式错；2:DNS出错；3:域名不存在；4:DNS不支持这类查询；5:DNS拒绝查询；6-15:保留字段。
        //      QDCOUNT:占16位，2字节。一个无符号数指示查询记录的个数。 
        //ANCOUNT:占16位，2字节。一个无符号数指明回复记录的个数。 
        //NSCOUNT:占16位，2字节。一个无符号数指明权威记录的个数。 
        //ARCOUNT:占16位，2字节。一个无符号数指明额外记录的个数。 

        //头部的12字节，初始化设置
        public byte[] Header = new byte[]{
            0x00,0xff,      //ID
            0x01,           //QR1+Opcode4+AA1+TC1+RD1
            0x00,          //RA1+Z3+Rcode4
            0x00,0x01,
            0x00,0x00,
            0x00,0x00,
            0x00,0x00
        };
        public void Parse(byte[] data)
        {
            Header = new byte[12];
            for (int i = 0; i < 12; i++)
            {
                Header[i] = data[i];
            }
        }
        /// <summary>
        /// 额外记录的个数
        /// </summary>
        public int ARCOUNT
        {
            get
            {
                return Header[10] * 256 + Header[11];
            }
        }
        /// <summary>
        /// 权威记录的个数
        /// </summary>
        public int NSCOUNT
        {
            get
            {
                return Header[8] * 256 + Header[9];
            }
        }
        /// <summary>
        /// 回复记录的个数
        /// </summary>
        public int ANCOUNT
        {
            get
            {
                return Header[6] * 256 + Header[7];
            }
        }
        /// <summary>
        /// 服务器返回码
        /// </summary>
        public RCode RCODE
        {
            get
            {
                return (RCode)(Header[3] & 0x0f);
            }
        }
        /// <summary>
        /// 查询(0)还是回复(1)
        /// </summary>
        public int QR
        {
            get
            {
                byte qr = Header[2];
                return (qr >> 7);
            }
        }
        /// <summary>
        /// 标识id
        /// </summary>
        public byte[] ID
        {
            get
            {
                return new byte[] { Header[0], Header[1] };
            }
        }
        /// <summary>
        /// 返回整个头部数据
        /// </summary>
        /// <returns></returns>
        public byte[] GetBytes()
        {
            return Header;
        }
        /// <summary>
        /// 设置标识id
        /// </summary>
        /// <param name="ID"></param>
        public void NewID(byte[] ID)
        {
            Header[0] = ID[0];
            Header[1] = ID[1];
        }
    }
}
