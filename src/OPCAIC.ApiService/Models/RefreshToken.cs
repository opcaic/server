using System.ComponentModel.DataAnnotations;

namespace OPCAIC.ApiService.Models
{
	public class RefreshToken
	{
		/// <summary>
		///     current refresh token of user
		/// </summary>
		public string Token { get; set; }
	}
}