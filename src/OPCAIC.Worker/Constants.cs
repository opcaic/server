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
			public static string Extra => "extra";
		}

		public static class FileNames
		{
			private static string ResultFileExt => ".json";
			public static string Metadata => ".metadata";
			public static string CheckerResult => "check-result" + ResultFileExt;
			public static string CompilerResult => "compile-result" + ResultFileExt;
			public static string ValidatorResult => "validate-result" + ResultFileExt;
			public static string ExecutorResult => "match-result" + ResultFileExt;
			public static string ModuleConfig => "config.json";
			public static string Log4NetConfig => "log4net.config";
			public static string CheckerPrefix => "check";
			public static string CompilerPrefix => "compile";
			public static string ValidatorPrefix => "validate";
			public static string ExecutorPrefix => "execute";
			public static string StdoutLogSuffix => ".stdout";
			public static string StderrLogSuffix => ".stderr";
		}
	}
}