using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyDnsPackage
{
 public   class MyDnsQuestion
    {
        //查询结构的3个组成
        byte[] _name;
        byte[] _type;
        byte[] _class;

        public MyDnsQuestion()
        {
            //初始化，默认查询A记录
            _type = new byte[] { 0x00, (byte)QueryType.A };
            _class = new byte[] { 0x00, (byte)QueryClass.IN };
        }
        public void Parse(byte[] data)
        {
            int offset = 12;
            for (int i = offset; i < data.Length; i++)
            {
                if (data[i] == 0)
                {
                    _name = new byte[i - offset + 1];
                    for (int j = offset; j <= i; j++)
                    {
                        _name[j - offset] = data[j];
                    }
                    break;
                }
            }
            int nLen = _name.Length;
            _type = new byte[] { data[offset + nLen], data[offset + nLen + 1] };
            _class = new byte[] { data[offset + nLen + 2], data[offset + nLen + 3] };
        }
        public byte[] GetBytes()
        {
            byte[] result = new byte[_name.Length + _type.Length + _class.Length];
            _name.CopyTo(result, 0);
            _type.CopyTo(result, _name.Length);
            _class.CopyTo(result, result.Length - _class.Length);
            return result;
        }
        public QueryType Type
        {
            set
            {
                _type = new byte[] { 0x00, (byte)value };
            }
        }
        public QueryClass Class
        {
            set
            {
                _class = new byte[] { 0x00, (byte)value };
            }
        }
        public string Qname
        {
            set
            {
                string[] arr = value.Split('.');
                _name = new byte[value.Length + 2];
                int seek = 0;
                foreach (string word in arr)
                {
                    byte[] len = new byte[] { (byte)word.Length };
                    len.CopyTo(_name, seek);
                    Encoding.UTF8.GetBytes(word).CopyTo(_name, seek + 1);
                    seek += word.Length + 1;
                }
            }
            get
            {
                int len = 0;
                int seek = 0;
                StringBuilder domain = new StringBuilder();
                len = (int)_name[0];
                while (len != 0)
                {
                    for (int i = len; i > 0; i--)
                    {
                        char word = (char)_name[++seek];
                        domain.Append(word);
                    }

                    len = (int)_name[++seek];
                    if (len > 0) domain.Append('.');
                }
                return domain.ToString();
            }
        }
    }
}
