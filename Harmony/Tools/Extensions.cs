// file:	Tools\Extensions.cs
//
// summary:	Implements the extensions class
/// 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Harmony
{
	/// <summary>A general extensions.</summary>
	public static class GeneralExtensions
	{
		/// <summary>An IEnumerable&lt;T&gt; extension method that joins.</summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <param name="enumeration">The enumeration to act on.</param>
		/// <param name="converter">  (Optional) The converter.</param>
		/// <param name="delimiter">  (Optional) The delimiter.</param>
		/// <returns>A string.</returns>
		///
		public static string Join<T>(this IEnumerable<T> enumeration, Func<T, string> converter = null, string delimiter = ", ")
		{
			if (converter == null) converter = t => t.ToString();
			return enumeration.Aggregate("", (prev, curr) => prev + (prev != "" ? delimiter : "") + converter(curr));
		}

		/// <summary>A Type[] extension method that descriptions the given parameters.</summary>
		/// <param name="parameters">The parameters to act on.</param>
		/// <returns>A string.</returns>
		///
		public static string Description(this Type[] parameters)
		{
			if (parameters == null) return "NULL";
			var pattern = @", \w+, Version=[0-9.]+, Culture=neutral, PublicKeyToken=[0-9a-f]+";
			return "(" + parameters.Join(p => p?.FullName == null ? "null" : Regex.Replace(p.FullName, pattern, "")) + ")";
		}

		/// <summary>A MethodBase extension method that full description.</summary>
		/// <param name="method">The method to act on.</param>
		/// <returns>A string.</returns>
		///
		public static string FullDescription(this MethodBase method)
		{
			var parameters = method.GetParameters().Select(p => p.ParameterType).ToArray();
			return method.DeclaringType.FullName + "." + method.Name + parameters.Description();
		}

		/// <summary>A ParameterInfo[] extension method that types the given pinfo.</summary>
		/// <param name="pinfo">The pinfo to act on.</param>
		/// <returns>A Type[].</returns>
		///
		public static Type[] Types(this ParameterInfo[] pinfo)
		{
			return pinfo.Select(pi => pi.ParameterType).ToArray();
		}

		/// <summary>A Dictionary&lt;S,T&gt; extension method that gets value safe.</summary>
		/// <typeparam name="S">Type of the s.</typeparam>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <param name="dictionary">The dictionary to act on.</param>
		/// <param name="key">		  The key.</param>
		/// <returns>The value safe.</returns>
		///
		public static T GetValueSafe<S, T>(this Dictionary<S, T> dictionary, S key)
		{
			T result;
			if (dictionary.TryGetValue(key, out result))
				return result;
			return default(T);
		}

		/// <summary>A Dictionary&lt;string,object&gt; extension method that gets typed value.</summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <param name="dictionary">The dictionary to act on.</param>
		/// <param name="key">		  The key.</param>
		/// <returns>The typed value.</returns>
		///
		public static T GetTypedValue<T>(this Dictionary<string, object> dictionary, string key)
		{
			object result;
			if (dictionary.TryGetValue(key, out result))
				if (result is T)
					return (T)result;
			return default(T);
		}
	}

	/// <summary>A collection extensions.</summary>
	public static class CollectionExtensions
	{
		/// <summary>An IEnumerable&lt;T&gt; extension method that does.</summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <param name="sequence">The sequence to act on.</param>
		/// <param name="action">  The action.</param>
		///
		public static void Do<T>(this IEnumerable<T> sequence, Action<T> action)
		{
			if (sequence == null) return;
			var enumerator = sequence.GetEnumerator();
			while (enumerator.MoveNext()) action(enumerator.Current);
		}

		/// <summary>An IEnumerable&lt;T&gt; extension method that executes if operation.</summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <param name="sequence"> The sequence to act on.</param>
		/// <param name="condition">The condition.</param>
		/// <param name="action">	 The action.</param>
		///
		public static void DoIf<T>(this IEnumerable<T> sequence, Func<T, bool> condition, Action<T> action)
		{
			sequence.Where(condition).Do(action);
		}

		/// <summary>Enumerates add in this collection.</summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <param name="sequence">The sequence to act on.</param>
		/// <param name="item">		The item.</param>
		/// <returns>An enumerator that allows foreach to be used to process add in this collection.</returns>
		///
		public static IEnumerable<T> Add<T>(this IEnumerable<T> sequence, T item)
		{
			return (sequence ?? Enumerable.Empty<T>()).Concat(new[] { item });
		}

		/// <summary>A T[] extension method that adds a range to array to 'items'.</summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <param name="sequence">The sequence to act on.</param>
		/// <param name="items">	The items.</param>
		/// <returns>A T[].</returns>
		///
		public static T[] AddRangeToArray<T>(this T[] sequence, T[] items)
		{
			return (sequence ?? Enumerable.Empty<T>()).Concat(items).ToArray();
		}

		/// <summary>A T[] extension method that adds to the array.</summary>
		/// <typeparam name="T">Generic type parameter.</typeparam>
		/// <param name="sequence">The sequence to act on.</param>
		/// <param name="item">		The item.</param>
		/// <returns>A T[].</returns>
		///
		public static T[] AddToArray<T>(this T[] sequence, T item)
		{
			return Add(sequence, item).ToArray();
		}
	}
}