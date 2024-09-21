/* Copyright 2015 Google Inc. All Rights Reserved.

Distributed under MIT license.
See file LICENSE for detail or copy at https://opensource.org/licenses/MIT
*/
namespace Org.Brotli.Dec
{
	/// <summary>Transformations on dictionary words.</summary>
	internal sealed class Transform
	{
		private readonly byte[] _prefix;

		private readonly int _type;

		private readonly byte[] _suffix;

		internal Transform(string prefix, int type, string suffix)
		{
			_prefix = ReadUniBytes(prefix);
			_type = type;
			_suffix = ReadUniBytes(suffix);
		}

		internal static byte[] ReadUniBytes(string uniBytes)
		{
			byte[] result = new byte[uniBytes.Length];
			for (int i = 0; i < result.Length; ++i)
			{
				result[i] = unchecked((byte)uniBytes[i]);
			}
			return result;
		}

		internal static readonly Transform[] Transforms = new Transform[] { new Transform(string.Empty, WordTransformType.Identity, string.Empty), new Transform(string.Empty, 
			WordTransformType.Identity, " "), new Transform(" ", WordTransformType.Identity, " "), new Transform(string.Empty, WordTransformType.OmitFirst1, string.Empty), new Transform
			(string.Empty, WordTransformType.UppercaseFirst, " "), new Transform(string.Empty, WordTransformType.Identity, " the "), new Transform(" ", WordTransformType.Identity
			, string.Empty), new Transform("s ", WordTransformType.Identity, " "), new Transform(string.Empty, WordTransformType.Identity, " of "), new Transform(string.Empty, WordTransformType
			.UppercaseFirst, string.Empty), new Transform(string.Empty, WordTransformType.Identity, " and "), new Transform(string.Empty, WordTransformType.OmitFirst2, string.Empty), new Transform
			(string.Empty, WordTransformType.OmitLast1, string.Empty), new Transform(", ", WordTransformType.Identity, " "), new Transform(string.Empty, WordTransformType.Identity
			, ", "), new Transform(" ", WordTransformType.UppercaseFirst, " "), new Transform(string.Empty, WordTransformType.Identity, " in "), new Transform(string.Empty, WordTransformType
			.Identity, " to "), new Transform("e ", WordTransformType.Identity, " "), new Transform(string.Empty, WordTransformType.Identity, "\""), new Transform(string.Empty, 
			WordTransformType.Identity, "."), new Transform(string.Empty, WordTransformType.Identity, "\">"), new Transform(string.Empty, WordTransformType.Identity, "\n"), new 
			Transform(string.Empty, WordTransformType.OmitLast3, string.Empty), new Transform(string.Empty, WordTransformType.Identity, "]"), new Transform(string.Empty, WordTransformType
			.Identity, " for "), new Transform(string.Empty, WordTransformType.OmitFirst3, string.Empty), new Transform(string.Empty, WordTransformType.OmitLast2, string.Empty), new Transform
			(string.Empty, WordTransformType.Identity, " a "), new Transform(string.Empty, WordTransformType.Identity, " that "), new Transform(" ", WordTransformType.UppercaseFirst
			, string.Empty), new Transform(string.Empty, WordTransformType.Identity, ". "), new Transform(".", WordTransformType.Identity, string.Empty), new Transform(" ", WordTransformType
			.Identity, ", "), new Transform(string.Empty, WordTransformType.OmitFirst4, string.Empty), new Transform(string.Empty, WordTransformType.Identity, " with "), new Transform
			(string.Empty, WordTransformType.Identity, "'"), new Transform(string.Empty, WordTransformType.Identity, " from "), new Transform(string.Empty, WordTransformType.Identity
			, " by "), new Transform(string.Empty, WordTransformType.OmitFirst5, string.Empty), new Transform(string.Empty, WordTransformType.OmitFirst6, string.Empty), new Transform
			(" the ", WordTransformType.Identity, string.Empty), new Transform(string.Empty, WordTransformType.OmitLast4, string.Empty), new Transform(string.Empty, WordTransformType
			.Identity, ". The "), new Transform(string.Empty, WordTransformType.UppercaseAll, string.Empty), new Transform(string.Empty, WordTransformType.Identity, " on "), new Transform
			(string.Empty, WordTransformType.Identity, " as "), new Transform(string.Empty, WordTransformType.Identity, " is "), new Transform(string.Empty, WordTransformType.OmitLast7
			, string.Empty), new Transform(string.Empty, WordTransformType.OmitLast1, "ing "), new Transform(string.Empty, WordTransformType.Identity, "\n\t"), new Transform(string.Empty
			, WordTransformType.Identity, ":"), new Transform(" ", WordTransformType.Identity, ". "), new Transform(string.Empty, WordTransformType.Identity, "ed "), new Transform
			(string.Empty, WordTransformType.OmitFirst9, string.Empty), new Transform(string.Empty, WordTransformType.OmitFirst7, string.Empty), new Transform(string.Empty, WordTransformType
			.OmitLast6, string.Empty), new Transform(string.Empty, WordTransformType.Identity, "("), new Transform(string.Empty, WordTransformType.UppercaseFirst, ", "), new Transform
			(string.Empty, WordTransformType.OmitLast8, string.Empty), new Transform(string.Empty, WordTransformType.Identity, " at "), new Transform(string.Empty, WordTransformType
			.Identity, "ly "), new Transform(" the ", WordTransformType.Identity, " of "), new Transform(string.Empty, WordTransformType.OmitLast5, string.Empty), new Transform(
			string.Empty, WordTransformType.OmitLast9, string.Empty), new Transform(" ", WordTransformType.UppercaseFirst, ", "), new Transform(string.Empty, WordTransformType.UppercaseFirst
			, "\""), new Transform(".", WordTransformType.Identity, "("), new Transform(string.Empty, WordTransformType.UppercaseAll, " "), new Transform(string.Empty, WordTransformType
			.UppercaseFirst, "\">"), new Transform(string.Empty, WordTransformType.Identity, "=\""), new Transform(" ", WordTransformType.Identity, "."), new Transform(".com/", 
			WordTransformType.Identity, string.Empty), new Transform(" the ", WordTransformType.Identity, " of the "), new Transform(string.Empty, WordTransformType.UppercaseFirst
			, "'"), new Transform(string.Empty, WordTransformType.Identity, ". This "), new Transform(string.Empty, WordTransformType.Identity, ","), new Transform(".", WordTransformType
			.Identity, " "), new Transform(string.Empty, WordTransformType.UppercaseFirst, "("), new Transform(string.Empty, WordTransformType.UppercaseFirst, "."), new Transform
			(string.Empty, WordTransformType.Identity, " not "), new Transform(" ", WordTransformType.Identity, "=\""), new Transform(string.Empty, WordTransformType.Identity, "er "
			), new Transform(" ", WordTransformType.UppercaseAll, " "), new Transform(string.Empty, WordTransformType.Identity, "al "), new Transform(" ", WordTransformType
			.UppercaseAll, string.Empty), new Transform(string.Empty, WordTransformType.Identity, "='"), new Transform(string.Empty, WordTransformType.UppercaseAll, "\""), new Transform
			(string.Empty, WordTransformType.UppercaseFirst, ". "), new Transform(" ", WordTransformType.Identity, "("), new Transform(string.Empty, WordTransformType.Identity, 
			"ful "), new Transform(" ", WordTransformType.UppercaseFirst, ". "), new Transform(string.Empty, WordTransformType.Identity, "ive "), new Transform(string.Empty, WordTransformType
			.Identity, "less "), new Transform(string.Empty, WordTransformType.UppercaseAll, "'"), new Transform(string.Empty, WordTransformType.Identity, "est "), new Transform
			(" ", WordTransformType.UppercaseFirst, "."), new Transform(string.Empty, WordTransformType.UppercaseAll, "\">"), new Transform(" ", WordTransformType.Identity, "='"
			), new Transform(string.Empty, WordTransformType.UppercaseFirst, ","), new Transform(string.Empty, WordTransformType.Identity, "ize "), new Transform(string.Empty, WordTransformType
			.UppercaseAll, "."), new Transform("\u00c2\u00a0", WordTransformType.Identity, string.Empty), new Transform(" ", WordTransformType.Identity, ","), new Transform(string.Empty
			, WordTransformType.UppercaseFirst, "=\""), new Transform(string.Empty, WordTransformType.UppercaseAll, "=\""), new Transform(string.Empty, WordTransformType.Identity
			, "ous "), new Transform(string.Empty, WordTransformType.UppercaseAll, ", "), new Transform(string.Empty, WordTransformType.UppercaseFirst, "='"), new Transform(" ", 
			WordTransformType.UppercaseFirst, ","), new Transform(" ", WordTransformType.UppercaseAll, "=\""), new Transform(" ", WordTransformType.UppercaseAll, ", "), new Transform
			(string.Empty, WordTransformType.UppercaseAll, ","), new Transform(string.Empty, WordTransformType.UppercaseAll, "("), new Transform(string.Empty, WordTransformType.
			UppercaseAll, ". "), new Transform(" ", WordTransformType.UppercaseAll, "."), new Transform(string.Empty, WordTransformType.UppercaseAll, "='"), new Transform(" ", WordTransformType
			.UppercaseAll, ". "), new Transform(" ", WordTransformType.UppercaseFirst, "=\""), new Transform(" ", WordTransformType.UppercaseAll, "='"), new Transform(" ", WordTransformType
			.UppercaseFirst, "='") };

		internal static int TransformDictionaryWord(byte[] dst, int dstOffset, byte[] word, int wordOffset, int len, Transform transform)
		{
			int offset = dstOffset;
			// Copy prefix.
			byte[] @string = transform._prefix;
			int tmp = @string.Length;
			int i = 0;
			// In most cases tmp < 10 -> no benefits from System.arrayCopy
			while (i < tmp)
			{
				dst[offset++] = @string[i++];
			}
			// Copy trimmed word.
			int op = transform._type;
			tmp = WordTransformType.GetOmitFirst(op);
			if (tmp > len)
			{
				tmp = len;
			}
			wordOffset += tmp;
			len -= tmp;
			len -= WordTransformType.GetOmitLast(op);
			i = len;
			while (i > 0)
			{
				dst[offset++] = word[wordOffset++];
				i--;
			}
			if (op == WordTransformType.UppercaseAll || op == WordTransformType.UppercaseFirst)
			{
				int uppercaseOffset = offset - len;
				if (op == WordTransformType.UppercaseFirst)
				{
					len = 1;
				}
				while (len > 0)
				{
					tmp = dst[uppercaseOffset] & unchecked((int)(0xFF));
					if (tmp < unchecked((int)(0xc0)))
					{
						if (tmp >= 'a' && tmp <= 'z')
						{
							dst[uppercaseOffset] ^= unchecked((byte)32);
						}
						uppercaseOffset += 1;
						len -= 1;
					}
					else if (tmp < unchecked((int)(0xe0)))
					{
						dst[uppercaseOffset + 1] ^= unchecked((byte)32);
						uppercaseOffset += 2;
						len -= 2;
					}
					else
					{
						dst[uppercaseOffset + 2] ^= unchecked((byte)5);
						uppercaseOffset += 3;
						len -= 3;
					}
				}
			}
			// Copy suffix.
			@string = transform._suffix;
			tmp = @string.Length;
			i = 0;
			while (i < tmp)
			{
				dst[offset++] = @string[i++];
			}
			return offset - dstOffset;
		}
	}
}
