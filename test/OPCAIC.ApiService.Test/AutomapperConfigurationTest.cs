using System.Reflection;
using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using OPCAIC.ApiService.IoC;
using OPCAIC.ApiService.Test.Services;
using OPCAIC.Application.Infrastructure.AutoMapper;
using Xunit;

namespace OPCAIC.ApiService.Test
{
	public class AutomapperConfigurationTest
	{
		[Fact]
		public void ConfigurationCorrect()
		{
			TestMapper.ConstructMapper().ConfigurationProvider.AssertConfigurationIsValid();
		}

		public class Base 
		{
			public string A { get; set; }
		}

		public class BaseDestination : ICustomMapping
		{
			public string B { get; set; }

			/// <inheritdoc />
			void ICustomMapping.CreateMapping(Profile configuration)
			{
				configuration.CreateMap<Base, BaseDestination>()
					.ForMember(m => m.B, opt => opt.MapFrom(m => m.A))
					.IncludeAllDerived();
			}
		}

		public class DerivedDestination : BaseDestination, IMapFrom<Base>
		{

		}

		[Fact]
		public void AutoProfile_InheritsMaps()
		{
			var mapper = new Mapper(new MapperConfiguration(cfg =>
			{
				cfg.AddProfile(new AutoMapperProfile(Assembly.GetExecutingAssembly()));
			}));

			mapper.ConfigurationProvider.AssertConfigurationIsValid();

			mapper.Map<DerivedDestination>(new Base());
		}
	}
}