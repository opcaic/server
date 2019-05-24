using System;

namespace OPCAIC.Messaging.Test
{
	public static class TestConnectionStringFactory
	{
		private static readonly Random
			rand = new Random(); // no seed! we want multiple processes to get different ports

		public static string GetConnectionString() => $"tcp://localhost:{rand.Next(5000, 50000)}";
	}
}
