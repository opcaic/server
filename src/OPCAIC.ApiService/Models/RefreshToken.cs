using System.ComponentModel.DataAnnotations;

namespace OPCAIC.ApiService.Models
{
	public class RefreshToken
	{
		/// <summary>
		/// current refresh token of user
		/// </summary>
		[Required]
		[MinLength(1)]
		public string Token { get; set; }
	}
}