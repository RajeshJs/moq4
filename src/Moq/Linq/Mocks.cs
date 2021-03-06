// Copyright (c) 2007, Clarius Consulting, Manas Technology Solutions, InSTEDD.
// All rights reserved. Licensed under the BSD 3-Clause License; see License.txt.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

using Moq.Linq;

namespace Moq
{
	/// <summary>
	/// Allows querying the universe of mocks for those that behave 
	/// according to the LINQ query specification.
	/// </summary>
	public static class Mocks
	{
		/// <summary>
		/// Access the universe of mocks of the given type, to retrieve those 
		/// that behave according to the LINQ query specification.
		/// </summary>
		/// <typeparam name="T">The type of the mocked object to query.</typeparam>
		public static IQueryable<T> Of<T>() where T : class
		{
			return CreateMockQuery<T>();
		}

		/// <summary>
		/// Access the universe of mocks of the given type, to retrieve those 
		/// that behave according to the LINQ query specification.
		/// </summary>
		/// <param name="specification">The predicate with the setup expressions.</param>
		/// <typeparam name="T">The type of the mocked object to query.</typeparam>
		[SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "By design")]
		public static IQueryable<T> Of<T>(Expression<Func<T, bool>> specification) where T : class
		{
			return CreateMockQuery<T>().Where(specification);
		}

		/// <summary>
		/// Creates an mock object of the indicated type.
		/// </summary>
		/// <typeparam name="T">The type of the mocked object.</typeparam>
		/// <returns>The mocked object created.</returns>
		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("Moved to Mock.Of<T>, as it's a single one, so no reason to be on Mocks.", true)]
		public static T OneOf<T>() where T : class
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// Creates an mock object of the indicated type.
		/// </summary>
		/// <param name="specification">The predicate with the setup expressions.</param>
		/// <typeparam name="T">The type of the mocked object.</typeparam>
		/// <returns>The mocked object created.</returns>
		[SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "By Design")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("Moved to Mock.Of<T>, as it's a single one, so no reason to be on Mocks.", true)]
		public static T OneOf<T>(Expression<Func<T, bool>> specification) where T : class
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// Creates the mock query with the underlying queryable implementation.
		/// </summary>
		internal static IQueryable<T> CreateMockQuery<T>() where T : class
		{
			var method = ((Func<IQueryable<T>>)CreateQueryable<T>).GetMethodInfo();
			return new MockQueryable<T>(Expression.Call(null,
				method));
		}

		/// <summary>
		/// Wraps the enumerator inside a queryable.
		/// </summary>
		internal static IQueryable<T> CreateQueryable<T>() where T : class
		{
			return CreateMocks<T>().AsQueryable();
		}

		/// <summary>
		/// Method that is turned into the actual call from .Query{T}, to 
		/// transform the queryable query into a normal enumerable query.
		/// This method is never used directly by consumers.
		/// </summary>
		private static IEnumerable<T> CreateMocks<T>() where T : class
		{
			do
			{
				var mock = new Mock<T>();
				mock.SetupAllProperties();

				yield return mock.Object;
			}
			while (true);
		}

		/// <summary>
		/// Extension method used to support Linq-like setup properties that are not virtual but do have 
		/// a getter and a setter, thereby allowing the use of Linq to Mocks to quickly initialize DTOs too :)
		/// </summary>
		internal static bool SetProperty<T, TResult>(Mock<T> target, Expression<Func<T, TResult>> propertyReference, TResult value)
			where T : class
		{
			var memberExpr = (MemberExpression)propertyReference.Body;
			var member = (PropertyInfo)memberExpr.Member;

			member.SetValue(target.Object, value, null);

			return true;
		}
	}
}
