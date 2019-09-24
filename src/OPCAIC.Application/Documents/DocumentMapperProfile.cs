using AutoMapper;
using OPCAIC.Application.Dtos.Documents;
using OPCAIC.Domain.Entities;

namespace OPCAIC.Application.Documents
{
	public class DocumentMapperProfile : Profile
	{
		public DocumentMapperProfile()
		{
			CreateMap<Document, DocumentDto>(MemberList.Destination);
		}
	}
}