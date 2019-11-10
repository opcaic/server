using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace OPCAIC.Application.Infrastructure
{
	public class FileDto
	{
		public static IEnumerable<FileDto> GetFilesInArchive(Stream archive)
		{
			var files = new List<FileDto>();

			if (archive == null)
			{
				return files;
			}

			using var zip = new ZipArchive(archive, ZipArchiveMode.Read);

			foreach (var e in zip.Entries)
			{
				files.Add( new FileDto { Filename = e.Name, Length = e.Length });
			}

			return files;
		}

		/// <summary>
		///     Name of the file.
		/// </summary>
		public string Filename { get; set; }

		/// <summary>
		///     Size of the file in bytes.
		/// </summary>
		public long Length { get; set; }
	}
}