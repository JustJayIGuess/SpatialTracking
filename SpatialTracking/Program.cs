#define VECTOR
#define WRITE

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;


namespace SpatialTracking
{
	class Program
	{
		static void Main(string[] args)
		{
			TrackingRoom room = new TrackingRoom();

			bool verbose = false;		// This should really be a preprocessor thingo but idc
			int iterations = 100000;	// Increase this to make the outputted error file more detailed
			float noise = 0.01f;

			// Progress bar
			int[] progSteps = new int[20];
			for (int i = 0; i < progSteps.Length; i++)
			{
				progSteps[i] = (int)MathF.Ceiling(iterations * ((float)i / progSteps.Length));
			}

			// Stores per-position data for simulated error of algorithm
			List<(Vector3, Vector3, float)> errors = new List<(Vector3, Vector3, float)>();

			if (!verbose)
			{
				Console.WriteLine(new string('_', progSteps.Length));
			}

#if VECTOR
			TrackingCamera a = new TrackingCamera(0, 0f, new Vector3(-1f, -1f, -1f), TrackingCamera.AngleDirection.Clockwise);
			TrackingCamera b = new TrackingCamera(1, 0f, new Vector3(-1f, -1f, 1f), TrackingCamera.AngleDirection.Clockwise);
			TrackingCamera c = new TrackingCamera(2, 0f, new Vector3(-1f, 1f, -1f), TrackingCamera.AngleDirection.Clockwise);
			TrackingCamera d = new TrackingCamera(3, 0f, new Vector3(-1f, 1f, 1f), TrackingCamera.AngleDirection.Clockwise);
			TrackingCamera e = new TrackingCamera(4, 0f, new Vector3(1f, -1f, -1f), TrackingCamera.AngleDirection.Clockwise);
			TrackingCamera f = new TrackingCamera(5, 0f, new Vector3(1f, -1f, 1f), TrackingCamera.AngleDirection.Clockwise);
			TrackingCamera g = new TrackingCamera(6, 0f, new Vector3(1f, 1f, -1f), TrackingCamera.AngleDirection.Clockwise);
			TrackingCamera h = new TrackingCamera(7, 0f, new Vector3(1f, 1f, 1f), TrackingCamera.AngleDirection.Clockwise);

			VectorTrackingSet trackingSet = new VectorTrackingSet(a, b, c, d, e, f, g, h);

			room.AddTrackingSet(trackingSet);
#elif LINEAR
			TrackingCamera a = new TrackingCamera(
				0,
				0.5f * MathF.PI,
				new Vector3(-1f, -1f, 0f),
				TrackingCamera.AngleDirection.Clockwise);
			TrackingCamera ap = new TrackingCamera(
				1,
				0f,
				new Vector3(1f, -1f, 0f),
				TrackingCamera.AngleDirection.Clockwise);
			TrackingCamera b = new TrackingCamera(
				2,
				0f,
				new Vector3(-1f, 1f, 0f),
				TrackingCamera.AngleDirection.Clockwise);
			TrackingCamera bp = new TrackingCamera(
				3,
				1.5f * MathF.PI,
				new Vector3(1f, 1f, 0f),
				TrackingCamera.AngleDirection.Clockwise);
			TrackingCamera c = new TrackingCamera(
				4,
				MathF.PI,
				new Vector3(0f, -1f, 0f),
				TrackingCamera.AngleDirection.Clockwise);
			TrackingCamera cp = new TrackingCamera(
				5,
				0.5f * MathF.PI,
				new Vector3(-1f, 0f, 0f),
				TrackingCamera.AngleDirection.Clockwise);
			TrackingCamera d = new TrackingCamera(
				6,
				0f,
				new Vector3(0f, 1f, 0f),
				TrackingCamera.AngleDirection.Clockwise);
			TrackingCamera dp = new TrackingCamera(
				7,
				1.5f * MathF.PI,
				new Vector3(1f, 0f, 0f),
				TrackingCamera.AngleDirection.Clockwise);

			// Pairing 8 cameras
			LinearTrackingSet trackingPairA = new LinearTrackingSet(a, ap);
			LinearTrackingSet trackingPairB = new LinearTrackingSet(b, bp);
			LinearTrackingSet trackingPairC = new LinearTrackingSet(c, cp);
			LinearTrackingSet trackingPairD = new LinearTrackingSet(d, dp);
			LinearTrackingSet trackingPairE = new LinearTrackingSet(c, dp);
			LinearTrackingSet trackingPairF = new LinearTrackingSet(d, cp);

			// Adding camera pairs
			room.AddTrackingSet(trackingPairA);
			room.AddTrackingSet(trackingPairB);
			room.AddTrackingSet(trackingPairC);
			room.AddTrackingSet(trackingPairD);
			room.AddTrackingSet(trackingPairE);
			room.AddTrackingSet(trackingPairF);
#endif
			Random random = new Random();
			for (int i = 0; i < iterations; i++)
			{
				// Select random point in room and simulate incoming (noisy) data from cameras for that point
				Vector3 target = Vector3.Random2D(-1f, 1f, -1f, 1f);
				target.z = ((float)random.NextDouble()) * 2f - 1f;
				float error = 0f;

#if VECTOR
				SocketInterface.SimulateVectorData(target, room, noise); // 0.001f rads is ~1 pixel for 60deg FOV camera at 720x1280
#elif LINEAR
				SocketInterface.SimulateLinearData(target, room, noise); // 0.001f rads is ~1 pixel for 60deg FOV camera at 720x1280
#endif
				// Tell the room to update based on simulated buffer stored in SocketInterface
				room.Update(verbose);

				// This should be within TrackingRoom
				Vector3 prediction = new Vector3(0f, 0f, 0f);
#if VECTOR
				prediction = (Vector3)trackingSet.PredictedPoint;
#elif LINEAR
				float totalConfidence = 0f;
				foreach (LinearTrackingSet trackingPair in room.TrackingSets)
				{
					totalConfidence += trackingPair.confidence;
				}

				foreach (LinearTrackingSet trackingPair in room.TrackingSets)
				{
					prediction += (Vector3)trackingPair.PredictedPoint * trackingPair.confidence / totalConfidence;
				}
				target.z = 0f;	// Linear doesn't support z, so disable for error calc.
#endif
				error += Vector3.SqrDistance(target, prediction);  // SqrDistance 'cause faster

				errors.Add((target, prediction, error));

				if (verbose)
				{
					Console.WriteLine($"Got {prediction}.");
					Console.WriteLine($"Average Error for {target}: {error}.\n\nBuffer:");
					SocketInterface.PrintBuffer();
				}
				else if (progSteps.Contains(i))
				{
					Console.Write("#");
				}
			}

			if (!verbose)
			{
				Console.WriteLine();
			}

#if WRITE
			// Write error data to file
			Console.WriteLine("Writing to file...");
			string arcType = room.TrackingSets[0] is LinearTrackingSet ? "Linear" : "Vector";
			using (TextWriter tw = new StreamWriter($"TrackingRoom{arcType}-{iterations}.txt"))
			{
				foreach ((Vector3, Vector3, float) error in errors)
				{
					tw.WriteLine($"{error.Item1}, {error.Item2}, {error.Item3}");
				}
			}
#endif

			Console.WriteLine("\n\nPress enter to exit.");
			Console.ReadLine();
		}

#if CIRCULAR
		//For circular tracking architecture
		public static CircularTrackingRoom room;

		static void Main(string[] args)
		{
			room = new CircularTrackingRoom(13f,
				(new Vector3(70f, 0f, 0f), Vector3.forward),
				(new Vector3(140f, 51f, 0f), Vector3.left),
				(new Vector3(70f, 103f, 0f), Vector3.backward),
				(new Vector3(0f, 20f, 0f), Vector3.right)
			);

			//Console.Write("x: ");
			//float x = float.Parse(Console.ReadLine());
			//Console.Write("y: ");
			//float y = float.Parse(Console.ReadLine());
			//Console.Write("z: ");
			//float z = float.Parse(Console.ReadLine());

			Vector3 v = new Vector3(70f, 50f, 0f);
			float[] angles = room.SetPositionAndCalculateAngles(v);

			Console.Write("[");
			foreach (float angle in angles)
			{
				Console.Write(angle + ", ");
			}
			Console.WriteLine("]");

			Vector3 output = room.SetAnglesAndCalculatePosition(angles);
			Console.WriteLine(output);

			float offset = 0.01f;

			Random rand = new Random((int)DateTime.Now.Ticks);
			for (int i = 0; i < 100; i++)
			{
				for (int j = 0; j < angles.Length; j++)
				{
					angles[j] += /*((float)rand.NextDouble() * 2f - 1f) * */ (rand.Next(0, 1) * 2f - 1f) * offset;
				}
				output = room.SetAnglesAndCalculatePosition(angles);
				Console.WriteLine($"{i}: {output}");
			}

			//List<(Vector3, Vector3, float)> errors = new List<(Vector3, Vector3, float)>();

			//for (int i = 0; i < 1000000; i++)
			//{
			//	Vector3 v = Vector3.Random2D(0f, 140f, 0f, 103f);

			//	float[] angles = room.SetPositionAndCalculateAngles(v);

			//	Vector3 output = room.SetAnglesAndCalculatePosition(angles);
			//	float error = Vector3.Distance(output, v);
			//	Console.WriteLine($"Error: {error}\n\t{v}\n\t{output}");

			//	errors.Add((v, output, error));
			//}

			//using (TextWriter tw = new StreamWriter("SavedList.txt"))
			//{
			//	foreach ((Vector3, Vector3, float) e in errors)
			//		tw.WriteLine($"{e.Item1}, {e.Item2}, {e.Item3}");
			//}

			Console.ReadLine();
		}
#endif

			}
}
