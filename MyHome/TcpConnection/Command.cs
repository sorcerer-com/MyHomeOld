using MyHome.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace MyHome.TcpConnection
{
    public class Command
    {
        public const int MinBytes = 8; // command conatains minimum 8 bytes

        public ECommandType Type { get; set; }
        public List<object> Arguments { get; private set; }


        public Command()
        {
            this.Type = ECommandType.Invalid;
            this.Arguments = new List<object>();
        }

        public Command(ECommandType type) : this()
        {
            this.Type = type;
        }

        public Command(ECommandType type, List<object> arguments) : this()
        {
            this.Type = type;
            if (arguments != null)
                this.Arguments = new List<object>(arguments);
        }

        public Command(Command cmd) : this(cmd.Type, cmd.Arguments)
        {
        }


        public List<byte> Serialize()
        {
            List<byte> data = new List<byte>();

            data.AddRange(BitConverter.GetBytes((int)this.Type));
            data.AddRange(BitConverter.GetBytes(this.Arguments.Count));

            int count = 0;
            for (int i = 0; i < this.Arguments.Count; i++)
            {
                object arg = this.Arguments[i];
                if (count == 0) // count arguments with equal types
                {
                    while (i + count < this.Arguments.Count && arg.GetType() == this.Arguments[i + count].GetType())
                        count++;

                    Flags flags = new Flags();
                    if (arg is byte) 
                        flags.SetFlag(1, true);
                    else if (arg is int) 
                        flags.SetFlag(2, true);
                    else if (arg is double)
                        flags.SetFlag(3, true);
                    else if (arg is string)
                        flags.SetFlag(4, true);
                    
                    if (count > 255) 
                        flags.SetFlag(5, true);
                    
                    data.Add(flags.Byte);
                    if (count > 255)
                        data.AddRange(BitConverter.GetBytes(count));
                    else
                        data.Add((byte)count);
                }

                if (arg is byte)  
                    data.Add((byte)arg);
                else if (arg is int)
                    data.AddRange(BitConverter.GetBytes((int)arg));
                else if (arg is double)
                    data.AddRange(BitConverter.GetBytes((double)arg));
                else if (arg is string)
                {
                    byte[] bytes = Encoding.Unicode.GetBytes((string)arg);
                    data.AddRange(BitConverter.GetBytes(bytes.Length));
                    data.AddRange(bytes);
                }

                count--;
            }
            return data;
        }

        public void DeSerialize(List<byte> data)
        {
            byte[] bytes = data.ToArray();
            int start = 0;
         
            this.Type = (ECommandType)BitConverter.ToInt32(bytes, start);
            start += 4;
            int size = BitConverter.ToInt32(bytes, start);
            start += 4;
            this.Arguments = new List<object>(size);

            int count = 0;
            Flags flags = new Flags();
            for (int i = 0; i < size; i++)
            {
                if (count == 0) // get count of arguments with equal types
                {
                    flags = new Flags(bytes[start]);
                    start++;
                    if (flags.GetFlag(5))
                    {
                        count = BitConverter.ToInt32(bytes, start);
                        start += 4;
                    }
                    else
                    {
                        count = bytes[start];
                        start++;
                    }
                }

                if (flags.GetFlag(1))
                {
                    this.Arguments.Add(bytes[start]);
                    start += 1;
                }
                else if (flags.GetFlag(2))
                {
                    this.Arguments.Add(BitConverter.ToInt32(bytes, start));
                    start += 4;
                }
                else if (flags.GetFlag(3))
                {
                    this.Arguments.Add(BitConverter.ToDouble(bytes, start));
                    start += 8;
                }
                else if (flags.GetFlag(4))
                {
                    int len = BitConverter.ToInt32(bytes, start);
                    start += 4;
                    string str = Encoding.Unicode.GetString(bytes, start, len);
                    start += len;

                    this.Arguments.Add(str);
                }

                count--;
            }
            data.RemoveRange(0, start);
        }


        public override string ToString()
        {
            string res = this.Type +": ";
            if (this.Arguments.Count < 100)
            {
                foreach (object obj in this.Arguments)
                    res += obj + ", ";
            }
            else
                res += "a lot of arguments (" + this.Arguments.Count + ")";
            return res;
        }

    }
}
