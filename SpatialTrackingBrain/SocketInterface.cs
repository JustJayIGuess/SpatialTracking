using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace SpatialTrackingBrain
{
	/// <summary>
	/// A static class for managing and interfacing with the real cameras through a network socket.
	/// </summary>
	static class SocketInterface
	{
		/// <summary>
		/// Represents whether the server is active.
		/// </summary>
		public static bool Active { get; private set; } = false;

		/// <summary>
		/// Buffer containing most recent data sent from cameras.
		/// Key represents the channel, value represents the observed angle from the physical camera on that channel.
		/// </summary>
		private static readonly Dictionary<int, float[]> channelBuffers = new Dictionary<int, float[]>();

		private static readonly Random random = new Random();

		// Maybe these should be read from a config file
		private const int Port = 1338;
		private const int BroadPort = 1339;
		private const string ResponseMessage = "STIPRSPN";
		private const string RequestMessage = "STIPRQST";
		private const string OkMessage = "STDATAOK";
		private const int BufferSize = 64;

		private static Socket dataSock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
		private static Socket discoverySock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
		private static EndPoint dataEP = new IPEndPoint(IPAddress.Any, Port);
		private static EndPoint discoveryEP = new IPEndPoint(IPAddress.Any, BroadPort);
		private static State state = new State();
		private static AsyncCallback recv = null;

		private static HashSet<EndPoint> clients = new HashSet<EndPoint>();

		public class State
		{
			public byte[] buffer = new byte[BufferSize];
		}

		public static void StartServer()
		{
			StartDiscovery();
			StartDataStream();
		}

		private static void StartDiscovery()
		{
			discoverySock.Bind(discoveryEP);
			clients.Clear();
			EndPoint epFrom = new IPEndPoint(IPAddress.Any, 0);

			// Dont like this
			discoverySock.BeginReceiveFrom(state.buffer, 0, BufferSize, SocketFlags.None, ref epFrom, recv = (ar) => {
				State so = (State)ar.AsyncState;
				int numBytes = discoverySock.EndReceiveFrom(ar, ref epFrom);

				if (Encoding.ASCII.GetString(so.buffer, 0, numBytes) == RequestMessage)
				{
					discoverySock.SendTo(Encoding.ASCII.GetBytes(ResponseMessage), epFrom);
					clients.Add(epFrom);
				}

				discoverySock.BeginReceiveFrom(so.buffer, 0, BufferSize, SocketFlags.None, ref epFrom, recv, so);
			}, state);
		}

		private static void StartDataStream()
		{
			dataSock.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.ReuseAddress, true);
			dataSock.Bind(dataEP);
			EndPoint epFrom = new IPEndPoint(IPAddress.Any, 0);

			// Dont like this
			dataSock.BeginReceiveFrom(state.buffer, 0, BufferSize, SocketFlags.None, ref epFrom, recv = (ar) => {
				State so = (State)ar.AsyncState;
				int numBytes = dataSock.EndReceiveFrom(ar, ref epFrom);

				if (clients.Contains(epFrom))
				{
					Console.WriteLine($"RECV: {Encoding.ASCII.GetString(so.buffer, 0, numBytes)}");
					dataSock.SendTo(Encoding.ASCII.GetBytes(OkMessage), epFrom);
				}

				dataSock.BeginReceiveFrom(so.buffer, 0, BufferSize, SocketFlags.None, ref epFrom, recv, so);
			}, state);
		}

		/// <summary>
		/// Calculates a simulated noisy data buffer based on a referenced room, at a specified world point.<br/>
		/// Note: This is for LinearTracking architecture only.
		/// </summary>
		public static void SimulateLinearData(Vector3 point, TrackingRoom room, float noise = 0f)
		{
			// Create a list-form copy of all the LinearTrackingPairs in the room.
			TrackingSet[] trackingSets = room.TrackingSets.ToArray();

			// Calculate observed angles for each camera in each LinearTrackingPair.
			foreach (LinearTrackingSet set in trackingSets)
			{
				float rawAngle1 = MathF.Atan((point.y - set.Cameras[0].WorldPosition.y) / (point.x - set.Cameras[0].WorldPosition.x));
				rawAngle1 *= (int)set.Cameras[0].MeasurementDirection;
				rawAngle1 += set.Cameras[0].ZeroAngle;
				if (rawAngle1 >= MathF.PI)
				{
					rawAngle1 -= MathF.PI;
				}
				channelBuffers[set.Cameras[0].Channel][0] = rawAngle1 + (float)(random.NextDouble() - 0.5f) * 2f * noise;

				float rawAngle2 = MathF.Atan((point.y - set.Cameras[1].WorldPosition.y) / (point.x - set.Cameras[1].WorldPosition.x));
				rawAngle2 *= (int)set.Cameras[1].MeasurementDirection;
				rawAngle2 += set.Cameras[1].ZeroAngle;
				if (rawAngle2 >= MathF.PI)
				{
					rawAngle2 -= MathF.PI;
				}
				channelBuffers[set.Cameras[1].Channel][0] = rawAngle2 + (float)(random.NextDouble() - 0.5f) * 2f * noise;
			}
		}

		/// <summary>
		/// Simulate data for a room using vector tracking.<br/><br/>
		/// Buffer format for vector data is:<br/>
		/// <c>
		/// [ [] ]
		/// </c>
		/// </summary>
		/// <param name="point"></param>
		/// <param name="room"></param>
		/// <param name="noise"></param>
		public static void SimulateVectorData(Vector3 point, TrackingRoom room, float noise = 0f)
		{
			TrackingSet trackingSet = room.TrackingSets[0];

			// Calculate observed angles for each camera in each LinearTrackingPair.
			foreach (TrackingCamera camera in trackingSet.Cameras)
			{
				Vector3 dir = (camera.WorldPosition - point).Normalized();

				channelBuffers[camera.Channel] = new float[] {
					MathF.Atan2(dir.y, dir.x) + (float)(random.NextDouble() - 0.5f) * 2f * noise, 
					MathF.Acos(dir.z) + (float)(random.NextDouble() - 0.5f) * 2f * noise
				};
			}
		}

		/// <summary>
		/// Adds the channel required by the cameras in <c>pair</c> if they are not already present.
		/// </summary>
		/// <param name="set">The tracking pair to be added.</param>
		/// <param name="initialValue">Optional parameter for the initial value of the buffer on this channel.</param>
		public static void AddChannels(TrackingSet set, float initialValue = -1f)
		{
			foreach (TrackingCamera camera in set.Cameras)
			{
				if (!channelBuffers.ContainsKey(camera.Channel))
				{
					channelBuffers.Add(camera.Channel, new float[] { initialValue });
				}
			}
		}

		/// <summary>
		/// Removes the channel <c>channel</c>.
		/// </summary>
		/// <param name="channel"></param>
		public static void RemoveChannel(int channel)
		{
			channelBuffers.Remove(channel);
		}

		/// <summary>
		/// Returns data from the buffer at channel.
		/// </summary>
		/// <param name="channel">The channel that data will be fetched from on the socket.</param>
		/// <returns></returns>
		public static float[] GetData(int channel)
		{
			if (channelBuffers.ContainsKey(channel))
			{
				return channelBuffers[channel];
			}
			else
			{
				return null;
			}
		}

		/// <summary>
		/// Displays the data buffer.
		/// </summary>
		public static void PrintBuffer()
		{
			Console.Write("[ ");
			foreach (KeyValuePair<int, float[]> kvp in channelBuffers)
			{
				Console.Write(kvp.Key + ": [" + string.Join(", ", kvp.Value) + "], ");
			}
			Console.WriteLine("]");
		}

	}
}
