namespace SpatialTrackingBrain
{
	/// <summary>
	/// A tracking set holds a number of <c>TrackingCamera</c>s, and update a <c>PredictedPoint</c> variable when <c>Update()</c> is called.
	/// </summary>
	abstract class TrackingSet
	{
		/// <summary>
		/// The set of cameras in this tracking set.
		/// </summary>
		public TrackingCamera[] Cameras { get; protected set; }
		/// <summary>
		/// The predicted position of the observed object.
		/// <br/>
		/// <c>null</c> if point has not yet been calculated.
		/// </summary>
		public Vector3? PredictedPoint { get; protected set; }

		public abstract void Update(bool verbose);
	}
}
