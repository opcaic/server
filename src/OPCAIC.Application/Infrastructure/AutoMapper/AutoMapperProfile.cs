using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AutoMapper;

namespace OPCAIC.Application.Infrastructure.AutoMapper
{
	/// <summary>
	///     Dynamically generated AutoMapper profile from interfaces <see cref="IMapFrom{T}" />, <see cref="IMapTo{T}" /> and
	///     <see cref="ICustomMapping" />
	/// </summary>
	public class AutoMapperProfile : Profile
	{
		/// <summary>
		///     List of all mapped types
		/// </summary>
		private HashSet<Type> mappedTypes = new HashSet<Type>();

		public AutoMapperProfile()
			: this(new[] { Assembly.GetExecutingAssembly() })
		{
		}

		public AutoMapperProfile(params Assembly[] assembliesToScan)
		{
			var types = assembliesToScan.SelectMany(a => a.GetExportedTypes()).Where(t =>
				!t.IsAbstract && !t.IsInterface).ToList();

			LoadSimpleMappings(types);
			LoadCustomMappings(types);
			LoadCustomConverters(types);
			CreateReflexiveMaps(mappedTypes);
		}

		private void CreateReflexiveMaps(IEnumerable<Type> types)
		{
			foreach (var type in types.Where(t => t.GetConstructor(Array.Empty<Type>()) != null))
			{
				CreateMap(type, type);
			}
		}

		private void LoadCustomConverters(List<Type> types)
		{
			foreach (var (type, map) in GetMaps(types, typeof(ITypeConverter<,>)))
			{
				CreateMap(map[0], map[1]).ConvertUsing(type);
			}
		}

		private void LoadCustomMappings(List<Type> types)
		{
			foreach (var type in types.Where(t => typeof(ICustomMapping).IsAssignableFrom(t)))
			{
				var map = type.GetInterfaceMap(typeof(ICustomMapping));

				// call the method only if the type actually implemented it
				if (map.TargetMethods[0].DeclaringType == type)
					((ICustomMapping)Activator.CreateInstance(type)).CreateMapping(this);
			}
		}

		private void LoadSimpleMappings(List<Type> types)
		{
			foreach (var (src, dest) in GetMaps(types, typeof(IMapTo<>)))
			{
				mappedTypes.Add(src);

				var map = CreateMap(src, dest[0], MemberList.Source).IncludeAllDerived();

				foreach (var property in src.GetNotMappedProperties())
				{
					map = map.ForSourceMember(property.Name, opt => opt.DoNotValidate());
				}
			}

			foreach (var (dest, src) in GetMaps(types, typeof(IMapFrom<>)))
			{
				mappedTypes.Add(dest);

				var map = CreateMap(src[0], dest, MemberList.Destination).IncludeAllDerived();

				foreach (var property in dest.GetNotMappedProperties())
				{
					map = map.ForMember(property.Name, opt => opt.Ignore());
				}

				var t = dest;
				while ((t = t.BaseType) != typeof(object))
				{
					map = map.IncludeBase(src[0], t);
				}
			}
		}

		private IEnumerable<(Type type, Type[] metadataType)> GetMaps(
			IEnumerable<Type> types, Type ifaceType)
		{
			foreach (var type in types)
			{
				foreach (var iface in type.GetInterfaces().Where(i
					=> i.IsGenericType && i.GetGenericTypeDefinition() == ifaceType))
				{
					yield return (type, iface.GetGenericArguments());
				}
			}
		}
	}
}