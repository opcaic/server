using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using OPCAIC.Application.Emails.Models;
using OPCAIC.Application.Emails.Templates;

namespace OPCAIC.Application.Emails.Queries
{
	public class GetEmailTypesQuery : IRequest<List<EmailTypeDto>>
	{
		public class Handler : IRequestHandler<GetEmailTypesQuery, List<EmailTypeDto>>
		{
			private readonly IMapper mapper;

			public Handler(IMapper mapper)
			{
				this.mapper = mapper;
			}

			/// <inheritdoc />
			public Task<List<EmailTypeDto>> Handle(GetEmailTypesQuery request, CancellationToken cancellationToken)
			{
				return Task.FromResult(mapper.Map<List<EmailTypeDto>>(EmailType.AllValues));
			}
		}
	}
}