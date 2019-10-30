using System;

namespace GitHubReleaseBot
{
	public static class Program
	{
		private static void Main(string[] args)
		{
			TcpHelper.StartServer(80);
			TcpHelper.Listen();
		}
	}
}