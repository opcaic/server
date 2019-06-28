using System.Collections.Generic;

namespace OPCAIC.Infrastructure.Entities
{
	public class Tournament : Entity
	{
		public string Name { get; set; }

		public string Description { get; set; }

		public virtual IList<Submission> Submissions { get; set; }
	}
}
