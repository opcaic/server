using FluentValidation;
using OPCAIC.Application.Dtos;

namespace OPCAIC.Application.Infrastructure.Validation
{
	public abstract class FilterValidator<T> : AbstractValidator<T>
		where T : FilterDtoBase
	{
		protected FilterValidator()
		{
			RuleFor(m => m.Offset).MinValue(0);
			RuleFor(m => m.Count).InRange(1, 100);
			RuleFor(m => m.SortBy).MinLength(1);
		}
	}
}