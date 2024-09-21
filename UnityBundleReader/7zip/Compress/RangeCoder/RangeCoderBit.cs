using System;

namespace SevenZip.Compression.RangeCoder
{
	struct BitEncoder
	{
		public const int KNumBitModelTotalBits = 11;
		public const uint KBitModelTotal = (1 << KNumBitModelTotalBits);
		const int KNumMoveBits = 5;
		const int KNumMoveReducingBits = 2;
		public const int KNumBitPriceShiftBits = 6;

		uint _prob;

		public void Init() { _prob = KBitModelTotal >> 1; }

		public void UpdateModel(uint symbol)
		{
			if (symbol == 0)
				_prob += (KBitModelTotal - _prob) >> KNumMoveBits;
			else
				_prob -= (_prob) >> KNumMoveBits;
		}

		public void Encode(Encoder encoder, uint symbol)
		{
			// encoder.EncodeBit(Prob, kNumBitModelTotalBits, symbol);
			// UpdateModel(symbol);
			uint newBound = (encoder.Range >> KNumBitModelTotalBits) * _prob;
			if (symbol == 0)
			{
				encoder.Range = newBound;
				_prob += (KBitModelTotal - _prob) >> KNumMoveBits;
			}
			else
			{
				encoder.Low += newBound;
				encoder.Range -= newBound;
				_prob -= (_prob) >> KNumMoveBits;
			}
			if (encoder.Range < Encoder.KTopValue)
			{
				encoder.Range <<= 8;
				encoder.ShiftLow();
			}
		}

		private static UInt32[] _probPrices = new UInt32[KBitModelTotal >> KNumMoveReducingBits];

		static BitEncoder()
		{
			const int kNumBits = (KNumBitModelTotalBits - KNumMoveReducingBits);
			for (int i = kNumBits - 1; i >= 0; i--)
			{
				UInt32 start = (UInt32)1 << (kNumBits - i - 1);
				UInt32 end = (UInt32)1 << (kNumBits - i);
				for (UInt32 j = start; j < end; j++)
					_probPrices[j] = ((UInt32)i << KNumBitPriceShiftBits) +
						(((end - j) << KNumBitPriceShiftBits) >> (kNumBits - i - 1));
			}
		}

		public uint GetPrice(uint symbol)
		{
			return _probPrices[(((_prob - symbol) ^ ((-(int)symbol))) & (KBitModelTotal - 1)) >> KNumMoveReducingBits];
		}
	  public uint GetPrice0() { return _probPrices[_prob >> KNumMoveReducingBits]; }
		public uint GetPrice1() { return _probPrices[(KBitModelTotal - _prob) >> KNumMoveReducingBits]; }
	}

	struct BitDecoder
	{
		public const int KNumBitModelTotalBits = 11;
		public const uint KBitModelTotal = (1 << KNumBitModelTotalBits);
		const int KNumMoveBits = 5;

		uint _prob;

		public void UpdateModel(int numMoveBits, uint symbol)
		{
			if (symbol == 0)
				_prob += (KBitModelTotal - _prob) >> numMoveBits;
			else
				_prob -= (_prob) >> numMoveBits;
		}

		public void Init() { _prob = KBitModelTotal >> 1; }

		public uint Decode(Decoder rangeDecoder)
		{
			uint newBound = (uint)(rangeDecoder.Range >> KNumBitModelTotalBits) * (uint)_prob;
			if (rangeDecoder.Code < newBound)
			{
				rangeDecoder.Range = newBound;
				_prob += (KBitModelTotal - _prob) >> KNumMoveBits;
				if (rangeDecoder.Range < Decoder.KTopValue)
				{
					rangeDecoder.Code = (rangeDecoder.Code << 8) | (byte)rangeDecoder.Stream.ReadByte();
					rangeDecoder.Range <<= 8;
				}
				return 0;
			}
			else
			{
				rangeDecoder.Range -= newBound;
				rangeDecoder.Code -= newBound;
				_prob -= (_prob) >> KNumMoveBits;
				if (rangeDecoder.Range < Decoder.KTopValue)
				{
					rangeDecoder.Code = (rangeDecoder.Code << 8) | (byte)rangeDecoder.Stream.ReadByte();
					rangeDecoder.Range <<= 8;
				}
				return 1;
			}
		}
	}
}
