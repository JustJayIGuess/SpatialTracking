using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

/*
 * 
 * One camera is transmitted over channel 2n and paired camera is transmitted over channel 2n + 1, where n is 'channel' provided in ctor.
 * Alternatively, this confusion can be avoided by first creating TrackingCamera objects representing the cameras, and these can be passed to an overloaded ctor.
 * 
 */

namespace SpatialTracking
{
	class LinearTrackingSet : TrackingSet
	{
		/// <summary>
		/// A float representing the confidence in the predicted point. Ranges from 0.0-1.0.
		/// <br/>
		/// <em><c>confidence = -1.0</c> if point has not yet been calculated.</em>
		/// </summary>
		public float confidence;

		/// <summary>
		/// Construct a <c>new LinearTrackingPair</c> on channel <c>channel</c> by first creating two new <c>TrackingCamera</c>s, and their respective world positions and angles.
		/// </summary>
		/// <param name="channel">The socket channel that this <c>LinearTrackingPair</c> will use.
		/// <c>camera1</c> will be assigned <c>channel</c> * 2 as its channel, and <c>camera2</c> will be assigned <c>channel</c> * 2 + 1.
		/// </param>
		/// <param name="zeroAngle1">The world angle at which this camera will observe an angle of 0.</param>
		/// <param name="worldPosition1">The world position of this camera.</param>
		/// <param name="measurementDirection1">The direction that angles are measured.</param>
		/// <param name="zeroAngle2">The world angle at which this camera will observe an angle of 0.</param>
		/// <param name="worldPosition2">The world position of this camera.</param>
		/// <param name="measurementDirection2">The direction that angles are measured.</param>
		public LinearTrackingSet(int channel,
			float zeroAngle1, Vector3 worldPosition1, TrackingCamera.AngleDirection measurementDirection1,
			float zeroAngle2, Vector3 worldPosition2, TrackingCamera.AngleDirection measurementDirection2)
		{
			Cameras = new TrackingCamera[2];
			Cameras[0] = new TrackingCamera(channel * 2, zeroAngle1, worldPosition1, measurementDirection1);
			Cameras[1] = new TrackingCamera(channel * 2 + 1, zeroAngle2, worldPosition2, measurementDirection2);
			PredictedPoint = null;
			confidence = -1f;
		}

		/// <summary>
		/// Construct a <c>new LinearTrackingPair</c> on channel <c>channel</c> based on two <c>TrackingCamera</c>s.
		/// </summary>
		/// <param name="camera1">The first camera used in this <c>LinearTrackingPair</c></param>
		/// <param name="camera2">The second camera used in this <c>LinearTrackingPair</c></param>
		public LinearTrackingSet(TrackingCamera camera1, TrackingCamera camera2)
		{
			Cameras = new TrackingCamera[2] { camera1, camera2 };
			PredictedPoint = null;
			confidence = -1f;
		}

		/// <summary>
		/// This is designed to act with mutual independence of TrackingRoom to follow the Observer-Subject programming pattern and allow for easy changing of values at runtime.
		/// </summary>
		public override void Update(bool verbose)
		{
			// Convert observed angles to true 'world angles'
			float trueAngle1 = Cameras[0].ObservedAngle * (int)Cameras[0].MeasurementDirection + Cameras[0].ZeroAngle;
			float trueAngle2 = Cameras[1].ObservedAngle * (int)Cameras[1].MeasurementDirection + Cameras[1].ZeroAngle;

			// Find gradients of rays exiting the cameras at their true angles
			float gradient1 = MathF.Tan(trueAngle1);
			float gradient2 = MathF.Tan(trueAngle2);
			
			// Find y-intercepts of these rays
			float intercept1 = Cameras[0].WorldPosition.y - gradient1 * Cameras[0].WorldPosition.x;
			float intercept2 = Cameras[1].WorldPosition.y - gradient2 * Cameras[1].WorldPosition.x;

			// Find intersection of the rays
			float x = (intercept2 - intercept1) / (gradient1 - gradient2);
			float y = gradient1 * x + intercept1;
			PredictedPoint = new Vector3(x, y);

			// Assign a confidence value (0f-1f) to the intersection. This is based on:
			//		- How perpendicular the rays are; if they are near-parallel then small deviations will
			//		  change the point of intersection drastically, so these should be assigned low confidence.
			//		- How large the gradients are; if they are very large, floating point precision becomes a
			//		  problem, so these should be assigned low confidence.
			confidence = (1f - MathF.Pow(MathF.Abs(MathF.Cos(trueAngle1 - trueAngle2)), 4f)) * MathF.Exp(-(MathF.Abs(gradient1) + MathF.Abs(gradient2)) / 1000f); // Term to reduce confidence when gradients are high and more suceptible to floating point error

			// Print info about this intersection if asked.
			if (verbose)
			{
				Console.WriteLine($"Calculated m1={gradient1}, m2={gradient2} from cameras:\n\t1:\n" +
					$"\t\tpos: {Cameras[0].WorldPosition}\n" +
					$"\t\tphi: {Cameras[0].ZeroAngle * 180f / MathF.PI}\n" +
					$"\t\ttheta: {Cameras[0].ObservedAngle * 180f / MathF.PI}\n\t2:\n" +
					$"\t\tpos: {Cameras[1].WorldPosition}\n" +
					$"\t\tphi: {Cameras[1].ZeroAngle * 180f / MathF.PI}\n" +
					$"\t\ttheta: {Cameras[1].ObservedAngle * 180f / MathF.PI}\n" +
					$"on channel ({Cameras[0].Channel}, {Cameras[1].Channel}) with confidence {confidence}");
				Console.WriteLine(PredictedPoint);
			}
		}
	}
}
