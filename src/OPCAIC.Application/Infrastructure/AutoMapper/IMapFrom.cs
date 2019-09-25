using AutoMapper;

namespace OPCAIC.Application.Infrastructure.AutoMapper
{
	/// <summary>
	///     Marker interface that the type <see cref="T" /> should directly map to the implementing type. The properties of the
	///     implementing type must be a subset of those of type <see cref="T" /> (the <see cref="MemberList.Destination" /> is
	///     applied).
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public interface IMapFrom<T>
	{
	}
}