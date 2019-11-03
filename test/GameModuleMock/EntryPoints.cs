using System;
using System.IO;
using System.Linq;
using System.Threading;
using Bogus;

namespace GameModuleMock
{
	public static class EntryPoints
	{
		private static Random rand = new Random();
		private static Faker faker = new Faker();

		public const string TestStdoutLog = @"This is a test stdout
log, which should be printed by the external game
module.
It is multiline on purpose.";

		public const string TestStdErrLog = @"This is a test stderr
log, which is a bit shorter 
than the stdout one.";

		public static int WaitIndefinitely()
		{
			Console.WriteLine("Entering eternal loop.");

			while (true)
			{
			}
		}

		public static int SleepFor(int ms)
		{
			Console.WriteLine($"Sleeping for {ms} ms");
			Thread.Sleep(ms);
			return 0;
		}

		public static int SleepFor(int ms, string inDir, string a, string b)
		{
			return SleepFor(ms);
		}

		public static int RandomResult(int probability, string inDir, string a)
		{
			Console.WriteLine($"Success probability: {probability}");

			Console.WriteLine(faker.Lorem.Paragraph());

			Console.WriteLine("Tossing a die for return value");
			if (rand.Next(100) < probability)
			{
				return 0;
			}

			if (rand.Next(10) < 9)
			{
				return 200; // negative result exit code
			}

			return 1; // general error
		}

		public static int RandomResult(int probability, string inDir, string a, string b)
		{
			return RandomResult(probability, inDir, a);
		}

		public static int RandomResult(int probability, string inDir, string a, string b, string c)
		{
			return RandomResult(probability, inDir, a, b);
		}

		public static int ExitWithCode(int code)
		{
			return code;
		}

		public static int ExitWithCode(int code, string inDir, string a)
		{
			return code;
		}

		public static int ExitWithCode(int code, string inDir, string a, string b)
		{
			return code;
		}

		public static int ExitWithCode(int code, string inDir, string a, string b, string c)
		{
			return code;
		}

		public static int EchoArgs(string inDir, string stdout, string stderr)
		{
			Console.WriteLine(stdout);
			Console.Error.WriteLine(stderr);
			return 0;
		}

		public static int SingleplayerExecute(int resultCount, string inDir, string binDir,
			string outDir)
		{
			Console.WriteLine($"Executing in single-player with {resultCount} players");
			var contents = @"{ 
	""results"": [" +
				string.Join(',', Enumerable.Repeat(@"
		{
			""Score"" : 1
		}", resultCount)) +
				@"
	]
}";
			Console.WriteLine(contents);
			File.WriteAllText(Path.Combine(outDir, "match-results.json"), contents);
			return 0;
		}

		public static int TwoPlayerExecute(string inputDir, string bin1, string bin2,
			string outDir)
		{
			Console.WriteLine($"Executing in two player game");
			Console.WriteLine("Input:");
			Console.WriteLine(inputDir);
			Console.WriteLine($"Players:");
			Console.WriteLine(bin1);
			Console.WriteLine(bin2);
			Console.WriteLine("Output:");
			Console.WriteLine(outDir);

			Console.WriteLine("Flipping a fair coin for winner");
			int flip = rand.Next(2);

			Console.WriteLine($"Coin flipped {flip}");

			var contents = $@"{{ 
	'results': [
		{{
			'Score' : {flip}
		}},
		{{
			'Score' : {1 - flip}
		}}
	]
}}";
			Console.WriteLine(contents);
			File.WriteAllText(Path.Combine(outDir, "match-result.json"), contents);
			return 0;
		}
	}
}