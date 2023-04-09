using System;
using System.Collections.Generic;
using System.Text;

namespace SpatialTrackingBrain
{
	class LinearTrackingRoom : TrackingRoom
	{
		public override Vector3? GetPredictedPoint()
		{
			Vector3 res = new Vector3(0f, 0f, 0f);
			float totalConfidence = 0f;
			foreach (LinearTrackingSet trackingSet in TrackingSets)
			{
				totalConfidence += trackingSet.confidence;
			}

			foreach (LinearTrackingSet trackingSet in TrackingSets)
			{
				res += (Vector3)trackingSet.PredictedPoint * trackingSet.confidence;
			}

			return res / totalConfidence;
		}
	}
}
