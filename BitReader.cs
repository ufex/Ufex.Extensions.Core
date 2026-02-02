using System;
using System.IO;

// Simple BitReader to handle non-byte-aligned reads
public class BitReader : IDisposable
{
    private BinaryReader _inner;
    private int _bitBuffer;
    private int _bitCount;

	public BitReader(Stream stream, bool leaveOpen = false)
	{
		_inner = new BinaryReader(stream, System.Text.Encoding.UTF8, leaveOpen: leaveOpen);
	}

    public int ReadBits(int count)
    {
        int result = 0;
        for (int i = 0; i < count; i++)
        {
            if (_bitCount == 0)
            {
                int b = _inner.ReadByte();
                if (b == -1) throw new EndOfStreamException();
                _bitBuffer = b;
                _bitCount = 8;
            }
            int bit = (_bitBuffer & 1);
            _bitBuffer >>= 1;
            _bitCount--;
            result |= (bit << i);
        }
        return result;
    }

    public void AlignToByteBoundary()
    {
        _bitCount = 0;
        _bitBuffer = 0;
    }

    public int ReadUInt16() => _inner.ReadUInt16(); // For stored blocks
    public int ReadByte() => _inner.ReadByte();
    public void Dispose() => _inner.Dispose();
}