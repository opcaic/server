using System.Threading;
using System.Threading.Tasks;
using OPCAIC.ApiService.Models.Documents;

namespace OPCAIC.ApiService.Interfaces
{
	public interface IDocumentService
	{
		Task<long> CreateAsync(NewDocumentModel document, CancellationToken cancellationToken);

		Task UpdateAsync(long id, UpdateDocumentModel model, CancellationToken cancellationToken);
		Task DeleteAsync(long id, CancellationToken cancellationToken);
	}
}