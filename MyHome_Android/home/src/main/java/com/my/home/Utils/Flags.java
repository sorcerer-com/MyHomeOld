package com.my.home.Utils;

/**
 * Created by Hristo on 14-03-07.
 */
public class Flags {
	private long flags;


	public Flags() {
		this.flags = 0;
	}

	public Flags(long flags) {
		this.flags = flags;
	}

	public Flags(Flags flags) {
		this.flags = flags.flags;
	}
	

	public boolean getFlag(long flag) {
		return (this.flags & (long)Math.pow(2, flag - 1)) != 0;
	}

	public void setFlag(long flag, boolean value) {
		if (value)
			this.flags |= (long)Math.pow(2, flag - 1);
		else
			this.flags &= ~(long)Math.pow(2, flag - 1);
	}


    public byte getByte() { return (byte)this.flags; }

    public int getInt() { return (int)this.flags; }

    public long getLong() { return (long)this.flags; }

}