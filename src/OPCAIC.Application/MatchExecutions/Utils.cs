using System.Text.RegularExpressions;

namespace OPCAIC.Application.MatchExecutions
{
	public class Utils
	{
		// forbid logs and match-results files from being listed and downloaded directly
		private static readonly Regex maskedFileRegex =
			new Regex(@"^(((compile\.\d+)|(execute))\.(stderr|stdout))|match-results\.json$", RegexOptions.Compiled);

		public static bool IsMaskedFile(string filename)
		{
			return maskedFileRegex.IsMatch(filename);
		}
	}
}