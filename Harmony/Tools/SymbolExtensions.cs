// file:	Tools\SymbolExtensions.cs
//
// summary:	Implements the symbol extensions class
using System;
/// 
using System.Linq.Expressions;
using System.Reflection;

namespace Harmony
{
	/// <summary>A symbol extensions.</summary>
	public static class SymbolExtensions
	{
		/// <summary>Given a lambda expression that calls a method, returns the method info.</summary>
		/// <param name="expression">The expression.</param>
		/// <returns>The method information.</returns>
		///
		/// ### <typeparam name="T">.</typeparam>
		///
		public static MethodInfo GetMethodInfo(Expression<Action> expression)
		{
			return GetMethodInfo((LambdaExpression)expression);
		}

		/// <summary>Given a lambda expression that calls a method, returns the method info.</summary>
		/// <typeparam name="T">.</typeparam>
		/// <param name="expression">The expression.</param>
		/// <returns>The method information.</returns>
		///
		public static MethodInfo GetMethodInfo<T>(Expression<Action<T>> expression)
		{
			return GetMethodInfo((LambdaExpression)expression);
		}

		/// <summary>Given a lambda expression that calls a method, returns the method info.</summary>
		/// <typeparam name="T">		.</typeparam>
		/// <typeparam name="TResult">Type of the result.</typeparam>
		/// <param name="expression">The expression.</param>
		/// <returns>The method information.</returns>
		///
		public static MethodInfo GetMethodInfo<T, TResult>(Expression<Func<T, TResult>> expression)
		{
			return GetMethodInfo((LambdaExpression)expression);
		}

		/// <summary>Given a lambda expression that calls a method, returns the method info.</summary>
		/// <exception cref="ArgumentException">Thrown when one or more arguments have unsupported or illegal values.</exception>
		/// <exception cref="Exception">			 Thrown when an exception error condition occurs.</exception>
		/// <param name="expression">The expression.</param>
		/// <returns>The method information.</returns>
		///
		public static MethodInfo GetMethodInfo(LambdaExpression expression)
		{
			var outermostExpression = expression.Body as MethodCallExpression;

			if (outermostExpression == null)
				throw new ArgumentException("Invalid Expression. Expression should consist of a Method call only.");

			var method = outermostExpression.Method;
			if (method == null)
				throw new Exception("Cannot find method for expression " + expression);

			return method;
		}
	}
}