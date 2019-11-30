using System;

public class Crc32
{
	const uint polynomial = 0xedb88320u;
	uint[] table = new uint[256];

	public Crc32()
	{
		InitializeTable();
	}

	uint[] InitializeTable()
	{
		for (var i = 0; i < 256; i++)
		{
			var entry = (UInt32)i;
			for (var j = 0; j < 8; j++)
				if ((entry & 1) == 1)
					entry = (entry >> 1) ^ polynomial;
				else
					entry = entry >> 1;
			table[i] = entry;
		}
		return table;
	}

	public bool IsDataCorrupted(byte[] data, uint crc)
	{
		return (Compute(data) != crc);
	}

	uint Compute(byte[] data)
	{
		return CalculateCrc(data);
	}

	public uint CalculateCrc(byte[] buffer)
	{
		uint crc = 0;
		for (int i = 0; i < buffer.Length; i++)
			crc = (crc >> 8) ^ table[buffer[i] ^ crc & 0xff];
		return crc;
	}
}