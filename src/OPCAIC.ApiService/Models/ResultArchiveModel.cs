using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace OPCAIC.ApiService.Models
{
	public class ResultArchiveModel
	{
		public IFormFile Archive { get; set; }
	}
}