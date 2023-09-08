namespace SpatialTrackingBrain
{
	/// <summary>
	/// Vector tracking algorithm uses only one <c>TrackingSet</c> that contains all cameras.
	/// This class just returns the predicted point of the sole tracking set attached to it.
	/// </summary>
	class VectorTrackingRoom : TrackingRoom
	{
		public override Vector3? GetPredictedPoint()
		{
			return TrackingSets[0].PredictedPoint;
		}
	}
}
