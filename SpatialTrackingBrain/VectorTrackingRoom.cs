using System;
using System.Collections.Generic;
using System.Text;

namespace SpatialTrackingBrain
{
	class VectorTrackingRoom : TrackingRoom
	{
		public override Vector3? GetPredictedPoint()
		{
			return TrackingSets[0].PredictedPoint;
		}
	}
}
