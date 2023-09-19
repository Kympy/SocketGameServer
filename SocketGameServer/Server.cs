using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SocketGameServer
{
	internal class Server : Singleton<Server>
	{
		private const int MaxBackLog = 2;
		private const int PortNumber = 8000;

		private Socket? socket = null;
		private List<Socket> listenList = new List<Socket>();
		private Dictionary<Socket, List<byte>> connectedClients = new Dictionary<Socket, List<byte>>();
		private List<Socket> cloneClientConnections = new List<Socket>();	

		public int ConnectedClientCount
		{
			get
			{
				if (connectedClients == null) return 0;
				return connectedClients.Keys.Count;
			}
		}

		~Server()
		{
			CloseSocket();
		}
		public void CreateSocket()
		{
			if (socket != null)
			{
				Console.WriteLine("Socket is already exist. Close old one and create new.");
				CloseSocket();
			}
			socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			IPEndPoint localIPEndPoint = new IPEndPoint(IPAddress.Any, PortNumber);
			Console.WriteLine("Binding...");
			socket.Bind(localIPEndPoint);
			Console.WriteLine($"Listen back log...{MaxBackLog}");
			socket.Listen(MaxBackLog);
			Console.WriteLine("Socket created.");
		}
		public void CloseSocket()
		{
			if (socket == null)
			{
				Console.WriteLine("Close socket is requested. But socket is null.");
				return;
			}
			socket.Close();
			socket = null;

			int count = 0;
			foreach(Socket client in connectedClients.Keys)
			{
				Console.WriteLine($"Close all client sockets...{count}");
				client.Close();
				count++;
			}
			connectedClients.Clear();
			Console.WriteLine($"Socket is closed. Closed client count : {count}");
		}
		public void Update()
		{
			if (socket == null) return;

			listenList.Clear();
			listenList.Add(socket);

			Socket.Select(listenList, null, null, 1000);

			foreach (Socket listen in listenList)
			{
				Socket newClient = listen.Accept();
				connectedClients.Add(newClient, new List<byte>());
				Console.WriteLine("Connected new client");
			}

			if (connectedClients.Keys.Count == 0) return;

			cloneClientConnections.Clear();
			foreach (var client in connectedClients.Keys)
			{
				if (client.Poll(1, SelectMode.SelectRead) == true && client.Available == 0)
				{
					client.Close();
					connectedClients.Remove(client);
					Console.WriteLine("Remove disconnected client : Clone()");
					continue;
				}
				cloneClientConnections.Add(client);
			}
			Console.WriteLine($"Connected : {connectedClients.Keys.Count}");
			
			if (cloneClientConnections == null || cloneClientConnections.Count == 0) return;

			Socket.Select(cloneClientConnections, null, null, 1000);
			foreach (var client in cloneClientConnections)
			{
				if (client.Connected == false)
				{
					client.Close();
					connectedClients.Remove(client);
					Console.Write("Remove disconnected client : Receive()");
					continue;
				}

				byte[] receivedBytes = new byte[512];
				List<byte> buffer = connectedClients[client];

				int read = client.Receive(receivedBytes);
				for (int i = 0; i < read; i++)
				{
					buffer.Add(receivedBytes[i]);
				}

				while (buffer.Count > 0)
				{
					int packetDataLength = buffer[0];

					if (buffer.Count > packetDataLength)
					{
						List<byte> packetBytes = new List<byte>(buffer);
						packetBytes.RemoveRange(packetDataLength, packetBytes.Count - (packetDataLength + 1));
						packetBytes.RemoveRange(0, 1);
						buffer.RemoveRange(0, packetDataLength + 1);
						byte[] readBytes = packetBytes.ToArray();

						Bullet bullet = Bullet.FromByteArray(readBytes);
						Console.WriteLine($"{bullet.GetType()}/{bullet.name} : {bullet.position}");
						
						int count = 0;
						foreach(Socket otherClient in connectedClients.Keys)
						{
							if (otherClient == client) continue;
							otherClient.Send(receivedBytes);
							Console.WriteLine($"Send to other client...{count}");
							count++;
						}
					}
				}
			}
		}
	}
}
