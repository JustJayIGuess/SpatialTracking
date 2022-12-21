using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace SpatialTracking
{
	class LinearTrackingRoom
	{
		public List<LinearTrackingPair> trackingPairs { get; private set; } = new List<LinearTrackingPair>();

		private Dictionary<LinearTrackingCameraInfo, int> cameras = new Dictionary<LinearTrackingCameraInfo, int>();

		public LinearTrackingRoom()
		{
		}

		public void Add(LinearTrackingPair trackingPair)
		{
			if (trackingPairs.Contains(trackingPair))
			{
				return;
			}
			trackingPairs.Add(trackingPair);
			SocketInterface.AddChannels(trackingPair);

			if (cameras.ContainsKey(trackingPair.camera1))
			{
				cameras[trackingPair.camera1] += 1;
			}
			else
			{
				cameras.Add(trackingPair.camera1, 1);
			}

			if (cameras.ContainsKey(trackingPair.camera2))
			{
				cameras[trackingPair.camera2] += 1;
			}
			else
			{
				cameras.Add(trackingPair.camera2, 1);
			}
		}

		public void Remove(LinearTrackingPair trackingPair)
		{
			trackingPairs.Remove(trackingPair);
			if (cameras[trackingPair.camera1] == 1)
			{
				cameras.Remove(trackingPair.camera1);
				SocketInterface.RemoveChannel(trackingPair.camera1.channel);
			}
			else
			{
				cameras[trackingPair.camera1] -= 1;
			}

			if (cameras[trackingPair.camera2] == 1)
			{
				cameras.Remove(trackingPair.camera2);
				SocketInterface.RemoveChannel(trackingPair.camera2.channel);
			}
			else
			{
				cameras[trackingPair.camera2] -= 1;
			}
		}

		public void Update(bool verbose = false)
		{
			foreach (KeyValuePair<LinearTrackingCameraInfo, int> entry in cameras)
			{
				entry.Key.Update();
			}

			foreach (LinearTrackingPair trackingPair in trackingPairs)
			{
				trackingPair.Update(verbose);
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
                            break;
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
