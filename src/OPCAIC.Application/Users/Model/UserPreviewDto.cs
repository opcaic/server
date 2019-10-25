using System;
using AutoMapper;
using OPCAIC.Application.Infrastructure.AutoMapper;
using OPCAIC.Domain.Entities;
using OPCAIC.Domain.Enums;

namespace OPCAIC.Application.Users.Model
{
	public class UserPreviewDto : ICustomMapping
	{
		public long Id { get; set; }

		public string Email { get; set; }

		public string Username { get; set; }

		public bool EmailVerified { get; set; }

		public UserRole Role { get; set; }

		public DateTime Created { get; set; }

		/// <inheritdoc />
		void ICustomMapping.CreateMapping(Profile configuration)
		{
			configuration.CreateMap<User, UserPreviewDto>(MemberList.Destination)
				.ForMember(u => u.EmailVerified,
					opt => opt.MapFrom(u => u.EmailConfirmed))
				.IncludeAllDerived();
		}
	}
}