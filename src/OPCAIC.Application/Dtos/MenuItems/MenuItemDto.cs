using System.Collections.Generic;
using AutoMapper;
using OPCAIC.Application.Infrastructure.AutoMapper;
using OPCAIC.Domain.Infrastructure;
using OPCAIC.Domain.ValueObjects;

namespace OPCAIC.Application.Tournaments.Models
{
	public class MenuItemDto : ValueObject, ICustomMapping
	{
		// nullable to allow for better error messages
		public MenuItemType? Type { get; set; }

		/// <inheritdoc />
		public void CreateMapping(Profile configuration)
		{
			// manual configuration co capture inheritance
			configuration.CreateMap<MenuItem, MenuItemDto>(MemberList.Destination)
				.Include<DocumentLinkMenuItem, DocumentLinkMenuItemDto>()
				.Include<ExternalUrlMenuItem, ExternalUrlMenuItemDto>()
				.ReverseMap()
				.Include<DocumentLinkMenuItemDto, DocumentLinkMenuItem>()
				.Include<ExternalUrlMenuItemDto, ExternalUrlMenuItem>();

			configuration.CreateMap<DocumentLinkMenuItem, DocumentLinkMenuItemDto>().ReverseMap();
			configuration.CreateMap<ExternalUrlMenuItem, ExternalUrlMenuItemDto>().ReverseMap();
		}

		/// <inheritdoc />
		protected override IEnumerable<object> GetAtomicValues()
		{
			// this method is not supposed to be called
			throw new System.NotImplementedException();
		}
	}
}