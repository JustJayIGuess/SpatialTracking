using System;

namespace SpatialTrackingBrain
{
	class CircularTrackingReference
	{
		public Vector3 Centre { get; private set; }
		public float CircleRadius { get; private set; }

		private Vector3? midpoint;
		public Vector3? Midpoint
		{
			get => midpoint;
			private set
			{
				midpoint = value;
				UpdateExtents();
			}
		}

		private float extentRadius;
		public float ExtentRadius
		{
			get => extentRadius;
			private set
			{
				extentRadius = value;
				UpdateExtents();
			}
		}

		private Vector3? direction;
		public Vector3? Direction
		{
			get => direction;
			private set
			{
				direction = value?.Normalized();
				UpdateExtents();
			}
		}

		private (Vector3 a, Vector3 b) extents;
		public (Vector3 a, Vector3 b) Extents { get => extents; }
		
		public float? ObservedAngle { get; set; } = null;

		private void UpdateExtents()
		{
			if (direction != null && midpoint != null)
			{
				Vector3 perp = (Vector3)direction?.UnitPerpendicular(Vector3.up);
				extents.a = (Vector3)midpoint + perp * extentRadius;
				extents.b = (Vector3)midpoint - perp * extentRadius;
			}
		}

		public CircularTrackingReference(Vector3 midpoint, float radius, Vector3 direction)
		{
			ExtentRadius = radius;
			Midpoint = midpoint.Copy();
			Direction = direction;
			ObservedAngle = null;
		}

		public void CalculateCentre()
		{
			if (Midpoint != null && Direction != null)
			{
				Centre = (Vector3)Midpoint + (Vector3)Direction * ExtentRadius / MathF.Tan((float)ObservedAngle);
				CircleRadius = ExtentRadius / MathF.Sin((float)ObservedAngle);
			}
		}
	}
}
