using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

/*
 * 
 * One camera is transmitted over channel 2n and paired camera is transmitted over channel 2n + 1, where n is 'channel' provided in ctor.
 * Alternatively, this confusion can be avoided by first creating LinearTrackingCamera objects representing the cameras, and these can be passed to an overloaded ctor.
 * 
 */

namespace SpatialTracking
{
	class LinearTrackingPair
	{
		/// <summary>
		/// One of the cameras in this <c>LinearTrackingPair</c>
		/// </summary>
		public LinearTrackingCamera camera1;
		/// <summary>
		/// One of the cameras in this <c>LinearTrackingPair</c>
		/// </summary>
		public LinearTrackingCamera camera2;
		/// <summary>
		/// The predicted position of the observed object.
		/// <br/>
		/// <c>null</c> if point has not yet been calculated.
		/// </summary>
		public Vector3? predictedPoint;
		/// <summary>
		/// A float representing the confidence in the predicted point. Ranges from 0.0-1.0.
		/// <br/>
		/// <em><c>cconfidence = -1.0</c> if point has not yet been calculated.</em>
		/// </summary>
		public float confidence;

		/// <summary>
		/// Construct a <c>new LinearTrackingPair</c> on channel <c>channel</c> by first creating two new <c>LinearTrackingCamera</c>s, and their respective world positions and angles.
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
		public LinearTrackingPair(int channel,
			float zeroAngle1, Vector3 worldPosition1, LinearTrackingCamera.AngleDirection measurementDirection1,
			float zeroAngle2, Vector3 worldPosition2, LinearTrackingCamera.AngleDirection measurementDirection2)
		{
			camera1 = new LinearTrackingCamera(channel * 2, zeroAngle1, worldPosition1, measurementDirection1);
			camera2 = new LinearTrackingCamera(channel * 2 + 1, zeroAngle2, worldPosition2, measurementDirection2);
			predictedPoint = null;
			confidence = -1f;
		}

		/// <summary>
		/// Construct a <c>new LinearTrackingPair</c> on channel <c>channel</c> based on two <c>LinearTrackingCamera</c>s.
		/// </summary>
		/// <param name="camera1">The first camera used in this <c>LinearTrackingPair</c></param>
		/// <param name="camera2">The second camera used in this <c>LinearTrackingPair</c></param>
		public LinearTrackingPair(LinearTrackingCamera camera1, LinearTrackingCamera camera2)
		{
			this.camera1 = camera1;
			this.camera2 = camera2;
			predictedPoint = null;
			confidence = -1f;
		}

		/// <summary>
		/// This is designed to act with mutual independence of LinearTrackingRoom to follow the Observer-Subject programming pattern and allow for easy changing of values at runtime.
		/// </summary>
		public void Update(bool verbose)
		{
			// Convert observed angles to true 'world angles'
			float trueAngle1 = camera1.observedAngle * (int)camera1.measurementDirection + camera1.zeroAngle;
			float trueAngle2 = camera2.observedAngle * (int)camera2.measurementDirection + camera2.zeroAngle;

			// Find gradients of rays exiting the cameras at their true angles
			float gradient1 = MathF.Tan(trueAngle1);
			float gradient2 = MathF.Tan(trueAngle2);
			
			// Find y-intercepts of these rays
			float intercept1 = camera1.worldPosition.y - gradient1 * camera1.worldPosition.x;
			float intercept2 = camera2.worldPosition.y - gradient2 * camera2.worldPosition.x;

			// Find intersection of the rays
			float x = (intercept2 - intercept1) / (gradient1 - gradient2);
			float y = gradient1 * x + intercept1;
			predictedPoint = new Vector3(x, y);

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
					$"\t\tpos: {camera1.worldPosition}\n" +
					$"\t\tphi: {camera1.zeroAngle * 180f / MathF.PI}\n" +
					$"\t\ttheta: {camera1.observedAngle * 180f / MathF.PI}\n\t2:\n" +
					$"\t\tpos: {camera2.worldPosition}\n" +
					$"\t\tphi: {camera2.zeroAngle * 180f / MathF.PI}\n" +
					$"\t\ttheta: {camera2.observedAngle * 180f / MathF.PI}\n" +
					$"on channel ({camera1.channel}, {camera2.channel}) with confidence {confidence}");
				Console.WriteLine(predictedPoint);
			}
		}
	}
}
