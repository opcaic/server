using System;
using OPCAIC.ApiService.Test;

namespace OPCAIC.Application.Test
{
	public static class EntityFakerExtensions
	{
		public static TDto Dto<TEntity, TDto>(this EntityFaker faker, Action<TDto> additionalSetup)
			where TEntity : class
		{
			var dto = TestMapper.Mapper.Map<TDto>(faker.Configure<TEntity>().Generate());
			additionalSetup(dto);
			return dto;
		}

		public static TDto Dto<TEntity, TDto>(this EntityFaker faker) where TEntity : class
		{
			return faker.Dto<TEntity, TDto>(d => { });
		}
	}
}