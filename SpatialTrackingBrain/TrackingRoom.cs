using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace SpatialTrackingBrain
{
	abstract class TrackingRoom
	{
		// Maybe these should be read from a config file
		private readonly static int port = 1338;
		private readonly static int broadPort = 1339;
		private readonly static string responseMessage = "STIPRSPN";
		private readonly static string requestMessage = "STIPRQST";
		private readonly static string okMessage = "STDATAOK";
		private readonly static int bufferSize = 16;

		private Socket dataSock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
		private Socket discoverySock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
		private EndPoint dataEP = new IPEndPoint(IPAddress.Any, port);
		private EndPoint discoveryEP = new IPEndPoint(IPAddress.Any, broadPort);
		private State state = new State();
		private AsyncCallback recv = null;

		public List<TrackingSet> TrackingSets { get; protected set; }
		protected Dictionary<TrackingCamera, int> cameraUsageCount;

		public class State
		{
			public byte[] buffer = new byte[bufferSize];
		}

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

		public void StartServer()
		{
			dataSock.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.ReuseAddress, true);
			dataSock.Bind(dataEP);
			discoverySock.Bind(discoveryEP);

			EndPoint epFrom = new IPEndPoint(IPAddress.Any, 0);

			// Dont like this
			dataSock.BeginReceiveFrom(state.buffer, 0, bufferSize, SocketFlags.None, ref epFrom, recv = (ar) => {
				State so = (State)ar.AsyncState;
				int bytes = dataSock.EndReceiveFrom(ar, ref epFrom);
				dataSock.BeginReceiveFrom(so.buffer, 0, bufferSize, SocketFlags.None, ref epFrom, recv, so);
				Console.WriteLine("RECV: {0}: {1}, {2}", epFrom.ToString(), bytes, Encoding.ASCII.GetString(so.buffer, 0, bytes));
			}, state);
		}
	}
}
