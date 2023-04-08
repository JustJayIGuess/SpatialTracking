using System;
using System.Collections.Generic;
using System.Text;

namespace SpatialTracking
{
	/// <summary>
	/// A class to manage individual cameras, their properties and their data stream.
	/// </summary>
	class TrackingCamera
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
		public float[] ObservedAngles { get; set; }
		/// <summary>
		/// The world angle at which the observedAngle should equal 0.
		/// </summary>
		public float ZeroAngle { get; protected set; }
		/// <summary>
		/// The direction in which angles should be measured.
		/// </summary>
		public AngleDirection MeasurementDirection { get; protected set; }
		/// <summary>
		/// The world position associated with this camera (can be camera's position of tracked point's known position)
		/// </summary>
		public Vector3 WorldPosition { get; protected set; }
		/// <summary>
		/// The channel that this object will read data from to update with the real camera it is representing.
		/// </summary>
		public int Channel { get; protected set; }

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
		public TrackingCamera(int channel, float zeroAngle, Vector3 worldPosition, AngleDirection measurementDirection)
		{
			WorldPosition = worldPosition;
			MeasurementDirection = measurementDirection;
			ZeroAngle = zeroAngle;
			Channel = channel;
			ObservedAngles = null;
		}

		/// <summary>
		/// Update the camera with data from the SocketInterface.
		/// </summary>
		public void Update()
		{
			ObservedAngles = SocketInterface.GetData(Channel);
		}
	}
}
