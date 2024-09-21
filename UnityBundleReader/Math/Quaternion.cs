using System.Runtime.InteropServices;

namespace UnityBundleReader.Math;

[StructLayout(LayoutKind.Sequential, Pack = 4)]
public struct Quaternion : IEquatable<Quaternion>
{
    public float X;
    public float Y;
    public float Z;
    public float W;

    public Quaternion(float x, float y, float z, float w)
    {
        X = x;
        Y = y;
        Z = z;
        W = w;
    }

    public float this[int index] {
        get {
            switch (index)
            {
                case 0: return X;
                case 1: return Y;
                case 2: return Z;
                case 3: return W;
                default: throw new ArgumentOutOfRangeException(nameof(index), "Invalid Quaternion index!");
            }
        }

        set {
            switch (index)
            {
                case 0:
                    X = value;
                    break;
                case 1:
                    Y = value;
                    break;
                case 2:
                    Z = value;
                    break;
                case 3:
                    W = value;
                    break;
                default: throw new ArgumentOutOfRangeException(nameof(index), "Invalid Quaternion index!");
            }
        }
    }

    public override int GetHashCode() => X.GetHashCode() ^ Y.GetHashCode()<<2 ^ Z.GetHashCode()>> 2 ^ W.GetHashCode()>> 1;

    public override bool Equals(object? other)
    {
        if (other is not Quaternion quaternion)
        {
            return false;
        }
        return Equals(quaternion);
    }

    public bool Equals(Quaternion other) => X.Equals(other.X) && Y.Equals(other.Y) && Z.Equals(other.Z) && W.Equals(other.W);

    public static float Dot(Quaternion a, Quaternion b) => a.X * b.X + a.Y * b.Y + a.Z * b.Z + a.W * b.W;

    static bool IsEqualUsingDot(float dot) => dot > 1.0f - KEpsilon;

    public static bool operator ==(Quaternion lhs, Quaternion rhs) => IsEqualUsingDot(Dot(lhs, rhs));

    public static bool operator !=(Quaternion lhs, Quaternion rhs) => !(lhs == rhs);

    const float KEpsilon = 0.000001F;
}
