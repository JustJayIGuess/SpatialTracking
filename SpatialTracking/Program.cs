using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SpatialTracking
{

	class Program
	{
		//public static CircularTrackingRoom room;

		static async Task Main(string[] args)
		{
			LinearTrackingRoom room = new LinearTrackingRoom();

			// Defining 8 cameras
			LinearTrackingCameraInfo a = new LinearTrackingCameraInfo(
				0, 
				0.5f * MathF.PI, 
				new Vector3(0f, 0f), 
				LinearTrackingCameraInfo.AngleDirection.Clockwise);
			LinearTrackingCameraInfo ap = new LinearTrackingCameraInfo(
				1,
				0f, 
				new Vector3(300f, 0f), 
				LinearTrackingCameraInfo.AngleDirection.Clockwise);
			LinearTrackingCameraInfo b = new LinearTrackingCameraInfo(
				2,
				0f,
				new Vector3(0f, 150f),
				LinearTrackingCameraInfo.AngleDirection.Clockwise);
			LinearTrackingCameraInfo bp = new LinearTrackingCameraInfo(
				3,
				1.5f * MathF.PI,
				new Vector3(300f, 150f),
				LinearTrackingCameraInfo.AngleDirection.Clockwise);
			LinearTrackingCameraInfo c = new LinearTrackingCameraInfo(
				4,
				MathF.PI,
				new Vector3(150f, 0f),
				LinearTrackingCameraInfo.AngleDirection.Clockwise);
			LinearTrackingCameraInfo cp = new LinearTrackingCameraInfo(
				5,
				0.5f * MathF.PI,
				new Vector3(0f, 75f),
				LinearTrackingCameraInfo.AngleDirection.Clockwise);
			LinearTrackingCameraInfo d = new LinearTrackingCameraInfo(
				6,
				0f,
				new Vector3(150f, 150f),
				LinearTrackingCameraInfo.AngleDirection.Clockwise);
			LinearTrackingCameraInfo dp = new LinearTrackingCameraInfo(
				7,
				1.5f * MathF.PI,
				new Vector3(300f, 75f),
				LinearTrackingCameraInfo.AngleDirection.Clockwise);

			// Pairing 8 cameras
			LinearTrackingPair trackingPairA = new LinearTrackingPair(a, ap);
			LinearTrackingPair trackingPairB = new LinearTrackingPair(b, bp);
			LinearTrackingPair trackingPairC = new LinearTrackingPair(c, cp);
			LinearTrackingPair trackingPairD = new LinearTrackingPair(d, dp);
			LinearTrackingPair trackingPairE = new LinearTrackingPair(c, dp);
			LinearTrackingPair trackingPairF = new LinearTrackingPair(d, cp);

			// Adding camera pairs
			room.Add(trackingPairA);
			room.Add(trackingPairB);
			room.Add(trackingPairC);
			room.Add(trackingPairD);
			room.Add(trackingPairE);
			room.Add(trackingPairF);

			//Vector3 target = new Vector3(242.12247f, 103.94585f);
			//SocketInterface.SimulateData(target, room, 0.001f); // 0.001f is ~1 pixel for 60deg FOV camera at 720x1280

			//room.Update(true);

			bool verbose = false; // This should really be a preprocessor thingo but idc
			int iterations = 100000;
			int[] progSteps = new int[20];
			for (int i = 0; i < progSteps.Length; i++)
			{
				progSteps[i] = (int)MathF.Ceiling(iterations * ((float)i / progSteps.Length));
			}

			List<(Vector3, Vector3, float, float)> errors = new List<(Vector3, Vector3, float, float)>();

			if (!verbose)
			{
				Console.WriteLine(new string('_', progSteps.Length));
			}
			for (int i = 0; i < iterations; i++)
			{
				Vector3 target = Vector3.Random2D(10f, 290f, 10f, 140f);
				float error = 0f;

				SocketInterface.SimulateData(target, room, 0.01f); // 0.001f rads is ~1 pixel for 60deg FOV camera at 720x1280

				room.Update(verbose);

				Vector3 sum = new Vector3(0f, 0f);
				float totalConfidence = 0f;
				foreach (LinearTrackingPair trackingPair in room.trackingPairs)
				{
					totalConfidence += trackingPair.confidence;
				}

				foreach (LinearTrackingPair trackingPair in room.trackingPairs)
				{
					sum += (Vector3)trackingPair.predictedPoint * trackingPair.confidence / totalConfidence;
				}
				error += Vector3.SqrDistance(target, sum);

				errors.Add((target, sum, error, totalConfidence));

				if (verbose)
				{
					Console.WriteLine($"Got {sum}.");
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

			Console.WriteLine("Writing to file...");
			using (TextWriter tw = new StreamWriter("LinearTrackingRoom.txt"))
			{
				foreach ((Vector3, Vector3, float, float) e in errors)
					tw.WriteLine($"{e.Item1}, {e.Item2}, {e.Item3}, {e.Item4}");
			}

			Console.WriteLine("\n\nPress enter to exit.");
			Console.ReadLine();
		}

		//For circular tracking architecture
		//static void Main(string[] args)
		//{
		//	room = new CircularTrackingRoom(13f,
		//		(new Vector3(70f, 0f), Vector3.forward),
		//		(new Vector3(140f, 51f), Vector3.left),
		//		(new Vector3(70f, 103f), Vector3.backward),
		//		(new Vector3(0f, 20f), Vector3.right)
		//	);

		//	//Console.Write("x: ");
		//	//float x = float.Parse(Console.ReadLine());
		//	//Console.Write("y: ");
		//	//float y = float.Parse(Console.ReadLine());
		//	//Console.Write("z: ");
		//	//float z = float.Parse(Console.ReadLine());

		//	Vector3 v = new Vector3(70f, 50f, 0f);
		//	float[] angles = room.SetPositionAndCalculateAngles(v);

		//	Console.Write("[");
		//	foreach (float angle in angles)
		//	{
		//		Console.Write(angle + ", ");
		//	}
		//	Console.WriteLine("]");

		//	Vector3 output = room.SetAnglesAndCalculatePosition(angles);
		//	Console.WriteLine(output);

		//	float offset = 0.01f;

		//	Random rand = new Random((int)DateTime.Now.Ticks);
		//	for (int i = 0; i < 100; i++)
		//	{
		//		for (int j = 0; j < angles.Length; j++)
		//		{
		//			angles[j] += /*((float)rand.NextDouble() * 2f - 1f) * */ (rand.Next(0, 1) * 2f - 1f) * offset;
		//		}
		//		output = room.SetAnglesAndCalculatePosition(angles);
		//		Console.WriteLine($"{i}: {output}");
		//	}

		//	//List<(Vector3, Vector3, float)> errors = new List<(Vector3, Vector3, float)>();

		//	//for (int i = 0; i < 1000000; i++)
		//	//{
		//	//	Vector3 v = Vector3.Random2D(0f, 140f, 0f, 103f);

		//	//	float[] angles = room.SetPositionAndCalculateAngles(v);

		//	//	Vector3 output = room.SetAnglesAndCalculatePosition(angles);
		//	//	float error = Vector3.Distance(output, v);
		//	//	Console.WriteLine($"Error: {error}\n\t{v}\n\t{output}");

		//	//	errors.Add((v, output, error));
		//	//}

		//	//using (TextWriter tw = new StreamWriter("SavedList.txt"))
		//	//{
		//	//	foreach ((Vector3, Vector3, float) e in errors)
		//	//		tw.WriteLine($"{e.Item1}, {e.Item2}, {e.Item3}");
		//	//}

		//	Console.ReadLine();
		//}
	}
}
