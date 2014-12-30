using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyHome.Utils
{
    public class Flags
    {
        private ulong flags;


        public byte Byte { get { return (byte)this.flags; } }

        public uint Int { get { return (uint)this.flags; } }

        public ulong Long { get { return (ulong)this.flags; } }


        public Flags()
        {
            this.flags = 0;
        }

        public Flags(ulong flags)
        {
            this.flags = flags;
        }

        public Flags(Flags flags)
        {
            this.flags = flags.flags;
        }

        public bool GetFlag(ulong flag)
        {
            return (this.flags & (ulong)Math.Pow(2, flag - 1)) != 0;
        }

        public void SetFlag(ulong flag, bool value)
        {
            if (value)
                this.flags |= (ulong)Math.Pow(2, flag - 1);
            else
                this.flags &= ~(ulong)Math.Pow(2, flag - 1);
        }
    }
}
