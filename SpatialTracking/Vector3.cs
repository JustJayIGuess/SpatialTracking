using System;

namespace SpatialTracking
{
	struct Vector3
	{
		private float[] data;

		public float x { get => data[0]; set => data[0] = value; }
		public float y { get => data[1]; set => data[1] = value; }
		public float z { get => data[2]; set => data[2] = value; }
		public Vector3(float x, float y)
		{
			data = new float[3];

			this.x = x;
			this.y = y;
			z = 0f;
		}

		public Vector3(float x, float y, float z)
		{
			data = new float[3];

			this.x = x;
			this.y = y;
			this.z = z;
		}

		public static readonly Vector3 up = new Vector3(0f, 0f, 1f);
		public static readonly Vector3 down = new Vector3(0f, 0f, -1f);
		public static readonly Vector3 left = new Vector3(-1f, 0f, 0f);
		public static readonly Vector3 right = new Vector3(1f, 0f, 0f);
		public static readonly Vector3 forward = new Vector3(0f, 1f, 0f);
		public static readonly Vector3 backward = new Vector3(0f, -1f, 0f);

		private static Random random = new Random();

		public static Vector3 operator+(Vector3 a, Vector3 b)
		{
			return new Vector3(a.x + b.x, a.y + b.y, a.z + b.z);
		}

		public static Vector3 operator -(Vector3 a, Vector3 b)
		{
			return new Vector3(a.x - b.x, a.y - b.y, a.z - b.z);
		}

		public static Vector3 operator -(Vector3 v)
		{
			return new Vector3(-v.x, -v.y, -v.z);
		}

		public float Magnitude()
		{
			return MathF.Sqrt(x * x + y * y + z * z);
		}

		public float SqrMagnitude()
		{
			return x * x + y * y + z * z;
		}

		public static Vector3 operator *(Vector3 v, float k)
		{
			return new Vector3(v.x * k, v.y * k, v.z * k);
		}

		public static Vector3 operator *(float k, Vector3 v)
		{
			return v * k;
		}

		public static Vector3 operator /(Vector3 v, float k)
		{
			return new Vector3(v.x / k, v.y / k, v.z / k);
		}

		public float this[int i] { get => data[i]; set => data[i] = value; }

		public void Normalize()
		{
			float invMag = 1f / Magnitude();
			x *= invMag;
			y *= invMag;
			z *= invMag;
		}

		public Vector3 Normalized()
		{
			float invMag = 1f / Magnitude();
			return new Vector3(x * invMag, y * invMag, z * invMag);
		}

		public Vector3 Copy()
		{
			return new Vector3(x, y, z);
		}

		public static Vector3 CrossProduct(Vector3 a, Vector3 b)
		{
			return new Vector3(
				a.y * b.z - a.z * b.y,
				a.z * b.x - a.x * b.z,
				a.x * b.y - a.y * b.x
			);
		}

		public static float DotProduct(Vector3 a, Vector3 b)
		{
			return a.x * b.x + a.y * b.y + a.z * b.z;
		}

		public static float AngleBetween(Vector3 a, Vector3 b)
		{
			float dot = DotProduct(a, b);
			float magprod = MathF.Sqrt(a.SqrMagnitude() * b.SqrMagnitude());	// Saves doing sqrt twice
			return MathF.Acos(dot / magprod);
		}

		public static float AngleBetween(Vector3 a, Vector3 b, Vector3 c)
		{
			return AngleBetween(a - b, c - b);
		}

		public Vector3 UnitPerpendicular(Vector3 planeNormal)
		{
			return CrossProduct(this, planeNormal).Normalized();
		}

		public static float Distance(Vector3 a, Vector3 b)
		{
			return (b - a).Magnitude();
		}

		public static float SqrDistance(Vector3 a, Vector3 b)
		{
			return (b - a).SqrMagnitude();
		}

		public static Vector3 Random2D(float xMin, float xMax, float yMin, float yMax)
		{
			return new Vector3((float)random.NextDouble() * (xMax - xMin) + xMin, (float)random.NextDouble() * (yMax - yMin) + yMin);
		}

		public override bool Equals(object obj)
		{
			return obj is Vector3 vector &&
				   x == vector.x &&
				   y == vector.y &&
				   z == vector.z;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(data, x, y, z);
		}

		public override string ToString()
		{
			return $"({x}, {y}, {z})";
		}
	}
}
