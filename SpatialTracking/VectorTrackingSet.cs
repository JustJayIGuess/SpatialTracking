using System;
using System.Collections.Generic;
using System.Text;

namespace SpatialTracking
{
	/// <summary>
	/// In this tracking set, each camera holds a horizontal and vertical angle (stored respectively
	/// as two consecutive numbers in ObservedAngles), with the known position of the tracked marker
	/// in the WorldPosition property. ZeroAngle is not used as the orientation of the camera is unknown.
	/// </summary>
	class VectorTrackingSet : TrackingSet
	{
		/// <summary>
		/// Construct a <c>new VectorTrackingPair</c> based on given <c>TrackingCamera</c>s.<br/>
		/// Note: each 'camera' symbolically represents one tracking marker.
		/// </summary>
		/// <param name="cameras">The cameras used in this <c>LinearTrackingPair</c></param>
		public VectorTrackingSet(params TrackingCamera[] cameras)
		{
			Cameras = cameras;
			PredictedPoint = null;
		}

		public override void Update(bool verbose)
		{
			Matrix[] directions = new Matrix[Cameras.Length];
			Matrix[] offsets = new Matrix[Cameras.Length];

			// ObservedAngles = [theta, phi]

			Matrix m = new Matrix(3, 3, 0f);
			for (int i = 0; i < Cameras.Length; i++)
			{
				directions[i] = new Vector3(Cameras[i].ObservedAngles[0], Cameras[i].ObservedAngles[1]).ToMatrix();
				offsets[i] = Cameras[i].WorldPosition.ToMatrix();

				m = m + Matrix.Identity(3) - (directions[i] * Matrix.Transpose(directions[i]));
			}

			throw new NotImplementedException();
		}
	}
}
