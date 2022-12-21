using System;
using System.Collections.Generic;
using System.Text;

namespace SpatialTracking
{
	/// <summary>
	/// A class to manage individual cameras, their properties and their data stream.
	/// </summary>
	class LinearTrackingCamera
	{
		/// <summary>
		/// The direction that angles are measured in.
		/// </summary>
		public enum AngleDirection
		{
			Clockwise = -1,
			Anticlockwise = 1
		}

		/// <summary>
		/// The angle observed from the cameras POV.
		/// </summary>
		public float observedAngle;
		/// <summary>
		/// The world angle at which the observedAngle should equal 0.
		/// </summary>
		public readonly float zeroAngle;
		/// <summary>
		/// The direction in which angles should be measured.
		/// </summary>
		public readonly AngleDirection measurementDirection;
		/// <summary>
		/// The world position of this camera.
		/// </summary>
		public readonly Vector3 worldPosition;
		/// <summary>
		/// The channel that this object will read data from to update with the real camera it is representing.
		/// </summary>
		public readonly int channel;

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
		public LinearTrackingCamera(int channel, float zeroAngle, Vector3 worldPosition, AngleDirection measurementDirection)
		{
			this.worldPosition = worldPosition;
			this.measurementDirection = measurementDirection;
			this.zeroAngle = zeroAngle;
			this.channel = channel;
			observedAngle = -1f;
		}

		/// <summary>
		/// Update the camera with data from the SocketInterface.
		/// </summary>
		public void Update()
		{
			observedAngle = SocketInterface.GetData(channel);
		}
	}
}
