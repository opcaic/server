using System;
using static OPCAIC.ApiService.Test.Services.TestMapper;

namespace OPCAIC.ApiService.Test
{
	public static class EntityFakerExtensions
	{
		public static TDto Dto<TEntity, TDto>(this EntityFaker faker, Action<TDto> additionalSetup)
			where TEntity : class
		{
			var dto = Mapper.Map<TDto>(faker.Configure<TEntity>().Generate());
			additionalSetup(dto);
			return dto;
		}

		public static TDto Dto<TEntity, TDto>(this EntityFaker faker) where TEntity : class
		{
			return faker.Dto<TEntity, TDto>(d => { });
		}
	}
}