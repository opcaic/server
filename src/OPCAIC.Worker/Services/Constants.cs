namespace OPCAIC.Worker.Services
{
	internal static class Constants
	{
		public static class DirectoryNames
		{
			public static string Source => "src";
			public static string Binary => "bin";
			public static string Extra => "extra";
		}

		public static class FileNames
		{
			private static string ResultFileExt = ".json";
			public static string Metadata = ".metadata";
			public static string CheckerResult = "check-result" + ResultFileExt;
			public static string CompilerResult = "compile-result" + ResultFileExt;
			public static string ValidatorResult = "validate-result" + ResultFileExt;
		}
	}
}