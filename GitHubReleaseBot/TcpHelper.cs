using System;
using System.Net;
using System.Text;
using System.Net.Sockets;

namespace GitHubReleaseBot
{
	internal class TcpHelper
	{
		private static TcpListener listener { get; set; }
		private static bool accept { get; set; }

		public static void StartServer(int port)
		{
			var address = IPAddress.Parse("127.0.0.1");
			listener = new TcpListener(address, port);

			listener.Start();
			accept = true;

			Console.WriteLine($"Server started. Listening to TCP clients at 127.0.0.1:{port}");
		}

		public static void Listen()
		{
			if (listener == null || !accept)
				return;

			// Continue listening.
			while (true)
			{
				Console.WriteLine("Waiting for client...");
				var clientTask = listener.AcceptTcpClientAsync();

				if (clientTask.Result == null)
					continue;

				Console.WriteLine("Client connected. Waiting for data.");
				var client = clientTask.Result;
				var message = "";

				while (message != null && !message.StartsWith("quit"))
				{
					var data =
						Encoding.ASCII.GetBytes(
							"Send next data: [enter 'quit' to terminate] ");
					client.GetStream().Write(data, 0, data.Length);

					var buffer = new byte[1024];
					client.GetStream().Read(buffer, 0, buffer.Length);

					message = Encoding.ASCII.GetString(buffer);
					Console.WriteLine(message);
				}

				Console.WriteLine("Closing connection.");
				client.GetStream().Dispose();
			}
		}
	}
}
