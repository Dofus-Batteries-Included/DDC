// LzBinTree.cs

using UnityBundleReader._7zip.Common;

namespace UnityBundleReader._7zip.Compress.LZ
{
	public class BinTree : InWindow, IMatchFinder
	{
		UInt32 _cyclicBufferPos;
		UInt32 _cyclicBufferSize = 0;
		UInt32 _matchMaxLen;

		UInt32[] _son;
		UInt32[] _hash;

		UInt32 _cutValue = 0xFF;
		UInt32 _hashMask;
		UInt32 _hashSizeSum = 0;

		bool _hashArray = true;

		const UInt32 KHash2Size = 1 << 10;
		const UInt32 KHash3Size = 1 << 16;
		const UInt32 KBt2HashSize = 1 << 16;
		const UInt32 KStartMaxLen = 1;
		const UInt32 KHash3Offset = KHash2Size;
		const UInt32 KEmptyHashValue = 0;
		const UInt32 KMaxValForNormalize = ((UInt32)1 << 31) - 1;
	
		UInt32 _kNumHashDirectBytes = 0;
		UInt32 _kMinMatchCheck = 4;
		UInt32 _kFixHashSize = KHash2Size + KHash3Size;
		
		public void SetType(int numHashBytes)
		{
			_hashArray = (numHashBytes > 2);
			if (_hashArray)
			{
				_kNumHashDirectBytes = 0;
				_kMinMatchCheck = 4;
				_kFixHashSize = KHash2Size + KHash3Size;
			}
			else
			{
				_kNumHashDirectBytes = 2;
				_kMinMatchCheck = 2 + 1;
				_kFixHashSize = 0;
			}
		}

		public new void SetStream(Stream stream) { base.SetStream(stream); }
		public new void ReleaseStream() { base.ReleaseStream(); }
		
		public new void Init()
		{
			base.Init();
			for (UInt32 i = 0; i < _hashSizeSum; i++)
				_hash[i] = KEmptyHashValue;
			_cyclicBufferPos = 0;
			ReduceOffsets(-1);
		}

		public new void MovePos()
		{
			if (++_cyclicBufferPos >= _cyclicBufferSize)
				_cyclicBufferPos = 0;
			base.MovePos();
			if (Pos == KMaxValForNormalize)
				Normalize();
		}

		public new Byte GetIndexByte(Int32 index) { return base.GetIndexByte(index); }

		public new UInt32 GetMatchLen(Int32 index, UInt32 distance, UInt32 limit)
		{ return base.GetMatchLen(index, distance, limit); }

		public new UInt32 GetNumAvailableBytes() { return base.GetNumAvailableBytes(); }

		public void Create(UInt32 historySize, UInt32 keepAddBufferBefore,
				UInt32 matchMaxLen, UInt32 keepAddBufferAfter)
		{
			if (historySize > KMaxValForNormalize - 256)
				throw new Exception();
			_cutValue = 16 + (matchMaxLen >> 1);
				
			UInt32 windowReservSize = (historySize + keepAddBufferBefore +
					matchMaxLen + keepAddBufferAfter) / 2 + 256;

			base.Create(historySize + keepAddBufferBefore, matchMaxLen + keepAddBufferAfter, windowReservSize);

			_matchMaxLen = matchMaxLen;

			UInt32 cyclicBufferSize = historySize + 1;
			if (_cyclicBufferSize != cyclicBufferSize)
				_son = new UInt32[(_cyclicBufferSize = cyclicBufferSize) * 2];

			UInt32 hs = KBt2HashSize;

			if (_hashArray)
			{
				hs = historySize - 1;
				hs |= (hs >> 1);
				hs |= (hs >> 2);
				hs |= (hs >> 4);
				hs |= (hs >> 8);
				hs >>= 1;
				hs |= 0xFFFF;
				if (hs > (1 << 24))
					hs >>= 1;
				_hashMask = hs;
				hs++;
				hs += _kFixHashSize;
			}
			if (hs != _hashSizeSum)
				_hash = new UInt32[_hashSizeSum = hs];
		}

		public UInt32 GetMatches(UInt32[] distances)
		{
			UInt32 lenLimit;
			if (Pos + _matchMaxLen <= StreamPos)
				lenLimit = _matchMaxLen;
			else
			{
				lenLimit = StreamPos - Pos;
				if (lenLimit < _kMinMatchCheck)
				{
					MovePos();
					return 0;
				}
			}

			UInt32 offset = 0;
			UInt32 matchMinPos = (Pos > _cyclicBufferSize) ? (Pos - _cyclicBufferSize) : 0;
			UInt32 cur = BufferOffset + Pos;
			UInt32 maxLen = KStartMaxLen; // to avoid items for len < hashSize;
			UInt32 hashValue, hash2Value = 0, hash3Value = 0;

			if (_hashArray)
			{
				UInt32 temp = CRC.Table[BufferBase[cur]] ^ BufferBase[cur + 1];
				hash2Value = temp & (KHash2Size - 1);
				temp ^= ((UInt32)(BufferBase[cur + 2]) << 8);
				hash3Value = temp & (KHash3Size - 1);
				hashValue = (temp ^ (CRC.Table[BufferBase[cur + 3]] << 5)) & _hashMask;
			}
			else
				hashValue = BufferBase[cur] ^ ((UInt32)(BufferBase[cur + 1]) << 8);

			UInt32 curMatch = _hash[_kFixHashSize + hashValue];
			if (_hashArray)
			{
				UInt32 curMatch2 = _hash[hash2Value];
				UInt32 curMatch3 = _hash[KHash3Offset + hash3Value];
				_hash[hash2Value] = Pos;
				_hash[KHash3Offset + hash3Value] = Pos;
				if (curMatch2 > matchMinPos)
					if (BufferBase[BufferOffset + curMatch2] == BufferBase[cur])
					{
						distances[offset++] = maxLen = 2;
						distances[offset++] = Pos - curMatch2 - 1;
					}
				if (curMatch3 > matchMinPos)
					if (BufferBase[BufferOffset + curMatch3] == BufferBase[cur])
					{
						if (curMatch3 == curMatch2)
							offset -= 2;
						distances[offset++] = maxLen = 3;
						distances[offset++] = Pos - curMatch3 - 1;
						curMatch2 = curMatch3;
					}
				if (offset != 0 && curMatch2 == curMatch)
				{
					offset -= 2;
					maxLen = KStartMaxLen;
				}
			}

			_hash[_kFixHashSize + hashValue] = Pos;

			UInt32 ptr0 = (_cyclicBufferPos << 1) + 1;
			UInt32 ptr1 = (_cyclicBufferPos << 1);

			UInt32 len0, len1;
			len0 = len1 = _kNumHashDirectBytes;
			
			if (_kNumHashDirectBytes != 0)
			{
				if (curMatch > matchMinPos)
				{
					if (BufferBase[BufferOffset + curMatch + _kNumHashDirectBytes] !=
							BufferBase[cur + _kNumHashDirectBytes])
					{
						distances[offset++] = maxLen = _kNumHashDirectBytes;
						distances[offset++] = Pos - curMatch - 1;
					}
				}
			}
			
			UInt32 count = _cutValue;
			
			while(true)
			{
				if(curMatch <= matchMinPos || count-- == 0)
				{
					_son[ptr0] = _son[ptr1] = KEmptyHashValue;
					break;
				}
				UInt32 delta = Pos - curMatch;
				UInt32 cyclicPos = ((delta <= _cyclicBufferPos) ?
							(_cyclicBufferPos - delta) :
							(_cyclicBufferPos - delta + _cyclicBufferSize)) << 1;

				UInt32 pby1 = BufferOffset + curMatch;
				UInt32 len = System.Math.Min(len0, len1);
				if (BufferBase[pby1 + len] == BufferBase[cur + len])
				{
					while(++len != lenLimit)
						if (BufferBase[pby1 + len] != BufferBase[cur + len])
							break;
					if (maxLen < len)
					{
						distances[offset++] = maxLen = len;
						distances[offset++] = delta - 1;
						if (len == lenLimit)
						{
							_son[ptr1] = _son[cyclicPos];
							_son[ptr0] = _son[cyclicPos + 1];
							break;
						}
					}
				}
				if (BufferBase[pby1 + len] < BufferBase[cur + len])
				{
					_son[ptr1] = curMatch;
					ptr1 = cyclicPos + 1;
					curMatch = _son[ptr1];
					len1 = len;
				}
				else
				{
					_son[ptr0] = curMatch;
					ptr0 = cyclicPos;
					curMatch = _son[ptr0];
					len0 = len;
				}
			}
			MovePos();
			return offset;
		}

		public void Skip(UInt32 num)
		{
			do
			{
				UInt32 lenLimit;
				if (Pos + _matchMaxLen <= StreamPos)
					lenLimit = _matchMaxLen;
				else
				{
					lenLimit = StreamPos - Pos;
					if (lenLimit < _kMinMatchCheck)
					{
						MovePos();
						continue;
					}
				}

				UInt32 matchMinPos = (Pos > _cyclicBufferSize) ? (Pos - _cyclicBufferSize) : 0;
				UInt32 cur = BufferOffset + Pos;

				UInt32 hashValue;

				if (_hashArray)
				{
					UInt32 temp = CRC.Table[BufferBase[cur]] ^ BufferBase[cur + 1];
					UInt32 hash2Value = temp & (KHash2Size - 1);
					_hash[hash2Value] = Pos;
					temp ^= ((UInt32)(BufferBase[cur + 2]) << 8);
					UInt32 hash3Value = temp & (KHash3Size - 1);
					_hash[KHash3Offset + hash3Value] = Pos;
					hashValue = (temp ^ (CRC.Table[BufferBase[cur + 3]] << 5)) & _hashMask;
				}
				else
					hashValue = BufferBase[cur] ^ ((UInt32)(BufferBase[cur + 1]) << 8);

				UInt32 curMatch = _hash[_kFixHashSize + hashValue];
				_hash[_kFixHashSize + hashValue] = Pos;

				UInt32 ptr0 = (_cyclicBufferPos << 1) + 1;
				UInt32 ptr1 = (_cyclicBufferPos << 1);

				UInt32 len0, len1;
				len0 = len1 = _kNumHashDirectBytes;

				UInt32 count = _cutValue;
				while (true)
				{
					if (curMatch <= matchMinPos || count-- == 0)
					{
						_son[ptr0] = _son[ptr1] = KEmptyHashValue;
						break;
					}

					UInt32 delta = Pos - curMatch;
					UInt32 cyclicPos = ((delta <= _cyclicBufferPos) ?
								(_cyclicBufferPos - delta) :
								(_cyclicBufferPos - delta + _cyclicBufferSize)) << 1;

					UInt32 pby1 = BufferOffset + curMatch;
					UInt32 len = System.Math.Min(len0, len1);
					if (BufferBase[pby1 + len] == BufferBase[cur + len])
					{
						while (++len != lenLimit)
							if (BufferBase[pby1 + len] != BufferBase[cur + len])
								break;
						if (len == lenLimit)
						{
							_son[ptr1] = _son[cyclicPos];
							_son[ptr0] = _son[cyclicPos + 1];
							break;
						}
					}
					if (BufferBase[pby1 + len] < BufferBase[cur + len])
					{
						_son[ptr1] = curMatch;
						ptr1 = cyclicPos + 1;
						curMatch = _son[ptr1];
						len1 = len;
					}
					else
					{
						_son[ptr0] = curMatch;
						ptr0 = cyclicPos;
						curMatch = _son[ptr0];
						len0 = len;
					}
				}
				MovePos();
			}
			while (--num != 0);
		}

		void NormalizeLinks(UInt32[] items, UInt32 numItems, UInt32 subValue)
		{
			for (UInt32 i = 0; i < numItems; i++)
			{
				UInt32 value = items[i];
				if (value <= subValue)
					value = KEmptyHashValue;
				else
					value -= subValue;
				items[i] = value;
			}
		}

		void Normalize()
		{
			UInt32 subValue = Pos - _cyclicBufferSize;
			NormalizeLinks(_son, _cyclicBufferSize * 2, subValue);
			NormalizeLinks(_hash, _hashSizeSum, subValue);
			ReduceOffsets((Int32)subValue);
		}

		public void SetCutValue(UInt32 cutValue) { _cutValue = cutValue; }
	}
}
