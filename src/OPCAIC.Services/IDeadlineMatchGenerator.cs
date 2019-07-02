using System.Collections.Generic;
using OPCAIC.Infrastructure.Entities;

namespace OPCAIC.Services
{
	public interface IDeadlineMatchGenerator
	{
		(IEnumerable<Match> matches, bool done) Generate(Tournament tournament);
	}
}