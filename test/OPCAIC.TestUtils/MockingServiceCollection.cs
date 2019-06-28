using System.Collections;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;

namespace OPCAIC.TestUtils
{
	/// <summary>
	///   Wrapper around <see cref="IServiceCollection" /> which supports simple specifying mocks.
	/// </summary>
	public class MockingServiceCollection : IServiceCollection
	{
		private readonly ServiceCollection serviceCollection = new ServiceCollection();

		/// <inheritdoc />
		public IEnumerator<ServiceDescriptor> GetEnumerator() => serviceCollection.GetEnumerator();

		/// <inheritdoc />
		IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable) serviceCollection).GetEnumerator();

		/// <inheritdoc />
		public void Add(ServiceDescriptor item) => serviceCollection.Add(item);

		/// <inheritdoc />
		public void Clear() => serviceCollection.Clear();

		/// <inheritdoc />
		public bool Contains(ServiceDescriptor item) => serviceCollection.Contains(item);

		/// <inheritdoc />
		public void CopyTo(ServiceDescriptor[] array, int arrayIndex)
			=> serviceCollection.CopyTo(array, arrayIndex);

		/// <inheritdoc />
		public bool Remove(ServiceDescriptor item) => serviceCollection.Remove(item);

		/// <inheritdoc />
		public int Count => serviceCollection.Count;

		/// <inheritdoc />
		public bool IsReadOnly => serviceCollection.IsReadOnly;

		/// <inheritdoc />
		public int IndexOf(ServiceDescriptor item) => serviceCollection.IndexOf(item);

		/// <inheritdoc />
		public void Insert(int index, ServiceDescriptor item) => serviceCollection.Insert(index, item);

		/// <inheritdoc />
		public void RemoveAt(int index) => serviceCollection.RemoveAt(index);

		/// <inheritdoc />
		public ServiceDescriptor this[int index]
		{
			get => serviceCollection[index];
			set => serviceCollection[index] = value;
		}

		public Mock<T> Mock<T>(MockBehavior behavior = MockBehavior.Default) where T : class
		{
			var mock = new Mock<T>(behavior);
			serviceCollection.RemoveAll(typeof(T));
			serviceCollection.AddSingleton(mock.Object);
			return mock;
		}
	}
}
