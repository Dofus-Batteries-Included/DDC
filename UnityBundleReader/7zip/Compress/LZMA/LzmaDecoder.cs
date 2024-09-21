// LzmaDecoder.cs

using UnityBundleReader._7zip.Compress.LZ;
using UnityBundleReader._7zip.Compress.RangeCoder;

namespace UnityBundleReader._7zip.Compress.LZMA
{
	public class Decoder : ICoder, ISetDecoderProperties // ,System.IO.Stream
	{
		class LenDecoder
		{
			BitDecoder _mChoice = new BitDecoder();
			BitDecoder _mChoice2 = new BitDecoder();
			BitTreeDecoder[] _mLowCoder = new BitTreeDecoder[Base.KNumPosStatesMax];
			BitTreeDecoder[] _mMidCoder = new BitTreeDecoder[Base.KNumPosStatesMax];
			BitTreeDecoder _mHighCoder = new BitTreeDecoder(Base.KNumHighLenBits);
			uint _mNumPosStates = 0;

			public void Create(uint numPosStates)
			{
				for (uint posState = _mNumPosStates; posState < numPosStates; posState++)
				{
					_mLowCoder[posState] = new BitTreeDecoder(Base.KNumLowLenBits);
					_mMidCoder[posState] = new BitTreeDecoder(Base.KNumMidLenBits);
				}
				_mNumPosStates = numPosStates;
			}

			public void Init()
			{
				_mChoice.Init();
				for (uint posState = 0; posState < _mNumPosStates; posState++)
				{
					_mLowCoder[posState].Init();
					_mMidCoder[posState].Init();
				}
				_mChoice2.Init();
				_mHighCoder.Init();
			}

			public uint Decode(RangeCoder.Decoder rangeDecoder, uint posState)
			{
				if (_mChoice.Decode(rangeDecoder) == 0)
					return _mLowCoder[posState].Decode(rangeDecoder);
				else
				{
					uint symbol = Base.KNumLowLenSymbols;
					if (_mChoice2.Decode(rangeDecoder) == 0)
						symbol += _mMidCoder[posState].Decode(rangeDecoder);
					else
					{
						symbol += Base.KNumMidLenSymbols;
						symbol += _mHighCoder.Decode(rangeDecoder);
					}
					return symbol;
				}
			}
		}

		class LiteralDecoder
		{
			struct Decoder2
			{
				BitDecoder[] _mDecoders;
				public void Create() { _mDecoders = new BitDecoder[0x300]; }
				public void Init() { for (int i = 0; i < 0x300; i++) _mDecoders[i].Init(); }

				public byte DecodeNormal(RangeCoder.Decoder rangeDecoder)
				{
					uint symbol = 1;
					do
						symbol = (symbol << 1) | _mDecoders[symbol].Decode(rangeDecoder);
					while (symbol < 0x100);
					return (byte)symbol;
				}

				public byte DecodeWithMatchByte(RangeCoder.Decoder rangeDecoder, byte matchByte)
				{
					uint symbol = 1;
					do
					{
						uint matchBit = (uint)(matchByte >> 7) & 1;
						matchByte <<= 1;
						uint bit = _mDecoders[((1 + matchBit) << 8) + symbol].Decode(rangeDecoder);
						symbol = (symbol << 1) | bit;
						if (matchBit != bit)
						{
							while (symbol < 0x100)
								symbol = (symbol << 1) | _mDecoders[symbol].Decode(rangeDecoder);
							break;
						}
					}
					while (symbol < 0x100);
					return (byte)symbol;
				}
			}

			Decoder2[] _mCoders;
			int _mNumPrevBits;
			int _mNumPosBits;
			uint _mPosMask;

			public void Create(int numPosBits, int numPrevBits)
			{
				if (_mCoders != null && _mNumPrevBits == numPrevBits &&
					_mNumPosBits == numPosBits)
					return;
				_mNumPosBits = numPosBits;
				_mPosMask = ((uint)1 << numPosBits) - 1;
				_mNumPrevBits = numPrevBits;
				uint numStates = (uint)1 << (_mNumPrevBits + _mNumPosBits);
				_mCoders = new Decoder2[numStates];
				for (uint i = 0; i < numStates; i++)
					_mCoders[i].Create();
			}

			public void Init()
			{
				uint numStates = (uint)1 << (_mNumPrevBits + _mNumPosBits);
				for (uint i = 0; i < numStates; i++)
					_mCoders[i].Init();
			}

			uint GetState(uint pos, byte prevByte)
			{ return ((pos & _mPosMask) << _mNumPrevBits) + (uint)(prevByte >> (8 - _mNumPrevBits)); }

			public byte DecodeNormal(RangeCoder.Decoder rangeDecoder, uint pos, byte prevByte)
			{ return _mCoders[GetState(pos, prevByte)].DecodeNormal(rangeDecoder); }

			public byte DecodeWithMatchByte(RangeCoder.Decoder rangeDecoder, uint pos, byte prevByte, byte matchByte)
			{ return _mCoders[GetState(pos, prevByte)].DecodeWithMatchByte(rangeDecoder, matchByte); }
		};

		OutWindow _mOutWindow = new OutWindow();
		RangeCoder.Decoder _mRangeDecoder = new RangeCoder.Decoder();

		BitDecoder[] _mIsMatchDecoders = new BitDecoder[Base.KNumStates << Base.KNumPosStatesBitsMax];
		BitDecoder[] _mIsRepDecoders = new BitDecoder[Base.KNumStates];
		BitDecoder[] _mIsRepG0Decoders = new BitDecoder[Base.KNumStates];
		BitDecoder[] _mIsRepG1Decoders = new BitDecoder[Base.KNumStates];
		BitDecoder[] _mIsRepG2Decoders = new BitDecoder[Base.KNumStates];
		BitDecoder[] _mIsRep0LongDecoders = new BitDecoder[Base.KNumStates << Base.KNumPosStatesBitsMax];

		BitTreeDecoder[] _mPosSlotDecoder = new BitTreeDecoder[Base.KNumLenToPosStates];
		BitDecoder[] _mPosDecoders = new BitDecoder[Base.KNumFullDistances - Base.KEndPosModelIndex];

		BitTreeDecoder _mPosAlignDecoder = new BitTreeDecoder(Base.KNumAlignBits);

		LenDecoder _mLenDecoder = new LenDecoder();
		LenDecoder _mRepLenDecoder = new LenDecoder();

		LiteralDecoder _mLiteralDecoder = new LiteralDecoder();

		uint _mDictionarySize;
		uint _mDictionarySizeCheck;

		uint _mPosStateMask;

		public Decoder()
		{
			_mDictionarySize = 0xFFFFFFFF;
			for (int i = 0; i < Base.KNumLenToPosStates; i++)
				_mPosSlotDecoder[i] = new BitTreeDecoder(Base.KNumPosSlotBits);
		}

		void SetDictionarySize(uint dictionarySize)
		{
			if (_mDictionarySize != dictionarySize)
			{
				_mDictionarySize = dictionarySize;
				_mDictionarySizeCheck = System.Math.Max(_mDictionarySize, 1);
				uint blockSize = System.Math.Max(_mDictionarySizeCheck, (1 << 12));
				_mOutWindow.Create(blockSize);
			}
		}

		void SetLiteralProperties(int lp, int lc)
		{
			if (lp > 8)
				throw new InvalidParamException();
			if (lc > 8)
				throw new InvalidParamException();
			_mLiteralDecoder.Create(lp, lc);
		}

		void SetPosBitsProperties(int pb)
		{
			if (pb > Base.KNumPosStatesBitsMax)
				throw new InvalidParamException();
			uint numPosStates = (uint)1 << pb;
			_mLenDecoder.Create(numPosStates);
			_mRepLenDecoder.Create(numPosStates);
			_mPosStateMask = numPosStates - 1;
		}

		bool _solid = false;
		void Init(Stream inStream, Stream outStream)
		{
			_mRangeDecoder.Init(inStream);
			_mOutWindow.Init(outStream, _solid);

			uint i;
			for (i = 0; i < Base.KNumStates; i++)
			{
				for (uint j = 0; j <= _mPosStateMask; j++)
				{
					uint index = (i << Base.KNumPosStatesBitsMax) + j;
					_mIsMatchDecoders[index].Init();
					_mIsRep0LongDecoders[index].Init();
				}
				_mIsRepDecoders[i].Init();
				_mIsRepG0Decoders[i].Init();
				_mIsRepG1Decoders[i].Init();
				_mIsRepG2Decoders[i].Init();
			}

			_mLiteralDecoder.Init();
			for (i = 0; i < Base.KNumLenToPosStates; i++)
				_mPosSlotDecoder[i].Init();
			// m_PosSpecDecoder.Init();
			for (i = 0; i < Base.KNumFullDistances - Base.KEndPosModelIndex; i++)
				_mPosDecoders[i].Init();

			_mLenDecoder.Init();
			_mRepLenDecoder.Init();
			_mPosAlignDecoder.Init();
		}

		public void Code(Stream inStream, Stream outStream,
			Int64 inSize, Int64 outSize, ICodeProgress progress)
		{
			Init(inStream, outStream);

			Base.State state = new Base.State();
			state.Init();
			uint rep0 = 0, rep1 = 0, rep2 = 0, rep3 = 0;

			UInt64 nowPos64 = 0;
			UInt64 outSize64 = (UInt64)outSize;
			if (nowPos64 < outSize64)
			{
				if (_mIsMatchDecoders[state.Index << Base.KNumPosStatesBitsMax].Decode(_mRangeDecoder) != 0)
					throw new DataErrorException();
				state.UpdateChar();
				byte b = _mLiteralDecoder.DecodeNormal(_mRangeDecoder, 0, 0);
				_mOutWindow.PutByte(b);
				nowPos64++;
			}
			while (nowPos64 < outSize64)
			{
				// UInt64 next = Math.Min(nowPos64 + (1 << 18), outSize64);
					// while(nowPos64 < next)
				{
					uint posState = (uint)nowPos64 & _mPosStateMask;
					if (_mIsMatchDecoders[(state.Index << Base.KNumPosStatesBitsMax) + posState].Decode(_mRangeDecoder) == 0)
					{
						byte b;
						byte prevByte = _mOutWindow.GetByte(0);
						if (!state.IsCharState())
							b = _mLiteralDecoder.DecodeWithMatchByte(_mRangeDecoder,
								(uint)nowPos64, prevByte, _mOutWindow.GetByte(rep0));
						else
							b = _mLiteralDecoder.DecodeNormal(_mRangeDecoder, (uint)nowPos64, prevByte);
						_mOutWindow.PutByte(b);
						state.UpdateChar();
						nowPos64++;
					}
					else
					{
						uint len;
						if (_mIsRepDecoders[state.Index].Decode(_mRangeDecoder) == 1)
						{
							if (_mIsRepG0Decoders[state.Index].Decode(_mRangeDecoder) == 0)
							{
								if (_mIsRep0LongDecoders[(state.Index << Base.KNumPosStatesBitsMax) + posState].Decode(_mRangeDecoder) == 0)
								{
									state.UpdateShortRep();
									_mOutWindow.PutByte(_mOutWindow.GetByte(rep0));
									nowPos64++;
									continue;
								}
							}
							else
							{
								UInt32 distance;
								if (_mIsRepG1Decoders[state.Index].Decode(_mRangeDecoder) == 0)
								{
									distance = rep1;
								}
								else
								{
									if (_mIsRepG2Decoders[state.Index].Decode(_mRangeDecoder) == 0)
										distance = rep2;
									else
									{
										distance = rep3;
										rep3 = rep2;
									}
									rep2 = rep1;
								}
								rep1 = rep0;
								rep0 = distance;
							}
							len = _mRepLenDecoder.Decode(_mRangeDecoder, posState) + Base.KMatchMinLen;
							state.UpdateRep();
						}
						else
						{
							rep3 = rep2;
							rep2 = rep1;
							rep1 = rep0;
							len = Base.KMatchMinLen + _mLenDecoder.Decode(_mRangeDecoder, posState);
							state.UpdateMatch();
							uint posSlot = _mPosSlotDecoder[Base.GetLenToPosState(len)].Decode(_mRangeDecoder);
							if (posSlot >= Base.KStartPosModelIndex)
							{
								int numDirectBits = (int)((posSlot >> 1) - 1);
								rep0 = ((2 | (posSlot & 1)) << numDirectBits);
								if (posSlot < Base.KEndPosModelIndex)
									rep0 += BitTreeDecoder.ReverseDecode(_mPosDecoders,
											rep0 - posSlot - 1, _mRangeDecoder, numDirectBits);
								else
								{
									rep0 += (_mRangeDecoder.DecodeDirectBits(
										numDirectBits - Base.KNumAlignBits) << Base.KNumAlignBits);
									rep0 += _mPosAlignDecoder.ReverseDecode(_mRangeDecoder);
								}
							}
							else
								rep0 = posSlot;
						}
						if (rep0 >= _mOutWindow.TrainSize + nowPos64 || rep0 >= _mDictionarySizeCheck)
						{
							if (rep0 == 0xFFFFFFFF)
								break;
							throw new DataErrorException();
						}
						_mOutWindow.CopyBlock(rep0, len);
						nowPos64 += len;
					}
				}
			}
			_mOutWindow.Flush();
			_mOutWindow.ReleaseStream();
			_mRangeDecoder.ReleaseStream();
		}

		public void SetDecoderProperties(byte[] properties)
		{
			if (properties.Length < 5)
				throw new InvalidParamException();
			int lc = properties[0] % 9;
			int remainder = properties[0] / 9;
			int lp = remainder % 5;
			int pb = remainder / 5;
			if (pb > Base.KNumPosStatesBitsMax)
				throw new InvalidParamException();
			UInt32 dictionarySize = 0;
			for (int i = 0; i < 4; i++)
				dictionarySize += ((UInt32)(properties[1 + i])) << (i * 8);
			SetDictionarySize(dictionarySize);
			SetLiteralProperties(lp, lc);
			SetPosBitsProperties(pb);
		}

		public bool Train(Stream stream)
		{
			_solid = true;
			return _mOutWindow.Train(stream);
		}

		/*
		public override bool CanRead { get { return true; }}
		public override bool CanWrite { get { return true; }}
		public override bool CanSeek { get { return true; }}
		public override long Length { get { return 0; }}
		public override long Position
		{
			get { return 0;	}
			set { }
		}
		public override void Flush() { }
		public override int Read(byte[] buffer, int offset, int count) 
		{
			return 0;
		}
		public override void Write(byte[] buffer, int offset, int count)
		{
		}
		public override long Seek(long offset, System.IO.SeekOrigin origin)
		{
			return 0;
		}
		public override void SetLength(long value) {}
		*/
	}
}
