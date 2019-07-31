using System;
using System.IO;
using System.Linq;
using System.Threading;

namespace GameModuleMock
{
	public static class EntryPoints
	{
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

		public static int SingleplayerExecute(int resultCount, string inDir, string binDir, string outDir)
		{
			Console.WriteLine($"Executing in single-player with {resultCount} players");
			var contents = @"{ 
	""results"": [" + string.Join(',',Enumerable.Repeat(@"
		{
			""Score"" : 1
		}", resultCount)) + @"
	]
}";
			Console.WriteLine(contents);
			File.WriteAllText(Path.Combine(outDir, "match-result.json"), contents);
			return 0;
		}
	}
}