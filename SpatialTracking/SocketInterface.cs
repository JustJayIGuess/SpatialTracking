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
		private static readonly Dictionary<int, float> buffer = new Dictionary<int, float>();

		private static readonly Random random = new Random();

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
				buffer[set.Cameras[0].Channel] = rawAngle1 + (float)(random.NextDouble() - 0.5f) * 2f * noise;

				float rawAngle2 = MathF.Atan((point.y - set.Cameras[1].WorldPosition.y) / (point.x - set.Cameras[1].WorldPosition.x));
				rawAngle2 *= (int)set.Cameras[1].MeasurementDirection;
				rawAngle2 += set.Cameras[1].ZeroAngle;
				if (rawAngle2 >= MathF.PI)
				{
					rawAngle2 -= MathF.PI;
				}
				buffer[set.Cameras[1].Channel] = rawAngle2 + (float)(random.NextDouble() - 0.5f) * 2f * noise;
			}
		}

		/// <summary>
		/// Adds the channel required by the cameras in <c>pair</c> if they are not already present.
		/// </summary>
		/// <param name="set">The tracking pair to be added.</param>
		/// <param name="initialValue">Optional parameter for the initial value of the buffer on this channel.</param>
		public static void AddChannels(TrackingSet set, float initialValue = -1f)
		{
			if (!buffer.ContainsKey(set.Cameras[0].Channel))
			{
				buffer.Add(set.Cameras[0].Channel, initialValue);
			}
			if (!buffer.ContainsKey(set.Cameras[1].Channel))
			{
				buffer.Add(set.Cameras[1].Channel, initialValue);
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
			Console.WriteLine("{" + string.Join(", ", buffer.Select(set => string.Format("{0}: {1}", set.Key, set.Value))) + "}");
		}

	}
}
