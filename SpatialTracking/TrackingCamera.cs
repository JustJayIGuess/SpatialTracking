using System;
using System.Collections.Generic;
using System.Text;

namespace SpatialTracking
{
	abstract class TrackingCamera
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
		public float ObservedAngle { get; set; }
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

		public abstract void Update();
	}
}
