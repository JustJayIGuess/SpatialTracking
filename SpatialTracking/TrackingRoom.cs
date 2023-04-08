﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace SpatialTracking
{
	class TrackingRoom
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

			if (cameraUsageCount.ContainsKey(trackingSet.Cameras[0]))
			{
				cameraUsageCount[trackingSet.Cameras[0]] += 1;
			}
			else
			{
				cameraUsageCount.Add(trackingSet.Cameras[0], 1);
			}

			if (cameraUsageCount.ContainsKey(trackingSet.Cameras[1]))
			{
				cameraUsageCount[trackingSet.Cameras[1]] += 1;
			}
			else
			{
				cameraUsageCount.Add(trackingSet.Cameras[1], 1);
			}
		}
		public void RemoveTrackingSet(TrackingSet trackingSet)
		{
			TrackingSets.Remove(trackingSet);
			if (cameraUsageCount[trackingSet.Cameras[0]] == 1)
			{
				cameraUsageCount.Remove(trackingSet.Cameras[0]);
				SocketInterface.RemoveChannel(trackingSet.Cameras[0].Channel);
			}
			else
			{
				cameraUsageCount[trackingSet.Cameras[0]] -= 1;
			}

			if (cameraUsageCount[trackingSet.Cameras[1]] == 1)
			{
				cameraUsageCount.Remove(trackingSet.Cameras[1]);
				SocketInterface.RemoveChannel(trackingSet.Cameras[1].Channel);
			}
			else
			{
				cameraUsageCount[trackingSet.Cameras[1]] -= 1;
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

		public void StartServer()
		{
			IPHostEntry ipHost = Dns.GetHostEntry(Dns.GetHostName());
			IPAddress ipAddr = ipHost.AddressList[0];
			IPEndPoint localEndPoint = new IPEndPoint(ipAddr, 11111);

			Console.WriteLine(ipAddr.ToString());

			Socket sender = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

			try
			{
				sender.Connect(localEndPoint);
			}
			catch (ArgumentNullException ane)
			{
				Console.WriteLine("ArgumentNullException : {0}", ane.ToString());
			}
			catch (SocketException se)
			{
				Console.WriteLine("SocketException : {0}", se.ToString());
			}
			catch (Exception e)
			{
				Console.WriteLine("Unexpected exception : {0}", e.ToString());
			}

			Console.WriteLine($"Socket connected to {sender.RemoteEndPoint}");
		}

		public void ExecuteServer()
		{
			IPHostEntry ipHost = Dns.GetHostEntry(Dns.GetHostName());
			IPAddress ipAddr = ipHost.AddressList[0];
			IPEndPoint localEndPoint = new IPEndPoint(ipAddr, 11111);

			Console.WriteLine(ipAddr.ToString());

			Socket listener = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

			try
			{
				listener.Bind(localEndPoint);

				listener.Listen(10);

				while (true)
				{
					Console.WriteLine("Waiting connection ... ");

					Socket clientSocket = listener.Accept();

					byte[] bytes = new byte[1024];
					string data = null;

					while (true)
					{
						int numByte = clientSocket.Receive(bytes);

						data += Encoding.ASCII.GetString(bytes,
												   0, numByte);

						if (data.IndexOf("<EOF>") > -1)
						{
							break;
						}
					}

					Console.WriteLine("Text received -> {0} ", data);
					byte[] message = Encoding.ASCII.GetBytes("Test Server");

					clientSocket.Send(message);

					clientSocket.Shutdown(SocketShutdown.Both);
					clientSocket.Close();
				}
			}

			catch (Exception e)
			{
				Console.WriteLine(e.ToString());
			}
		}
	}
}