using System.IO.Compression;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using FluentValidation.Validators;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
using OPCAIC.Application.Infrastructure;
using OPCAIC.Application.Infrastructure.Validation;

namespace OPCAIC.ApiService.ModelValidationHandling
{
	public static class ArchiveValidationHelper
	{
		public static void ValidateArchive(IFormFile archive, CustomContext context)
		{
			ValidateArchive(archive, context, int.MaxValue);
		}

		public static void ValidateArchive(IFormFile archive, CustomContext context, long maxSize)
		{
			try
			{
				long size;
				using (var zipFile = new ZipArchive(archive.OpenReadStream(), ZipArchiveMode.Read))
				{
					size = zipFile.Entries.Sum(e => e.Length);
				}

				if (size > maxSize)
				{
					context.AddFailure(nameof(archive), "Zip archive contents are larger than maximum allowed size", ValidationErrorCodes.InvalidZipSize, ("maximumSize", maxSize));
				}
			}
			catch 
			{
				context.AddFailure(nameof(archive), "ZIP archive is corrupted.");
			}
		}
	}
}