using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OPCAIC.ApiService.Models.Users
{
	public class UserPreviewModel
	{
		public string Email { get; set; }

		public string UserName { get; set; }

		public bool EmailVerified { get; set; }

		public long UserRole { get; set; }

		public DateTime Created { get; set; }
	}
}
