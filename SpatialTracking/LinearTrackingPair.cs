using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

/*
 * 
 * One camera is transmitted over channel 2n and paired camera is transmitted over channel 2n + 1
 * 
 */

namespace SpatialTracking
{
	class LinearTrackingPair
	{
		public LinearTrackingCameraInfo camera1;
		public LinearTrackingCameraInfo camera2;
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

		public LinearTrackingPair(int channel,
			float zeroAngle1, Vector3 worldPosition1, LinearTrackingCameraInfo.AngleDirection measurementDirection1,
			float zeroAngle2, Vector3 worldPosition2, LinearTrackingCameraInfo.AngleDirection measurementDirection2)
		{
			camera1 = new LinearTrackingCameraInfo(channel * 2, zeroAngle1, worldPosition1, measurementDirection1);
			camera2 = new LinearTrackingCameraInfo(channel * 2 + 1, zeroAngle2, worldPosition2, measurementDirection2);
			predictedPoint = null;
			confidence = -1f;
		}

		public LinearTrackingPair(LinearTrackingCameraInfo camera1, LinearTrackingCameraInfo camera2)
		{
			this.camera1 = camera1;
			this.camera2 = camera2;
			predictedPoint = null;
			confidence = -1f;
		}

		/// <summary>
		/// This will eventually use a socket to transfer data from Raspberry Pis on each camera to the laptop running this program and rendering logic.<br/><br/>
		/// This is designed to act with mutual independence of LinearTrackingRoom to follow the Observer-Subject programming pattern and allow for easy changing of values at runtime.
		/// </summary>
		public void Update(bool verbose)
		{
			float trueAngle1 = camera1.observedAngle * (int)camera1.measurementDirection + camera1.zeroAngle;
			float trueAngle2 = camera2.observedAngle * (int)camera2.measurementDirection + camera2.zeroAngle;

			float gradient1 = MathF.Tan(trueAngle1);
			float gradient2 = MathF.Tan(trueAngle2);

			float intercept1 = camera1.worldPosition.y - gradient1 * camera1.worldPosition.x;
			float intercept2 = camera2.worldPosition.y - gradient2 * camera2.worldPosition.x;

			float x = (intercept2 - intercept1) / (gradient1 - gradient2);
			float y = gradient1 * x + intercept1;

			predictedPoint = new Vector3(x, y);
			//confidence = MathF.Abs(trueAngle2 - trueAngle1);
			confidence = (1f - MathF.Pow(MathF.Abs(MathF.Cos(trueAngle1 - trueAngle2)), 4f)) * MathF.Exp(-(MathF.Abs(gradient1) + MathF.Abs(gradient2)) / 1000f); // Term to reduce confidence when gradients are high and more suceptible to floating point error

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
