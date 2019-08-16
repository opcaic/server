using System;
using System.IO;

namespace OPCAIC.TestUtils
{
	/// <summary>
	///     Manages location and lifetime of working directories for unit tests.
	/// </summary>
	public class TestDirectoryManager : IDisposable
	{
		private readonly DirectoryInfo RootDirectory;

		public TestDirectoryManager()
		{
			RootDirectory =
				Directory.CreateDirectory(Path.Combine(Path.GetTempPath(),
					Guid.NewGuid().ToString()));
		}

		/// <inheritdoc />
		public void Dispose()
		{
			// mass cleanup
			RootDirectory.Delete(true);
		}

		public DirectoryInfo GetNewDirectory()
		{
			return RootDirectory.CreateSubdirectory(Guid.NewGuid().ToString());
		}
	}
}