namespace OPCAIC.Worker
{
	internal static class Constants
	{
		public const int GameModuleNegativeExitCode = 200;

		public static class DirectoryNames
		{
			public static string Source => "src";
			public static string Binary => "bin";
			public static string Input => "in";
			public static string Output => "out";
		}

		public static class FileNames
		{
			public static string MatchResults => "match-results.json";
			public static string EntryPointsConfig => "entrypoints.json";
			public static string GameModuleConfig => "config.json";
			public static string CheckerPrefix => "check";
			public static string CompilerPrefix => "compile";
			public static string ValidatorPrefix => "validate";
			public static string ExecutorPrefix => "execute";
			public static string StdoutLogSuffix => ".stdout";
			public static string StderrLogSuffix => ".stderr";
		}
	}
}