using System;

namespace OPCAIC.Messaging.Test
{
	public static class TestConnectionStringFactory
	{
		static Random rand = new Random(); // no seed! we want multiple processes to get different ports
		public static string GetConnectionString()
		{
			// not perfect, but will work in most cases
			return $"tcp://localhost:{rand.Next(5000, 50000)}";
		}
	}
}