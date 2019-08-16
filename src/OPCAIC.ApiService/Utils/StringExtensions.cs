namespace OPCAIC.ApiService.Utils
{
	public static class StringExtensions
	{
		public static string FirstLetterToLower(this string text)
		{
			return string.IsNullOrEmpty(text)
				? text
				: char.ToLowerInvariant(text[0]) + text.Substring(1);
		}
	}
}