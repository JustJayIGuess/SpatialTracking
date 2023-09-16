using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace SpatialTrackingBrain
{
	abstract class TrackingRoom
	{
		public List<TrackingSet> TrackingSets { get; protected set; }
		protected Dictionary<TrackingCamera, int> cameraUsageCount;

		public TrackingRoom()
		{
			TrackingSets = new List<TrackingSet>();
			cameraUsageCount = new Dictionary<TrackingCamera, int>();
		}

		public void AddTrackingSet(TrackingSet trackingSet)
		{
			if (TrackingSets.Contains(trackingSet))
			{
				return;
			}
			TrackingSets.Add(trackingSet);
			SocketInterface.AddChannels(trackingSet);

			foreach (TrackingCamera camera in trackingSet.Cameras)
			{
				if (cameraUsageCount.ContainsKey(camera))
				{
					cameraUsageCount[camera] += 1;
				}
				else
				{
					cameraUsageCount.Add(camera, 1);
				}
			}
		}

		public void RemoveTrackingSet(TrackingSet trackingSet)
		{
			TrackingSets.Remove(trackingSet);
			foreach (TrackingCamera camera in trackingSet.Cameras)
			{
				if (cameraUsageCount[camera] == 1)
				{
					cameraUsageCount.Remove(camera);
					SocketInterface.RemoveChannel(camera.Channel);
				}
				else
				{
					cameraUsageCount[camera] -= 1;
				}
			}
		}
		public void Update(bool verbose = false)
		{
			foreach (KeyValuePair<TrackingCamera, int> entry in cameraUsageCount)
			{
				entry.Key.Update();
			}

			foreach (TrackingSet trackingSet in TrackingSets)
			{
				trackingSet.Update(verbose);
			}
		}

		public abstract Vector3? GetPredictedPoint();

		//public void StartServer()
		//{
		//	IPHostEntry ipHost = Dns.GetHostEntry(Dns.GetHostName());
		//	IPAddress ipAddr = ipHost.AddressList[0];
		//	IPEndPoint dataEndpoint = new IPEndPoint(ipAddr, port);
		//	IPEndPoint discoveryEndpoint = new IPEndPoint(ipAddr, broadPort);

		//	Console.WriteLine(ipAddr.ToString());

		//	Socket sender = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Udp);

		//	try
		//	{
		//		sender.Connect(dataEndpoint);
		//	}
		//	catch (ArgumentNullException ane)
		//	{
		//		Console.WriteLine("ArgumentNullException : {0}", ane.ToString());
		//	}
		//	catch (SocketException se)
		//	{
		//		Console.WriteLine("SocketException : {0}", se.ToString());
		//	}
		//	catch (Exception e)
		//	{
		//		Console.WriteLine("Unexpected exception : {0}", e.ToString());
		//	}

		//	Console.WriteLine($"Socket connected to {sender.RemoteEndPoint}");
		//}		
	}
}
