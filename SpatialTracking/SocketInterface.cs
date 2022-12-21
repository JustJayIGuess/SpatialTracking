using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace SpatialTracking
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
		private static Dictionary<int, float> buffer = new Dictionary<int, float>();

		private static Random random = new Random();

		/// <summary>
		/// Note: This is completely untested as of yet. Still working on Client-side socket stuff.
		/// </summary>
		/// <returns></returns>
		public static async System.Threading.Tasks.Task InitServer()
		{
			if (Active)
			{
				return;
			}

			IPHostEntry ipHostInfo = await Dns.GetHostEntryAsync("localhost");
			IPAddress ipAddress = ipHostInfo.AddressList[0];
			IPEndPoint endpoint = new IPEndPoint(ipAddress, 1337);

			using Socket listener = new Socket(endpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

			listener.Bind(endpoint);
			listener.Listen(100);

			Socket handler = await listener.AcceptAsync();

			while (true)
			{
				byte[] buffer = new byte[1024];
				int received = await handler.ReceiveAsync(buffer, SocketFlags.None);
				string response = Encoding.UTF8.GetString(buffer, 0, received);

				string eom = "<|EOM|>";
				if (response.IndexOf(eom) > -1)
				{
					Console.WriteLine($"Socket server received message: \"{response.Replace(eom, "")}\"");

					string ackMessage = "<|ACK|>";
					byte[] echoBytes = Encoding.UTF8.GetBytes(ackMessage);
					await handler.SendAsync(echoBytes, 0);
					Console.WriteLine($"Socket server sent acknowledgment: \"{ackMessage}\"");

					break;
				}
			}
		}

		/// <summary>
		/// Calculates a simulated noisy data buffer based on a referenced room, at a specified world point.
		/// </summary>
		public static void SimulateData(Vector3 point, LinearTrackingRoom room, float noise = 0f)
		{
			// Create a list-form copy of all the LinearTrackingPairs in the room.
			LinearTrackingPair[] trackingPairs = room.trackingPairs.ToArray();

			// Calculate observed angles for each camera in each LinearTrackingPair.
			foreach (LinearTrackingPair pair in trackingPairs)
			{
				float rawAngle1 = MathF.Atan((point.y - pair.camera1.worldPosition.y) / (point.x - pair.camera1.worldPosition.x));
				rawAngle1 *= (int)pair.camera1.measurementDirection;
				rawAngle1 += pair.camera1.zeroAngle;
				if (rawAngle1 >= MathF.PI)
				{
					rawAngle1 -= MathF.PI;
				}
				buffer[pair.camera1.channel] = rawAngle1 + (float)(random.NextDouble() - 0.5f) * 2f * noise;

				float rawAngle2 = MathF.Atan((point.y - pair.camera2.worldPosition.y) / (point.x - pair.camera2.worldPosition.x));
				rawAngle2 *= (int)pair.camera2.measurementDirection;
				rawAngle2 += pair.camera2.zeroAngle;
				if (rawAngle2 >= MathF.PI)
				{
					rawAngle2 -= MathF.PI;
				}
				buffer[pair.camera2.channel] = rawAngle2 + (float)(random.NextDouble() - 0.5f) * 2f * noise;
			}
		}

		/// <summary>
		/// Adds the channel required by the cameras in <c>pair</c> if they are not already present.
		/// </summary>
		/// <param name="pair">The tracking pair to be added.</param>
		/// <param name="initialValue">Optional parameter for the initial value of the buffer on this channel.</param>
		public static void AddChannels(LinearTrackingPair pair, float initialValue = -1f)
		{
			if (!buffer.ContainsKey(pair.camera1.channel))
			{
				buffer.Add(pair.camera1.channel, initialValue);
			}
			if (!buffer.ContainsKey(pair.camera2.channel))
			{
				buffer.Add(pair.camera2.channel, initialValue);
			}
		}

		/// <summary>
		/// Removes the channel <c>channel</c>.
		/// </summary>
		/// <param name="channel"></param>
		public static void RemoveChannel(int channel)
		{
			buffer.Remove(channel);
		}

		/// <summary>
		/// Returns data from the buffer at channel.
		/// </summary>
		/// <param name="channel">The channel that data will be fetched from on the socket.</param>
		/// <returns></returns>
		public static float GetData(int channel)
		{
			if (buffer.ContainsKey(channel))
			{
				return buffer[channel];
			}
			else
			{
				return -1;
			}
		}

		/// <summary>
		/// Displays the data buffer.
		/// </summary>
		public static void PrintBuffer()
		{
			Console.WriteLine("{" + string.Join(", ", buffer.Select(pair => string.Format("{0}: {1}", pair.Key, pair.Value))) + "}");
		}

	}
}
