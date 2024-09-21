// InBuffer.cs

namespace UnityBundleReader._7zip.Common
{
	public class InBuffer
	{
		byte[] _mBuffer;
		uint _mPos;
		uint _mLimit;
		uint _mBufferSize;
		Stream _mStream;
		bool _mStreamWasExhausted;
		ulong _mProcessedSize;

		public InBuffer(uint bufferSize)
		{
			_mBuffer = new byte[bufferSize];
			_mBufferSize = bufferSize;
		}

		public void Init(Stream stream)
		{
			_mStream = stream;
			_mProcessedSize = 0;
			_mLimit = 0;
			_mPos = 0;
			_mStreamWasExhausted = false;
		}

		public bool ReadBlock()
		{
			if (_mStreamWasExhausted)
				return false;
			_mProcessedSize += _mPos;
			int aNumProcessedBytes = _mStream.Read(_mBuffer, 0, (int)_mBufferSize);
			_mPos = 0;
			_mLimit = (uint)aNumProcessedBytes;
			_mStreamWasExhausted = (aNumProcessedBytes == 0);
			return (!_mStreamWasExhausted);
		}


		public void ReleaseStream()
		{
			// m_Stream.Close(); 
			_mStream = null;
		}

		public bool ReadByte(byte b) // check it
		{
			if (_mPos >= _mLimit)
				if (!ReadBlock())
					return false;
			b = _mBuffer[_mPos++];
			return true;
		}

		public byte ReadByte()
		{
			// return (byte)m_Stream.ReadByte();
			if (_mPos >= _mLimit)
				if (!ReadBlock())
					return 0xFF;
			return _mBuffer[_mPos++];
		}

		public ulong GetProcessedSize()
		{
			return _mProcessedSize + _mPos;
		}
	}
}
