using System;
using System.Collections.Generic;
using System.Text;
using OPCAIC.Application.Dtos.Users;

namespace OPCAIC.Application.Dtos.Submissions
{
	public class SubmissionScoreViewDto
	{
		public long Id { get; set; }
		public double Score { get; set; }
		public UserReferenceDto Author { get; set; }
	}
}
