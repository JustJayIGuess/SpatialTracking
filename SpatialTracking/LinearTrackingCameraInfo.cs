using System;
using System.Collections.Generic;
using System.Text;

namespace SpatialTracking
{
	class LinearTrackingCameraInfo
	{
		public enum AngleDirection
		{
			Clockwise = -1,
			Anticlockwise = 1
		}

		public float observedAngle;
		public readonly float zeroAngle;
		public readonly AngleDirection measurementDirection;
		public readonly Vector3 worldPosition;
		public readonly int channel;

		/// <summary>
		/// 
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
		public LinearTrackingCameraInfo(int channel, float zeroAngle, Vector3 worldPosition, AngleDirection measurementDirection)
		{
			this.worldPosition = worldPosition;
			this.measurementDirection = measurementDirection;
			this.zeroAngle = zeroAngle;
			this.channel = channel;
			observedAngle = -1f;
		}

		public void Update()
		{
			observedAngle = SocketInterface.GetSimulatedData(channel);
		}
	}
}
