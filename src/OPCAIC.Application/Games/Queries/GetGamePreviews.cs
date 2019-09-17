using MediatR;
using OPCAIC.ApiService.Models.Games;
using OPCAIC.Application.Specifications;

namespace OPCAIC.Application.Games.Queries
{
	public class GetGamePreviews : IRequest<PagedResult<GamePreviewModel>>
	{
		
	}
}