using System.Runtime.InteropServices;

namespace UnityBundleReader.Math;

[StructLayout(LayoutKind.Sequential, Pack = 4)]
public struct Vector3 : IEquatable<Vector3>
{
    public float X;
    public float Y;
    public float Z;

    public Vector3(float x, float y, float z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    public float this[int index] {
        get {
            switch (index)
            {
                case 0: return X;
                case 1: return Y;
                case 2: return Z;
                default: throw new ArgumentOutOfRangeException(nameof(index), "Invalid Vector3 index!");
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
                default: throw new ArgumentOutOfRangeException(nameof(index), "Invalid Vector3 index!");
            }
        }
    }

    public override int GetHashCode() => X.GetHashCode() ^ Y.GetHashCode()<<2 ^ Z.GetHashCode()>> 2;

    public override bool Equals(object? other)
    {
        if (other is not Vector3 vector3)
        {
            return false;
        }
        return Equals(vector3);
    }

    public bool Equals(Vector3 other) => X.Equals(other.X) && Y.Equals(other.Y) && Z.Equals(other.Z);

    public void Normalize()
    {
        float length = Length();
        if (length > KEpsilon)
        {
            float invNorm = 1.0f / length;
            X *= invNorm;
            Y *= invNorm;
            Z *= invNorm;
        }
        else
        {
            X = 0;
            Y = 0;
            Z = 0;
        }
    }

    public float Length() => (float)System.Math.Sqrt(LengthSquared());

    public float LengthSquared() => X * X + Y * Y + Z * Z;

    public static Vector3 Zero => new();

    public static Vector3 One => new(1.0f, 1.0f, 1.0f);

    public static Vector3 operator +(Vector3 a, Vector3 b) => new(a.X + b.X, a.Y + b.Y, a.Z + b.Z);

    public static Vector3 operator -(Vector3 a, Vector3 b) => new(a.X - b.X, a.Y - b.Y, a.Z - b.Z);

    public static Vector3 operator -(Vector3 a) => new(-a.X, -a.Y, -a.Z);

    public static Vector3 operator *(Vector3 a, float d) => new(a.X * d, a.Y * d, a.Z * d);

    public static Vector3 operator *(float d, Vector3 a) => new(a.X * d, a.Y * d, a.Z * d);

    public static Vector3 operator /(Vector3 a, float d) => new(a.X / d, a.Y / d, a.Z / d);

    public static bool operator ==(Vector3 lhs, Vector3 rhs) => (lhs - rhs).LengthSquared() < KEpsilon * KEpsilon;

    public static bool operator !=(Vector3 lhs, Vector3 rhs) => !(lhs == rhs);

    public static implicit operator Vector2(Vector3 v) => new(v.X, v.Y);

    public static implicit operator Vector4(Vector3 v) => new(v.X, v.Y, v.Z, 0.0F);

    const float KEpsilon = 0.00001F;
}
