using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace MyDnsPackage
{
    public class MyDns
    {

        public MyDnsHeader header;
        public MyDnsQuestion question;
        public MyDnsRecord record;
        public byte[] recvData;

        private readonly int local_port = 8807;
        private readonly short package_ttl = 3;

        private byte[] newid()
        {
            byte[] result = new byte[2];
            Random rd = new Random();
            rd.NextBytes(result);
            return result;
        }

        public bool Search(string host, QueryType? qtype, string dns, byte[] id)
        {
            string _dns = string.IsNullOrEmpty(dns) ? "8.8.8.8" : dns;
            byte[] _id = id == null ? newid() : id;
            QueryType _qtype = qtype == null ? QueryType.A : (QueryType)qtype;

            header = new MyDnsHeader();
            header.NewID(_id);
            question = new MyDnsQuestion();
            question.Class = QueryClass.IN;
            question.Type = _qtype;
            question.Qname = host;
            byte[] data_head = header.GetBytes();
            byte[] data_question = question.GetBytes();
            byte[] sendData = new byte[data_head.Length + data_question.Length];
            data_head.CopyTo(sendData, 0);
            data_question.CopyTo(sendData, data_head.Length);

            byte[] recvData;
            recvData = udpSend(sendData, _dns);
            Parse(recvData);
            return header.RCODE == 0 ? true : false;
        }

        void Parse(byte[] data)
        {
            header.Parse(data);
            question.Parse(data);
            record = new MyDnsRecord();
            record.Parse(data, question.GetBytes().Length + 12);
        }

        byte[] tcpSend(byte[] sendData, string remoteHost)
        {
            //未实现
            return new byte[12];
        }

        byte[] udpSend(byte[] sendData, string remoteHost)
        {
            IPEndPoint host = new IPEndPoint(IPAddress.Parse(remoteHost), 53);
            IPEndPoint myip = new IPEndPoint(IPAddress.Any, local_port);
            UdpClient myClient = new UdpClient(myip);
            //myClient.Ttl = package_ttl;
            myClient.Send(sendData, sendData.Length, host);
            byte[] recvData = myClient.Receive(ref host);
            myClient.Close();
            return recvData;
        }
        public static string GetLabelName(byte[] data, int offset, out int labelLen)
        {
            bool alreadyJump = false;
            int seek = offset;
            int len = data[seek];
            labelLen = 0;
            StringBuilder result = new StringBuilder(63);
            while (len > 0 && seek < data.Length)
            {
                if (len > 191 && len < 255)
                {
                    if (alreadyJump)
                    {
                        labelLen = seek - offset;
                        return result.ToString();
                    }
                    int tempLen;
                    result.Append(GetLabelName(data, data[++seek] + (len - 192) * 256, out tempLen));
                    alreadyJump = true;
                    labelLen = seek - offset;
                    return result.ToString();
                }
                else if (len < 64)
                {
                    for (; len > 0; len--)
                    {
                        result.Append((char)data[++seek]);
                    }
                    len = data[++seek];
                    if (len > 0) result.Append(".");
                }
            }
            labelLen = seek - offset;
            return result.ToString();
        }
    }
}
