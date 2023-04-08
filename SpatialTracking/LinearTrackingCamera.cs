using System;
using System.Collections.Generic;
using System.Text;

namespace SpatialTracking
{
	/// <summary>
	/// A class to manage individual cameras, their properties and their data stream.
	/// </summary>
	class LinearTrackingCamera : TrackingCamera
	{
		/// <summary>
		/// Create a new camera on channel <c>channel</c>.
		/// </summary>
		/// <param name="zeroAngle">
		/// The positive angle in radians measured anticlockwise from the x axis where the camera will measure an angle of 0 radians.<br/>
		/// 0 &lt;= zeroAngle &lt;= 2pi
		/// </param>
		/// <param name="worldPosition">
		/// The position of the camera in the world
		/// </param>
		/// <param name="measurementDirection">
		/// The direction that the camera measures angles from the zeroangle point, as seen from above.
		/// </param>
		public LinearTrackingCamera(int channel, float zeroAngle, Vector3 worldPosition, TrackingCamera.AngleDirection measurementDirection)
		{
			WorldPosition = worldPosition;
			MeasurementDirection = measurementDirection;
			ZeroAngle = zeroAngle;
			Channel = channel;
			ObservedAngle = -1f;
		}

		/// <summary>
		/// Update the camera with data from the SocketInterface.
		/// </summary>
		public override void Update()
		{
			ObservedAngle = SocketInterface.GetData(Channel);
		}
	}
}
