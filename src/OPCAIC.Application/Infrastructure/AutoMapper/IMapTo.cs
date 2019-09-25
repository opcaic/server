using AutoMapper;

namespace OPCAIC.Application.Infrastructure.AutoMapper
{
	/// <summary>
	///     Marker interface that implementing type should directly map to type <see cref="T" />. The properties of type
	///     <see cref="T" /> must be a subset of the given type (the <see cref="MemberList.Source" /> is applied).
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public interface IMapTo<T>
	{
	}
}