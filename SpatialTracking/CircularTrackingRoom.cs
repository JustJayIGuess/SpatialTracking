using System;
using System.Collections.Generic;

namespace SpatialTracking
{
	class CircularTrackingRoom
	{
		private readonly float referenceRadius;
		private Vector3? trackingTarget = null;
		private readonly float mergeDistanceSqr = 0.05f;

		public List<CircularTrackingReference> References { get; private set; }

		public CircularTrackingRoom(float referenceRadius)
		{
			this.referenceRadius = referenceRadius;
			References = new List<CircularTrackingReference>();
		}

		public CircularTrackingRoom(float referenceRadius, params (Vector3 midpoint, Vector3 direction)[] initReferences)
		{
			this.referenceRadius = referenceRadius;
			References = new List<CircularTrackingReference>();

			foreach ((Vector3 midpoint, Vector3 direction) in initReferences)
			{
				AddTrackingReference(midpoint, direction);
			}
		}

		public void AddTrackingReference(Vector3 midpoint, Vector3 direction)
		{
			References.Add(new CircularTrackingReference(midpoint, referenceRadius, direction));
		}

		/// <summary>
		/// Currently only considers 2D
		/// </summary>
		/// <param name="position"></param>
		public float[] SetPositionAndCalculateAngles(Vector3 position)
		{
			trackingTarget = position.Copy();
			float[] angles = new float[References.Count];

			for (int i = 0; i < References.Count; i++)
			{
				CircularTrackingReference reference = References[i];
				reference.ObservedAngle = Vector3.AngleBetween(reference.Extents.a, (Vector3)trackingTarget, reference.Extents.b);
				angles[i] = (float)reference.ObservedAngle;
			}

			return angles;
		}

		public Vector3 SetAnglesAndCalculatePosition(params float[] angles)
		{
			if (angles.Length != References.Count)
			{
				throw new Exception($"Incorrect number of angles given in SetAnglesAndCalculatePosition(): {angles.Length} given when {References.Count} angle/s were expected!");
			}
			else
			{
				for (int i = 0; i < References.Count; i++)
				{
					References[i].ObservedAngle = angles[i];
					References[i].CalculateCentre();
				}

				List<Vector3> intersections = new List<Vector3>();

				for (int i = 0; i < References.Count; i++)
				{
					CircularTrackingReference p0 = References[i];
					CircularTrackingReference p1 = References[(i + 1) % References.Count];

					float r0Sqr = p0.CircleRadius * p0.CircleRadius;
					float r1Sqr = p1.CircleRadius * p1.CircleRadius;

					float d = Vector3.Distance(p0.Centre, p1.Centre);
					float a = (r0Sqr - r1Sqr + d * d) / (2f * d);

					float discriminant = r0Sqr - (a * a);
					if (discriminant < 0f)
					{
						continue;
					}
					float h = MathF.Sqrt(discriminant);

					Vector3 p2 = p0.Centre + (a / d) * (p1.Centre - p0.Centre);
					Vector3 offset = (h / d) * new Vector3(1f, 1f, 0f);
					offset.x *= (p1.Centre.y - p0.Centre.y);
					offset.y *= (p0.Centre.x - p1.Centre.x);

					Vector3 p3 = p2 + offset;
					Vector3 p4 = p2 - offset;

					intersections.Add(p3);
					intersections.Add(p4);
				}

				int[] scores = new int[intersections.Count];
				for (int i = 0; i < intersections.Count - 1; i++)
				{
					for (int j = i + 1; j < intersections.Count; j++)
					{
						if (Vector3.SqrDistance(intersections[i], intersections[j]) < mergeDistanceSqr)
						{
							scores[i]++;
							scores[j]++;
						}
					}
				}

				List<int> bestIndices = new List<int>() { 0 };
				for (int i = 0; i < intersections.Count; i++)
				{
					if (scores[i] > scores[bestIndices[0]])
					{
						bestIndices.Clear();
						bestIndices.Add(i);
					}
					else if (scores[i] == scores[bestIndices[0]])
					{
						bestIndices.Add(i);
					}
				}

				Vector3 sum = new Vector3(0f, 0f, 0f);
				for (int i = 0; i < bestIndices.Count; i++)
				{
					sum += intersections[bestIndices[i]];
				}
				sum /= bestIndices.Count;

				trackingTarget = sum;
				return sum;
			}
		}
	}
}
